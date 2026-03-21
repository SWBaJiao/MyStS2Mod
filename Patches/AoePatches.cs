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

            // ===== 在原始敌人列表上追加队友（排除自己）=====
            var alliesExceptSelf = combatState.Allies
                .Where(c => c.IsAlive && !IsSameCreature(c, attacker))
                .ToList();

            if (alliesExceptSelf.Count == 0) return;

            var combined = __result.Concat(alliesExceptSelf).ToList().AsReadOnly();
            __result = combined;
        }

        /// <summary>
        /// 判断两个 Creature 是否是同一个（三重比较保险）
        /// </summary>
        private static bool IsSameCreature(Creature a, Creature b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a.CombatId == b.CombatId) return true;
            if (a.Player != null && b.Player != null && ReferenceEquals(a.Player, b.Player))
                return true;
            return false;
        }
    }
}
