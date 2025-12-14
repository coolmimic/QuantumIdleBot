using Microsoft.EntityFrameworkCore;
using QuantumIdleModels.Entities;
using QuantumIdleWEB.Data;
using System.Text.Json;
using Telegram.Bot;

namespace QuantumIdleWEB.Services.Tron
{
    public class TronMonitorService : BackgroundService
    {


        private readonly IConfiguration _config;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TronMonitorService> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _myWalletAddress;
        // ================= 配置区域 =================
        // 🔴 请务必修改为你的真实收款地址

        // USDT-TRC20 合约地址 (这是固定的，不用改)
        private const string USDT_CONTRACT = "TR7NHqjeKQxGTCi8q8ZY4pL8otSzgjLj6t";
        // ===========================================

        public TronMonitorService(IServiceProvider serviceProvider, ILogger<TronMonitorService> logger, IConfiguration config)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _config = config; // 注入
            _myWalletAddress = _config["Tron:WalletAddress"];
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(15);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🚀 波场智能查账服务已启动 (按需扫描模式)...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // =========================================================
                    // 步骤 1：创建作用域，检查数据库有没有活儿干
                    // =========================================================
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        var bot = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

                        // 获取所有 [待支付] 且 [未过期] 的订单
                        // 注意：这里不使用 AsNoTracking，因为如果匹配成功我们需要直接修改它
                        var pendingOrders = await db.PaymentOrders
                            .Where(o => o.Status == 0 && o.ExpireTime > DateTime.Now)
                            .ToListAsync();

                        // 如果没有订单，直接跳过 API 查询，节省资源
                        if (pendingOrders.Count > 0)
                        {
                            // =========================================================
                            // 步骤 2：只有存在待支付订单时，才去查链
                            // =========================================================
                            _logger.LogInformation($"监测到 {pendingOrders.Count} 笔待支付订单，正在扫描链上数据...");

                            var transactions = await FetchLatestTransactions();

                            if (transactions != null && transactions.Count > 0)
                            {
                                // 开始匹配
                                await MatchOrdersAsync(db, bot, pendingOrders, transactions);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // 如果是数据库连接错误，记录更详细的信息
                    if (ex.Message.Contains("SQL Server") || ex.Message.Contains("network-related"))
                    {
                        _logger.LogError($"查账服务数据库连接失败: {ex.Message}");
                        _logger.LogWarning("请检查数据库连接字符串配置，或确保数据库服务正在运行");
                    }
                    else
                    {
                        _logger.LogError($"查账服务异常: {ex.Message}");
                    }
                }

                // 休息 10 秒
                await Task.Delay(10000, stoppingToken);
            }
        }

        // =========================================================
        // 核心逻辑 A：获取链上数据
        // =========================================================
        private async Task<List<TronTransaction>> FetchLatestTransactions()
        {
            // TronGrid API: 查询最近的 TRC20 交易 (Limit 20 足够了，因为我们每10秒查一次)
            string url = $"https://api.trongrid.io/v1/accounts/{_myWalletAddress}/transactions/trc20?contract_address={USDT_CONTRACT}&limit=20";

            try
            {
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"TronGrid API 请求失败: {response.StatusCode}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<TronGridResponse>(json);
                return result?.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError($"请求波场 API 出错: {ex.Message}");
                return null;
            }
        }

        // =========================================================
        // 核心逻辑 B：智能匹配 (尾数 + 宽容度)
        // =========================================================
        private async Task MatchOrdersAsync(ApplicationDbContext db, ITelegramBotClient bot, List<PaymentOrder> pendingOrders, List<TronTransaction> txs)
        {
            foreach (var tx in txs)
            {
                // 1. 基础过滤：必须是 USDT，必须是转给我
                if (tx.TokenInfo.Symbol != "USDT") continue;
                if (tx.To != _myWalletAddress) continue;

                // 2. 查重：防止同一笔 TXID 重复入账
                // 注意：这里查全表，确保这笔交易没被之前的任何订单用过
                bool isUsed = await db.PaymentOrders.AnyAsync(o => o.TxId == tx.TransactionId);
                if (isUsed) continue;

                // 3. 解析金额
                // 波场 API 返回的 value 是字符串整数，例如 "1000000" 代表 1.000000 U
                if (!decimal.TryParse(tx.Value, out decimal rawVal)) continue;
                int decimals = tx.TokenInfo.Decimals > 0 ? tx.TokenInfo.Decimals : 6;
                decimal txAmount = rawVal / (decimal)Math.Pow(10, decimals);

                // 4. 提取链上金额的尾数 (保留3位小数)
                // 例如 4.923 -> 0.923
                decimal txTail = Math.Round(txAmount - Math.Floor(txAmount), 3);

                // 5. 在待支付订单里寻找匹配对象
                // 规则：(订单实付金额的尾数 == 链上金额的尾数)
                var matchOrder = pendingOrders.FirstOrDefault(o =>
                {
                    decimal orderTail = Math.Round(o.RealAmount - Math.Floor(o.RealAmount), 3);
                    return orderTail == txTail;
                });

                if (matchOrder != null)
                {
                    // 6. 二次校验：宽容度检查 (防止拿小钱撞大单)
                    // 允许误差：0 到 2.0 U (涵盖交易所手续费 0.8U ~ 1U)
                    decimal diff = matchOrder.RealAmount - txAmount;

                    if (diff >= 0 && diff <= 2.0m)
                    {
                        // ✅ 匹配成功！执行发货
                        await DeliverOrder(db, bot, matchOrder, tx.TransactionId, txAmount);

                        // 匹配成功后，从内存列表中移除，防止同一个订单被多次匹配
                        pendingOrders.Remove(matchOrder);
                    }
                }
            }
        }

        // =========================================================
        // 核心逻辑 C：发货与入库
        // =========================================================
        private async Task DeliverOrder(ApplicationDbContext db, ITelegramBotClient bot, PaymentOrder order, string txId, decimal paidAmount)
        {
            _logger.LogInformation($"订单匹配成功！User: {order.TelegramId}, TXID: {txId}");

            // 1. 更新订单状态
            order.Status = 1; // 已支付
            order.TxId = txId;
            order.CreateTime = DateTime.Now;

            // 2. 生成卡密 (时间+GUID 格式)
            string key = $"{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 16).ToUpper()}";

            var card = new CardKey
            {
                KeyCode = key,
                DurationDays = order.DurationDays,
                BatchName = $"AutoPay-{order.TelegramId}",
                CreateTime = DateTime.Now,
                IsRedeemed = false
            };
            db.CardKeys.Add(card);

            // 3. 保存数据库变更
            await db.SaveChangesAsync();

            // 4. 发送 Telegram 通知
            var msg = $"✅ **支付成功！**\n\n" +
                      $"实收金额：`{paidAmount}` USDT\n" +
                      $"购买商品：{order.DurationDays} 天授权\n" +
                      $"----------------------------\n" +
                      $"🔑 **您的卡密**：\n`{key}`\n" +
                      $"----------------------------\n" +
                      $"⚠️ 请复制卡密到软件客户端进行激活。";

            try
            {
                await bot.SendMessage(order.TelegramId, msg, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"无法给用户发送消息: {ex.Message}");
            }
        }
    }
}
