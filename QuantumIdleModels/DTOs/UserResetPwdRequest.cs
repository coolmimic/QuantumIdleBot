using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace QuantumIdleModels.DTOs
{
    public class UserResetPwdRequest
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string OldPassword { get; set; }

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; }

        // 验证码相关
        public string? CaptchaId { get; set; }
        public string? CaptchaCode { get; set; }
    }
}
