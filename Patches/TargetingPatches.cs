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
    // Patch 1: NTargetManager.AllowedToTargetNode (PUBLIC 方法 — 主要 Patch)
    //
    // 这是目标选择的入口方法，由 OnNodeHovered/OnNodeUnhovered 调用。
    // AllowedToTargetCreature 是 private 方法，可能被 JIT 内联，
    // 所以我们必须 patch 这个 public 的调用者来确保可靠拦截。
    //
    // 原始逻辑：AnyEnemy 时只允许选中敌方 Creature
    // 修改后：按住开关键 + 卡牌在白名单中 → 也允许选中队友
    // ================================================================
    [HarmonyPatch(typeof(NTargetManager), nameof(NTargetManager.AllowedToTargetNode))]
    public static class AllowedToTargetNodePatch
    {
        static void Postfix(NTargetManager __instance, Node node, ref bool __result)
        {
            // 已经允许的不用管
            if (__result) return;

            // 必须按住开关键
            if (!FriendlyFireState.IsToggleKeyHeld) return;

            // 提取 Creature
            Creature? creature = null;
            if (node is NCreature nCreature)
                creature = nCreature.Entity;
            // NMultiplayerPlayerState 的情况也处理
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

            // 通过反射获取 _validTargetsType
            var validTargetsType = GetValidTargetsType(__instance);
            if (validTargetsType != TargetType.AnyEnemy) return;

            // 目标必须是活着的玩家方 Creature（队友）
            if (!creature.IsPlayer || creature.IsDead) return;

            // 检查白名单
            var cardName = FriendlyFireState.CurrentTargetingCardName;
            if (cardName != null && !FriendlyFireConfig.IsSingleTargetAllowed(cardName))
                return;

            // 允许选中队友！
            __result = true;
        }

        /// <summary>
        /// 缓存反射字段，避免每次调用都查找
        /// </summary>
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
    // Patch 2: NTargetManager.AllowedToTargetCreature (PRIVATE 方法 — 备用)
    //
    // 作为 AllowedToTargetNode 的补充：如果 JIT 没有内联此方法，
    // 这个 Patch 也能拦截。双保险。
    // ================================================================
    [HarmonyPatch(typeof(NTargetManager), "AllowedToTargetCreature")]
    public static class AllowedToTargetCreaturePatch
    {
        static void Postfix(NTargetManager __instance, Creature creature, ref bool __result)
        {
            if (__result) return;
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
    // Patch 3: CardModel.IsValidTarget
    //
    // 当玩家点击确认目标时，游戏再次检查 IsValidTarget。
    // 如果不 patch 这里，即使选中了队友，打出卡牌时也会被拒绝。
    //
    // 原始逻辑：AnyEnemy → target.Side != Owner.Creature.Side
    // 修改后：按住开关键 + 白名单 → 对队友也返回 true
    // ================================================================
    [HarmonyPatch(typeof(CardModel), nameof(CardModel.IsValidTarget))]
    public static class IsValidTargetPatch
    {
        static void Postfix(CardModel __instance, Creature target, ref bool __result)
        {
            if (__result) return;
            if (target == null || !target.IsAlive) return;
            if (!FriendlyFireState.IsToggleKeyHeld) return;

            if (__instance.TargetType == TargetType.AnyEnemy)
            {
                // 目标必须是己方（队友）
                if (target.Side != __instance.Owner.Creature.Side) return;

                // 检查白名单（用卡牌定义名或类名）
                var cardName = GetCardName(__instance);
                if (!FriendlyFireConfig.IsSingleTargetAllowed(cardName)) return;

                __result = true;
            }
        }

        /// <summary>
        /// 获取卡牌名称，优先使用 CardDefinition.Name，兜底用类名
        /// </summary>
        internal static string GetCardName(CardModel card)
        {
            try
            {
                // 优先用类名（和白名单匹配）
                var name = card.GetType().Name;
                if (name != "CardModel" && !string.IsNullOrEmpty(name))
                    return name;

                // 兜底：用 Name 属性
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
    // Patch 4: 追踪当前正在选择目标的卡牌
    //
    // 注意：SingleCreatureTargeting 是 async Task 方法！
    // Harmony 的 Postfix 在 Task 创建时就执行（不等待完成），
    // 所以绝对不能在 Postfix 中清除状态。
    //
    // 策略：只在 Prefix 中设置卡牌名，不清除。
    // 下次选卡时会自然覆盖，选完目标后由其他地方（TargetSelection）重置。
    // ================================================================
    [HarmonyPatch(typeof(NMouseCardPlay), "SingleCreatureTargeting")]
    public static class TrackTargetingCardPatch
    {
        static void Prefix(NMouseCardPlay __instance)
        {
            try
            {
                // NMouseCardPlay 继承自 NCardPlay，Card 属性在基类上
                var cardProp = AccessTools.Property(typeof(NMouseCardPlay), "Card")
                    ?? AccessTools.Property(typeof(NMouseCardPlay).BaseType!, "Card");

                if (cardProp != null)
                {
                    var card = cardProp.GetValue(__instance) as CardModel;
                    if (card != null)
                    {
                        FriendlyFireState.CurrentTargetingCardName =
                            IsValidTargetPatch.GetCardName(card);
                        Plugin.Logger.Info(
                            $"Targeting started for card: {FriendlyFireState.CurrentTargetingCardName}");
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.Info($"TrackTargetingCard error: {ex.Message}");
                FriendlyFireState.CurrentTargetingCardName = null;
            }
        }

        // 不要添加 Postfix！
        // SingleCreatureTargeting 是 async Task，Postfix 会在 Task 创建后立即执行
        // 那时候玩家还没选择目标呢，清除状态会导致白名单检查失败
    }

    // ================================================================
    // Patch 5: TargetSelection 的 Prefix — 也追踪卡牌
    //
    // TargetSelection 是 SingleCreatureTargeting 的调用者，
    // 作为 TrackTargetingCardPatch 的备份
    // ================================================================
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
            catch
            {
                // 静默失败，TrackTargetingCardPatch 会兜底
            }
        }
    }
}
