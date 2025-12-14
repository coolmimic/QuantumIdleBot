using QuantumIdleDesktop.Models.DrawRules;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace QuantumIdleDesktop.Views.Base
{
    public abstract partial class DrawRuleEditorBase : UserControl
    {
        /// <summary>
        /// 【读】从界面控件获取最新的配置对象
        /// </summary>
        public abstract DrawRuleConfigBase GetConfiguration();

        /// <summary>
        /// 【写】将配置对象的数据填充到界面控件中
        /// </summary>
        public abstract void SetConfiguration(DrawRuleConfigBase config);
    }
}
