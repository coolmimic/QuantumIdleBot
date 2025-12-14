using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuantumIdleWEB.Data;
using QuantumIdleModels.Entities;
using System.Security.Claims;
using System.Text.Json;

namespace QuantumIdleWEB.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SchemeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SchemeController> _logger;

        public SchemeController(
            ApplicationDbContext context,
            ILogger<SchemeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        /// <summary>
        /// 获取当前用户的所有方案
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSchemes()
        {
            try
            {
                var schemes = await _context.Schemes
                    .Where(s => s.UserId == CurrentUserId)
                    .OrderByDescending(s => s.UpdateTime)
                    .ToListAsync();

                return Ok(new { success = true, data = schemes });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取方案列表失败");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 根据ID获取方案详情
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetScheme(int id)
        {
            try
            {
                var scheme = await _context.Schemes
                    .FirstOrDefaultAsync(s => s.Id == id && s.UserId == CurrentUserId);

                if (scheme == null)
                {
                    return NotFound(new { success = false, message = "方案不存在" });
                }

                return Ok(new { success = true, data = scheme });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取方案详情失败");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 创建新方案
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateScheme([FromBody] CreateSchemeRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return BadRequest(new { success = false, message = "方案名称不能为空" });
                }

                // 序列化配置时保持原有的字段名大小写
                string oddsConfigJson = null;
                if (request.OddsConfig != null)
                {
                    oddsConfigJson = JsonSerializer.Serialize(request.OddsConfig);
                    _logger.LogInformation("保存 OddsConfig: {OddsConfig}", oddsConfigJson);
                }

                string drawRuleConfigJson = null;
                if (request.DrawRuleConfig != null)
                {
                    drawRuleConfigJson = JsonSerializer.Serialize(request.DrawRuleConfig);
                    _logger.LogInformation("保存 DrawRuleConfig: {DrawRuleConfig}", drawRuleConfigJson);
                }

                var scheme = new Scheme
                {
                    UserId = CurrentUserId,
                    SchemeId = Guid.NewGuid().ToString(),
                    Name = request.Name,
                    TgGroupName = request.TgGroupName,
                    TgGroupId = request.TgGroupId,
                    GameType = request.GameType,
                    PlayMode = request.PlayMode,
                    OddsType = request.OddsType,
                    PositionLst = JsonSerializer.Serialize(request.PositionLst ?? new List<int>()),
                    OddsConfig = oddsConfigJson,
                    DrawRule = request.DrawRule,
                    DrawRuleConfig = drawRuleConfigJson,
                    EnableStopProfitLoss = request.EnableStopProfitLoss,
                    StopProfitAmount = request.StopProfitAmount,
                    StopLossAmount = request.StopLossAmount,
                    IsEnabled = request.IsEnabled ?? true,
                    CreateTime = DateTime.Now,
                    UpdateTime = DateTime.Now
                };

                _context.Schemes.Add(scheme);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, data = scheme, message = "方案创建成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建方案失败");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 更新方案
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateScheme(int id, [FromBody] UpdateSchemeRequest request)
        {
            try
            {
                var scheme = await _context.Schemes
                    .FirstOrDefaultAsync(s => s.Id == id && s.UserId == CurrentUserId);

                if (scheme == null)
                {
                    return NotFound(new { success = false, message = "方案不存在" });
                }

                // 更新字段
                if (!string.IsNullOrWhiteSpace(request.Name))
                    scheme.Name = request.Name;

                if (request.TgGroupName != null)
                    scheme.TgGroupName = request.TgGroupName;

                if (request.TgGroupId.HasValue)
                    scheme.TgGroupId = request.TgGroupId.Value;

                if (request.GameType.HasValue)
                    scheme.GameType = request.GameType.Value;

                if (request.PlayMode.HasValue)
                    scheme.PlayMode = request.PlayMode.Value;

                if (request.OddsType.HasValue)
                    scheme.OddsType = request.OddsType.Value;

                if (request.PositionLst != null)
                    scheme.PositionLst = JsonSerializer.Serialize(request.PositionLst);

                if (request.OddsConfig != null)
                {
                    var oddsConfigJson = JsonSerializer.Serialize(request.OddsConfig);
                    _logger.LogInformation("更新 OddsConfig: {OddsConfig}", oddsConfigJson);
                    scheme.OddsConfig = oddsConfigJson;
                }

                if (request.DrawRule.HasValue)
                    scheme.DrawRule = request.DrawRule.Value;

                if (request.DrawRuleConfig != null)
                {
                    var drawRuleConfigJson = JsonSerializer.Serialize(request.DrawRuleConfig);
                    _logger.LogInformation("更新 DrawRuleConfig: {DrawRuleConfig}", drawRuleConfigJson);
                    scheme.DrawRuleConfig = drawRuleConfigJson;
                }

                if (request.EnableStopProfitLoss.HasValue)
                    scheme.EnableStopProfitLoss = request.EnableStopProfitLoss.Value;

                if (request.StopProfitAmount.HasValue)
                    scheme.StopProfitAmount = request.StopProfitAmount.Value;

                if (request.StopLossAmount.HasValue)
                    scheme.StopLossAmount = request.StopLossAmount.Value;

                if (request.IsEnabled.HasValue)
                    scheme.IsEnabled = request.IsEnabled.Value;

                scheme.UpdateTime = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, data = scheme, message = "方案更新成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新方案失败");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 删除方案
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteScheme(int id)
        {
            try
            {
                var scheme = await _context.Schemes
                    .FirstOrDefaultAsync(s => s.Id == id && s.UserId == CurrentUserId);

                if (scheme == null)
                {
                    return NotFound(new { success = false, message = "方案不存在" });
                }

                _context.Schemes.Remove(scheme);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "方案删除成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除方案失败");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 切换方案启用状态
        /// </summary>
        [HttpPost("{id}/toggle")]
        public async Task<IActionResult> ToggleScheme(int id)
        {
            try
            {
                var scheme = await _context.Schemes
                    .FirstOrDefaultAsync(s => s.Id == id && s.UserId == CurrentUserId);

                if (scheme == null)
                {
                    return NotFound(new { success = false, message = "方案不存在" });
                }

                scheme.IsEnabled = !scheme.IsEnabled;
                scheme.UpdateTime = DateTime.Now;
                await _context.SaveChangesAsync();

                return Ok(new { success = true, data = scheme, message = $"方案已{(scheme.IsEnabled ? "启用" : "禁用")}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "切换方案状态失败");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }

    // 请求模型
    public class CreateSchemeRequest
    {
        public string Name { get; set; }
        public string TgGroupName { get; set; }
        public long TgGroupId { get; set; }
        public int GameType { get; set; }
        public int PlayMode { get; set; }
        public int OddsType { get; set; }
        public List<int> PositionLst { get; set; }
        public object OddsConfig { get; set; }
        public int DrawRule { get; set; }
        public object DrawRuleConfig { get; set; }
        public bool EnableStopProfitLoss { get; set; }
        public decimal StopProfitAmount { get; set; }
        public decimal StopLossAmount { get; set; }
        public bool? IsEnabled { get; set; }
    }

    public class UpdateSchemeRequest
    {
        public string Name { get; set; }
        public string TgGroupName { get; set; }
        public long? TgGroupId { get; set; }
        public int? GameType { get; set; }
        public int? PlayMode { get; set; }
        public int? OddsType { get; set; }
        public List<int> PositionLst { get; set; }
        public object OddsConfig { get; set; }
        public int? DrawRule { get; set; }
        public object DrawRuleConfig { get; set; }
        public bool? EnableStopProfitLoss { get; set; }
        public decimal? StopProfitAmount { get; set; }
        public decimal? StopLossAmount { get; set; }
        public bool? IsEnabled { get; set; }
    }
}

