using System;
using System.Collections.Generic;
using System.Text;

namespace QuantumIdleDesktop.Utils
{
    public static class UIHelper
    {

        /// <summary>
        /// 【核心方法】将枚举绑定到 ComboBox (下拉框)
        /// 用法: comboBox1.BindEnum<GameType>();
        /// </summary>
        public static void BindEnum<TEnum>(this ComboBox cmb) where TEnum : Enum
        {
            var list = GetEnumDataSource<TEnum>();

            cmb.DisplayMember = "Text";
            cmb.ValueMember = "Value";
            cmb.DataSource = list;

            if (cmb.Items.Count > 0) cmb.SelectedIndex = 0;
        }

        /// <summary>
        /// 【核心方法】将枚举绑定到 ListBox (列表框)
        /// 用法: listBox1.BindEnum<GameType>();
        /// </summary>
        public static void BindEnum<TEnum>(this ListBox lb) where TEnum : Enum
        {
            var list = GetEnumDataSource<TEnum>();

            lb.DisplayMember = "Text";
            lb.ValueMember = "Value";
            lb.DataSource = list;

            if (lb.Items.Count > 0) lb.SelectedIndex = 0;
        }

        /// <summary>
        /// 【核心方法】将枚举绑定到 DataGridView 的下拉列
        /// 用法: colGameType.BindEnum<GameType>();
        /// </summary>
        public static void BindEnum<TEnum>(this DataGridViewComboBoxColumn col) where TEnum : Enum
        {
            var list = GetEnumDataSource<TEnum>();

            col.DisplayMember = "Text";
            col.ValueMember = "Value";
            col.DataSource = list;
        }

        // --- 内部私有方法：生成数据源 ---
        private static List<EnumItem> GetEnumDataSource<TEnum>() where TEnum : Enum
        {
            var list = new List<EnumItem>();
            foreach (TEnum item in Enum.GetValues(typeof(TEnum)))
            {
                list.Add(new EnumItem
                {
                    Text = item.GetDescription(), // 自动调用之前的扩展方法获取中文
                    Value = item
                });
            }
            return list;
        }

        /// <summary>
        /// 【便捷获取】获取 ComboBox 选中的枚举值
        /// 用法: GameType type = comboBox1.GetSelectedEnum<GameType>();
        /// </summary>
        public static TEnum GetSelectedEnum<TEnum>(this ComboBox cmb) where TEnum : Enum
        {
            if (cmb.SelectedValue == null) return default;
            return (TEnum)cmb.SelectedValue;
        }

        /// <summary>
        /// 【便捷获取】获取 ListBox 选中的枚举值
        /// </summary>
        public static TEnum GetSelectedEnum<TEnum>(this ListBox lb) where TEnum : Enum
        {
            if (lb.SelectedValue == null) return default;
            return (TEnum)lb.SelectedValue;
        }
    }


    /// <summary>
    /// 专门用于 UI 绑定的通用项
    /// </summary>
    public class EnumItem
    {
        public string Text { get; set; }  // 显示的中文 (Description)
        public object Value { get; set; } // 实际的枚举值

        public override string ToString() => Text; // 兼容 ListBox 直接添加的情况
    }
}
