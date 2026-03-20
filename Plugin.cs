using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using MyStS2Mod.Utils;

namespace MyStS2Mod
{
    /// <summary>
    /// Friendly Fire Mod 入口
    ///
    /// 功能：
    /// - 按住 Alt（可配置）时，单体攻击牌可以选择队友作为目标
    /// - 按住 Alt 时，AOE 攻击牌会攻击除自己以外的所有人（敌人 + 队友）
    /// - 通过 friendly_fire_config.json 控制白名单和快捷键
    /// - 卡牌的特殊效果（如 Bash 的易伤）对队友同样生效
    /// </summary>
    [ModInitializer(nameof(Init))]
    public static class Plugin
    {
        private static Harmony? _harmony;

        public static void Init()
        {
            Console.WriteLine($"[{ModInfo.NAME}] v{ModInfo.VERSION} 正在加载...");

            // 1. 加载配置
            FriendlyFireConfig.Load();

            // 2. 初始化 Harmony，自动扫描并注册所有 [HarmonyPatch] 类
            _harmony = new Harmony(ModInfo.GUID);
            _harmony.PatchAll(Assembly.GetExecutingAssembly());

            var patchedMethods = _harmony.GetPatchedMethods();
            int count = 0;
            foreach (var m in patchedMethods) count++;
            Console.WriteLine($"[{ModInfo.NAME}] 已注册 {count} 个 Harmony Patch");

            Console.WriteLine($"[{ModInfo.NAME}] 加载完成！按住 {FriendlyFireConfig.ToggleKey} 键启用友伤模式");
        }
    }
}
