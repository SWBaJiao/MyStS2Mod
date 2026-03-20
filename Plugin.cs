using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
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

        var count = 0;
        foreach (var _ in harmony.GetPatchedMethods()) count++;
        Logger.Info($"Registered {count} Harmony patches");

        Logger.Info($"Loaded! Hold [{FriendlyFireConfig.ToggleKey}] to enable friendly fire");
    }
}
