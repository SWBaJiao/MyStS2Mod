using System;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MyStS2Mod.Utils;

namespace MyStS2Mod.Patches
{
    // ================================================================
    // 诅咒牌转移系统
    //
    // 功能：按住触发键时，诅咒牌变为可打出状态，消耗能量后
    //       将诅咒转移到队友的手牌中。
    //
    // 多人同步安全性：
    //   诅咒牌正常不可打出（Unplayable 关键词）。
    //   如果 PlayCardAction 中出现了诅咒牌 + 队友目标，
    //   所有客户端都能推断这是转移操作，无需额外信号。
    //
    // 参考实现：GlimpseBeyond（窥探彼岸）— 给队友抽牌堆放灵魂
    //   Soul.Create(creature.Player, ...) → CardPileCmd.AddGeneratedCardsToCombat
    // ================================================================

    // ================================================================
    // Patch 1: CanPlay — 让诅咒牌在满足条件时可打出
    //
    // 层级：执行层（所有客户端一致）
    //
    // 原始逻辑：Keywords.Contains(Unplayable) → reason |= HasUnplayableKeyword
    // 修改后：对诅咒牌清除 HasUnplayableKeyword 标志，
    //         但要求有足够能量支付转移费用。
    //
    // 注意：执行层不检查 Alt 键。诅咒牌能被打出的唯一入口
    //       是 UI 层的 TargetSelection patch 将其导入目标选择流程。
    // ================================================================
    [HarmonyPatch]
    public static class CurseCanPlayPatch
    {
        // 匹配带 out 参数的重载: CanPlay(out UnplayableReason, out AbstractModel)
        static System.Reflection.MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(CardModel), nameof(CardModel.CanPlay),
                new Type[] { typeof(UnplayableReason).MakeByRefType(), typeof(AbstractModel).MakeByRefType() });
        }

        static void Postfix(CardModel __instance, ref UnplayableReason reason, ref bool __result)
        {
            if (__result) return; // 已经可以打出，不需要干预

            // 只处理诅咒牌
            if (__instance.Type != CardType.Curse) return;

            // 配置检查
            if (!FriendlyFireConfig.CurseTransferEnabled) return;

            // 必须在战斗中
            var combatState = __instance.CombatState;
            if (combatState == null) return;
            if (__instance.Owner?.PlayerCombatState == null) return;

            // 必须有队友存活（多人模式检查）
            var teammates = combatState.GetTeammatesOf(__instance.Owner.Creature);
            bool hasLivingTeammate = teammates.Any(c =>
                c != null && c.IsAlive && c.IsPlayer
                && c != __instance.Owner.Creature);
            if (!hasLivingTeammate) return;

            // 清除 Unplayable 标志
            reason &= ~UnplayableReason.HasUnplayableKeyword;

            // 检查能量是否足够
            int currentEnergy = (int)__instance.Owner.PlayerCombatState.Energy;
            if (currentEnergy < FriendlyFireConfig.CurseTransferCost)
            {
                reason |= UnplayableReason.EnergyCostTooHigh;
            }

            // 重新判断
            __result = reason == UnplayableReason.None;
        }
    }

    // ================================================================
    // Patch 2: TargetSelection — 拦截诅咒牌，强制进入 AnyAlly 选择模式
    //
    // 层级：UI层（仅本地）
    //
    // 原始逻辑：TargetType.None → 走 MultiCreatureTargeting
    // 修改后：诅咒牌 + Alt → SingleCreatureTargeting(AnyAlly)
    // ================================================================
    [HarmonyPatch(typeof(NMouseCardPlay), "TargetSelection")]
    public static class CurseTargetSelectionPatch
    {
        // 优先级高于已有的 TrackTargetSelectionPatch
        [HarmonyPriority(Priority.High)]
        static bool Prefix(NMouseCardPlay __instance, TargetMode targetMode, ref Task __result)
        {
            // UI 层：必须按住触发键
            if (!FriendlyFireState.IsToggleKeyHeld) return true;

            if (!FriendlyFireConfig.CurseTransferEnabled) return true;

            // 获取当前卡牌
            var card = GetCard(__instance);
            if (card == null) return true;

            // 只处理诅咒牌
            if (card.Type != CardType.Curse) return true;

            // 追踪卡牌名（供 AllowedToTargetNode 使用）
            FriendlyFireState.CurrentTargetingCardName =
                IsValidTargetPatch.GetCardName(card);

            // 强制进入 AnyAlly 单体目标选择
            _singleCreatureTargeting ??= AccessTools.Method(
                typeof(NMouseCardPlay), "SingleCreatureTargeting");

            if (_singleCreatureTargeting == null)
            {
                Plugin.Logger.Info("CurseTransfer: SingleCreatureTargeting 方法未找到");
                return true;
            }

            // ★ 关键：必须传递原始 targetMode，不能用 default(TargetMode)！
            // default(TargetMode) = None → NTargetManager.IsInSelection = false
            // → OnNodeHovered 和 _Input 都会直接 return → 无法悬停/点击
            __result = (Task)_singleCreatureTargeting.Invoke(
                __instance,
                new object[] { targetMode, TargetType.AnyAlly })!;

            Plugin.Logger.Info($"诅咒牌 {IsValidTargetPatch.GetCardName(card)} 进入目标选择模式");
            return false; // 跳过原始 TargetSelection
        }

        private static System.Reflection.MethodInfo? _singleCreatureTargeting;

        private static System.Reflection.PropertyInfo? _cardProp;

        private static CardModel? GetCard(NMouseCardPlay instance)
        {
            _cardProp ??= AccessTools.Property(typeof(NMouseCardPlay), "Card")
                ?? AccessTools.Property(typeof(NMouseCardPlay).BaseType!, "Card");
            return _cardProp?.GetValue(instance) as CardModel;
        }
    }

    // ================================================================
    // Patch 3: OnPlayWrapper — 拦截诅咒牌的执行，改为转移逻辑
    //
    // 层级：执行层（所有客户端一致）
    //
    // 原始逻辑：调用 OnPlay → 卡牌效果 → 移到弃牌堆/消耗堆
    // 修改后：诅咒牌 + 队友目标 → 消耗能量 + 移除原卡 + 队友手牌加新诅咒
    //
    // 参考：GlimpseBeyond 的 Soul.Create(creature.Player, ...) 模式
    // ================================================================
    [HarmonyPatch(typeof(CardModel), nameof(CardModel.OnPlayWrapper))]
    public static class CurseTransferOnPlayPatch
    {
        [HarmonyPriority(Priority.High)]
        static bool Prefix(CardModel __instance, PlayerChoiceContext choiceContext,
            Creature? target, ref Task __result)
        {
            // 只处理诅咒牌
            if (__instance.Type != CardType.Curse) return true;

            // 必须有目标（诅咒牌正常没有目标，有目标 = 转移模式）
            if (target == null) return true;

            // 目标必须是己方玩家（队友），且不是自己
            if (!target.IsPlayer || target.IsDead) return true;
            if (target.Player == __instance.Owner) return true;

            // 配置检查
            if (!FriendlyFireConfig.CurseTransferEnabled) return true;

            // 拦截原方法，执行转移
            __result = ExecuteCurseTransfer(__instance, choiceContext, target);
            return false;
        }

        private static async Task ExecuteCurseTransfer(
            CardModel curseCard, PlayerChoiceContext choiceContext, Creature target)
        {
            try
            {
                var combatState = curseCard.CombatState;
                if (combatState == null) return;

                Plugin.Logger.Info(
                    $"诅咒转移: {curseCard.GetType().Name} → {target.Player?.NetId}");

                // 1. 消耗能量
                int cost = FriendlyFireConfig.CurseTransferCost;
                curseCard.Owner?.PlayerCombatState?.LoseEnergy(cost);

                // 2. 从战斗中移除原诅咒牌
                await CardPileCmd.RemoveFromCombat(curseCard, skipVisuals: false);

                // 3. 为队友创建同类型的诅咒卡实例
                //    使用反射调用 CombatState.CreateCard<T>(Player) 泛型方法
                var curseType = curseCard.GetType();
                var createCardMethod = typeof(CombatState)
                    .GetMethods()
                    .FirstOrDefault(m => m.Name == "CreateCard"
                        && m.IsGenericMethod
                        && m.GetParameters().Length == 1
                        && m.GetParameters()[0].ParameterType.Name == "Player");

                CardModel? newCurse = null;
                if (createCardMethod != null)
                {
                    var generic = createCardMethod.MakeGenericMethod(curseType);
                    newCurse = generic.Invoke(combatState, new object[] { target.Player! }) as CardModel;
                }

                if (newCurse == null)
                {
                    Plugin.Logger.Info($"诅咒转移: 创建新卡失败 ({curseType.Name})，尝试备用方法");

                    // 备用：直接通过 Activator 创建实例
                    newCurse = Activator.CreateInstance(curseType) as CardModel;
                    if (newCurse == null)
                    {
                        Plugin.Logger.Info("诅咒转移: 备用方法也失败了");
                        return;
                    }
                }

                // 4. 加入队友手牌
                var result = await CardPileCmd.AddGeneratedCardToCombat(
                    newCurse, PileType.Hand, addedByPlayer: true);

                // 5. 只给接收者显示预览（参考 GlimpseBeyond）
                if (LocalContext.IsMe(target))
                {
                    CardCmd.PreviewCardPileAdd(result);
                }

                Plugin.Logger.Info(
                    $"诅咒转移完成: {curseType.Name}, 消耗 {cost} 能量");
            }
            catch (Exception ex)
            {
                Plugin.Logger.Info($"诅咒转移异常: {ex.Message}");
            }
        }
    }
}
