using Godot;
using MyStS2Mod.Utils;
using static MyStS2Mod.Utils.I18n;

namespace MyStS2Mod.UI
{
    /// <summary>
    /// 友伤模式屏幕提示
    ///
    /// 当按住开关键（默认 Alt）时，在屏幕上显示"友军伤害开启"文字提示。
    /// 使用 Godot CanvasLayer + Label 实现，始终显示在最上层。
    ///
    /// 生命周期：
    ///   - Plugin.Initialize() 中创建并添加到场景树
    ///   - _Process() 每帧检测开关键状态
    ///   - 按住时显示，松开时隐藏
    /// </summary>
    public partial class FriendlyFireIndicator : CanvasLayer
    {
        private Label _label = null!;
        private Panel _background = null!;
        private bool _wasVisible;

        public override void _Ready()
        {
            // CanvasLayer 设置为最高层，确保在所有 UI 之上
            Layer = 100;

            // 背景面板
            _background = new Panel();
            _background.Name = "FriendlyFireBg";

            // 设置面板样式：半透明红色背景 + 圆角
            var style = new StyleBoxFlat();
            style.BgColor = new Color(0.8f, 0.15f, 0.15f, 0.75f);
            style.CornerRadiusTopLeft = 6;
            style.CornerRadiusTopRight = 6;
            style.CornerRadiusBottomLeft = 6;
            style.CornerRadiusBottomRight = 6;
            style.ContentMarginLeft = 16;
            style.ContentMarginRight = 16;
            style.ContentMarginTop = 8;
            style.ContentMarginBottom = 8;
            _background.AddThemeStyleboxOverride("panel", style);

            // 文字标签
            _label = new Label();
            _label.Name = "FriendlyFireLabel";
            _label.Text = $"{I18n.T("friendly_fire_active")} [{FriendlyFireConfig.ToggleKey}]";
            _label.HorizontalAlignment = HorizontalAlignment.Center;
            _label.VerticalAlignment = VerticalAlignment.Center;

            // 文字样式：白色加粗
            _label.AddThemeColorOverride("font_color", Colors.White);
            _label.AddThemeColorOverride("font_shadow_color", new Color(0, 0, 0, 0.6f));
            _label.AddThemeFontSizeOverride("font_size", 18);

            // 阴影偏移
            _label.AddThemeConstantOverride("shadow_offset_x", 2);
            _label.AddThemeConstantOverride("shadow_offset_y", 2);

            _background.AddChild(_label);

            // 初始布局 — 屏幕顶部居中
            // 先用固定尺寸，_Process 中动态调整
            _background.Position = new Vector2(0, 12);
            _background.Size = new Vector2(260, 40);
            _label.Position = Vector2.Zero;
            _label.Size = _background.Size;

            AddChild(_background);

            // 默认隐藏
            _background.Visible = false;
            _wasVisible = false;
        }

        public override void _Process(double delta)
        {
            bool shouldShow = FriendlyFireState.IsToggleKeyHeld;

            if (shouldShow != _wasVisible)
            {
                _background.Visible = shouldShow;
                _wasVisible = shouldShow;

                // 显示时更新位置（屏幕顶部居中）
                if (shouldShow)
                {
                    UpdatePosition();
                }
            }
        }

        private void UpdatePosition()
        {
            var viewport = GetViewport();
            if (viewport == null) return;

            var screenSize = viewport.GetVisibleRect().Size;
            var bgWidth = _background.Size.X;
            _background.Position = new Vector2(
                (screenSize.X - bgWidth) / 2f,
                12
            );
        }
    }
}
