using QuantumIdleDesktop.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuantumIdleDesktop.Strategies.JudgeRules
{
    internal class JudgeFactory
    {
        private static readonly Dictionary<GameType, IJudgeStrategy> _strategies = new Dictionary<GameType, IJudgeStrategy>
        {
            { GameType.Minesweeper, new MinesweeperJudge() }
        };

        public static IJudgeStrategy GetStrategy(GameType type)
        {
            if (_strategies.TryGetValue(type, out var strategy))
            {
                return strategy;
            }
            // 默认返回一个总是输的策略，防止报错
            return new DefaultJudge();
        }
    }
    /// <summary>
    /// 默认兜底策略 (防止未实现的游戏类型导致空指针报错)
    /// </summary>
    public class DefaultJudge : IJudgeStrategy
    {
        public int Judge(OrderModel order)
        {
            // 默认返回 0，表示没中奖
            return 0;
        }
    }
}
