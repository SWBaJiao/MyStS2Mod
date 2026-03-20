using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MyStS2Mod.Patches
{
    /// <summary>
    /// 针对特定危险卡牌的专属 Patch
    ///
    /// "危险卡牌" = 在 OnPlay 中直接访问 cardPlay.Target.Monster 属性的卡牌
    /// 当目标是 Player 时 Monster 为 null，会导致 NullReferenceException 崩溃
    ///
    /// 修复策略：用 Prefix 拦截 OnPlay，当目标是玩家时执行安全的替代逻辑
    /// </summary>

    // ========================================================================
    // GoForTheEyes（直捣黄龙）
    //
    // 原始逻辑：造成伤害 → 如果目标有攻击意图 → 施加虚弱
    // 崩溃点：cardPlay.Target.Monster.IntendsToAttack （Player 无 Monster）
    // 友伤逻辑：造成伤害 → 对队友直接施加虚弱（跳过意图判断）
    // ========================================================================
    [HarmonyPatch(typeof(GoForTheEyes), "OnPlay")]
    public static class GoForTheEyesPatch
    {
        /// <summary>
        /// Prefix: 当目标是玩家（队友）时，替换整个 OnPlay 逻辑
        /// 返回 false = 跳过原方法，避免 NRE
        /// 返回 true  = 正常执行原方法
        /// </summary>
        static bool Prefix(
            GoForTheEyes __instance,
            PlayerChoiceContext choiceContext,
            CardPlay cardPlay,
            ref Task __result)
        {
            // 目标不是玩家 → 走原逻辑
            if (cardPlay.Target == null || !cardPlay.Target.IsPlayer)
                return true;

            // 目标是队友 → 执行安全的替代逻辑
            __result = FriendlyFireOnPlay(__instance, choiceContext, cardPlay);
            return false; // 跳过原方法
        }

        private static async Task FriendlyFireOnPlay(
            GoForTheEyes instance,
            PlayerChoiceContext choiceContext,
            CardPlay cardPlay)
        {
            var target = cardPlay.Target!;

            // 1. 伤害照常执行
            var damageVar = AccessTools.Property(typeof(GoForTheEyes), "DynamicVars")?.GetValue(instance);
            var damageField = damageVar?.GetType().GetProperty("Damage")?.GetValue(damageVar);
            var baseValue = (decimal)(damageField?.GetType().GetProperty("BaseValue")?.GetValue(damageField) ?? 3m);

            await DamageCmd.Attack(baseValue)
                .FromCard(instance)
                .Targeting(target)
                .WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
                .Execute(choiceContext);

            // 2. 对队友直接施加虚弱（跳过意图判断）
            var weakVar = damageVar?.GetType().GetProperty("Weak")?.GetValue(damageVar);
            var weakValue = (decimal)(weakVar?.GetType().GetProperty("BaseValue")?.GetValue(weakVar) ?? 1m);

            var ownerProp = AccessTools.Property(typeof(GoForTheEyes).BaseType!, "Owner");
            var owner = ownerProp?.GetValue(instance);
            var creatureProp = owner?.GetType().GetProperty("Creature");
            var ownerCreature = creatureProp?.GetValue(owner) as Creature;

            if (ownerCreature != null)
            {
                await PowerCmd.Apply<WeakPower>(target, weakValue, ownerCreature, instance);
            }

            Console.WriteLine($"[{ModInfo.NAME}] GoForTheEyes 友伤: 对队友造成伤害并施加虚弱 (跳过意图判断)");
        }
    }
}
