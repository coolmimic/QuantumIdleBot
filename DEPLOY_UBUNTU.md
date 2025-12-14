# Ubuntu 部署指南 - QuantumIdleBot

## 服务器配置
- 4H8G Ubuntu (22.04 LTS 推荐)
- SQL Server 2022

---

## 第一步：服务器初始化

```bash
# 更新系统
sudo apt update && sudo apt upgrade -y

# 安装必要工具
sudo apt install -y curl wget git unzip nginx
```

---

## 第二步：安装 SQL Server 2022

```bash
# 1. 导入 Microsoft GPG 密钥
curl https://packages.microsoft.com/keys/microsoft.asc | sudo tee /etc/apt/trusted.gpg.d/microsoft.asc

# 2. 添加仓库
sudo add-apt-repository "$(curl https://packages.microsoft.com/config/ubuntu/22.04/mssql-server-2022.list)"

# 3. 安装 SQL Server
sudo apt update
sudo apt install -y mssql-server

# 4. 配置 SQL Server（选择免费的 Express 版）
sudo /opt/mssql/bin/mssql-conf setup
# 选择 3) Express
# 设置 SA 密码（记住这个密码！）

# 5. 验证运行
systemctl status mssql-server

# 6. 安装命令行工具（可选，用于手动操作数据库）
curl https://packages.microsoft.com/config/ubuntu/22.04/prod.list | sudo tee /etc/apt/sources.list.d/mssql-release.list
sudo apt update
sudo ACCEPT_EULA=Y apt install -y mssql-tools18 unixodbc-dev
echo 'export PATH="$PATH:/opt/mssql-tools18/bin"' >> ~/.bashrc
source ~/.bashrc
```

---

## 第三步：安装 .NET 10

由于 .NET 10 是较新版本，apt 仓库可能暂未收录。推荐使用 **Snap** 或 **微软官方安装脚本**。

### 方法 A：使用 Snap（推荐，简单）

```bash
# 安装 .NET SDK 10.0
sudo snap install dotnet-sdk --channel=10.0/stable --classic

# 注册 dotnet 命令别名
sudo snap alias dotnet-sdk.dotnet dotnet

# 验证
dotnet --version
```

### 方法 B：使用官方安装脚本（通用）

如果 Snap 安装失败，可以使用脚本安装：

```bash
# 1. 下载安装脚本
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x dotnet-install.sh

# 2. 安装 .NET 10 SDK
./dotnet-install.sh --channel 10.0

# 3. 配置环境变量
echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
echo 'export PATH=$PATH:$HOME/.dotnet' >> ~/.bashrc
source ~/.bashrc

# 4. 验证
dotnet --version
```

---

## 第四步：上传并发布项目

### 方式一：直接上传源码（推荐）

```bash
# 1. 创建目录
sudo mkdir -p /var/www/quantumidle
sudo chown $USER:$USER /var/www/quantumidle

# 2. 上传项目（在本地 Windows 执行）
# 使用 scp 或 FileZilla 上传整个项目到 /var/www/quantumidle

# 3. 在服务器发布
cd /var/www/quantumidle/QuantumIdleWEB
dotnet publish -c Release -o /var/www/quantumidle/publish

cd /var/www/quantumidle/QuantumIdleMobile
dotnet publish -c Release -o /var/www/quantumidle/publish-mobile
```

### 方式二：本地发布后上传

```powershell
# 在 Windows 本地执行
cd c:\mywork\Antigravity\QuantumIdleBot
dotnet publish QuantumIdleWEB -c Release -o .\publish\web
dotnet publish QuantumIdleMobile -c Release -o .\publish\mobile

# 然后上传 publish 文件夹到服务器
```

---

## 第五步：配置数据库连接

编辑 appsettings.json（发布目录中）：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=QuantumIdleBot;User Id=sa;Password=你的SA密码;TrustServerCertificate=True;"
  }
}
```

---

## 第六步：创建 Systemd 服务

```bash
# 创建 WEB 服务
sudo nano /etc/systemd/system/quantumidle-web.service
```

写入以下内容：

```ini
[Unit]
Description=QuantumIdle WEB Service
After=network.target mssql-server.service

[Service]
WorkingDirectory=/var/www/quantumidle/publish
ExecStart=/usr/bin/dotnet /var/www/quantumidle/publish/QuantumIdleWEB.dll
Restart=always
RestartSec=10
SyslogIdentifier=quantumidle-web
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:5000

[Install]
WantedBy=multi-user.target
```

```bash
# 创建 Mobile 服务
sudo nano /etc/systemd/system/quantumidle-mobile.service
```

写入：

```ini
[Unit]
Description=QuantumIdle Mobile Service
After=network.target quantumidle-web.service

[Service]
WorkingDirectory=/var/www/quantumidle/publish-mobile
ExecStart=/usr/bin/dotnet /var/www/quantumidle/publish-mobile/QuantumIdleMobile.dll
Restart=always
RestartSec=10
SyslogIdentifier=quantumidle-mobile
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:5001

[Install]
WantedBy=multi-user.target
```

```bash
# 启动服务
sudo systemctl daemon-reload
sudo systemctl enable quantumidle-web quantumidle-mobile
sudo systemctl start quantumidle-web quantumidle-mobile

# 查看状态
sudo systemctl status quantumidle-web
sudo systemctl status quantumidle-mobile
```

---

## 第七步：配置 Nginx 反向代理

```bash
sudo nano /etc/nginx/sites-available/quantumidle
```

写入：

```nginx
server {
    listen 80;
    server_name 你的域名或IP;

    # Mobile 前端
    location / {
        proxy_pass http://localhost:5001;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    # API 后端
    location /api {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

```bash
# 启用配置
sudo ln -s /etc/nginx/sites-available/quantumidle /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx

# 防火墙
sudo ufw allow 80
sudo ufw allow 443
```

---

## 第八步：执行数据库迁移

```bash
cd /var/www/quantumidle/QuantumIdleWEB
dotnet ef database update
```

---

## 完成！

访问 `http://你的服务器IP` 即可看到应用。

## 常用命令

```bash
# 查看日志
sudo journalctl -u quantumidle-web -f
sudo journalctl -u quantumidle-mobile -f

# 重启服务
sudo systemctl restart quantumidle-web quantumidle-mobile

# 更新代码后重新发布
cd /var/www/quantumidle/QuantumIdleWEB
dotnet publish -c Release -o /var/www/quantumidle/publish
sudo systemctl restart quantumidle-web
```
