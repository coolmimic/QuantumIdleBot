using System;
using System.Collections.Generic;
using System.Text;

namespace QuantumIdleDesktop
{
    public static class ControlExtensions
    {
        /// <summary>
        /// 线程安全的 UI 更新操作 (异步，不阻塞后台线程)
        /// </summary>
        public static void UIThread(this Control control, Action code)
        {
            if (control.IsDisposed || !control.IsHandleCreated) return;

            if (control.InvokeRequired)
            {
                control.BeginInvoke(code);
            }
            else
            {
                code.Invoke();
            }
        }

        /// <summary>
        /// 线程安全的 UI 更新操作 (同步，等待 UI 更新完毕才继续)
        /// 一般用于需要立即获取 UI 状态的场景
        /// </summary>
        public static void UIThreadSync(this Control control, Action code)
        {
            if (control.IsDisposed || !control.IsHandleCreated) return;

            if (control.InvokeRequired)
            {
                control.Invoke(code);
            }
            else
            {
                code.Invoke();
            }
        }
    }
}
