using System.Text.Json.Serialization;

namespace QuantumIdleWEB.Services.Tron
{

    public class TronGridResponse
    {
        [JsonPropertyName("data")]
        public List<TronTransaction> Data { get; set; }
    }

    // 每一笔交易的详情
    public class TronTransaction
    {
        [JsonPropertyName("transaction_id")]
        public string TransactionId { get; set; }

        [JsonPropertyName("block_timestamp")]
        public long BlockTimestamp { get; set; }

        [JsonPropertyName("from")]
        public string From { get; set; }

        [JsonPropertyName("to")]
        public string To { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; } // 注意：波场返回的是字符串类型的整数，如 "1000000"

        [JsonPropertyName("token_info")]
        public TronTokenInfo TokenInfo { get; set; }
    }

    public class TronTokenInfo
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; } // 必须是 "USDT"

        [JsonPropertyName("decimals")]
        public int Decimals { get; set; } // 通常是 6
    }
}

