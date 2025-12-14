using QuantumIdleDesktop.GameCore;
using QuantumIdleDesktop.Models;
using QuantumIdleDesktop.Models.Odds;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuantumIdleDesktop.Strategies.OddsRules
{
    /// <summary>
    /// 倍率/资金策略接口 (BLL)
    /// </summary>
    public interface IOddsRule
    {
        /// <summary>
        /// 获取下一期的倍数 (只负责返回 int 类型的倍数，例如 1, 2, 5)
        /// </summary>
        /// <param name="scheme">方案对象 (提供配置和当前层级状态)</param>
        /// <returns>倍数</returns>
        int GetNextMultiplier(SchemeModel scheme);

        /// <summary>
        /// 结算更新：根据上一单的输赢，更新方案的层级状态
        /// </summary>
        /// <param name="scheme">方案模型 (需要更新里面的 CurrentStep 或 LossCount)</param>
        /// <param name="lastOrder">上一单的订单信息 (包含输赢结果)</param>
        void UpdateState(SchemeModel scheme, OrderModel lastOrder);
    }
}
