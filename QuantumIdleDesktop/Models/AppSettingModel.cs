using QuantumIdleDesktop.Models.Odds;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuantumIdleDesktop.Models
{
    public class AppSettingModel
    {
        // --- 1. 注单延迟 ---
        public bool EnableDelay { get; set; } = false;
        public int DelayMinSeconds { get; set; } = 1;
        public int DelayMaxSeconds { get; set; } = 3;

        // --- 2. 止盈止损止流 (风控) ---
        public bool EnableRiskControl { get; set; } = false;
        public decimal StopProfitAmount { get; set; } = 1000;   // 止盈
        public decimal StopLossAmount { get; set; } = 500;      // 止损
        public decimal StopTurnoverAmount { get; set; } = 5000; // 止流 (流水上限)

        // --- 3. 定时挂机 ---
        public bool EnableSchedule { get; set; } = false;
        // 我们只需要存 "时:分"，所以用 TimeSpan 或者 DateTime 均可，这里存 DateTime 方便控件绑定
        // 保存时只取 TimeOfDay 即可
        public DateTime ScheduleStartTime { get; set; } = DateTime.Today.AddHours(9); // 默认早9点
        public DateTime ScheduleEndTime { get; set; } = DateTime.Today.AddHours(21);  // 默认晚9点

        public List<SchemeRotationConfig> SchemeRotations { get; set; } = new List<SchemeRotationConfig>();
        public MultiplyConfig MultiplyConfig { get; set; } = new MultiplyConfig();
    }
}
