using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MyStS2Mod.Utils;

namespace MyStS2Mod.Patches
{
    /// <summary>
    /// AOE 友伤补丁：AttackCommand.GetPossibleTargets [执行层 — 网络同步]
    ///
    /// 原始逻辑：多目标攻击时，调用 combatState.GetOpponentsOf(Attacker) 获取所有敌方
    /// 修改后：白名单中的 AOE 卡牌 → 返回敌人 + 队友（排除自己）
    ///
    /// ★★★ 多人游戏关键 ★★★
    /// GetPossibleTargets 在 AttackCommand.Execute() 中被调用，
    /// 是所有客户端都会独立执行的代码路径。
    ///
    /// 如果这里检查 Alt 键：
    ///   - 出牌者：Alt 按着 → 目标包含队友 → HP 变化
    ///   - 队友端：Alt 没按 → 目标只有敌人 → HP 不变
    ///   → State Divergence → 断连！
    ///
    /// 因此 AOE 友伤不检查 Alt 键，只检查白名单。
    /// 白名单配置对所有安装了 mod 的客户端都一样，
    /// 所以所有端计算出相同的目标列表 → 状态一致。
    ///
    /// 如果你不想某张 AOE 卡牌打到队友，从 aoe_whitelist 中移除即可。
    /// </summary>
    [HarmonyPatch(typeof(AttackCommand), "GetPossibleTargets")]
    public static class GetPossibleTargetsPatch
    {
        private static System.Reflection.FieldInfo? _combatStateField;

        static void Postfix(AttackCommand __instance, ref IReadOnlyList<Creature> __result)
        {
            // ★ 执行层：使用网络同步的信号标志，不检查本地 Alt 键！
            if (!FriendlyFireState.IsAoeFriendlyFireActive) return;

            // 只处理多目标（AOE）攻击
            if (!__instance.IsMultiTargeted) return;

            // 获取 Attacker
            var attacker = __instance.Attacker;
            if (attacker == null) return;

            // 只处理玩家的攻击（不改变怪物的 AOE）
            if (attacker.Side != CombatSide.Player) return;

            // 获取 _combatState
            _combatStateField ??= AccessTools.Field(typeof(AttackCommand), "_combatState");
            if (_combatStateField == null) return;
            var combatState = _combatStateField.GetValue(__instance) as CombatState;
            if (combatState == null) return;

            // 检查白名单（白名单配置在所有客户端上相同 → 结果一致）
            var modelSource = __instance.ModelSource as CardModel;
            if (modelSource != null)
            {
                var cardName = modelSource.GetType().Name;
                if (!FriendlyFireConfig.IsAoeAllowed(cardName)) return;
            }

            // ===== 在原始敌人列表上追加其他玩家的角色（排除自己和自己的召唤物）=====
            var attackerPlayer = GetOwnerPlayer(attacker);

            var otherPlayerCreatures = combatState.Allies
                .Where(c => c.IsAlive && !BelongsToPlayer(c, attackerPlayer))
                .ToList();

            if (otherPlayerCreatures.Count == 0) return;

            var combined = __result.Concat(otherPlayerCreatures).ToList().AsReadOnly();
            __result = combined;
        }

        /// <summary>
        /// 获取一个 Creature 所属的 Player。
        ///
        /// STS2 中 Creature 与 Player 的关系：
        ///   - 玩家角色：creature.Player = 对应的 Player
        ///   - 召唤物/宠物：creature.Player = null, creature.PetOwner = 主人的 Player
        ///   - 怪物：creature.Player = null, creature.PetOwner = null
        /// </summary>
        private static MegaCrit.Sts2.Core.Entities.Players.Player? GetOwnerPlayer(Creature creature)
        {
            // 优先用 Player（玩家角色本身）
            if (creature.Player != null)
                return creature.Player;
            // 其次用 PetOwner（召唤物的主人）
            if (creature.PetOwner != null)
                return creature.PetOwner;
            return null;
        }

        /// <summary>
        /// 判断 Creature 是否属于指定 Player（包括该玩家的召唤物）。
        ///
        /// 排除逻辑：
        ///   亡灵契约师(Player A) 打出 AOE →
        ///     亡灵契约师本体: Player=A → 属于A → 排除 ✅
        ///     亡灵契约师的召唤物: PetOwner=A → 属于A → 排除 ✅
        ///     铁甲战士(Player B): Player=B → 不属于A → 命中 ✅
        ///     铁甲战士的宠物: PetOwner=B → 不属于A → 命中 ✅
        /// </summary>
        private static bool BelongsToPlayer(
            Creature creature,
            MegaCrit.Sts2.Core.Entities.Players.Player? player)
        {
            if (player == null) return false;

            // 该生物本身就是这个玩家的角色
            if (creature.Player != null && ReferenceEquals(creature.Player, player))
                return true;

            // 该生物是这个玩家的召唤物/宠物
            if (creature.PetOwner != null && ReferenceEquals(creature.PetOwner, player))
                return true;

            return false;
        }
    }
}
