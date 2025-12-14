using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuantumIdleDesktop.Services
{
    public class ServerVersionInfo
    {
        public string Version { get; set; }
        public string Description { get; set; }
    }

    public class UpdateChecker
    {
        // ⚠️ 请确保地址正确
        private const string VersionInfoUrl = "https://qt-a2l.pages.dev/version.json";
        private const string UpdaterExe = "QuantumIdleUpdater.exe";

        public async Task<bool> CheckAsync()
        {
            try
            {
                // 设置较短超时 (3秒)，避免无网时启动卡顿太久
                using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(3) })
                {
                    // 开发环境跳过 SSL 验证 (生产环境建议去掉)
                    var handler = new HttpClientHandler { ServerCertificateCustomValidationCallback = delegate { return true; } };

                    using (var http = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(3) })
                    {
                        // 1. 获取服务器版本
                        string json = await http.GetStringAsync(VersionInfoUrl);

                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var serverInfo = JsonSerializer.Deserialize<ServerVersionInfo>(json, options);

                        if (serverInfo == null) return false;

                        // 2. 获取本地版本
                        Version localVer = Assembly.GetExecutingAssembly().GetName().Version!;
                        Version serverVer = Version.Parse(serverInfo.Version);

                        AppGlobal.localVer = localVer.ToString();

                        // 3. 核心修改：强制更新逻辑
                        if (serverVer > localVer)
                        {
                            return LaunchUpdater();
                        }
                    }
                }
            }
            catch (Exception)
            {
                // 网络不通或服务器挂了，静默跳过，允许用户离线使用
                return false;
            }

            return false; // 无更新
        }

        private bool LaunchUpdater()
        {
            if (!File.Exists(UpdaterExe))
            {
                MessageBox.Show($"致命错误：缺少更新组件 ({UpdaterExe})，请重新下载客户端！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false; // 启动失败
            }

            string currentExe = AppDomain.CurrentDomain.FriendlyName;
            string args = $"\"{VersionInfoUrl}\" \"{currentExe}\"";

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = UpdaterExe,
                    Arguments = args,
                    UseShellExecute = true
                });
                return true; // 启动成功
            }
            catch (Exception ex)
            {
                MessageBox.Show($"无法启动更新程序: {ex.Message}", "错误");
                return false;
            }
        }
    }
}