using System.Collections.Generic;
using Godot;

namespace MyStS2Mod.Utils
{
    /// <summary>
    /// 轻量多语言支持
    ///
    /// 通过 Godot TranslationServer 自动检测游戏语言，
    /// 支持中文(zh)、英文(en)、日语(ja)、韩语(ko)，默认回退到英文。
    ///
    /// 用法：I18n.T("friendly_fire_active")
    /// </summary>
    public static class I18n
    {
        private static readonly Dictionary<string, Dictionary<string, string>> _translations = new()
        {
            ["zh"] = new()
            {
                ["friendly_fire_active"]    = "友军伤害开启",
                ["curse_transfer"]          = "诅咒转移",
                ["curse_transfer_done"]     = "诅咒转移完成",
                ["curse_transfer_fail"]     = "诅咒转移失败",
                ["self_redirect"]           = "目标重定向",
                ["mod_loaded"]              = "Mod 加载完成！按住 [{0}] 启用友伤",
                ["config_loaded"]           = "配置加载成功",
                ["config_load_fail"]        = "配置加载失败，使用默认配置",
                ["patches_registered"]      = "已注册 {0} 个 Harmony 补丁",
            },
            ["en"] = new()
            {
                ["friendly_fire_active"]    = "Friendly Fire ON",
                ["curse_transfer"]          = "Curse Transfer",
                ["curse_transfer_done"]     = "Curse transferred",
                ["curse_transfer_fail"]     = "Curse transfer failed",
                ["self_redirect"]           = "Target Redirect",
                ["mod_loaded"]              = "Mod loaded! Hold [{0}] to enable friendly fire",
                ["config_loaded"]           = "Config loaded",
                ["config_load_fail"]        = "Config load failed, using defaults",
                ["patches_registered"]      = "Registered {0} Harmony patches",
            },
            ["ja"] = new()
            {
                ["friendly_fire_active"]    = "フレンドリーファイア ON",
                ["curse_transfer"]          = "呪い転送",
                ["curse_transfer_done"]     = "呪い転送完了",
                ["curse_transfer_fail"]     = "呪い転送失敗",
                ["self_redirect"]           = "ターゲットリダイレクト",
                ["mod_loaded"]              = "Mod ロード完了！[{0}] を押してフレンドリーファイアを有効化",
                ["config_loaded"]           = "設定ロード完了",
                ["config_load_fail"]        = "設定ロード失敗、デフォルト使用",
                ["patches_registered"]      = "{0} 個の Harmony パッチを登録",
            },
            ["ko"] = new()
            {
                ["friendly_fire_active"]    = "아군 피해 활성화",
                ["curse_transfer"]          = "저주 전달",
                ["curse_transfer_done"]     = "저주 전달 완료",
                ["curse_transfer_fail"]     = "저주 전달 실패",
                ["self_redirect"]           = "대상 리다이렉트",
                ["mod_loaded"]              = "Mod 로드 완료! [{0}]을 눌러 아군 피해 활성화",
                ["config_loaded"]           = "설정 로드 완료",
                ["config_load_fail"]        = "설정 로드 실패, 기본값 사용",
                ["patches_registered"]      = "Harmony 패치 {0}개 등록",
            },
        };

        private static string _currentLang = "en";

        /// <summary>
        /// 初始化：从 Godot TranslationServer 检测游戏语言
        /// </summary>
        public static void Init()
        {
            try
            {
                // Godot 的 locale 格式: "zh_CN", "en", "ja", "ko" 等
                var locale = TranslationServer.GetLocale();
                // 取前两个字符作为语言代码
                var lang = locale?.Length >= 2 ? locale[..2].ToLowerInvariant() : "en";

                if (_translations.ContainsKey(lang))
                    _currentLang = lang;
                else
                    _currentLang = "en"; // 默认回退英文

                System.Console.WriteLine($"[{ModInfo.NAME}] I18n: locale={locale}, lang={_currentLang}");
            }
            catch
            {
                _currentLang = "en";
            }
        }

        /// <summary>
        /// 获取翻译文本。支持 string.Format 参数。
        /// </summary>
        public static string T(string key, params object[] args)
        {
            // 当前语言 → 英文回退 → key 本身
            string text;
            if (_translations.TryGetValue(_currentLang, out var dict) && dict.TryGetValue(key, out text!))
            { }
            else if (_translations["en"].TryGetValue(key, out text!))
            { }
            else
            {
                text = key;
            }

            return args.Length > 0 ? string.Format(text, args) : text;
        }

        /// <summary>当前语言代码</summary>
        public static string CurrentLanguage => _currentLang;
    }
}
