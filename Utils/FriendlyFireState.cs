using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace MyStS2Mod.Utils
{
    /// <summary>
    /// 友伤Mod全局运行时状态
    /// </summary>
    public static class FriendlyFireState
    {
        // ==================== 通用状态 ====================

        /// <summary>开关键是否正在被按住（本地输入，仅用于 UI 层）</summary>
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
                    "F1" => Key.F1, "F2" => Key.F2, "F3" => Key.F3, "F4" => Key.F4,
                    _ => Key.Alt
                };
                return Input.IsKeyPressed(key);
            }
        }

        /// <summary>当前正在进行目标选择的卡牌类名（UI 层使用）</summary>
        public static string? CurrentTargetingCardName { get; set; }

        // ==================== AOE 友伤状态 ====================

        /// <summary>
        /// AOE 友伤执行标志（执行层 — 网络同步安全）
        /// 从 PlayCardAction 的 TargetId 信号推断，不依赖本地输入。
        /// </summary>
        public static bool IsAoeFriendlyFireActive { get; set; }

        // ==================== Self 卡重定向状态 ====================

        /// <summary>
        /// UI 层：Self 卡的重定向目标（本地）
        /// 在 TargetSelection 中由玩家选择，在 EnqueueManualPlay 中读取并注入。
        /// </summary>
        public static Creature? SelfRedirectTarget { get; set; }

        /// <summary>
        /// 执行层：当前卡牌打出的重定向目标（网络同步安全）
        ///
        /// 从 PlayCardAction.TargetId 推断：
        ///   Self 卡 + TargetId 非空 → 重定向到 TargetId 对应的 Creature
        ///
        /// 生命周期：
        ///   - PlayCardAction.ExecuteAction Prefix 中设置
        ///   - OnPlayWrapper Prefix 中设置（备用）
        ///   - 下一次 ExecuteAction Prefix 时重置
        /// </summary>
        public static Creature? CurrentRedirectTarget { get; set; }

        /// <summary>
        /// 执行层：当前正在打出的卡牌
        /// 用于在 GainBlock/PowerCmd.Apply 中判断重定向条件。
        /// </summary>
        public static CardModel? CurrentPlayingCard { get; set; }

        // ==================== 诅咒转移状态 ====================

        /// <summary>
        /// 执行层：当前是否正在执行诅咒转移（网络同步安全）
        /// 从 PlayCardAction 推断：诅咒牌 + 有目标 = 转移模式
        /// </summary>
        public static bool IsCurseTransferActive { get; set; }
    }
}
