using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace QuantumIdleDesktop.Models.Odds
{

    // 1. 开启多态支持，指定 JSON 中用来区分类型的字段名为 "$type" (你也可以改成 "type" 或 "discriminator")
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    // 2. 注册已知的子类：LinearOddsConfig，并给它一个别名 "linear"
    [JsonDerivedType(typeof(LinearOddsConfig), typeDiscriminator: "linear")]
    // 3. 【未来扩展】：如果有新子类，直接在这里加一行即可
    // [JsonDerivedType(typeof(NewOddsConfig), typeDiscriminator: "new_type")]
    public abstract class OddsConfigBase
    {
        /// <summary>
        /// 具体的类型 (由子类重写返回)
        /// </summary>
        public abstract OddsType Type { get; }
    }
}
