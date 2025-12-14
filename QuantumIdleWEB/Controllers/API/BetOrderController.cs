using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuantumIdleModels.DTOs;
using QuantumIdleModels.Entities;
using QuantumIdleWEB.Data;
using System.Security.Claims; 

namespace QuantumIdleWeb.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // <--- 3. 加上这个！只有带 Token 才能访问这里面的接口
    public class BetOrderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public BetOrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- 定义一个属性，方便获取当前用户ID ---
        // ClaimTypes.NameIdentifier 对应的是我们在 Login 里填入的 sub (用户ID)
        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        /// <summary>
        /// 批量上传注单
        /// </summary>
        [HttpPost("add-batch")]
        public async Task<IActionResult> AddBatchOrders([FromBody] List<CreateBetOrderDto> inputs)
        {
            if (inputs == null || inputs.Count == 0)
            {
                return Ok(new { success = true, message = "没有需要上传的数据", count = 0 });
            }

            // 4. 获取真实的当前用户 ID
            int userId = CurrentUserId;

            // 2. 预处理：提取所有上传的 SourceRefId
            var incomingRefIds = inputs.Select(i => i.SourceRefId).ToList();

            // 3. 查重 (只查当前用户的，避免和其他用户冲突)
            var existingRefIds = await _context.BetOrders
                .Where(o => o.AppUserId == userId && incomingRefIds.Contains(o.SourceRefId))
                .Select(o => o.SourceRefId)
                .ToListAsync();

            // 4. 过滤
            var newOrders = new List<BetOrder>();

            foreach (var input in inputs)
            {
                if (existingRefIds.Contains(input.SourceRefId))
                {
                    continue;
                }

                var order = new BetOrder
                {
                    AppUserId = userId, // <--- 这里填入真实 ID
                    SourceRefId = input.SourceRefId,
                    IssueNumber = input.IssueNumber,
                    GameType = input.GameType,
                    PlayMode = input.PlayMode,
                    SchemeId = input.SchemeId,
                    BetContent = input.BetContent,
                    Amount = input.Amount,
                    IsSimulation = input.IsSimulation,
                    BetTime = input.BetTime,
                    Status = 0,
                    Profit = 0,
                    PayoutAmount = 0,
                    IsWin = false
                };

                newOrders.Add(order);
            }

            // 5. 批量写入
            if (newOrders.Count > 0)
            {
                await _context.BetOrders.AddRangeAsync(newOrders);
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                success = true,
                message = $"成功导入 {newOrders.Count} 条注单，跳过重复 {inputs.Count - newOrders.Count} 条",
                count = newOrders.Count
            });
        }

        /// <summary>
        /// 获取注单列表
        /// </summary>
        [HttpGet("list")]
        public async Task<IActionResult> GetList(int page = 1, int size = 20)
        {
            int userId = CurrentUserId; // <--- 获取真实 ID

            var list = await _context.BetOrders
                .Where(o => o.AppUserId == userId) // <--- 只查自己的数据
                .OrderByDescending(o => o.BetTime)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            return Ok(new { success = true, data = list });
        }

        /// <summary>
        /// 结算订单
        /// </summary>
        [HttpPost("settle")]
        public async Task<IActionResult> SettleOrder([FromBody] UpdateBetResultDto input)
        {
            int userId = CurrentUserId; // <--- 获取真实 ID

            // 查找订单时，同时限制 AppUserId，防止恶意用户修改别人的订单
            var order = await _context.BetOrders
                .FirstOrDefaultAsync(o => o.SourceRefId == input.SourceRefId && o.AppUserId == userId);

            if (order == null) return NotFound("未找到该订单，或该订单不属于你");

            order.OpenResult = input.OpenResult;
            order.PayoutAmount = input.PayoutAmount;
            order.Profit = input.PayoutAmount - order.Amount;
            order.IsWin = input.IsWin;
            order.Status = 1;
            order.SettleTime = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }
    }
}