using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.Models;
using MyStS2Mod.Utils;

namespace MyStS2Mod.Patches
{
    // ================================================================
    // 多人游戏 AOE 友伤同步机制
    //
    // 问题：AOE 卡牌不像单体卡那样有明确的 targetId。
    //       GetPossibleTargets 在所有客户端独立计算目标列表。
    //       如果只在按 Alt 的客户端扩展目标，就会 desync。
    //
    // 解决方案：利用 PlayCardAction.TargetId 作为信号载体
    //
    //   正常 AOE：TargetId = null（不需要指定目标）
    //   友伤 AOE：TargetId = 自身 CombatId（信号：要打队友）
    //
    //   这个 TargetId 通过 NetPlayCardAction 自动同步到所有客户端。
    //   接收端检测到 "AllEnemies + TargetId = 自身" → 激活 AOE 友伤。
    // ================================================================

    // ================================================================
    // Patch A: PlayCardAction 构造函数（本地创建 Action 时）
    //
    // 当玩家按住 Alt 打出 AOE 卡牌时，
    // 将 TargetId 从 null 改为自身 CombatId 作为信号。
    //
    // 只有第一个构造函数会在本地调用：
    //   PlayCardAction(CardModel cardModel, Creature? target)
    //
    // 第二个构造函数从网络反序列化，已经携带了信号，不需要修改。
    // ================================================================
    [HarmonyPatch(typeof(PlayCardAction),
        MethodType.Constructor,
        new Type[] { typeof(CardModel), typeof(Creature) })]
    public static class PlayCardActionCtorPatch
    {
        /// <summary>缓存 TargetId 的 backing field</summary>
        private static System.Reflection.FieldInfo? _targetIdField;

        static void Postfix(PlayCardAction __instance, CardModel cardModel, Creature? target)
        {
            // 只在本地按住 Alt 时修改
            if (!FriendlyFireState.IsToggleKeyHeld) return;

            // 只处理 AOE 攻击卡（AllEnemies / RandomEnemy）
            var tt = cardModel.TargetType;
            if (tt != TargetType.AllEnemies && tt != TargetType.RandomEnemy) return;

            // 检查 AOE 白名单
            var cardName = cardModel.GetType().Name;
            if (!FriendlyFireConfig.IsAoeAllowed(cardName)) return;

            // 获取自身 CombatId 作为信号
            var selfId = cardModel.Owner?.Creature?.CombatId;
            if (!selfId.HasValue) return;

            // 写入 TargetId（readonly 属性，通过 backing field 修改）
            _targetIdField ??= AccessTools.Field(typeof(PlayCardAction), "<TargetId>k__BackingField");
            if (_targetIdField == null) return;

            _targetIdField.SetValue(__instance, selfId);

            Plugin.Logger.Info(
                $"AOE 友伤信号已编码: card={cardName}, TargetId={selfId} (self)");
        }
    }

    // ================================================================
    // Patch B: PlayCardAction.ExecuteAction（所有客户端都执行）
    //
    // 在 Action 执行前，检测 AOE 友伤信号：
    //   如果卡牌是 AllEnemies/RandomEnemy 且 TargetId 非空
    //   且 TargetId 指向己方生物 → 激活 AOE 友伤标志
    //
    // 这个 Prefix 在所有客户端上以相同的输入执行，
    // 产生相同的结果 → 不会 desync。
    //
    // 注意：ExecuteAction 是 async Task，
    //       但 Prefix 在方法体开始前同步执行，
    //       GetPossibleTargets 在方法体内同步调用，
    //       Godot 单线程 → 标志在整个执行过程中有效。
    // ================================================================
    [HarmonyPatch(typeof(PlayCardAction), "ExecuteAction")]
    public static class ExecuteActionPatch
    {
        static void Prefix(PlayCardAction __instance)
        {
            // 每次 Action 执行前先重置标志
            FriendlyFireState.IsAoeFriendlyFireActive = false;

            try
            {
                // 获取 _card 字段（此时可能还没初始化，需要用 NetCombatCard 重建）
                // 但是 ExecuteAction 开头就会设置 _card:
                //   _card = NetCombatCard.ToCardModel();
                // 我们的 Prefix 在它之前运行，所以用 CardModelId 来获取卡牌信息

                var targetId = __instance.TargetId;
                if (!targetId.HasValue) return; // 正常 AOE，无信号

                // 通过 NetCombatCard 获取卡牌模型来检查 TargetType
                var card = __instance.NetCombatCard.ToCardModel();
                if (card == null) return;

                var tt = card.TargetType;
                if (tt != TargetType.AllEnemies && tt != TargetType.RandomEnemy) return;

                // 检查 TargetId 是否指向己方生物（信号验证）
                var ownerCreature = __instance.Player?.Creature;
                if (ownerCreature == null) return;

                // TargetId 应该等于卡牌拥有者的 CombatId
                if (targetId.Value != ownerCreature.CombatId) return;

                // 检查白名单（所有客户端配置相同 → 结果一致）
                var cardName = card.GetType().Name;
                if (!FriendlyFireConfig.IsAoeAllowed(cardName)) return;

                // ★ 激活 AOE 友伤！
                FriendlyFireState.IsAoeFriendlyFireActive = true;

                Plugin.Logger.Info(
                    $"AOE 友伤信号检测到: card={cardName}, " +
                    $"owner={__instance.Player?.NetId}, targetId={targetId}");
            }
            catch (Exception ex)
            {
                Plugin.Logger.Info($"ExecuteAction AOE 检测异常: {ex.Message}");
                FriendlyFireState.IsAoeFriendlyFireActive = false;
            }
        }
    }
}
