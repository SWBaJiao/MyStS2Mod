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
    /// Patch 4: AttackCommand.GetPossibleTargets
    ///
    /// 原始逻辑：多目标攻击时，调用 combatState.GetOpponentsOf(Attacker) 获取所有敌方
    /// 修改后：按住开关键 + AOE 白名单 → 返回除攻击者以外的所有 Creature（敌人 + 队友）
    ///
    /// 关键代码路径：
    ///   GetPossibleTargets() {
    ///     if (IsMultiTargeted) {
    ///       if (_sourceType == Monster) return combatState.PlayerCreatures;
    ///       return combatState.GetOpponentsOf(Attacker);  // ← 我们要改这里
    ///     }
    ///   }
    /// </summary>
    [HarmonyPatch(typeof(AttackCommand), "GetPossibleTargets")]
    public static class GetPossibleTargetsPatch
    {
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
            var csField = AccessTools.Field(typeof(AttackCommand), "_combatState");
            if (csField == null) return;
            var combatState = csField.GetValue(__instance) as CombatState;
            if (combatState == null) return;

            // 获取卡牌来源，检查白名单
            var modelSourceProp = AccessTools.Property(typeof(AttackCommand), "ModelSource");
            if (modelSourceProp != null)
            {
                var modelSource = modelSourceProp.GetValue(__instance) as CardModel;
                if (modelSource != null)
                {
                    var cardName = modelSource.GetType().Name;
                    if (!FriendlyFireConfig.IsAoeAllowed(cardName)) return;
                }
            }

            // 核心修改：返回除攻击者以外的所有 Creature（敌人 + 队友）
            __result = combatState.Creatures
                .Where(c => c.IsAlive && c != attacker)
                .ToList()
                .AsReadOnly();

            Console.WriteLine($"[{ModInfo.NAME}] AOE 友伤生效，目标数: {__result.Count}");
        }
    }
}
