using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuantumIdleModels.Entities;
using QuantumIdleWEB.Data;

namespace QuantumIdleWEB.Controllers.API
{
    /// <summary>
    /// 管理员后台 API
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;
        private readonly QuantumIdleWEB.Services.CaptchaService _captchaService;

        // 管理员密码
        private static readonly string[] AdminPasswords = { "mimic998", "adb168" };

        public AdminController(ApplicationDbContext context, ILogger<AdminController> logger, QuantumIdleWEB.Services.CaptchaService captchaService)
        {
            _context = context;
            _logger = logger;
            _captchaService = captchaService;
        }

        /// <summary>
        /// 验证管理员密码
        /// </summary>
        private bool ValidateAdmin(string password)
        {
            return !string.IsNullOrEmpty(password) && AdminPasswords.Contains(password);
        }

        // ========================================
        // 管理员登录
        // ========================================

        /// <summary>
        /// 管理员登录验证
        /// </summary>
        [HttpPost("login")]
        public IActionResult Login([FromBody] AdminLoginRequest request)
        {
            // 验证码验证
            if (string.IsNullOrEmpty(request.CaptchaId) || string.IsNullOrEmpty(request.CaptchaCode))
            {
                return BadRequest(new { success = false, message = "请输入验证码" });
            }
            if (!_captchaService.Validate(request.CaptchaId, request.CaptchaCode))
            {
                return BadRequest(new { success = false, message = "验证码错误或已过期" });
            }

            if (ValidateAdmin(request.Password))
            {
                return Ok(new { success = true, message = "登录成功" });
            }
            return Unauthorized(new { success = false, message = "密码错误" });
        }

        // ========================================
        // 系统统计
        // ========================================

        /// <summary>
        /// 获取系统统计数据
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats([FromQuery] string password)
        {
            if (!ValidateAdmin(password))
                return Unauthorized(new { success = false, message = "密码错误" });

            var stats = new
            {
                userCount = await _context.Users.CountAsync(),
                orderCount = await _context.BetOrders.CountAsync(),
                cardCount = await _context.CardKeys.CountAsync(),
                unusedCardCount = await _context.CardKeys.CountAsync(c => !c.IsRedeemed),
                schemeCount = await _context.Schemes.CountAsync(),
                todayOrders = await _context.BetOrders.CountAsync(o => o.BetTime.Date == DateTime.Today),
                todayProfit = await _context.BetOrders.Where(o => o.BetTime.Date == DateTime.Today).SumAsync(o => o.Profit)
            };

            return Ok(new { success = true, data = stats });
        }

        // ========================================
        // 用户管理
        // ========================================

        /// <summary>
        /// 获取用户列表
        /// </summary>
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers(
            [FromQuery] string password,
            [FromQuery] int page = 1,
            [FromQuery] int size = 20,
            [FromQuery] string? search = null)
        {
            if (!ValidateAdmin(password))
                return Unauthorized(new { success = false, message = "密码错误" });

            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.UserName.Contains(search));
            }

            var total = await query.CountAsync();
            var users = await query
                .OrderByDescending(u => u.Id)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.ExpireTime,
                    u.IsActive,
                    u.Profit,
                    u.Turnover,
                    u.SimProfit,
                    u.SimTurnover,
                    u.TelegramChatId,
                    isExpired = u.ExpireTime < DateTime.Now
                })
                .ToListAsync();

            return Ok(new { success = true, data = users, total, page, size });
        }

        // ========================================
        // 卡密管理
        // ========================================

        /// <summary>
        /// 获取卡密列表
        /// </summary>
        [HttpGet("cards")]
        public async Task<IActionResult> GetCards(
            [FromQuery] string password,
            [FromQuery] int page = 1,
            [FromQuery] int size = 20,
            [FromQuery] bool? isRedeemed = null)
        {
            if (!ValidateAdmin(password))
                return Unauthorized(new { success = false, message = "密码错误" });

            var query = _context.CardKeys.AsQueryable();

            if (isRedeemed.HasValue)
            {
                query = query.Where(c => c.IsRedeemed == isRedeemed.Value);
            }

            var total = await query.CountAsync();
            var cards = await query
                .OrderByDescending(c => c.Id)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(c => new
                {
                    c.Id,
                    c.KeyCode,
                    c.DurationDays,
                    c.BatchName,
                    c.CreateTime,
                    c.IsRedeemed,
                    c.UsedTime,
                    c.UsedByAppUserId
                })
                .ToListAsync();

            return Ok(new { success = true, data = cards, total, page, size });
        }

        /// <summary>
        /// 批量生成卡密
        /// </summary>
        [HttpPost("cards/generate")]
        public async Task<IActionResult> GenerateCards([FromBody] GenerateCardsRequest request)
        {
            if (!ValidateAdmin(request.Password))
                return Unauthorized(new { success = false, message = "密码错误" });

            var cards = new List<CardKey>();
            for (int i = 0; i < request.Count; i++)
            {
                var keyCode = $"{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid():N}".ToUpper();
                cards.Add(new CardKey
                {
                    KeyCode = keyCode,
                    DurationDays = request.DurationDays,
                    BatchName = request.BatchName ?? $"Admin-{DateTime.Now:yyyyMMdd}",
                    CreateTime = DateTime.Now,
                    IsRedeemed = false
                });
            }

            _context.CardKeys.AddRange(cards);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = $"成功生成 {request.Count} 张卡密",
                data = cards.Select(c => c.KeyCode).ToList()
            });
        }

        // ========================================
        // 注单查询
        // ========================================

        /// <summary>
        /// 获取注单列表
        /// </summary>
        [HttpGet("orders")]
        public async Task<IActionResult> GetOrders(
            [FromQuery] string password,
            [FromQuery] int page = 1,
            [FromQuery] int size = 20,
            [FromQuery] int? userId = null,
            [FromQuery] string? issueNumber = null)
        {
            if (!ValidateAdmin(password))
                return Unauthorized(new { success = false, message = "密码错误" });

            var query = _context.BetOrders.AsQueryable();

            if (userId.HasValue)
            {
                query = query.Where(o => o.AppUserId == userId.Value);
            }
            if (!string.IsNullOrEmpty(issueNumber))
            {
                query = query.Where(o => o.IssueNumber.Contains(issueNumber));
            }

            var total = await query.CountAsync();
            var orders = await query
                .OrderByDescending(o => o.Id)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(o => new
                {
                    o.Id,
                    o.AppUserId,
                    o.IssueNumber,
                    o.BetContent,
                    o.Amount,
                    o.OpenResult,
                    o.Profit,
                    o.Status,
                    o.IsWin,
                    o.IsSimulation,
                    o.BetTime,
                    o.SettleTime,
                    o.TgGroupId
                })
                .ToListAsync();

            return Ok(new { success = true, data = orders, total, page, size });
        }

        // ========================================
        // 方案查询
        // ========================================

        /// <summary>
        /// 获取方案列表
        /// </summary>
        [HttpGet("schemes")]
        public async Task<IActionResult> GetSchemes(
            [FromQuery] string password,
            [FromQuery] int page = 1,
            [FromQuery] int size = 20,
            [FromQuery] int? userId = null)
        {
            if (!ValidateAdmin(password))
                return Unauthorized(new { success = false, message = "密码错误" });

            var query = _context.Schemes.AsQueryable();

            if (userId.HasValue)
            {
                query = query.Where(s => s.UserId == userId.Value);
            }

            var total = await query.CountAsync();
            var schemes = await query
                .OrderByDescending(s => s.Id)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(s => new
                {
                    s.Id,
                    s.UserId,
                    s.Name,
                    s.GameType,
                    s.PlayMode,
                    s.TgGroupId,
                    s.TgGroupName,
                    s.IsEnabled,
                    s.Profit,
                    s.Turnover,
                    s.SimProfit,
                    s.SimTurnover
                })
                .ToListAsync();

            return Ok(new { success = true, data = schemes, total, page, size });
        }
    }

    // ========================================
    // DTO 类
    // ========================================

    public class AdminLoginRequest
    {
        public string Password { get; set; }
        public string? CaptchaId { get; set; }
        public string? CaptchaCode { get; set; }
    }

    public class GenerateCardsRequest
    {
        public string Password { get; set; }
        public int Count { get; set; } = 1;
        public int DurationDays { get; set; } = 30;
        public string? BatchName { get; set; }
    }
}
