using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace QuantumIdleDesktop.Utils
{
    public static class EnumHelper
    {
        /// <summary>
        /// 获取枚举的 Description 中文描述
        /// </summary>
        public static string GetDescription(this Enum value)
        {
            if (value == null) return "";

            FieldInfo field = value.GetType().GetField(value.ToString());

            if (field == null) return value.ToString();

            // 获取 Description 特性
            DescriptionAttribute attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;

            // 如果有中文描述就返回中文，否则返回英文原名
            return attribute == null ? value.ToString() : attribute.Description;
        }
    }
}
