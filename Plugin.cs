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
///
/// 继承 Godot.Node 是 STS2 Mod 的标准做法：
/// - 游戏通过 ScriptManagerBridge 注册 Godot 脚本
/// - 必须加 partial 关键字（Godot source generator 要求）
///
/// 功能：
/// - 按住 Alt（可配置）时，单体攻击牌可以选择队友作为目标
/// - 按住 Alt 时，AOE 攻击牌会攻击除自己以外的所有人（敌人 + 队友）
/// - 通过 friendly_fire_config.json 控制白名单和快捷键
/// - 卡牌的特殊效果（如 Bash 的易伤）对队友同样生效
/// - 按住开关键时屏幕显示"友军伤害开启"提示
/// </summary>
[ModInitializer(nameof(Initialize))]
public partial class Plugin : Node
{
    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } =
        new(ModInfo.GUID, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
        Logger.Info($"{ModInfo.NAME} v{ModInfo.VERSION} initializing...");

        // 注册 Godot 脚本（使 .pck 中的场景/脚本可用）
        Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(Assembly.GetExecutingAssembly());

        // 加载配置
        FriendlyFireConfig.Load();

        // 初始化 Harmony，自动扫描并注册所有 [HarmonyPatch] 类
        Harmony harmony = new(ModInfo.GUID);
        harmony.PatchAll(Assembly.GetExecutingAssembly());

        // 输出已注册的 Patch 信息（调试用）
        var count = 0;
        foreach (var method in harmony.GetPatchedMethods())
        {
            count++;
            Logger.Info($"  Patched: {method.DeclaringType?.Name}.{method.Name}");
        }
        Logger.Info($"Registered {count} Harmony patches");

        // 添加屏幕提示 UI（延迟到场景树准备好后）
        try
        {
            var indicator = new FriendlyFireIndicator();
            indicator.Name = "FriendlyFireIndicator";
            // 使用 CallDeferred 确保在场景树稳定后添加
            var sceneTree = Engine.GetMainLoop() as SceneTree;
            if (sceneTree?.Root != null)
            {
                sceneTree.Root.CallDeferred("add_child", indicator);
                Logger.Info("Friendly fire indicator UI added");
            }
            else
            {
                Logger.Info("Warning: Could not add indicator UI — scene tree not ready");
            }
        }
        catch (Exception ex)
        {
            Logger.Info($"Warning: Failed to add indicator UI: {ex.Message}");
        }

        Logger.Info($"Loaded! Hold [{FriendlyFireConfig.ToggleKey}] to enable friendly fire");
    }
}
