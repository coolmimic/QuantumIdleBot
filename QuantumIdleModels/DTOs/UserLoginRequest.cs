using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace QuantumIdleModels.DTOs
{
    public class UserLoginRequest
    {
        // 1. 限制用户名：必须 4-20 位，且只能包含字母和数字 (防止注入或怪异字符)
        [Required(ErrorMessage = "用户名不能为空")]
        [StringLength(20, MinimumLength = 4, ErrorMessage = "用户名长度必须在 4 到 20 位之间")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "用户名只能包含字母和数字")]
        public string UserName { get; set; }

        // 2. 限制密码：最少 6 位
        [Required(ErrorMessage = "密码不能为空")]
        [MinLength(6, ErrorMessage = "密码长度至少需要 6 位")]
        public string Password { get; set; }

        // 3. 验证码相关（注册时需要）
        public string? CaptchaId { get; set; }
        public string? CaptchaCode { get; set; }
    }
}
