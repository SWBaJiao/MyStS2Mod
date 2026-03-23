using System;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MyStS2Mod.Utils;

namespace MyStS2Mod.Patches
{
    // ================================================================
    // Self 卡重定向系统
    //
    // 原始行为：Self 卡（防御、能力等）只能作用于自己
    // 修改后：按住 Alt + Self 卡 → 进入目标选择 → 选择队友/敌人
    //
    // 信号传递链：
    //   1. TargetSelection Prefix → 强制 Self 卡进入 AnyPlayer 选择
    //   2. 玩家选中目标 → _target 被设置
    //   3. TryPlayCard Prefix → 捕获 _target 到 SelfRedirectTarget
    //   4. EnqueueManualPlay Prefix → 注入 redirect target
    //   5. PlayCardAction(card, target) → TargetId = target.CombatId
    //   6. 网络同步 → 所有客户端收到 TargetId
    //   7. ExecuteAction → target = redirect creature
    //   8. OnPlayWrapper → CardPlay.Target = redirect creature
    //   9. CreatureCmd.GainBlock / PowerCmd.Apply → 检测重定向并替换目标
    // ================================================================

    // ================================================================
    // Patch S1: TargetSelection [UI 层]
    //
    // 原始逻辑：Self 卡走 MultiCreatureTargeting（直接拖到播放区，无选择）
    // 修改后：Alt + Self 白名单卡 → SingleCreatureTargeting(AnyPlayer)
    //         进入选目标模式，AllowedToTargetNode 的已有 Patch 扩展选择范围
    // ================================================================
    [HarmonyPatch(typeof(NMouseCardPlay), "TargetSelection")]
    public static class SelfTargetSelectionPatch
    {
        private static MethodInfo? _singleCreatureTargeting;

        static bool Prefix(NMouseCardPlay __instance, TargetMode targetMode, ref Task __result)
        {
            if (!FriendlyFireState.IsToggleKeyHeld) return true;

            var card = GetCard(__instance);
            if (card == null || card.TargetType != TargetType.Self) return true;

            var cardName = card.GetType().Name;
            if (!FriendlyFireConfig.IsSelfTargetAllowed(cardName)) return true;

            // 缓存 SingleCreatureTargeting 方法
            _singleCreatureTargeting ??= AccessTools.Method(
                typeof(NMouseCardPlay), "SingleCreatureTargeting");
            if (_singleCreatureTargeting == null) return true;

            // 调用前先执行原始 TargetSelection 的前置逻辑
            try
            {
                var cardNode = AccessTools.Property(typeof(NCardPlay), "CardNode")
                    ?.GetValue(__instance);
                // TryShowEvokingOrbs + AnimFlash
                AccessTools.Method(typeof(NCardPlay), "TryShowEvokingOrbs")
                    ?.Invoke(__instance, null);
            }
            catch { }

            // 替换为 SingleCreatureTargeting(targetMode, AnyPlayer)
            // AnyPlayer 允许选择所有玩家方生物
            // AllowedToTargetNode 的已有 Patch 会将范围扩展到敌方
            __result = (Task)_singleCreatureTargeting.Invoke(
                __instance, new object[] { targetMode, TargetType.AnyPlayer })!;

            Plugin.Logger.Info($"Self 卡 {cardName} 进入目标选择模式");
            return false; // 跳过原始 TargetSelection
        }

        private static CardModel? GetCard(NMouseCardPlay instance)
        {
            try
            {
                var prop = AccessTools.Property(typeof(NCardPlay), "Card");
                return prop?.GetValue(instance) as CardModel;
            }
            catch { return null; }
        }
    }

    // ================================================================
    // Patch S2: TryPlayCard [UI 层]
    //
    // 捕获目标选择结果。Self 卡的 _target 正常被 TryPlayCard 忽略
    // （因为 Self 不是 AnyEnemy/AnyAlly，走 TryManualPlay(null)）。
    // 我们在 Prefix 中保存 _target，稍后由 EnqueueManualPlay 注入。
    // ================================================================
    [HarmonyPatch(typeof(NCardPlay), "TryPlayCard")]
    public static class SelfTryPlayCardPatch
    {
        static void Prefix(NCardPlay __instance, Creature? target)
        {
            FriendlyFireState.SelfRedirectTarget = null;

            if (target == null) return;

            var card = GetCard(__instance);
            if (card == null || card.TargetType != TargetType.Self) return;
            if (!FriendlyFireConfig.IsSelfTargetAllowed(card.GetType().Name)) return;

            // 保存重定向目标，EnqueueManualPlay 会消费它
            FriendlyFireState.SelfRedirectTarget = target;
        }

        private static CardModel? GetCard(NCardPlay instance)
        {
            try
            {
                var prop = AccessTools.Property(typeof(NCardPlay), "Card");
                return prop?.GetValue(instance) as CardModel;
            }
            catch { return null; }
        }
    }

    // ================================================================
    // Patch S3: EnqueueManualPlay [信号编码层]
    //
    // 原始：Self 卡调用 EnqueueManualPlay(null) → TargetId = null
    // 修改：注入 SelfRedirectTarget → TargetId = target.CombatId
    // 这个 TargetId 通过 NetPlayCardAction 同步到所有客户端。
    // ================================================================
    [HarmonyPatch(typeof(CardModel), "EnqueueManualPlay")]
    public static class SelfEnqueueManualPlayPatch
    {
        static void Prefix(CardModel __instance, ref Creature? target)
        {
            if (__instance.TargetType != TargetType.Self) return;
            if (target != null) return; // 已经有目标，不覆盖

            if (FriendlyFireState.SelfRedirectTarget != null)
            {
                target = FriendlyFireState.SelfRedirectTarget;
                FriendlyFireState.SelfRedirectTarget = null; // 消费掉

                Plugin.Logger.Info(
                    $"Self 卡 {__instance.GetType().Name} 重定向目标已编码: " +
                    $"CombatId={target.CombatId}");
            }
        }
    }

    // ================================================================
    // Patch S4: OnPlayWrapper [执行层 — 所有客户端]
    //
    // 设置执行层重定向状态。
    // OnPlayWrapper 接收的 target 来自 PlayCardAction.ExecuteAction，
    // 该 target 通过 TargetId 网络同步，所有客户端一致。
    //
    // 当 Self 卡 + target != Owner.Creature → 标记重定向模式
    // 后续的 GainBlock / PowerCmd 会检查此标记
    // ================================================================
    [HarmonyPatch(typeof(CardModel), nameof(CardModel.OnPlayWrapper))]
    public static class SelfOnPlayWrapperPatch
    {
        static void Prefix(CardModel __instance, Creature? target)
        {
            FriendlyFireState.CurrentRedirectTarget = null;
            FriendlyFireState.CurrentPlayingCard = null;

            if (target == null) return;
            if (__instance.TargetType != TargetType.Self) return;

            // target != Owner.Creature → 重定向模式
            if (target != __instance.Owner?.Creature)
            {
                FriendlyFireState.CurrentRedirectTarget = target;
                FriendlyFireState.CurrentPlayingCard = __instance;
            }
        }
    }

    // ================================================================
    // Patch S5: CreatureCmd.GainBlock [执行层 — 格挡重定向]
    //
    // 防御牌 OnPlay: CreatureCmd.GainBlock(Owner.Creature, block, cardPlay)
    // 当 cardPlay.Target 指向非自身生物时 → 重定向 creature 参数
    //
    // 网络安全：cardPlay.Target 来自 PlayCardAction.TargetId（已同步）
    // ================================================================
    [HarmonyPatch(typeof(CreatureCmd), nameof(CreatureCmd.GainBlock),
        new Type[] { typeof(Creature), typeof(BlockVar), typeof(CardPlay), typeof(bool) })]
    public static class GainBlockRedirectPatch1
    {
        static void Prefix(ref Creature creature, CardPlay? cardPlay)
        {
            SelfRedirectHelper.RedirectIfNeeded(ref creature, cardPlay);
        }
    }

    [HarmonyPatch(typeof(CreatureCmd), nameof(CreatureCmd.GainBlock),
        new Type[] { typeof(Creature), typeof(decimal), typeof(ValueProp),
                     typeof(CardPlay), typeof(bool) })]
    public static class GainBlockRedirectPatch2
    {
        static void Prefix(ref Creature creature, CardPlay? cardPlay)
        {
            SelfRedirectHelper.RedirectIfNeeded(ref creature, cardPlay);
        }
    }

    // ================================================================
    // Patch S6: PowerCmd.Apply [执行层 — 能力重定向]
    //
    // 能力牌 OnPlay: PowerCmd.Apply<T>(Owner.Creature, amount, applier, this)
    // PowerCmd.Apply 没有 CardPlay 参数，用全局状态判断。
    //
    // 网络安全：CurrentRedirectTarget 在 OnPlayWrapper Prefix 中设置，
    // 所有端的 target 来自 TargetId → CurrentRedirectTarget 一致。
    // ================================================================
    [HarmonyPatch(typeof(PowerCmd), nameof(PowerCmd.Apply),
        new Type[] { typeof(PowerModel), typeof(Creature), typeof(decimal),
                     typeof(Creature), typeof(CardModel), typeof(bool) })]
    public static class PowerApplyRedirectPatch
    {
        static void Prefix(ref Creature target, Creature? applier, CardModel? cardSource)
        {
            if (FriendlyFireState.CurrentRedirectTarget == null) return;
            if (FriendlyFireState.CurrentPlayingCard != cardSource) return;
            if (cardSource?.TargetType != TargetType.Self) return;

            // 只重定向"给自己"的能力（target == Owner.Creature）
            if (target != cardSource.Owner?.Creature) return;

            target = FriendlyFireState.CurrentRedirectTarget;
        }
    }

    // ================================================================
    // 工具方法：GainBlock 重定向检查
    // ================================================================
    internal static class SelfRedirectHelper
    {
        public static void RedirectIfNeeded(ref Creature creature, CardPlay? cardPlay)
        {
            if (cardPlay?.Target == null) return;
            if (cardPlay.Card?.TargetType != TargetType.Self) return;

            var owner = cardPlay.Card.Owner?.Creature;
            if (creature != owner) return;
            if (cardPlay.Target == owner) return;

            creature = cardPlay.Target;
        }
    }
}
