using QuantumIdleDesktop.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuantumIdleDesktop.Utils
{
    public class SchemeFileHelper
    {
        // 1. 定义固定的文件夹和文件名
        private const string FolderName = "Data";
        private const string FileName = "Schemes.json";

        // 获取完整的物理路径：程序运行目录/Data/Schemes.json
        private static string FilePath => Path.Combine(AppContext.BaseDirectory, FolderName, FileName);

        // 2. 配置序列化选项 (.NET 10/Core 标准做法)
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true, // 格式化 JSON，方便人工查看
            PropertyNameCaseInsensitive = true, // 忽略大小写
            Converters = { new JsonStringEnumConverter() }, // 枚举转字符串
                                                            // 如果有中文字符，防止被转义为 \uXXXX
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        /// <summary>
        /// 保存整个列表到 Data/Schemes.json
        /// </summary>
        public static async Task SaveListAsync(List<SchemeModel> schemes)
        {
            if (schemes == null) schemes = new List<SchemeModel>();

            // 确保 Data 文件夹存在
            string folderPath = Path.Combine(AppContext.BaseDirectory, FolderName);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // 序列化并写入
            string json = JsonSerializer.Serialize(schemes, _jsonOptions);
            await File.WriteAllTextAsync(FilePath, json);
        }

        /// <summary>
        /// 读取所有方案 (如果文件不存在，返回空列表)
        /// </summary>
        public static async Task<List<SchemeModel>> LoadListAsync()
        {
            if (!File.Exists(FilePath))
            {
                return new List<SchemeModel>(); // 文件不存在时返回空集合，而不是报错
            }

            try
            {
                string json = await File.ReadAllTextAsync(FilePath);
                var result = JsonSerializer.Deserialize<List<SchemeModel>>(json, _jsonOptions);
                return result ?? new List<SchemeModel>();
            }
            catch (Exception ex)
            {
                // 这里可以记录日志
                Console.WriteLine($"读取方案文件失败: {ex.Message}");
                // 读取失败返回空，防止程序崩溃，或者根据需求 throw ex;
                return new List<SchemeModel>();
            }
        }
    }
}
