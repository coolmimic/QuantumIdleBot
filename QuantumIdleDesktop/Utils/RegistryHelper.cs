using Microsoft.Win32;
using System;
using System.Text;

namespace QuantumIdleDesktop.Utils
{
    public static class RegistryHelper
    {
        // 注册表路径：HKEY_CURRENT_USER\Software\QuantumIdleDesktop
        private const string RegistryPath = @"Software\QuantumIdleDesktop";

        /// <summary>
        /// 保存账户和密码到注册表
        /// </summary>
        public static void SaveLoginInfo(string username, string password)
        {
            try
            {
                // 打开或创建项
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryPath))
                {
                    if (key != null)
                    {
                        key.SetValue("Username", username);
                        // 简单的 Base64 编码 (防君子不防小人，生产环境建议用 AES 加密)
                        string encodedPass = Convert.ToBase64String(Encoding.UTF8.GetBytes(password));
                        key.SetValue("Password", encodedPass);
                    }
                }
            }
            catch (Exception ex)
            {
                // 记录日志或忽略，不要因为存不了密码导致程序崩溃
                Console.WriteLine("注册表写入失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 读取账户和密码
        /// </summary>
        /// <returns>(账号, 密码)</returns>
        public static (string User, string Pass) GetLoginInfo()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryPath))
                {
                    if (key != null)
                    {
                        string user = key.GetValue("Username")?.ToString() ?? "";
                        string passRaw = key.GetValue("Password")?.ToString() ?? "";

                        string pass = "";
                        if (!string.IsNullOrEmpty(passRaw))
                        {
                            // 解码 Base64
                            pass = Encoding.UTF8.GetString(Convert.FromBase64String(passRaw));
                        }

                        return (user, pass);
                    }
                }
            }
            catch
            {
                // 读取失败就算了
            }
            return ("", "");
        }
    }
}