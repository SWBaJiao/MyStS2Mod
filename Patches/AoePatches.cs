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
    /// AOE 友伤补丁：AttackCommand.GetPossibleTargets
    ///
    /// 原始逻辑：多目标攻击时，调用 combatState.GetOpponentsOf(Attacker) 获取所有敌方
    /// 修改后：按住开关键 + AOE 白名单 → 返回敌人 + 队友（排除自己）
    ///
    /// 实现策略（v2 修复）：
    ///   不替换整个列表，而是在原始结果（所有敌人）的基础上，
    ///   追加 Allies 列表中排除攻击者自身的队友。
    ///   这样更安全：
    ///   1. 敌人列表保持原始逻辑不变
    ///   2. 队友排除使用多重比较确保不会打到自己
    ///   3. Allies 是 _allies 字段直接引用，引用比较更可靠
    /// </summary>
    [HarmonyPatch(typeof(AttackCommand), "GetPossibleTargets")]
    public static class GetPossibleTargetsPatch
    {
        /// <summary>缓存反射字段，避免每次调用都查找</summary>
        private static System.Reflection.FieldInfo? _combatStateField;

        static void Postfix(AttackCommand __instance, ref IReadOnlyList<Creature> __result)
        {
            // 必须按住开关键
            if (!FriendlyFireState.IsToggleKeyHeld) return;

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

            // 获取卡牌来源，检查白名单
            var modelSource = __instance.ModelSource as CardModel;
            if (modelSource != null)
            {
                var cardName = modelSource.GetType().Name;
                if (!FriendlyFireConfig.IsAoeAllowed(cardName)) return;
            }

            // ===== 核心逻辑：在原始敌人列表上追加队友（排除自己）=====
            //
            // __result 此时 = combatState.GetOpponentsOf(Attacker) = 所有活着的敌人
            // Allies = _allies 列表引用（包含所有己方生物）
            //
            // 排除自己的三重保险：
            //   1. ReferenceEquals 引用比较
            //   2. CombatId 值比较（每个生物唯一）
            //   3. Player 对象引用比较
            var alliesExceptSelf = combatState.Allies
                .Where(c => c.IsAlive && !IsSameCreature(c, attacker))
                .ToList();

            if (alliesExceptSelf.Count == 0)
            {
                // 没有活着的队友，不需要修改
                return;
            }

            // 拼接：原始敌人列表 + 队友（排除自己）
            var combined = __result.Concat(alliesExceptSelf).ToList().AsReadOnly();

            Plugin.Logger.Info(
                $"AOE 友伤生效: 敌人={__result.Count}, 队友={alliesExceptSelf.Count}, " +
                $"总目标={combined.Count}, 攻击者={attacker.Name}(ID:{attacker.CombatId})");

            __result = combined;
        }

        /// <summary>
        /// 判断两个 Creature 是否是同一个（三重比较保险）
        /// </summary>
        private static bool IsSameCreature(Creature a, Creature b)
        {
            // 1. 引用相等 — 最快
            if (ReferenceEquals(a, b)) return true;

            // 2. CombatId 相等 — 每个生物在战斗中有唯一 ID
            if (a.CombatId == b.CombatId) return true;

            // 3. 同一个玩家的生物
            if (a.Player != null && b.Player != null && ReferenceEquals(a.Player, b.Player))
                return true;

            return false;
        }
    }
}
