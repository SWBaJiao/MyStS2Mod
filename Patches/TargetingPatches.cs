using System;
using System.Linq;
using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MyStS2Mod.Utils;

namespace MyStS2Mod.Patches
{
    /// <summary>
    /// Patch 1: NTargetManager.AllowedToTargetCreature
    ///
    /// 原始逻辑：AnyEnemy 时只允许选中 CombatSide.Enemy
    /// 修改后：按住开关键 + 卡牌在白名单中 → 也允许选中队友（己方玩家）
    /// </summary>
    [HarmonyPatch(typeof(NTargetManager), "AllowedToTargetCreature")]
    public static class AllowedToTargetCreaturePatch
    {
        /// <summary>
        /// Postfix: 原方法返回 false 时，检查是否应该允许友伤选中
        /// </summary>
        static void Postfix(NTargetManager __instance, Creature creature, ref bool __result)
        {
            // 已经允许的不用管
            if (__result) return;

            // 必须按住开关键
            if (!FriendlyFireState.IsToggleKeyHeld) return;

            // 通过反射获取 _validTargetsType 字段
            var field = AccessTools.Field(typeof(NTargetManager), "_validTargetsType");
            if (field == null) return;
            var rawValue = field.GetValue(__instance);
            if (rawValue == null) return;
            var validTargetsType = (TargetType)rawValue;

            // 只对 AnyEnemy 类型的卡牌生效（单体攻击牌）
            if (validTargetsType != TargetType.AnyEnemy) return;

            // 目标必须是活着的玩家（队友）
            if (!creature.IsPlayer || creature.IsDead) return;

            // 检查白名单
            var cardName = FriendlyFireState.CurrentTargetingCardName;
            if (cardName != null && !FriendlyFireConfig.IsSingleTargetAllowed(cardName))
                return;

            // 允许选中队友！
            __result = true;
        }
    }

    /// <summary>
    /// Patch 2: CardModel.IsValidTarget
    ///
    /// 原始逻辑：AnyEnemy 时要求 target.Side != Owner.Creature.Side
    /// 修改后：按住开关键 + 白名单 → 对队友也返回 true
    /// </summary>
    [HarmonyPatch(typeof(CardModel), "IsValidTarget")]
    public static class IsValidTargetPatch
    {
        static void Postfix(CardModel __instance, Creature target, ref bool __result)
        {
            if (__result) return;
            if (target == null || !target.IsAlive) return;
            if (!FriendlyFireState.IsToggleKeyHeld) return;

            // 单体攻击牌: AnyEnemy → 允许选队友
            if (__instance.TargetType == TargetType.AnyEnemy)
            {
                // 目标必须是己方
                if (target.Side != __instance.Owner.Creature.Side) return;

                // 检查白名单
                var cardName = __instance.GetType().Name;
                if (!FriendlyFireConfig.IsSingleTargetAllowed(cardName)) return;

                __result = true;
            }
        }
    }

    /// <summary>
    /// Patch 3: 追踪当前正在选择目标的卡牌
    ///
    /// 在 NMouseCardPlay.SingleCreatureTargeting 开始时记录卡牌类名，
    /// 供 AllowedToTargetCreaturePatch 查询白名单
    /// </summary>
    [HarmonyPatch(typeof(NMouseCardPlay), "SingleCreatureTargeting")]
    public static class TrackTargetingCardPatch
    {
        static void Prefix(NMouseCardPlay __instance)
        {
            try
            {
                // NMouseCardPlay 继承自某个包含 Card 属性的基类
                var cardProp = AccessTools.Property(__instance.GetType(), "Card")
                    ?? AccessTools.Property(__instance.GetType().BaseType, "Card");
                if (cardProp != null)
                {
                    var card = cardProp.GetValue(__instance) as CardModel;
                    FriendlyFireState.CurrentTargetingCardName = card?.GetType().Name;
                }
            }
            catch
            {
                FriendlyFireState.CurrentTargetingCardName = null;
            }
        }

        static void Postfix()
        {
            FriendlyFireState.CurrentTargetingCardName = null;
        }
    }
}
