# Docker 一键部署（最简单）

## 服务器准备

在 Ubuntu 服务器上执行：

```bash
# 安装 Docker
curl -fsSL https://get.docker.com | sh

# 安装 Docker Compose
sudo apt install -y docker-compose
```

---

## 部署步骤

### 1. 上传项目到服务器

把整个项目文件夹上传到服务器的 `/opt/quantumidle/`

可以用 **scp** 或 **FileZilla**：
```bash
# 本地执行（Windows PowerShell）
scp -r c:\mywork\Antigravity\QuantumIdleBot root@你的服务器IP:/opt/quantumidle
```

### 2. 启动服务

```bash
# SSH 登录服务器后
cd /opt/quantumidle

# 一键启动所有服务！
docker-compose up -d --build
```

### 3. 完成！

访问 `http://你的服务器IP` 即可

---

## 常用命令

```bash
# 查看运行状态
docker-compose ps

# 查看日志
docker-compose logs -f web
docker-compose logs -f mobile

# 重启服务
docker-compose restart

# 停止服务
docker-compose down

# 更新代码后重新部署
git pull  # 或重新上传代码
docker-compose up -d --build
```

---

## 修改密码

编辑 `docker-compose.yml` 中的 `SA_PASSWORD`，然后重新启动：
```bash
docker-compose down
docker-compose up -d --build
```
