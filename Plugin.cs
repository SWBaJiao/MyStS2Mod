using System;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using MyStS2Mod.UI;
using MyStS2Mod.Utils;

namespace MyStS2Mod;

/// <summary>
/// Friendly Fire Mod 入口
/// </summary>
[ModInitializer(nameof(Initialize))]
public partial class Plugin : Node
{
    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } =
        new(ModInfo.GUID, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
        Logger.Info($"{ModInfo.NAME} v{ModInfo.VERSION} initializing...");

        // 注册 Godot 脚本
        Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(Assembly.GetExecutingAssembly());

        // 初始化多语言（必须在配置加载前，因为日志需要翻译）
        I18n.Init();

        // 加载配置
        FriendlyFireConfig.Load();

        // 初始化 Harmony
        Harmony harmony = new(ModInfo.GUID);
        harmony.PatchAll(Assembly.GetExecutingAssembly());

        var count = 0;
        foreach (var method in harmony.GetPatchedMethods())
        {
            count++;
            Logger.Info($"  Patched: {method.DeclaringType?.Name}.{method.Name}");
        }
        Logger.Info(I18n.T("patches_registered", count));

        // 添加屏幕提示 UI
        try
        {
            var indicator = new FriendlyFireIndicator();
            indicator.Name = "FriendlyFireIndicator";
            var sceneTree = Engine.GetMainLoop() as SceneTree;
            if (sceneTree?.Root != null)
            {
                sceneTree.Root.CallDeferred("add_child", indicator);
            }
        }
        catch (Exception ex)
        {
            Logger.Info($"Warning: Failed to add indicator UI: {ex.Message}");
        }

        Logger.Info(I18n.T("mod_loaded", FriendlyFireConfig.ToggleKey));
    }
}
