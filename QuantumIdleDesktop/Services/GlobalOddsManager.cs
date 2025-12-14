using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using QuantumIdleDesktop.Models;

namespace QuantumIdleDesktop.Services
{
    /// <summary>
    /// 全局赔率管理器 (单例)
    /// 负责管理 odds.json 配置文件，提供查询和保存功能
    /// </summary>
    public class GlobalOddsManager
    {
        // ==========================================
        // 1. 常量与静态路径定义
        // ==========================================
        private const string DATA_DIRECTORY = "Data";

        // 使用 static readonly 确保路径只初始化一次且不可变
        private static readonly string CONFIG_FILE_PATH = Path.Combine(DATA_DIRECTORY, "Odds.json");

        // ==========================================
        // 2. 单例模式
        // ==========================================
        public static GlobalOddsManager Instance { get; } = new GlobalOddsManager();

        // 内存缓存: Key=游戏类型, Value=(Key=玩法, Value=赔率)
        private Dictionary<GameType, Dictionary<GamePlayMode, decimal>> _oddsCache;

        /// <summary>
        /// 私有构造函数
        /// </summary>
        private GlobalOddsManager()
        {
            _oddsCache = new Dictionary<GameType, Dictionary<GamePlayMode, decimal>>();

            // 确保 Data 文件夹存在
            if (!Directory.Exists(DATA_DIRECTORY))
            {
                Directory.CreateDirectory(DATA_DIRECTORY);
            }

            // 初始化加载
            LoadConfig();
        }

        // ==========================================
        // 3. 核心功能：加载与默认生成
        // ==========================================

        /// <summary>
        /// 加载配置文件 (如果不存在则生成默认)
        /// </summary>
        public void LoadConfig()
        {
            // 情况 A: 文件不存在 -> 生成默认配置并保存
            if (!File.Exists(CONFIG_FILE_PATH))
            {
                Console.WriteLine($"配置文件不存在，正在生成默认配置: {CONFIG_FILE_PATH}");
                GenerateDefaultData();
                SaveConfig(); // 保存后，_oddsCache 已经有数据了
                return;
            }

            // 情况 B: 文件存在 -> 读取并解析
            try
            {
                string json = File.ReadAllText(CONFIG_FILE_PATH);

                // 1. 反序列化为临时字典 (String Key)
                var tempDict = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, decimal>>>(json);

                if (tempDict == null)
                {
                    GenerateDefaultData(); // JSON为空，重新生成
                    return;
                }

                // 2. 转换为强类型缓存 (Enum Key)
                var newCache = new Dictionary<GameType, Dictionary<GamePlayMode, decimal>>();

                foreach (var gameKvp in tempDict)
                {
                    if (Enum.TryParse(gameKvp.Key, out GameType gType))
                    {
                        var modeDict = new Dictionary<GamePlayMode, decimal>();
                        foreach (var modeKvp in gameKvp.Value)
                        {
                            if (Enum.TryParse(modeKvp.Key, out GamePlayMode pMode))
                            {
                                modeDict[pMode] = modeKvp.Value;
                            }
                        }
                        newCache[gType] = modeDict;
                    }
                }

                _oddsCache = newCache;
                Console.WriteLine("赔率配置加载成功！");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载赔率配置失败: {ex.Message}，将使用默认内存值。");
                GenerateDefaultData();
            }
        }

        /// <summary>
        /// 生成默认赔率数据 (内存中)
        /// </summary>
        private void GenerateDefaultData()
        {
            _oddsCache = new Dictionary<GameType, Dictionary<GamePlayMode, decimal>>();

            // 1. 扫雷 / 骰子
            var minesweeper = new Dictionary<GamePlayMode, decimal>
            {
                { GamePlayMode.BigSmallOddEven, 1.98m },
                { GamePlayMode.Digital, 9.0m }  // 猜数字通常赔率较高
            };
            _oddsCache[GameType.Minesweeper] = minesweeper;

            // 2. 加拿大28 (PC28)
            var canada28 = new Dictionary<GamePlayMode, decimal>
            {    // 通常有回水，赔率稍低
                { GamePlayMode.BigSmallOddEven, 1.95m },
                { GamePlayMode.Digital, 10.0m }
            };
            _oddsCache[GameType.Canada28] = canada28;

            // 3. 快三
            var kuai3 = new Dictionary<GamePlayMode, decimal>
            {
                { GamePlayMode.BigSmallOddEven, 1.96m },
                { GamePlayMode.Digital, 8.5m }
            };
            _oddsCache[GameType.Kuai3] = kuai3;
        }

        // ==========================================
        // 4. 核心功能：保存
        // ==========================================

        /// <summary>
        /// 保存当前内存中的配置到 JSON 文件
        /// </summary>
        public void SaveConfig()
        {
            try
            {
                // 1. 转换为 String Key 的字典以便序列化
                var saveDict = new Dictionary<string, Dictionary<string, decimal>>();

                foreach (var gameKv in _oddsCache)
                {
                    var modeDict = new Dictionary<string, decimal>();
                    foreach (var modeKv in gameKv.Value)
                    {
                        modeDict[modeKv.Key.ToString()] = modeKv.Value;
                    }
                    saveDict[gameKv.Key.ToString()] = modeDict;
                }

                // 2. 配置序列化选项 (格式化 + 不转义中文)
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                // 3. 写入文件
                string json = JsonSerializer.Serialize(saveDict, options);
                File.WriteAllText(CONFIG_FILE_PATH, json);

                Console.WriteLine($"赔率配置已保存至: {CONFIG_FILE_PATH}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存赔率配置失败: {ex.Message}");
                throw; // 抛出异常供 UI 层捕获提示用户
            }
        }

        // ==========================================
        // 5. 公开方法：增删改查
        // ==========================================

        /// <summary>
        /// 获取指定赔率
        /// </summary>
        public decimal GetOdds(GameType gameType, GamePlayMode playMode)
        {
            if (_oddsCache.TryGetValue(gameType, out var modeDict))
            {
                if (modeDict.TryGetValue(playMode, out decimal odds))
                {
                    return odds;
                }
            }
            // 没找到配置返回 0，业务层可以据此判断异常
            return 0m;
        }

        /// <summary>
        /// 更新或添加赔率 (不自动保存，需手动调用 SaveConfig)
        /// </summary>
        public void UpdateOdds(GameType gameType, GamePlayMode playMode, decimal newOdds)
        {
            if (!_oddsCache.ContainsKey(gameType))
            {
                _oddsCache[gameType] = new Dictionary<GamePlayMode, decimal>();
            }
            _oddsCache[gameType][playMode] = newOdds;
        }
    }
}