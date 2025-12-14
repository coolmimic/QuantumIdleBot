using QuantumIdleDesktop.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuantumIdleDesktop.Strategies
{
    public interface IGameStrategy
    {
        /// <summary>
        /// 获取当前游戏支持的所有玩法模式
        /// (替代原 GameResultHelper.GetSupportedModes)
        /// </summary>
        List<GamePlayMode> GetSupportedModes();

        /// <summary>
        /// 解析开奖结果
        /// </summary>
        List<string> ParseResult(SchemeModel scheme, string rawCode);

        /// <summary>
        /// 生成下注建议
        /// </summary>
        List<string> GetBetSuggestion(SchemeModel scheme, string lastResultTag, bool isFollow);

        /// <summary>
        /// 判奖逻辑
        /// </summary>
        int Judge(OrderModel order);
    }
}
