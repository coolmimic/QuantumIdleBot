using QuantumIdleDesktop.Models;
using QuantumIdleDesktop.Strategies.Games;
using System;
using System.Collections.Generic;

namespace QuantumIdleDesktop.Strategies
{
    public static class GameStrategyFactory
    {
        // 使用字典缓存实例，避免每次调用都 new 一个新对象
        private static readonly Dictionary<GameType, IGameStrategy> _strategyCache;

        static GameStrategyFactory()
        {
            // 在静态构造函数中初始化所有策略
            _strategyCache = new Dictionary<GameType, IGameStrategy>
            {
                // 1. 扫雷
                { GameType.Minesweeper, new MinesweeperStrategy() },

                // 2. 快三
                { GameType.Kuai3, new KuaiSanStrategy() },

                // 3. 时时彩 (以及类似的5球彩种)
                { GameType.HashLottery, new SSCStrategy() },
                // 如果 Canada28 的逻辑和 SSC 不同，你需要写 Canada28Strategy
                // 如果 Canada28 逻辑类似快三 (都是3个球求和)，可以复用快三策略：
                // { GameType.Canada28, new KuaiSanStrategy() } 
            };
        }

        /// <summary>
        /// 获取指定游戏类型的策略实例
        /// </summary>
        /// <param name="gameType">游戏类型</param>
        /// <returns>对应的策略实现</returns>
        /// <exception cref="NotImplementedException">如果游戏类型未配置策略</exception>
        public static IGameStrategy GetStrategy(GameType gameType)
        {
            if (_strategyCache.TryGetValue(gameType, out var strategy))
            {
                return strategy;
            }

            return null;

            // 如果传入了未实现的游戏类型，抛出异常提示开发者
            throw new NotImplementedException($"游戏类型 [{gameType}] 的策略尚未在工厂中注册。请在 GameStrategyFactory 中添加映射。");
        }
    }
}