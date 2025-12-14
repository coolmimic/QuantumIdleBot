using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using QuantumIdleDesktop.Models;
using QuantumIdleModels.DTOs; 

namespace QuantumIdleDesktop.Services
{
    public class AuthApiService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private const string BaseUrl = "https://9988.gg:8443/api/";

        public AuthApiService()
        {
            var handler = new HttpClientHandler();
            // 开发环境绕过 SSL 证书验证
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                                             
            _httpClient = new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }
        private StringContent GetJsonContent(object obj)
        {
            var json = JsonSerializer.Serialize(obj, _jsonOptions);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }
        // ============================================================
        // 1. 登录 (T 变成了 UserLoginResponse)
        // ============================================================
        public async Task<ApiResult<UserLoginResponse>> LoginAsync(string username, string password)
        {
            var requestDto = new UserLoginRequest { UserName = username, Password = password };
            var content = GetJsonContent(requestDto);

            return await PostAsync<UserLoginResponse>("User/login", content);
        }
        // ============================================================
        // 2. 注册 (无 Data，用 object)
        // ============================================================
        public async Task<ApiResult<object>> RegisterAsync(string username, string password)
        {
            var requestDto = new UserLoginRequest { UserName = username, Password = password };
            var content = GetJsonContent(requestDto);

            return await PostAsync<object>("User/register", content);
        }
        // ============================================================
        // 3. 激活 (无 Data，但有 ExpireTime 在外壳里)
        // ============================================================
        public async Task<ApiResult<object>> ActivateAsync(string username, string cardCode)
        {
            var requestDto = new ActivateCardRequest { UserName = username, CardCode = cardCode };
            var content = GetJsonContent(requestDto);

            return await PostAsync<object>("CardKey/activate", content);
        }
        // ============================================================
        // 4. 重置密码
        // ============================================================
        public async Task<ApiResult<object>> ResetPasswordAsync(string username, string oldPassword, string newPassword)
        {
            var requestDto = new UserResetPwdRequest
            {
                UserName = username,
                OldPassword = oldPassword,
                NewPassword = newPassword
            };
            var content = GetJsonContent(requestDto);

            return await PostAsync<object>("User/reset-password", content);
        }
        // ============================================================
        // 通用 POST 方法 (提取公共逻辑，减少代码量)
        // ============================================================
        private async Task<ApiResult<T>> PostAsync<T>(string endpoint, HttpContent content)
        {
            try
            {
                var response = await _httpClient.PostAsync(endpoint, content);
                var responseString = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<ApiResult<T>>(responseString, _jsonOptions);

                if (result == null)
                    return new ApiResult<T> { Success = false, Message = "服务器响应为空" };

                // 如果 HTTP 状态码是 400/401/500，强制设置 Success = false
                if (!response.IsSuccessStatusCode)
                    result.Success = false;

                return result;
            }
            catch (Exception ex)
            {
                return new ApiResult<T> { Success = false, Message = "请求异常: " + ex.Message };
            }
        }
    }
}