using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace MyStS2Mod.Patches
{
    /// <summary>
    /// Harmony Patch 示例
    ///
    /// 常见 Patch 目标 (根据反编译结果):
    ///   - CardModel: 卡牌行为 (OnPlayWrapper, CanPlay, UpgradeInternal 等)
    ///   - MonsterModel: 怪物行为
    ///   - CombatManager: 战斗流程 (StartCombatInternal, EndCombatInternal 等)
    ///   - Hook: 游戏钩子系统 (ModifyXValue, ShouldDraw 等)
    /// </summary>
    public static class ExamplePatch
    {
        /*
        // 示例: 修改卡牌星星消耗
        [HarmonyPatch(typeof(CardModel), nameof(CardModel.GetStarCostWithModifiers))]
        public static class ModifyStarCost
        {
            static void Postfix(CardModel __instance, ref int __result)
            {
                // 所有卡牌星星消耗减 1 (最低为 0)
                if (__result > 0)
                {
                    __result -= 1;
                }
            }
        }
        */

        /*
        // 示例: 在卡牌打出时添加额外效果
        [HarmonyPatch(typeof(CardModel), nameof(CardModel.OnPlayWrapper))]
        public static class OnCardPlayed
        {
            static void Prefix(CardModel __instance)
            {
                System.Console.WriteLine($"[{ModInfo.NAME}] 打出了卡牌: {__instance.GetType().Name}");
            }
        }
        */
    }
}
