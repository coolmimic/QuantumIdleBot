namespace QuantumIdleWEB.Strategies.DrawRules
{
    /// <summary>
    /// 游戏辅助类
    /// </summary>
    public static class GameHelper
    {
        /// <summary>
        /// 将开奖结果解析为标签
        /// </summary>
        public static List<string> ParseResultToTags(string result, int gameType, int playMode)
        {
            var tags = new List<string>();

            switch (gameType)
            {
                case 0: // 扫雷 (1-6)
                    if (int.TryParse(result, out int num) && num >= 1 && num <= 6)
                    {
                        tags.Add(num > 3 ? "大" : "小");
                        tags.Add(num % 2 == 0 ? "双" : "单");
                        tags.Add(num.ToString());
                    }
                    break;

                case 4: // 哈希彩
                    var numbers = result.Split(',').Select(s => int.TryParse(s.Trim(), out int n) ? n : 0);
                    int sum = numbers.Sum();
                    tags.Add(sum >= 23 ? "大" : "小");
                    tags.Add(sum % 2 == 0 ? "双" : "单");
                    tags.Add(sum.ToString());
                    break;

                default:
                    tags.Add(result);
                    break;
            }

            return tags;
        }

        /// <summary>
        /// 获取相反标签
        /// </summary>
        public static string GetOpposite(string tag)
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
                _ => tag
            };
        }

        /// <summary>
        /// 生成智能下注内容
        /// </summary>
        public static List<string> GetBetSuggestion(int gameType, int playMode, string targetTag, bool isFollow)
        {
            var result = new List<string>();

            if (string.IsNullOrWhiteSpace(targetTag)) return result;

            switch (gameType)
            {
                case 0: // 扫雷
                    if (playMode == 1) // Digital
                    {
                        if (isFollow)
                        {
                            result.Add(targetTag);
                        }
                        else
                        {
                            var allNumbers = Enumerable.Range(1, 6).Select(x => x.ToString());
                            result.AddRange(allNumbers.Where(n => n != targetTag));
                        }
                    }
                    else // BigSmallOddEven
                    {
                        if (isFollow)
                        {
                            result.Add(targetTag);
                        }
                        else
                        {
                            result.Add(GetOpposite(targetTag));
                        }
                    }
                    break;

                default:
                    if (isFollow)
                    {
                        result.Add(targetTag);
                    }
                    else
                    {
                        result.Add(GetOpposite(targetTag));
                    }
                    break;
            }

            return result;
        }
    }
}
