using System.ComponentModel.DataAnnotations;

namespace QuantumIdleModels.DTOs
{
    /// <summary>
    /// 使用卡密重置密码请求
    /// </summary>
    public class ResetPasswordWithCardRequest
    {
        /// <summary>
        /// 用户名
        /// </summary>
        [Required]
        public string UserName { get; set; }

        /// <summary>
        /// 卡密码
        /// </summary>
        [Required]
        public string CardCode { get; set; }

        /// <summary>
        /// 新密码
        /// </summary>
        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; }
    }
}
