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
        /// 开关键是否正在被按住
        /// 所有 Patch 都检查此属性来决定是否启用友伤
        /// </summary>
        public static bool IsToggleKeyHeld
        {
            get
            {
                // Godot 的 Input.IsKeyPressed 检测物理按键状态
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
        /// 当前正在进行目标选择的卡牌类名（由 Patch 设置）
        /// 用于判断白名单
        /// </summary>
        public static string? CurrentTargetingCardName { get; set; }
    }
}
