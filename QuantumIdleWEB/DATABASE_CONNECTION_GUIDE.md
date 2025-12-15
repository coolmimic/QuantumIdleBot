# 数据库连接配置指南

## 问题说明

如果看到以下错误：
```
查账服务异常: 在与 SQL Server 建立连接时出现与网络相关的或特定于实例的错误
```

这表示无法连接到数据库服务器。

## 解决方案

### 方案 1：连接远程数据库（推荐）

如果数据库在其他电脑上，请修改 `appsettings.json` 中的连接字符串：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=数据库服务器IP,1433;Database=QuantumIdleDB;User Id=sa;Password=你的密码;TrustServerCertificate=True;"
  }
}
```

**示例：**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=192.168.1.100,1433;Database=QuantumIdleDB;User Id=sa;Password=Shashen@168;TrustServerCertificate=True;"
  }
}
```

### 方案 2：检查本地 SQL Server

如果使用本地数据库，请确保：

1. **SQL Server 服务正在运行**
   ```powershell
   # 检查 SQL Server 服务状态
   Get-Service | Where-Object {$_.Name -like "*SQL*"}
   
   # 如果服务未运行，启动它
   Start-Service MSSQLSERVER
   ```

2. **SQL Server 已启用 TCP/IP 协议**
   - 打开 SQL Server Configuration Manager
   - 展开 "SQL Server 网络配置"
   - 启用 "TCP/IP" 协议
   - 重启 SQL Server 服务

3. **防火墙允许 SQL Server 端口（默认 1433）**

### 方案 3：测试数据库连接

可以使用以下命令测试连接：

```powershell
# 使用 sqlcmd（如果已安装）
sqlcmd -S 服务器地址,1433 -U sa -P 密码 -Q "SELECT @@VERSION"
```

### 方案 4：暂时禁用 TronMonitorService（如果不需要）

如果暂时不需要查账服务，可以在 `Program.cs` 中注释掉：

```csharp
// builder.Services.AddHostedService<TronMonitorService>();
```

## 常见连接字符串格式

### 本地数据库（默认实例）
```
Server=.;Database=QuantumIdleDB;User Id=sa;Password=密码;TrustServerCertificate=True;
```

### 本地数据库（命名实例）
```
Server=.\SQLEXPRESS;Database=QuantumIdleDB;User Id=sa;Password=密码;TrustServerCertificate=True;
```

### 远程数据库
```
Server=192.168.1.100,1433;Database=QuantumIdleDB;User Id=sa;Password=密码;TrustServerCertificate=True;
```

### 使用 Windows 身份验证（本地）
```
Server=.;Database=QuantumIdleDB;Integrated Security=True;TrustServerCertificate=True;
```

## 注意事项

1. **修改连接字符串后需要重启服务**
2. **确保数据库服务器允许远程连接**
3. **确保防火墙开放了 SQL Server 端口（默认 1433）**
4. **确保用户名和密码正确**


