using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace MyStS2Mod.Utils
{
    /// <summary>
    /// 友伤Mod配置管理器
    /// 从 friendly_fire_config.json 加载配置
    /// </summary>
    public static class FriendlyFireConfig
    {
        /// <summary>按住此键时允许单体攻击牌选择队友（Godot Key 枚举名）</summary>
        public static string ToggleKey { get; private set; } = "Alt";

        /// <summary>
        /// 允许友伤的单体攻击牌白名单（类名）
        /// 空列表 = 所有单体攻击牌都允许
        /// </summary>
        public static HashSet<string> SingleTargetWhitelist { get; private set; } = new();

        /// <summary>是否启用 AOE 扩展（攻击除自己外所有人）</summary>
        public static bool AoeEnabled { get; private set; } = true;

        /// <summary>
        /// 允许 AOE 友伤的卡牌白名单（类名）
        /// 空列表 = 所有 AOE 攻击牌都允许
        /// </summary>
        public static HashSet<string> AoeWhitelist { get; private set; } = new();

        /// <summary>
        /// 从 Mod DLL 同目录下加载 friendly_fire_config.json
        /// </summary>
        public static void Load()
        {
            try
            {
                var dllDir = Path.GetDirectoryName(typeof(FriendlyFireConfig).Assembly.Location);
                var configPath = Path.Combine(dllDir ?? ".", "friendly_fire_config.json");

                if (!File.Exists(configPath))
                {
                    Console.WriteLine($"[{ModInfo.NAME}] 配置文件不存在: {configPath}，使用默认配置");
                    return;
                }

                var json = File.ReadAllText(configPath);
                using var doc = JsonDocument.Parse(json, new JsonDocumentOptions
                {
                    CommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                });

                var root = doc.RootElement;

                if (root.TryGetProperty("toggle_key", out var toggleKey))
                    ToggleKey = toggleKey.GetString() ?? "Alt";

                if (root.TryGetProperty("aoe_enabled", out var aoeEnabled))
                    AoeEnabled = aoeEnabled.GetBoolean();

                SingleTargetWhitelist = ParseStringArray(root, "single_target_whitelist");
                AoeWhitelist = ParseStringArray(root, "aoe_whitelist");

                Console.WriteLine($"[{ModInfo.NAME}] 配置加载成功:");
                Console.WriteLine($"  开关键: {ToggleKey}");
                Console.WriteLine($"  单体白名单: {(SingleTargetWhitelist.Count == 0 ? "全部允许" : string.Join(", ", SingleTargetWhitelist))}");
                Console.WriteLine($"  AOE启用: {AoeEnabled}");
                Console.WriteLine($"  AOE白名单: {(AoeWhitelist.Count == 0 ? "全部允许" : string.Join(", ", AoeWhitelist))}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{ModInfo.NAME}] 配置加载失败: {ex.Message}，使用默认配置");
            }
        }

        /// <summary>
        /// 判断某张单体攻击牌是否允许友伤
        /// </summary>
        public static bool IsSingleTargetAllowed(string cardClassName)
        {
            // 白名单为空 = 全部允许
            if (SingleTargetWhitelist.Count == 0) return true;
            return SingleTargetWhitelist.Contains(cardClassName);
        }

        /// <summary>
        /// 判断某张 AOE 卡牌是否允许友伤扩展
        /// </summary>
        public static bool IsAoeAllowed(string cardClassName)
        {
            if (!AoeEnabled) return false;
            if (AoeWhitelist.Count == 0) return true;
            return AoeWhitelist.Contains(cardClassName);
        }

        private static HashSet<string> ParseStringArray(JsonElement root, string propertyName)
        {
            var result = new HashSet<string>(StringComparer.Ordinal);
            if (root.TryGetProperty(propertyName, out var arr) && arr.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in arr.EnumerateArray())
                {
                    var val = item.GetString();
                    if (!string.IsNullOrWhiteSpace(val))
                        result.Add(val);
                }
            }
            return result;
        }
    }
}
