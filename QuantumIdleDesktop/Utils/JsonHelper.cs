using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace QuantumIdleDesktop.Utils
{
    /// <summary>
    /// 通用 JSON 序列化文件读写工具类
    /// </summary>
    public static class JsonHelper
    {
        // 配置项：全局单例，避免重复创建开销
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            // 1. 格式化输出 (缩进)，让文件人类可读
            WriteIndented = true,
            // 2. 允许不严格的 JSON 字符转义 (关键：解决中文变成 \uXXXX 的问题)
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            // 3. 读取时忽略大小写 (增强兼容性)
            PropertyNameCaseInsensitive = true,
            // 4. 允许注释 (如果手动修改配置文件写了注释，不会报错)
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        /// <summary>
        /// 异步：将对象保存到文件
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="filePath">文件路径 (如 "settings.json")</param>
        /// <param name="data">要保存的数据对象</param>
        public static async Task SaveAsync<T>(string filePath, T data)
        {
            try
            {
                // 自动创建不存在的文件夹
                string dir = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                // 序列化
                string jsonString = JsonSerializer.Serialize(data, _options);

                // 写入文件 (覆盖模式)
                await File.WriteAllTextAsync(filePath, jsonString);
            }
            catch (Exception ex)
            {
                // 实际开发中建议记录日志
                throw new Exception($"保存文件 {filePath} 失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 异步：从文件加载对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="filePath">文件路径</param>
        /// <returns>读取成功的对象，如果文件不存在则返回 null</returns>
        public static async Task<T> LoadAsync<T>(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return default(T); // 文件不存在，返回 null
            }

            try
            {
                string jsonString = await File.ReadAllTextAsync(filePath);

                if (string.IsNullOrWhiteSpace(jsonString))
                {
                    return default(T);
                }

                return JsonSerializer.Deserialize<T>(jsonString, _options);
            }
            catch (Exception ex)
            {
                // 如果文件损坏 (JSON格式错误)，建议备份旧文件并返回 null，或者抛出异常
                // 这里为了稳健，返回 null 让上层决定是否使用默认配置
                return default(T);
            }
        }

        /// <summary>
        /// 同步版本：保存 (用于某些不能异步的场景，如程序退出时)
        /// </summary>
        public static void Save<T>(string filePath, T data)
        {
            string dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

            string jsonString = JsonSerializer.Serialize(data, _options);
            File.WriteAllText(filePath, jsonString);
        }

        /// <summary>
        /// 同步版本：加载
        /// </summary>
        public static T Load<T>(string filePath)
        {
            if (!File.Exists(filePath)) return default(T);
            string jsonString = File.ReadAllText(filePath);
            if (string.IsNullOrWhiteSpace(jsonString)) return default(T);
            return JsonSerializer.Deserialize<T>(jsonString, _options);
        }
    }
}