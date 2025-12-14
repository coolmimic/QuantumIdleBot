using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens; // <--- 新增
using QuantumIdleModels.DTOs;
using QuantumIdleModels.Entities;
using QuantumIdleWEB.Data;
using System.IdentityModel.Tokens.Jwt; // <--- 新增
using System.Security.Claims; // <--- 新增
using System.Security.Cryptography;
using System.Text;

namespace QuantumIdleWEB.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration; // <--- 1. 新增配置注入

        // 修改构造函数，接收 IConfiguration
        public UserController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet("getitme")]
        public string getitme()
        {
            return DateTime.Now.ToString();
        }

        /// <summary>
        /// 注册接口
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserLoginRequest request)
        {
            // 1. 检查用户名是否已存在
            if (await _context.Users.AnyAsync(u => u.UserName == request.UserName))
            {
                return BadRequest(new { success = false, message = "用户名已存在" });
            }

            // 2. 创建用户实体
            var newUser = new AppUser
            {
                UserName = request.UserName,
                PasswordHash = ComputeHash(request.Password),
                CreateTime = DateTime.Now,
                IsActive = 0
            };

            // 3. 保存到数据库
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "注册成功" });
        }

        /// <summary>
        /// 登录接口
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
        {
            // 1. 查找用户
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == request.UserName);

            if (user == null)
            {
                return BadRequest(new { success = false, message = "账号不存在" });
            }

            // 2. 验证密码
            string inputHash = ComputeHash(request.Password);
            if (user.PasswordHash != inputHash)
            {
                return BadRequest(new { success = false, message = "密码错误" });
            }

            // 3. 检查状态
            if (user.IsActive == 2)
            {
                return BadRequest(new { success = false, message = "账号已被封禁" });
            }

            // 4. 生成 JWT Token
            string jwtToken = GenerateJwtToken(user); // <--- 调用生成方法

            // 5. 返回信息
            return Ok(new
            {
                success = true,
                message = "登录成功",
                data = new UserLoginResponse
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    ExpireTime = user.ExpireTime,
                    IsActive = user.IsActive,
                    Token = jwtToken // <--- 返回真正的 Token
                }
            });
        }

        /// <summary>
        /// 重置密码接口（使用旧密码验证）
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] UserResetPwdRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == request.UserName);

            if (user == null)
            {
                return BadRequest(new { success = false, message = "账号不存在" });
            }

            // 验证旧密码
            if (user.PasswordHash != ComputeHash(request.OldPassword))
            {
                return BadRequest(new { success = false, message = "旧密码错误" });
            }

            // 更新密码
            user.PasswordHash = ComputeHash(request.NewPassword);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "密码修改成功" });
        }

        // --- JWT 生成工具方法 ---
        private string GenerateJwtToken(AppUser user)
        {
            // 读取配置文件中的 Key
            var jwtSettings = _configuration.GetSection("Jwt");
            var keyString = jwtSettings["Key"];

            // 安全检查：防止配置文件没写 Key 导致报错
            if (string.IsNullOrEmpty(keyString))
            {
                throw new Exception("JWT Key is missing in appsettings.json");
            }

            var key = Encoding.UTF8.GetBytes(keyString);

            // 定义 Token 里包含的信息 (Claims)
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), // 用户ID
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName), // 用户名
                new Claim("IsActive", user.IsActive.ToString()) // 自定义：用户状态
            };

            // 创建签名凭证
            var creds = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256
            );

            // 生成 Token
            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(24), // Token 有效期 24 小时
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // --- 哈希辅助方法 ---
        private string ComputeHash(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                var builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        /// <summary>
        /// 重置用户数据（清除盈亏和投注记录）
        /// </summary>
        [HttpPost("reset-data")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> ResetData()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized(new { success = false, message = "未授权" });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. 清除该用户的投注记录
                var orders = await _context.BetOrders.Where(o => o.AppUserId == userId).ToListAsync();
                _context.BetOrders.RemoveRange(orders);

                // 2. 重置用户盈亏数据
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.Profit = 0;
                    user.Turnover = 0;
                    user.SimProfit = 0;
                    user.SimTurnover = 0;
                }

                // 3. 重置该用户所有方案的盈亏数据
                var schemes = await _context.Schemes.Where(s => s.UserId == userId).ToListAsync();
                foreach (var scheme in schemes)
                {
                    scheme.Profit = 0;
                    scheme.Turnover = 0;
                    scheme.SimProfit = 0;
                    scheme.SimTurnover = 0;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { success = true, message = $"已清除 {orders.Count} 条投注记录，重置 {schemes.Count} 个方案盈亏" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { success = false, message = $"重置失败: {ex.Message}" });
            }
        }
    }
}