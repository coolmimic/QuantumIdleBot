using QuantumIdleDesktop.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuantumIdleDesktop.Strategies.Games
{
    public class KuaiSanStrategy : IGameStrategy
    {
        /// <summary>
        /// 定义快三支持的玩法
        /// </summary>
        public List<GamePlayMode> GetSupportedModes()
        {
            return new List<GamePlayMode>
            {
                GamePlayMode.BigSmallOddEven, // 大小单双
                GamePlayMode.Digital,         // 和值数字 (3-18)
                GamePlayMode.Combination,     // 组合 (大单/小双)
                GamePlayMode.HighOdds         // 高倍 (豹子)
            };
        }

        /// <summary>
        /// 解析开奖结果 (用于界面显示)
        /// </summary>
        public List<string> ParseResult(SchemeModel scheme, string rawCode)
        {
            // 1. 解析原始数据 (支持 "1+2+3=6" 或 "1,2,3")
            if (!TryParseKuaiSanResult(rawCode, out int sum, out List<int> diceList))
            {
                return new List<string> { rawCode };
            }

            // 2. 根据玩法生成显示标签
            return GetWinTags(scheme.PlayMode, sum, diceList);
        }

        /// <summary>
        /// 生成下注建议
        /// </summary>
        public List<string> GetBetSuggestion(SchemeModel scheme, string lastResultTag, bool isFollow)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(lastResultTag)) return result;

            switch (scheme.PlayMode)
            {
                case GamePlayMode.Digital:
                    // --- 和值 (3-18) ---
                    if (isFollow)
                    {
                        result.Add(lastResultTag);
                    }
                    else
                    {
                        // 杀号：3-18 中排除上期和值
                        // Range(3, 16) 生成 3 到 18 (共16个数)
                        var allSums = Enumerable.Range(3, 16).Select(x => x.ToString());
                        result.AddRange(allSums.Where(x => x != lastResultTag));
                    }
                    break;

                case GamePlayMode.BigSmallOddEven:
                case GamePlayMode.Combination:
                    // --- 大小单双 / 组合 ---
                    if (isFollow)
                    {
                        result.Add(lastResultTag);
                    }
                    else
                    {
                        string opp = GetOpposite(lastResultTag);
                        if (!string.IsNullOrEmpty(opp)) result.Add(opp);
                    }
                    break;

                case GamePlayMode.HighOdds:
                    // --- 豹子 ---
                    // 如果上期是豹子，且策略是跟，则建议投豹子
                    if (lastResultTag == "豹子" && isFollow)
                    {
                        result.Add("豹子");
                    }
                    // 豹子通常不建议反投(杀豹子)，因为赔率高且中奖率低，全包成本太高
                    break;
            }

            return result;
        }

        /// <summary>
        /// 判奖逻辑
        /// </summary>
        public int Judge(OrderModel order)
        {
            // 1. 解析开奖数据
            if (!TryParseKuaiSanResult(order.OpenResult, out int sum, out List<int> diceList))
            {
                return 0;
            }

            // 2. 获取该玩法下的中奖标签
            List<string> winTags = GetWinTags(order.PlayMode, sum, diceList);

            if (winTags.Count == 0 || string.IsNullOrEmpty(order.BetContent))
                return 0;

            // 3. 对比用户下注内容
            var userBets = order.BetContent
                .Split(new[] { ',', '，', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .ToList();

            return userBets.Count(bet => winTags.Contains(bet));
        }

        // ==========================================
        //           私有 核心逻辑 / 辅助方法
        // ==========================================

        /// <summary>
        /// 核心转换逻辑：根据玩法 + 和值 + 骰子明细，生成所有中奖标签
        /// </summary>
        private List<string> GetWinTags(GamePlayMode playMode, int sum, List<int> diceList)
        {
            var tags = new List<string>();

            switch (playMode)
            {
                case GamePlayMode.BigSmallOddEven:
                    // 范围：3-18。通常 3-10 为小，11-18 为大
                    tags.Add(sum >= 11 ? "大" : "小");
                    tags.Add(sum % 2 == 0 ? "双" : "单");
                    break;

                case GamePlayMode.Digital:
                    // 数字玩法：中奖标签就是和值本身
                    tags.Add(sum.ToString());
                    break;

                case GamePlayMode.Combination:
                    // 组合玩法：拼接大小和单双，例如 "大单", "小双"
                    string bs = sum >= 11 ? "大" : "小";
                    string oe = sum % 2 == 0 ? "双" : "单";
                    tags.Add(bs + oe);
                    break;

                case GamePlayMode.HighOdds:
                    // 豹子判断
                    if (diceList.Count == 3 &&
                        diceList[0] == diceList[1] &&
                        diceList[1] == diceList[2])
                    {
                        tags.Add("豹子");
                    }
                    break;
            }
            return tags;
        }

        /// <summary>
        /// 辅助方法：解析快三开奖字符串
        /// 支持: "4+6+5=15" 或 "1,2,3" 或 "1,2,3=6"
        /// </summary>
        private bool TryParseKuaiSanResult(string rawResult, out int sum, out List<int> diceList)
        {
            sum = 0;
            diceList = new List<int>();

            if (string.IsNullOrWhiteSpace(rawResult)) return false;

            try
            {
                // 1. 先按 '=' 分割，获取左边的骰子部分和右边的和值
                var parts = rawResult.Split('=');
                var dicePart = parts[0];

                // 2. 解析骰子明细 (同时支持 '+' 和 ',')
                diceList = dicePart.Split(new[] { '+', ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => int.Parse(s.Trim()))
                    .ToList();

                if (diceList.Count != 3) return false;

                // 3. 确定和值
                // 优先用数据源提供的和值(等号右边)，没有则自己计算
                if (parts.Length > 1 && int.TryParse(parts[1], out int parsedSum))
                {
                    sum = parsedSum;
                }
                else
                {
                    sum = diceList.Sum();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

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