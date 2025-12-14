using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace QuantumIdleWEB.Data
{

    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // 1. 获取 appsettings.json 配置
            IConfigurationRoot configuration = new ConfigurationBuilder()
                // 设置项目根目录，以便找到 appsettings.json
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            // 2. 从配置中读取连接字符串
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();

            // 3. 配置使用 SQL Server
            builder.UseSqlServer(connectionString);

            // 4. 返回 DbContext 实例
            return new ApplicationDbContext(builder.Options);
        }
    }
}

