using QuantumIdleDesktop.Models;
using System;

namespace QuantumIdleDesktop.Strategies
{
    public static class GameResultHelper
    {
        /// <summary>
        /// 主入口：接收原始字符串，根据游戏类型分发
        /// </summary>
        public static List<string> NormalizeResult(GameType gameType, GamePlayMode playMode, string code)
        {
            // 直接分发 string code，不预先转 int
            switch (gameType)
            {
                case GameType.Minesweeper:
                    return HandleMinesweeper(playMode, code);
                case GameType.Kuai3:

                case GameType.Canada28:
                case GameType.Canada20:

                default:
                    return new List<string> { code };
            }
        }





        /// <summary>
        /// 扫雷逻辑：通常是单个数字 (0-9)
        /// </summary>
        private static List<string> HandleMinesweeper(GamePlayMode playMode, string code)
        {
            if (!int.TryParse(code, out int num) || num < 0 || num > 9)
                return new List<string> { code };

            var tags = new List<string>();

            switch (playMode)
            {
                case GamePlayMode.BigSmallOddEven:
                    tags.Add(num > 3 ? "大" : "小");
                    tags.Add(num % 2 == 0 ? "双" : "单");
                    return tags;
                default:
                    return new List<string> { code };
            }
        }







        /// <summary>
        /// 获取下注内容列表 (总入口)
        /// 原方法名: asdasd
        /// </summary>
        /// <param name="gameType">游戏类型</param>
        /// <param name="playMode">玩法模式 (大小单双 / 数字)</param>
        /// <param name="code">参照的号码或特征 (如 "大", "1", "单")</param>
        /// <param name="isFollow">是否正投 (True=跟/正投, False=斩/反投)</param>
        /// <returns>生成的下注内容列表</returns>
        public static List<string> GetBetContent(GameType gameType, GamePlayMode playMode, string code, bool isFollow)
        {
            switch (gameType)
            {
                case GameType.Minesweeper: // 扫雷
                    return GetMinesweeperStrategy(playMode, code, isFollow);

                // 其他游戏类型暂时留空或返回原值
                case GameType.Kuai3:
                case GameType.Canada28:
                case GameType.Canada20:
                default:
                    // 默认行为：正投返回自己，反投返回空（或者你可以定义默认反投逻辑）
                    return isFollow ? new List<string> { code } : new List<string>();
            }
        }

        /// <summary>
        /// 扫雷具体的策略实现
        /// 原方法名: dasdas
        /// </summary>
        /// <param name="playMode">玩法模式</param>
        /// <param name="code">特征值 (如 "大", "单" 或 "1")</param>
        /// <param name="isFollow">True=跟, False=反</param>
        public static List<string> GetMinesweeperStrategy(GamePlayMode playMode, string code, bool isFollow)
        {
            // 结果容器
            var result = new List<string>();

            switch (playMode)
            {
                case GamePlayMode.BigSmallOddEven:
                    // 处理 大小单双
                    // 核心逻辑：如果是正投，返回原值；如果是反投，返回相反值
                    string target = isFollow ? code : GetOppositeAttribute(code);

                    if (!string.IsNullOrEmpty(target))
                    {
                        result.Add(target);
                    }
                    break;

                case GamePlayMode.Digital:
                    // 处理 数字 (1-6)
                    // 假设 code 是 "1", "2"... "6"

                    // 校验一下 code 是否在 1-6 范围内（防止异常数据）
                    if (int.TryParse(code, out int num) && num >= 1 && num <= 6)
                    {
                        if (isFollow)
                        {
                            // 正投：直接投这个号
                            result.Add(code);
                        }
                        else
                        {
                            // 反投 (斩)：投除了这个号以外的所有号码 (1-6范围)
                            // 也就是：如果 code 是 "1"，反投就是 "2,3,4,5,6"
                            var allNumbers = new[] { "1", "2", "3", "4", "5", "6" };
                            result.AddRange(allNumbers.Where(n => n != code));
                        }
                    }
                    break;

                default:
                    break;
            }

            return result;
        }


        /// <summary>
        /// 辅助方法：获取相反的属性
        /// </summary>
        private static string GetOppositeAttribute(string code)
        {
            return code switch
            {
                "大" => "小",
                "小" => "大",
                "单" => "双",
                "双" => "单",
                _ => null // 无法识别的属性返回 null
            };
        }

    }
}