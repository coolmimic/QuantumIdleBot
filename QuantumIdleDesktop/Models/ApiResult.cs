using System;
using System.Collections.Generic;
using System.Text;

namespace QuantumIdleDesktop.Models
{
    // 用于解析 API 返回的通用 JSON 格式
    public class ApiResult<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public System.DateTime NewExpireTime { get; set; } // 用于激活接口返回
    }

}
