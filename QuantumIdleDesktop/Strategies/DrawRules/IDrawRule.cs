using QuantumIdleDesktop.GameCore;
using QuantumIdleDesktop.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuantumIdleDesktop.Strategies.DrawRules
{
    public interface IDrawRule
    {
        /// <summary>
        /// 获取下一期下注内容
        /// </summary>
        /// <param name="scheme">方案配置</param>
        /// <param name="context">群历史记录</param>
        /// <returns>下注内容列表</returns>
        List<string> GetNextBet(SchemeModel scheme, GroupGameContext context);
    }
}
