using System.Collections.Concurrent;

namespace QuantumIdleWEB.Services
{
    /// <summary>
    /// 验证码服务
    /// </summary>
    public class CaptchaService
    {
        // 验证码缓存: Key = captchaId, Value = (answer, expireTime)
        private readonly ConcurrentDictionary<string, (string Answer, DateTime ExpireTime)> _captchaStore = new();
        private readonly Random _random = new();
        private readonly object _cleanupLock = new();
        private DateTime _lastCleanup = DateTime.MinValue;

        private const int CAPTCHA_LENGTH = 4;
        private const int CAPTCHA_EXPIRE_MINUTES = 5;
        private const string CHARS = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // 排除易混淆字符

        /// <summary>
        /// 生成验证码
        /// </summary>
        /// <returns>(captchaId, answer, imageBase64)</returns>
        public (string CaptchaId, string Answer, string ImageBase64) GenerateCaptcha()
        {
            // 定期清理过期验证码
            CleanupExpired();

            // 生成随机答案
            var answer = new string(Enumerable.Range(0, CAPTCHA_LENGTH)
                .Select(_ => CHARS[_random.Next(CHARS.Length)])
                .ToArray());

            // 生成唯一ID
            var captchaId = Guid.NewGuid().ToString("N");

            // 存储
            _captchaStore[captchaId] = (answer, DateTime.Now.AddMinutes(CAPTCHA_EXPIRE_MINUTES));

            // 生成简单的 SVG 验证码图片
            var svg = GenerateSvgCaptcha(answer);
            var base64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(svg));

            return (captchaId, answer, $"data:image/svg+xml;base64,{base64}");
        }

        /// <summary>
        /// 验证验证码
        /// </summary>
        public bool Validate(string captchaId, string userInput)
        {
            if (string.IsNullOrEmpty(captchaId) || string.IsNullOrEmpty(userInput))
                return false;

            if (_captchaStore.TryRemove(captchaId, out var stored))
            {
                if (DateTime.Now <= stored.ExpireTime)
                {
                    return string.Equals(stored.Answer, userInput.ToUpper().Trim(), StringComparison.OrdinalIgnoreCase);
                }
            }

            return false;
        }

        /// <summary>
        /// 生成 SVG 验证码图片
        /// </summary>
        private string GenerateSvgCaptcha(string text)
        {
            var width = 120;
            var height = 40;

            var sb = new System.Text.StringBuilder();
            sb.Append($"<svg xmlns='http://www.w3.org/2000/svg' width='{width}' height='{height}'>");
            
            // 背景
            sb.Append($"<rect width='{width}' height='{height}' fill='#f0f0f0'/>");

            // 干扰线
            for (int i = 0; i < 5; i++)
            {
                var x1 = _random.Next(width);
                var y1 = _random.Next(height);
                var x2 = _random.Next(width);
                var y2 = _random.Next(height);
                var color = $"rgb({_random.Next(100, 200)},{_random.Next(100, 200)},{_random.Next(100, 200)})";
                sb.Append($"<line x1='{x1}' y1='{y1}' x2='{x2}' y2='{y2}' stroke='{color}' stroke-width='1'/>");
            }

            // 文字
            for (int i = 0; i < text.Length; i++)
            {
                var x = 15 + i * 25;
                var y = 25 + _random.Next(-5, 5);
                var rotate = _random.Next(-15, 15);
                var color = $"rgb({_random.Next(0, 100)},{_random.Next(0, 100)},{_random.Next(0, 100)})";
                sb.Append($"<text x='{x}' y='{y}' font-size='22' font-family='Arial' font-weight='bold' fill='{color}' transform='rotate({rotate},{x},{y})'>{text[i]}</text>");
            }

            // 干扰点
            for (int i = 0; i < 30; i++)
            {
                var x = _random.Next(width);
                var y = _random.Next(height);
                var color = $"rgb({_random.Next(0, 255)},{_random.Next(0, 255)},{_random.Next(0, 255)})";
                sb.Append($"<circle cx='{x}' cy='{y}' r='1' fill='{color}'/>");
            }

            sb.Append("</svg>");
            return sb.ToString();
        }

        /// <summary>
        /// 清理过期验证码
        /// </summary>
        private void CleanupExpired()
        {
            // 每分钟最多清理一次
            if ((DateTime.Now - _lastCleanup).TotalMinutes < 1)
                return;

            lock (_cleanupLock)
            {
                if ((DateTime.Now - _lastCleanup).TotalMinutes < 1)
                    return;

                _lastCleanup = DateTime.Now;
                var now = DateTime.Now;
                var expiredKeys = _captchaStore
                    .Where(kv => kv.Value.ExpireTime < now)
                    .Select(kv => kv.Key)
                    .ToList();

                foreach (var key in expiredKeys)
                {
                    _captchaStore.TryRemove(key, out _);
                }
            }
        }
    }
}
