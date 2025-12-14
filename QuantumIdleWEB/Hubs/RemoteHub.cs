using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace QuantumIdleWEB.Hubs
{
    public class RemoteHub : Hub
    {
        //8555756240:AAE-1-zAmlLGSDjGjgWcgKjRWNEsgQm9ZhA

        // 客户端连接后，调用此方法注册自己的 UserId
        // 这样服务器就知道哪个连接属于哪个用户
        public async Task RegisterClient(string userId)
        {
            // 将当前连接 ID 加入到以 userId 命名的组中
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);

            // (可选) 通知客户端注册成功
            await Clients.Caller.SendAsync("ReceiveLog", $"服务器：用户 {userId} 注册成功，通道建立。");
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // 自动处理断线逻辑
            await base.OnDisconnectedAsync(exception);
        }
    }
}
