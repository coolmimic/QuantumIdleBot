using QuantumIdleDesktop.Models;
using QuantumIdleDesktop.Services;
using QuantumIdleDesktop.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace QuantumIdleDesktop
{
    public class CacheData
    {

        public static QuantumIdleModels.DTOs.UserLoginResponse SoftwareUser { get; set;  }
        public static AppSettingModel Settings { get; set; } = new AppSettingModel();
        public static List<TelegramGroupModel> GroupLst { get; set; } = new List<TelegramGroupModel>();

        public static readonly object OrderLock = new object();
        public static List<SchemeModel> Schemes { get; set; }
        public static TelegramService tgService { get; set;  }
        public static List<OrderModel> Orders { get; set; }

        public static async Task OnLoad()
        {
            Orders = new List<OrderModel>();
            Schemes = await SchemeFileHelper.LoadListAsync();
            Settings = await JsonHelper.LoadAsync<AppSettingModel>("Data\\Settings.json");
        }
    }
}
