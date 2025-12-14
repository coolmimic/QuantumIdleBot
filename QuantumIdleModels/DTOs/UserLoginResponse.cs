using System;
using System.Collections.Generic;
using System.Text;

namespace QuantumIdleModels.DTOs
{
    public class UserLoginResponse
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Token { get; set; } 
        public System.DateTime ExpireTime { get; set; }
        public int IsActive { get; set; }
    }
}
