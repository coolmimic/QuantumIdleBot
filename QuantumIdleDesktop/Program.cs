using QuantumIdleDesktop.Forms;
using QuantumIdleDesktop.Services;
using QuantumIdleDesktop.Views.DrawRules;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuantumIdleDesktop
{
    internal static class Program
    {
        // 定义日志文件路径
        private static readonly string LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash.log");

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void  Main()
        {
            // 1. 设置全局异常捕获 (必须在所有代码之前)
            SetupGlobalExceptionHandling();

            // 2. 初始化应用配置 (高DPI支持等)
            ApplicationConfiguration.Initialize();

            try
            {
                // 3. 检查自动更新 (强制模式)
                // 逻辑：如果 CheckAsync 返回 true，说明已启动 Updater.exe，主程序只需静默退出即可
                var updater = new UpdateChecker();
                bool isUpdating = updater.CheckAsync().GetAwaiter().GetResult();

                if (isUpdating)
                {
                    // 正在更新，直接结束主进程，释放文件占用
                    return;
                }

                // 4. 启动主登录窗体
                Application.Run(new FormLogin());
            }
            catch (Exception ex)
            {
                // 捕获 Main 函数内部的漏网之鱼
                LogFatalError(ex);
            }
        }

        // =========================================================
        // 异常处理逻辑
        // =========================================================

        private static void SetupGlobalExceptionHandling()
        {
            // 设置异常处理模式：CatchException 表示由我们代码处理，而不是直接弹系统那个丑陋的错误框
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            // 1. 捕获 UI 线程异常 (最常见，比如点击按钮报错)
            Application.ThreadException += (sender, e) =>
            {
                LogFatalError(e.Exception);
            };

            // 2. 捕获非 UI 线程异常 (后台 Task 报错)
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                if (e.ExceptionObject is Exception ex)
                {
                    LogFatalError(ex);
                }
            };

            // 3. 捕获 Task 内部未观察到的异常
            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                LogFatalError(e.Exception);
                e.SetObserved(); // 标记为已处理，防止程序崩溃
            };
        }

        /// <summary>
        /// 记录错误日志并提示用户
        /// </summary>
        private static void LogFatalError(Exception ex)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("--------------------------------------------------");
                sb.AppendLine($"时间: {DateTime.Now}");
                sb.AppendLine($"类型: {ex.GetType().Name}");
                sb.AppendLine($"信息: {ex.Message}");
                sb.AppendLine("堆栈:");
                sb.AppendLine(ex.StackTrace);

                if (ex.InnerException != null)
                {
                    sb.AppendLine("--- 内部异常 ---");
                    sb.AppendLine(ex.InnerException.ToString());
                }
                sb.AppendLine("--------------------------------------------------");
                sb.AppendLine("");

                // 写入文件 (追加模式)
                File.AppendAllText(LogPath, sb.ToString());

                // 弹窗提示用户 (生产环境建议弹窗，让用户知道发生了什么)
                // 如果是静默崩溃，用户会以为没点开
                MessageBox.Show(
                    $"程序遇到严重错误，已停止运行。\n\n错误信息: {ex.Message}\n\n详情请查看目录下的 crash.log 文件。",
                    "系统崩溃",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                // 强制退出 (根据需要，如果是严重错误建议退出，防止数据损坏)
                Application.Exit();
            }
            catch
            {
                // 如果连写日志都报错（比如磁盘满了），那就真的没办法了，直接静默退出
            }
        }
    }
}