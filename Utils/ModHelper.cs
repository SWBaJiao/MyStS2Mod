using System;
using System.IO;
using System.Reflection;

namespace MyStS2Mod.Utils
{
    /// <summary>
    /// Mod 工具类 - 提供通用辅助方法
    /// </summary>
    public static class ModHelper
    {
        public static string ModDirectory =>
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".";

        public static byte[]? LoadAssetBytes(string relativePath)
        {
            try
            {
                var fullPath = Path.Combine(ModDirectory, "Assets", relativePath);
                if (!File.Exists(fullPath))
                {
                    Console.WriteLine($"[{ModInfo.NAME}] 资源文件不存在: {fullPath}");
                    return null;
                }
                return File.ReadAllBytes(fullPath);
            }
            catch (Exception e)
            {
                Console.WriteLine($"[{ModInfo.NAME}] 加载资源失败: {relativePath}, 错误: {e.Message}");
                return null;
            }
        }

        public static string LoadLocalization(string language = "zh-CN")
        {
            var path = Path.Combine(ModDirectory, "Assets", "localization", $"{language}.json");
            if (File.Exists(path))
                return File.ReadAllText(path);
            Console.WriteLine($"[{ModInfo.NAME}] 本地化文件不存在: {path}");
            return "{}";
        }
    }
}
