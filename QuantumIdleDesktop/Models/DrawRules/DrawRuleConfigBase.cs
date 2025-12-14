using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace QuantumIdleDesktop.Models.DrawRules
{



    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(FixedNumberDrawRuleConfig), typeDiscriminator: "fixed_number")]
    [JsonDerivedType(typeof(FollowLastDrawRuleConfig), typeDiscriminator: "followlast_number")]
    [JsonDerivedType(typeof(SlayDragonFollowDragonRuleConfig), typeDiscriminator: "SlayDragon_number")]
    [JsonDerivedType(typeof(NumberTrendRuleConfig), typeDiscriminator: "NumberTrend_number")]
    [JsonDerivedType(typeof(PatternTrendRuleConfig), typeDiscriminator: "PatternTrend_number")]
    [JsonDerivedType(typeof(BranchTrendRuleConfig), typeDiscriminator: "BranchTrend_number")]
    [JsonDerivedType(typeof(ResultFollowRuleConfig), typeDiscriminator: "ResultFollow_number")]
    public abstract class DrawRuleConfigBase
    {
        public abstract DrawRuleType Type { get; }
    }
}
