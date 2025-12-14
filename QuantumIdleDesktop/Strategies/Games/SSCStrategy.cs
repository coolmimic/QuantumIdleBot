using QuantumIdleDesktop.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuantumIdleDesktop.Strategies.Games
{
    public class SSCStrategy : IGameStrategy
    {
        /// <summary>
        /// 定义时时彩支持的玩法
        /// </summary>
        public List<GamePlayMode> GetSupportedModes()
        {
            return new List<GamePlayMode>
            {
                GamePlayMode.PositionDigit,              // 至少选择1个位置
                GamePlayMode.PositionBigSmallOddEven,    // 至少选择1个位置
                GamePlayMode.DragonTiger,                // 至少选择2个位置
                GamePlayMode.Sum                        // 至少选择2个位置
            };
        }

        /// <summary>
        /// 解析开奖结果
        /// </summary>
        public List<string> ParseResult(SchemeModel scheme, string rawCode)
        {
            // 1. 基础数据清洗 "1,2,3,4,5" -> [1, 2, 3, 4, 5]
            var nums = ParseRawCode(rawCode);
            if (nums.Count != 5) return new List<string>(); // SSC必须是5位

            var resultTags = new List<string>();
            var posList = scheme.PositionLst; // 获取方案配置的位置列表

            switch (scheme.PlayMode)
            {
                case GamePlayMode.PositionDigit:
                    // 必须指定1个位置 (例如：0=万位)
                    if (posList != null && posList.Count == 1)
                    {
                        int index = posList[0];
                        if (index >= 0 && index < nums.Count)
                        {
                            resultTags.Add(nums[index].ToString());
                        }
                    }
                    break;

                case GamePlayMode.PositionBigSmallOddEven:
                    // 必须指定1个位置
                    if (posList != null && posList.Count == 1)
                    {
                        int index = posList[0];
                        if (index >= 0 && index < nums.Count)
                        {
                            int val = nums[index];
                            // 0-4为小，5-9为大
                            resultTags.Add(val >= 5 ? "大" : "小");
                            resultTags.Add(val % 2 == 0 ? "双" : "单");
                        }
                    }
                    break;

                case GamePlayMode.DragonTiger:
                    // 必须指定2个位置 (A vs B)
                    if (posList != null && posList.Count == 2)
                    {
                        int idxA = posList[0];
                        int idxB = posList[1];
                        if (idxA >= 0 && idxA < 5 && idxB >= 0 && idxB < 5)
                        {
                            int a = nums[idxA];
                            int b = nums[idxB];

                            if (a > b) resultTags.Add("龙");
                            else if (a < b) resultTags.Add("虎");
                            else resultTags.Add("和");
                        }
                    }
                    break;

                case GamePlayMode.Sum:
                    // 计算指定位置的和值，如果没指定位置则默认算总和
                    if (posList != null && posList.Count > 0)
                    {
                        int sum = 0;
                        foreach (var idx in posList)
                        {
                            if (idx >= 0 && idx < 5) sum += nums[idx];
                        }
                        resultTags.Add(sum.ToString());

                        // 也可以附加和值的大小单双属性
                        // resultTags.Add(sum >= 23 ? "大" : "小"); // 仅作示例，具体阈值视规则而定
                    }
                    break;
            }

            return resultTags;
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
                case GamePlayMode.PositionDigit:
                    // --- 定位胆 (0-9) ---
                    if (isFollow)
                    {
                        result.Add(lastResultTag);
                    }
                    else
                    {
                        // 杀号：0-9 中排除上期号码
                        var all = Enumerable.Range(0, 10).Select(x => x.ToString());
                        result.AddRange(all.Where(x => x != lastResultTag));
                    }
                    break;

                case GamePlayMode.PositionBigSmallOddEven:
                    // --- 大小单双 ---
                    if (isFollow)
                    {
                        result.Add(lastResultTag);
                    }
                    else
                    {
                        string opp = GetOpposite(lastResultTag);
                        if (opp != null) result.Add(opp);
                    }
                    break;

                case GamePlayMode.DragonTiger:
                    // --- 龙虎 ---
                    if (lastResultTag == "和")
                    {
                        // 如果上期是和，策略通常有几种：
                        // 1. 观望 (返回空)
                        // 2. 也是跟和
                        // 这里默认如果不跟和，则无建议
                        if (isFollow) result.Add("和");
                    }
                    else
                    {
                        if (isFollow)
                        {
                            result.Add(lastResultTag);
                        }
                        else
                        {
                            // 龙的反面是虎，虎的反面是龙
                            string opp = GetOpposite(lastResultTag);
                            if (opp != null) result.Add(opp);
                        }
                    }
                    break;

                case GamePlayMode.Sum:
                    // --- 和值 ---
                    // 和值如果是具体数字，反投逻辑较复杂，通常不建议全包反投
                    if (isFollow)
                    {
                        result.Add(lastResultTag);
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
            // 1. 解析开奖数据
            var nums = ParseRawCode(order.OpenResult);
            if (nums.Count != 5) return 0;
            var winTags = new List<string>();
            var posList = order.PositionLst;
            if (posList == null || posList.Count == 0) return 0;

            switch (order.PlayMode)
            {
                case GamePlayMode.PositionDigit:
                case GamePlayMode.PositionBigSmallOddEven:
                    if (posList.Count == 1)
                    {
                        int idx = posList[0];
                        int val = nums[idx];
                        if (order.PlayMode == GamePlayMode.PositionDigit)
                            winTags.Add(val.ToString());
                        else
                        {
                            winTags.Add(val >= 5 ? "大" : "小");
                            winTags.Add(val % 2 == 0 ? "双" : "单");
                        }
                    }
                    break;

                case GamePlayMode.DragonTiger:
                    if (posList.Count == 2)
                    {
                        int a = nums[posList[0]];
                        int b = nums[posList[1]];
                        if (a > b) winTags.Add("龙");
                        else if (a < b) winTags.Add("虎");
                        else winTags.Add("和");
                    }
                    break;
                case GamePlayMode.Sum:
                    int sum = 0;
                    foreach (var idx in posList) sum += nums[idx];
                    winTags.Add(sum.ToString());
                    break;
            }

            // 3. 匹配下注内容
            if (winTags.Count == 0 || string.IsNullOrEmpty(order.BetContent)) return 0;

            var userBets = order.BetContent
                .Split(new[] { ',', '，', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .ToList();

            return userBets.Count(bet => winTags.Contains(bet));
        }

        // =======================
        // 私有辅助方法
        // =======================

        private List<int> ParseRawCode(string rawCode)
        {
            var list = new List<int>();
            if (string.IsNullOrWhiteSpace(rawCode)) return list;

            var parts = rawCode.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                if (int.TryParse(part, out int n))
                {
                    list.Add(n);
                }
            }
            return list;
        }

        private string GetOpposite(string tag)
        {
            return tag switch
            {
                "大" => "小",
                "小" => "大",
                "单" => "双",
                "双" => "单",
                "龙" => "虎",
                "虎" => "龙",
                _ => null
            };
        }
    }
}