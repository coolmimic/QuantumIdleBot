using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Runtime.InteropServices;

namespace QuantumIdleUpdater
{
    // ==========================================
    // 2. 窗体逻辑
    // ==========================================
    public partial class FrmUpdater : Form
    {
        private readonly string _manifestUrl; // version.json 地址
        private readonly string _mainExeName; // 主程序名
        private readonly string _appRoot;     // 运行目录

        // --- 拖动无边框窗体 API ---
        [DllImport("user32.dll")] public static extern bool ReleaseCapture();
        [DllImport("user32.dll")] public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

        public FrmUpdater(string[] args)
        {
            InitializeComponent();

            // 获取当前运行目录
            _appRoot = AppDomain.CurrentDomain.BaseDirectory;

            // 解析参数 (防止直接双击报错)
            if (args.Length >= 2)
            {
                _manifestUrl = args[0];
                _mainExeName = args[1];
            }
            else
            {
                // 调试用默认值 (可选)
                _manifestUrl = "";
                _mainExeName = "";
            }
        }

        private async void FrmUpdater_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_manifestUrl))
            {
                MessageBox.Show("参数错误：缺少更新地址。", "错误");
                Application.Exit();
                return;
            }

            // 窗体加载后立即开始
            await StartSyncProcess();
        }

        // ==========================================
        // 核心流程
        // ==========================================
        private async Task StartSyncProcess()
        {
            try
            {
                // 1. 等待主程序退出
                UpdateUI("正在等待主程序关闭...", 0);
                await WaitForAppExit();

                // 2. 下载清单 (version.json)
                UpdateUI("获取更新列表...", 5);
                var manifest = await DownloadManifestAsync(_manifestUrl);

                if (manifest == null || manifest.Files == null)
                    throw new Exception("版本文件解析失败");

                // 3. 遍历文件列表进行同步
                int totalCount = manifest.Files.Count;
                int processedCount = 0;

                using (var client = new HttpClient())
                {
                    // 开发环境绕过SSL (生产环境建议去掉)
                    var handler = new HttpClientHandler { ServerCertificateCustomValidationCallback = delegate { return true; } };
                    using (var http = new HttpClient(handler))
                    {
                        // 设置基础 URL (确保以 / 结尾)
                        string baseUrl = manifest.BaseUrl.TrimEnd('/') + "/";

                        foreach (var file in manifest.Files)
                        {
                            processedCount++;

                            // 计算进度 (5% ~ 100%)
                            int progress = 5 + (int)((double)processedCount / totalCount * 95);

                            string localPath = Path.Combine(_appRoot, file.Path);
                            string remoteUrl = baseUrl + file.Path.Replace("\\", "/");

                            // --- ⚡️ 极简判断逻辑 ---
                            if (NeedDownload(localPath, file.SkipIfExists))
                            {
                                UpdateUI($"正在更新: {file.Path}", progress);
                                await DownloadSingleFileAsync(http, remoteUrl, localPath);
                            }
                            else
                            {
                                UpdateUI($"保留配置: {file.Path}", progress);
                            }
                        }
                    }
                }

                // 4. 完成重启
                UpdateUI("更新完成，正在启动...", 100);
                await Task.Delay(1000); // 让用户看到100%

                Process.Start(new ProcessStartInfo
                {
                    FileName = _mainExeName,
                    UseShellExecute = true
                });

                Application.Exit();
            }
            catch (Exception ex)
            {
                lblStatus.ForeColor = Color.Red;
                lblStatus.Text = "更新失败";
                MessageBox.Show($"更新出错: {ex.Message}\n\n请联系客服下载完整包。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        // ==========================================
        // 辅助逻辑方法
        // ==========================================

        /// <summary>
        /// 判断是否需要下载
        /// </summary>
        private bool NeedDownload(string localPath, bool skipIfExists)
        {
            // 1. 如果是受保护文件 (如配置文件)，且本地已存在 -> 跳过
            if (skipIfExists && File.Exists(localPath))
                return false;

            // 2. 其他所有情况 (包括文件不存在，或者普通DLL/EXE) -> 强制下载覆盖
            return true;
        }

        /// <summary>
        /// 下载并写入单个文件
        /// </summary>
        private async Task DownloadSingleFileAsync(HttpClient client, string url, string localPath)
        {
            // 确保目标文件夹存在 (处理 data/config.json 这种情况)
            string dir = Path.GetDirectoryName(localPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            // 下载流并写入
            using (var response = await client.GetAsync(url))
            {
                response.EnsureSuccessStatusCode();
                using (var fs = new FileStream(localPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await response.Content.CopyToAsync(fs);
                }
            }
        }

        /// <summary>
        /// 下载并解析清单 JSON
        /// </summary>
        private async Task<UpdateManifest> DownloadManifestAsync(string url)
        {
            using (var client = new HttpClient())
            {
                var handler = new HttpClientHandler { ServerCertificateCustomValidationCallback = delegate { return true; } };
                using (var http = new HttpClient(handler))
                {
                    string json = await http.GetStringAsync(url);
                    // 忽略大小写解析
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<UpdateManifest>(json, options);
                }
            }
        }

        /// <summary>
        /// 循环检查主程序是否已退出
        /// </summary>
        private async Task WaitForAppExit()
        {
            string processName = Path.GetFileNameWithoutExtension(_mainExeName);

            // 最多尝试等待 10 秒，防止死循环
            for (int i = 0; i < 20; i++)
            {
                var processes = Process.GetProcessesByName(processName);
                if (processes.Length == 0) return; // 已退出

                await Task.Delay(500);
            }

            // 如果还没退，尝试强杀 (可选)
            // Process.GetProcessesByName(processName).ToList().ForEach(p => p.Kill());
        }

        // ==========================================
        // UI 交互逻辑
        // ==========================================

        // 更新界面状态 (线程安全)
        private void UpdateUI(string text, int percent)
        {
            if (InvokeRequired)
            {
                Invoke(() => UpdateUI(text, percent));
                return;
            }

            lblStatus.Text = text;
            lblPercent.Text = $"{percent}%";

            // 动态调整自定义进度条 (Panel) 的宽度
            // 假设底槽叫做 pnlTrack，填充条叫做 pnlBar
            if (pnlTrack.Width > 0)
            {
                int newWidth = (int)(pnlTrack.Width * (percent / 100.0));
                pnlBar.Width = newWidth;
            }
        }

        // 顶部栏拖动窗口
        private void pnlTitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, 0xA1, 0x2, 0);
            }
        }

        // 关闭按钮
        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}