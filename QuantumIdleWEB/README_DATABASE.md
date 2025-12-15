# 数据库配置说明

## 连接远程数据库

如果需要连接到其他电脑的数据库，请修改 `appsettings.json` 或 `appsettings.Development.json` 中的连接字符串。

### 连接字符串格式

#### SQL Server 连接字符串格式：

```
Server=服务器IP或主机名,端口;Database=数据库名;User Id=用户名;Password=密码;TrustServerCertificate=True;
```

### 配置示例

#### 1. 本地数据库（同一台电脑）
```json
"DefaultConnection": "Server=.;Database=QuantumIdleDB;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
```

#### 2. 远程数据库（其他电脑）
```json
"DefaultConnection": "Server=192.168.1.100,1433;Database=QuantumIdleDB;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
```

#### 3. 使用命名实例
```json
"DefaultConnection": "Server=192.168.1.100\\SQLEXPRESS,1433;Database=QuantumIdleDB;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
```

### 注意事项

1. **防火墙设置**：确保远程 SQL Server 的防火墙允许连接（默认端口 1433）
2. **SQL Server 配置**：确保 SQL Server 已启用 TCP/IP 协议
3. **SQL Server 身份验证**：确保使用 SQL Server 身份验证模式，或配置 Windows 身份验证
4. **网络连通性**：确保能够 ping 通远程服务器

### 测试连接

可以使用以下命令测试数据库连接：

```bash
# 使用 sqlcmd（如果已安装）
sqlcmd -S 192.168.1.100,1433 -U sa -P YourPassword -Q "SELECT @@VERSION"
```

### 常见问题

1. **连接超时**：检查网络连接和防火墙设置
2. **身份验证失败**：检查用户名和密码是否正确
3. **数据库不存在**：确保数据库名称正确，或先创建数据库


