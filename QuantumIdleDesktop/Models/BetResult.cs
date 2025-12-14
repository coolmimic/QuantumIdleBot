using System;
using System.Collections.Generic;
using System.Text;

namespace QuantumIdleDesktop.Models
{
    public enum BetResult
    {
        /// <summary>
        /// 未知/未结算
        /// </summary>
        None = 0,

        /// <summary>
        /// 中 (赢)
        /// </summary>
        Win = 1,

        /// <summary>
        /// 挂 (输)
        /// </summary>
        Loss = 2,

        /// <summary>
        /// 平 (走水/和局)
        /// </summary>
        Draw = 3
    }
}
