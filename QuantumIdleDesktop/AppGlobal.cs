using System;
using System.Collections.Generic;
using System.Text;

namespace QuantumIdleDesktop
{
    /// <summary>
    /// 全局应用状态管理类
    /// </summary>
    public static class AppGlobal
    {

        public static string localVer { get; set; }

        /// <summary>
        /// 是否开启模拟模式
        /// </summary>
        public static bool IsSimulation { get; set; } = false;

        /// <summary>
        /// 是否正在挂机运行
        /// </summary>
        public static bool IsRunning { get; set; } = false;

        /// <summary>
        /// 实盘余额
        /// </summary>
        public static decimal Balance { get; set; } = 0;

        /// <summary>
        /// 实盘盈亏
        /// </summary>
        public static decimal Profit { get; set; } = 0;

        /// <summary>
        /// 实盘流水
        /// </summary>
        public static decimal Turnover { get; set; } = 0;

        /// <summary>
        /// 模拟盈亏
        /// </summary>
        public static decimal SimProfit { get; set; } = 0;

        /// <summary>
        /// 模拟流水
        /// </summary>
        public static decimal SimTurnover { get; set; } = 0;
    }
}
