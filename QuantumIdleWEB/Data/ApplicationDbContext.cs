using Microsoft.EntityFrameworkCore;
using QuantumIdleModels.Entities;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace QuantumIdleWEB.Data
{
    public class ApplicationDbContext : DbContext
    {

        // 构造函数：将配置选项传递给基类
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ======================================================
        // 数据集映射 (对应数据库中的表)
        // ======================================================

        /// <summary>
        /// 用户表
        /// </summary>
        public DbSet<AppUser> Users { get; set; }

        /// <summary>
        /// 卡密库存表 (之前定义的 CardKey)
        /// </summary>
        public DbSet<CardKey> CardKeys { get; set; }

        /// <summary>
        /// 卡密使用记录表 (之前定义的 CardUsageLog)
        /// </summary>
        public DbSet<CardUsageLog> CardUsageLogs { get; set; }

        /// <summary>
        /// 注单表 (之前定义的 BetOrder)
        /// </summary>
        public DbSet<BetOrder> BetOrders { get; set; }


        public DbSet<PaymentOrder> PaymentOrders { get; set; }

        /// <summary>
        /// 方案表
        /// </summary>
        public DbSet<Scheme> Schemes { get; set; }

        /// <summary>
        /// Telegram 群组表
        /// </summary>
        public DbSet<TelegramChat> TelegramChats { get; set; }

        // ======================================================
        // 可以在这里进行额外的模型配置 (Fluent API)
        // ======================================================
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 示例：设置 User 表的 UserName 唯一索引，防止重复注册
            modelBuilder.Entity<AppUser>()
                .HasIndex(u => u.UserName)
                .IsUnique();

            // 示例：设置 CardKey 表的 KeyCode 唯一索引
            modelBuilder.Entity<CardKey>()
                .HasIndex(c => c.KeyCode)
                .IsUnique();

            // 如果你的金额字段涉及计算，建议明确指定精度，防止警告
            modelBuilder.Entity<BetOrder>()
               .Property(b => b.Amount)
               .HasColumnType("decimal(18,4)");


            modelBuilder.Entity<BetOrder>()
                .Property(b => b.Profit)
                .HasColumnType("decimal(18,4)");

            // Scheme 表配置
            modelBuilder.Entity<Scheme>()
                .HasIndex(s => new { s.UserId, s.SchemeId })
                .IsUnique();

            modelBuilder.Entity<Scheme>()
                .Property(s => s.StopProfitAmount)
                .HasColumnType("decimal(18,4)");

            modelBuilder.Entity<Scheme>()
                .Property(s => s.StopLossAmount)
                .HasColumnType("decimal(18,4)");

            // AppUser 盈亏字段精度配置
            modelBuilder.Entity<AppUser>()
                .Property(u => u.Profit)
                .HasColumnType("decimal(18,4)");
            modelBuilder.Entity<AppUser>()
                .Property(u => u.Turnover)
                .HasColumnType("decimal(18,4)");
            modelBuilder.Entity<AppUser>()
                .Property(u => u.SimProfit)
                .HasColumnType("decimal(18,4)");
            modelBuilder.Entity<AppUser>()
                .Property(u => u.SimTurnover)
                .HasColumnType("decimal(18,4)");

        }

    }
}
