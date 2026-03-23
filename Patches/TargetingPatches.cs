using System;
using System.Linq;
using System.Collections.Generic;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MyStS2Mod.Utils;

namespace MyStS2Mod.Patches
{
    // ================================================================
    // 【多人游戏架构说明】
    //
    // STS2 多人模式使用 **确定性锁步** (Deterministic Lockstep)：
    //   1. 玩家 A 打出卡牌 → 发送 PlayCardAction(card, targetId) 到网络
    //   2. 所有客户端独立执行相同的 Action
    //   3. 执行后比较 checksum，不一致则断开连接
    //
    // 因此 Patch 分为两层：
    //   - UI 层 (AllowedToTargetNode): 检查 Alt 键 → 仅本地执行，控制选择
    //   - 执行层 (IsValidTarget, GetPossibleTargets): 不检查 Alt 键
    //     → 所有客户端都执行，只检查白名单，确保结果一致
    //
    // 这样当 Action 通过网络同步时，所有端的执行结果相同，不会 desync。
    // ================================================================

    // ================================================================
    // Patch 1: NTargetManager.AllowedToTargetNode [UI 层 — 本地]
    //
    // 只在玩家拖动卡牌选目标时调用。
    // 保留 Alt 键检查：必须按住 Alt 才能将鼠标指向队友。
    // 这是纯本地 UI 操作，不参与网络同步。
    // ================================================================
    [HarmonyPatch(typeof(NTargetManager), nameof(NTargetManager.AllowedToTargetNode))]
    public static class AllowedToTargetNodePatch
    {
        static void Postfix(NTargetManager __instance, Node node, ref bool __result)
        {
            if (__result) return;

            // UI 层：必须按住开关键
            if (!FriendlyFireState.IsToggleKeyHeld) return;

            // 提取 Creature
            Creature? creature = null;
            if (node is NCreature nCreature)
                creature = nCreature.Entity;
            else
            {
                var playerStateProp = AccessTools.Property(node.GetType(), "Player");
                if (playerStateProp != null)
                {
                    var player = playerStateProp.GetValue(node);
                    if (player != null)
                    {
                        var creatureProp = AccessTools.Property(player.GetType(), "Creature");
                        creature = creatureProp?.GetValue(player) as Creature;
                    }
                }
            }

            if (creature == null) return;

            if (creature.IsDead) return;

            var validTargetsType = GetValidTargetsType(__instance);
            var cardName = FriendlyFireState.CurrentTargetingCardName;

            // --- AnyEnemy 类型：允许选中队友（攻击卡友伤）---
            if (validTargetsType == TargetType.AnyEnemy)
            {
                if (!creature.IsPlayer) return;
                if (cardName != null && !FriendlyFireConfig.IsSingleTargetAllowed(cardName))
                    return;
                __result = true;
                return;
            }

            // --- AnyPlayer 类型：Self 卡重定向时也允许选中敌人 ---
            // SelfTargetPatches 将 Self 卡的 TargetType 改为 AnyPlayer 进入目标选择。
            // 原始的 AnyPlayer 只允许选玩家方，这里扩展为也允许选敌方。
            if (validTargetsType == TargetType.AnyPlayer)
            {
                if (creature.IsPlayer) return; // 原始逻辑已允许，不用管
                // 敌方 Creature — 检查 Self 卡白名单
                if (cardName != null && !FriendlyFireConfig.IsSelfTargetAllowed(cardName))
                    return;
                __result = true;
            }
        }

        private static System.Reflection.FieldInfo? _validTargetsTypeField;

        internal static TargetType GetValidTargetsType(NTargetManager instance)
        {
            _validTargetsTypeField ??= AccessTools.Field(typeof(NTargetManager), "_validTargetsType");
            if (_validTargetsTypeField == null) return TargetType.None;
            var raw = _validTargetsTypeField.GetValue(instance);
            return raw != null ? (TargetType)raw : TargetType.None;
        }
    }

    // ================================================================
    // Patch 2: NTargetManager.AllowedToTargetCreature [UI 层 — 备用]
    //
    // 同 Patch 1，但 patch private 方法作为备份。
    // JIT 可能内联此方法，所以 Patch 1 才是主力。
    // ================================================================
    [HarmonyPatch(typeof(NTargetManager), "AllowedToTargetCreature")]
    public static class AllowedToTargetCreaturePatch
    {
        static void Postfix(NTargetManager __instance, Creature creature, ref bool __result)
        {
            if (__result) return;

            // UI 层：必须按住开关键
            if (!FriendlyFireState.IsToggleKeyHeld) return;

            var validTargetsType = AllowedToTargetNodePatch.GetValidTargetsType(__instance);
            if (validTargetsType != TargetType.AnyEnemy) return;

            if (!creature.IsPlayer || creature.IsDead) return;

            var cardName = FriendlyFireState.CurrentTargetingCardName;
            if (cardName != null && !FriendlyFireConfig.IsSingleTargetAllowed(cardName))
                return;

            __result = true;
        }
    }

    // ================================================================
    // Patch 3: CardModel.IsValidTarget [执行层 — 网络同步]
    //
    // 在 PlayCardAction.ExecuteAction() 中被调用：
    //   if (!_card.IsValidTarget(target)) { Cancel(); return; }
    //
    // 这是所有客户端都会执行的逻辑！如果这里检查 Alt 键：
    //   - 出牌者的客户端：Alt 按着 → true → 伤害生效
    //   - 队友的客户端：Alt 没按 → false → Cancel → 状态不一致 → 断连！
    //
    // 修复：不检查 Alt 键。只要卡牌在白名单中且目标是队友，就允许。
    // UI 层 (AllowedToTargetNode) 已经拦住了误操作。
    // ================================================================
    [HarmonyPatch(typeof(CardModel), nameof(CardModel.IsValidTarget))]
    public static class IsValidTargetPatch
    {
        static void Postfix(CardModel __instance, Creature target, ref bool __result)
        {
            if (__result) return;
            if (target == null || !target.IsAlive) return;

            // ★ 执行层：不检查 Alt 键！所有客户端必须得到相同结果。

            var tt = __instance.TargetType;

            // --- 单体攻击卡（AnyEnemy）→ 允许攻击队友 ---
            if (tt == TargetType.AnyEnemy)
            {
                // 目标必须是己方（队友）
                if (target.Side != __instance.Owner.Creature.Side) return;

                var cardName = GetCardName(__instance);
                if (!FriendlyFireConfig.IsSingleTargetAllowed(cardName)) return;

                __result = true;
                return;
            }

            // --- Self 卡重定向 → 允许任意活着的目标 ---
            // Self 卡 + 非null目标 = 重定向模式（目标通过 TargetId 同步）
            if (tt == TargetType.Self)
            {
                var cardName = GetCardName(__instance);
                if (!FriendlyFireConfig.IsSelfTargetAllowed(cardName)) return;

                __result = true;
                return;
            }

            // --- 诅咒牌转移 → 允许诅咒牌指向队友 ---
            // 诅咒牌原始 TargetType 为 None，如果出现在这里说明是转移模式
            if (tt == TargetType.None && __instance.Type == CardType.Curse)
            {
                // 目标必须是己方活着的玩家，且不是自己
                if (!target.IsPlayer) return;
                if (target.Player == __instance.Owner) return;
                if (!FriendlyFireConfig.CurseTransferEnabled) return;

                __result = true;
                return;
            }

            // --- AOE 攻击卡（AllEnemies / RandomEnemy）→ 允许"自身目标"信号通过 ---
            // PlayCardAction 构造函数将 TargetId 设为自身 CombatId 作为友伤信号。
            // ExecuteAction 中 GetCreatureAsync(TargetId) 解析出自身 Creature。
            // 原始 IsValidTarget 对 AllEnemies+非null目标 返回 false，
            // 这里放行，让 ExecuteAction 继续执行到 OnPlay → GetPossibleTargets。
            if (tt == TargetType.AllEnemies || tt == TargetType.RandomEnemy)
            {
                // 只允许自身作为信号（同一方、同一玩家）
                if (target.Side != __instance.Owner.Creature.Side) return;
                if (target.Player == null || __instance.Owner == null) return;
                if (target.Player != __instance.Owner) return;

                var cardName = GetCardName(__instance);
                if (!FriendlyFireConfig.IsAoeAllowed(cardName)) return;

                __result = true;
            }
        }

        /// <summary>
        /// 获取卡牌名称，优先用类名（和白名单匹配）
        /// </summary>
        internal static string GetCardName(CardModel card)
        {
            try
            {
                var name = card.GetType().Name;
                if (name != "CardModel" && !string.IsNullOrEmpty(name))
                    return name;

                var nameProp = AccessTools.Property(typeof(CardModel), "Name");
                if (nameProp != null)
                {
                    var val = nameProp.GetValue(card) as string;
                    if (!string.IsNullOrEmpty(val)) return val;
                }
            }
            catch { }

            return card.GetType().Name;
        }
    }

    // ================================================================
    // Patch 4 & 5: 追踪当前正在选择目标的卡牌 [UI 层 — 本地]
    //
    // 这些 Patch 只影响 UI 目标选择流程，不涉及网络同步。
    // SingleCreatureTargeting 是 async Task — 不要在 Postfix 中清除状态。
    // ================================================================
    [HarmonyPatch(typeof(NMouseCardPlay), "SingleCreatureTargeting")]
    public static class TrackTargetingCardPatch
    {
        static void Prefix(NMouseCardPlay __instance)
        {
            try
            {
                var cardProp = AccessTools.Property(typeof(NMouseCardPlay), "Card")
                    ?? AccessTools.Property(typeof(NMouseCardPlay).BaseType!, "Card");

                if (cardProp != null)
                {
                    var card = cardProp.GetValue(__instance) as CardModel;
                    if (card != null)
                    {
                        FriendlyFireState.CurrentTargetingCardName =
                            IsValidTargetPatch.GetCardName(card);
                    }
                }
            }
            catch
            {
                FriendlyFireState.CurrentTargetingCardName = null;
            }
        }
    }

    [HarmonyPatch(typeof(NMouseCardPlay), "TargetSelection")]
    public static class TrackTargetSelectionPatch
    {
        static void Prefix(NMouseCardPlay __instance)
        {
            try
            {
                var cardProp = AccessTools.Property(typeof(NMouseCardPlay), "Card")
                    ?? AccessTools.Property(typeof(NMouseCardPlay).BaseType!, "Card");

                if (cardProp != null)
                {
                    var card = cardProp.GetValue(__instance) as CardModel;
                    if (card != null)
                    {
                        FriendlyFireState.CurrentTargetingCardName =
                            IsValidTargetPatch.GetCardName(card);
                    }
                }
            }
            catch { }
        }
    }
}
