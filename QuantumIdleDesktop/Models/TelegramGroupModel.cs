using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using TL;

namespace QuantumIdleDesktop.Models
{
    public class TelegramGroupModel
    {
        /// <summary>
        /// 群组的唯一标识符（Telegram API 使用）。
        /// </summary>
        [JsonPropertyName("id")]
        public long Id { get; set; }

        /// <summary>
        /// 群组的显示名称。
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }
        public bool IsChannel { get; set; }
        public InputPeerChannel Peer { get; set; }
    }
}
