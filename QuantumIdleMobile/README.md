# QuantumIdleMobile - 量子挂机移动版

## 项目说明

这是量子挂机系统的移动版网页应用，提供移动端友好的界面来管理注单、查看统计信息等。

## 功能特性

- ✅ 用户登录/登出
- ✅ 仪表板（显示账户信息和统计数据）
- ✅ 注单列表（查看和管理下注记录）
- ✅ 设置页面（配置 API 地址等）
- ✅ 响应式设计，适配移动设备

## 技术栈

- ASP.NET Core 10.0
- MVC 架构
- 响应式 CSS
- JavaScript (原生)

## 项目结构

```
QuantumIdleMobile/
├── Controllers/          # 控制器
│   └── HomeController.cs
├── Views/                # 视图
│   ├── Home/
│   │   ├── Index.cshtml      # 首页
│   │   ├── Login.cshtml      # 登录页
│   │   ├── Dashboard.cshtml # 仪表板
│   │   ├── Orders.cshtml    # 注单列表
│   │   └── Settings.cshtml  # 设置
│   └── Shared/
│       └── _Layout.cshtml    # 布局
├── wwwroot/              # 静态资源
│   ├── css/
│   │   └── mobile.css    # 移动版样式
│   └── js/
│       └── mobile.js     # 通用 JavaScript
└── Program.cs            # 程序入口
```

## 配置说明

### API 服务器地址

默认 API 服务器地址在 `appsettings.json` 中配置：

```json
{
  "ApiBaseUrl": "http://localhost:5000/api"
}
```

用户也可以在设置页面中自定义 API 地址。

## 运行项目

```bash
cd QuantumIdleMobile
dotnet run
```

访问地址：`http://localhost:5000`

## 与后端服务的关系

移动版通过 REST API 与 `QuantumIdleWEB` 后端服务通信：

- **用户认证**：`/api/user/login`
- **注单管理**：`/api/betorder/*`
- **Telegram 功能**：`/api/telegram/*`（由后端维护 Telegram 连接）

## 注意事项

1. 移动版不直接维护 Telegram 连接，所有 Telegram 相关操作都通过后端 API 完成
2. WinForm 桌面版继续由自身维护 Telegram 连接（不受影响）
3. 需要先在后端服务中登录并初始化 Telegram 客户端

