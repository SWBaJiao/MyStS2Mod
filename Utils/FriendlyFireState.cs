using Godot;

namespace MyStS2Mod.Utils
{
    /// <summary>
    /// 友伤Mod全局运行时状态
    /// 追踪开关键是否被按住、当前正在打出的卡牌等
    /// </summary>
    public static class FriendlyFireState
    {
        /// <summary>
        /// 开关键是否正在被按住（本地输入，仅用于 UI 层）
        /// </summary>
        public static bool IsToggleKeyHeld
        {
            get
            {
                var key = FriendlyFireConfig.ToggleKey switch
                {
                    "Alt" => Key.Alt,
                    "Shift" => Key.Shift,
                    "Ctrl" => Key.Ctrl,
                    "Tab" => Key.Tab,
                    "Space" => Key.Space,
                    "F1" => Key.F1,
                    "F2" => Key.F2,
                    "F3" => Key.F3,
                    "F4" => Key.F4,
                    _ => Key.Alt
                };
                return Input.IsKeyPressed(key);
            }
        }

        /// <summary>
        /// 当前正在进行目标选择的卡牌类名（UI 层使用）
        /// </summary>
        public static string? CurrentTargetingCardName { get; set; }

        /// <summary>
        /// AOE 友伤执行标志（执行层 — 网络同步安全）
        ///
        /// 当此标志为 true 时，GetPossibleTargets 将扩展目标列表。
        /// 此标志不依赖本地输入，而是从 PlayCardAction 的 TargetId 信号推断。
        ///
        /// 生命周期：
        ///   - PlayCardAction.ExecuteAction Prefix 中设置（所有客户端都执行）
        ///   - 下一次 PlayCardAction.ExecuteAction Prefix 时重置
        ///   - Godot 是单线程的，不存在并发问题
        /// </summary>
        public static bool IsAoeFriendlyFireActive { get; set; }
    }
}
