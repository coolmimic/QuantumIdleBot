using QuantumIdleDesktop.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuantumIdleDesktop.Strategies.Games
{
    public class MinesweeperStrategy : IGameStrategy
    {
        /// <summary>
        /// 定义扫雷支持的玩法
        /// </summary>
        public List<GamePlayMode> GetSupportedModes()
        {
            return new List<GamePlayMode>
            {
                GamePlayMode.BigSmallOddEven, // 大小单双
                GamePlayMode.Digital,         // 数字 (1-6)
                GamePlayMode.Combination      // 组合 (大单/小双等)
            };
        }

        /// <summary>
        /// 解析开奖结果 (用于界面显示)
        /// </summary>
        public List<string> ParseResult(SchemeModel scheme, string rawCode)
        {
            // 1. 数据清洗：扫雷范围 1-6
            if (!int.TryParse(rawCode, out int num) || num < 1 || num > 6)
            {
                // 如果不是1-6的数字，视为无效或保持原样
                return new List<string> { rawCode };
            }

            // 2. 调用核心逻辑获取标签
            return GetWinTags(scheme.PlayMode, num);
        }

        /// <summary>
        /// 生成下注建议 (策略核心)
        /// </summary>
        public List<string> GetBetSuggestion(SchemeModel scheme, string lastResultTag, bool isFollow)
        {
            var result = new List<string>();

            if (string.IsNullOrWhiteSpace(lastResultTag)) return result;

            switch (scheme.PlayMode)
            {
                case GamePlayMode.Digital:
                    // --- 数字玩法 (1-6) ---
                    if (isFollow)
                    {
                        // 正投：跟
                        result.Add(lastResultTag);
                    }
                    else
                    {
                        // 反投 (杀号)：下注除了上期号码以外的所有号码
                        // 【修正】范围改为 1-6
                        var allNumbers = Enumerable.Range(1, 6).Select(x => x.ToString());
                        result.AddRange(allNumbers.Where(n => n != lastResultTag));
                    }
                    break;

                case GamePlayMode.BigSmallOddEven:
                case GamePlayMode.Combination:
                    // --- 双面盘 / 组合玩法 ---
                    if (isFollow)
                    {
                        result.Add(lastResultTag);
                    }
                    else
                    {
                        string opposite = GetOpposite(lastResultTag);
                        if (!string.IsNullOrEmpty(opposite))
                        {
                            result.Add(opposite);
                        }
                    }
                    break;
            }

            return result;
        }

        /// <summary>
        /// 判奖逻辑
        /// </summary>
        public int Judge(OrderModel order)
        {
            // 1. 解析开奖号码 (严格限制 1-6)
            if (!int.TryParse(order.OpenResult, out int openCode) || openCode < 1 || openCode > 6)
            {
                return 0;
            }

            // 2. 获取该号码在该玩法下的所有中奖标签
            List<string> winTags = GetWinTags(order.PlayMode, openCode);

            if (winTags.Count == 0 || string.IsNullOrEmpty(order.BetContent))
                return 0;

            // 3. 解析用户下注内容
            var userBets = order.BetContent
                .Split(new[] { ',', '，', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .ToList();

            // 4. 计算交集
            return userBets.Count(bet => winTags.Contains(bet));
        }

        // ==========================================
        //           私有 核心逻辑 / 辅助方法
        // ==========================================

        /// <summary>
        /// 核心转换逻辑：根据玩法和数字，计算出中奖标签
        /// </summary>
        private List<string> GetWinTags(GamePlayMode mode, int num)
        {
            var tags = new List<string>();

            switch (mode)
            {
                case GamePlayMode.BigSmallOddEven:
                    // 【修正】1,2,3 为小；4,5,6 为大
                    tags.Add(num > 3 ? "大" : "小");
                    tags.Add(num % 2 == 0 ? "双" : "单");
                    break;

                case GamePlayMode.Combination:
                    // 【修正】同上
                    string bs = num > 3 ? "大" : "小";
                    string oe = num % 2 == 0 ? "双" : "单";
                    tags.Add(bs + oe); // 例如 "大单"
                    break;

                case GamePlayMode.Digital:
                default:
                    tags.Add(num.ToString());
                    break;
            }
            return tags;
        }

        /// <summary>
        /// 获取相反属性 (用于反投)
        /// </summary>
        private string GetOpposite(string tag)
        {
            return tag switch
            {
                "大" => "小",
                "小" => "大",
                "单" => "双",
                "双" => "单",
                "大单" => "小双",
                "大双" => "小单",
                "小单" => "大双",
                "小双" => "大单",
                _ => null
            };
        }
    }
}