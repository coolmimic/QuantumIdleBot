namespace QuantumIdleUpdater
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // 开发调试用：如果没有参数，伪造一个（防止直接双击报错）
            if (args.Length == 0)
            {
                MessageBox.Show("请不要直接运行此程序，它由主程序自动调用。", "提示");
            }

            ApplicationConfiguration.Initialize();
            // 把参数传给窗体构造函数
            Application.Run(new FrmUpdater(args));
        }
    }
}