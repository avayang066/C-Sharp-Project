// EF Core 相關引用
using Microsoft.EntityFrameworkCore;
using MyApp.Models;

namespace MyApp.Data
{
    // 資料庫上下文：管理所有資料表與關聯
    public class ApplicationDbContext : DbContext
    {
        // --- 建構函式區 ---
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // 無參數建構函式供 migration 使用
        public ApplicationDbContext() { }

        // --- 資料庫連線設定 ---
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // 使用 SQLite
                optionsBuilder.UseSqlite("Data Source=MyAppDB.db");
            }
        }

        // --- 資料表對應 ---
        public DbSet<Machine> Machines { get; set; }
        public DbSet<AlarmEvent> AlarmEvents { get; set; }
        public DbSet<ProductionLog> ProductionLogs { get; set; }
        public DbSet<User> Users { get; set; }

        // --- 關聯與索引設定 ---
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // AlarmEvent 關聯 ProductionLog
            modelBuilder
                .Entity<AlarmEvent>()
                .HasOne(a => a.ProductionLog)
                .WithMany()
                .HasForeignKey(a => a.ProductionLogId)
                .OnDelete(DeleteBehavior.Restrict);

            // AlarmEvent 關聯 Machine
            modelBuilder
                .Entity<AlarmEvent>()
                .HasOne(a => a.Machine)
                .WithMany()
                .HasForeignKey(a => a.MachineId)
                .OnDelete(DeleteBehavior.Restrict);

            // ProductionLog 關聯 Machine
            modelBuilder
                .Entity<ProductionLog>()
                .HasOne(p => p.Machine)
                .WithMany()
                .HasForeignKey(p => p.MachineId)
                .OnDelete(DeleteBehavior.Restrict);

            // 重要索引
            modelBuilder.Entity<Machine>().HasIndex(x => x.MachineCode);
            modelBuilder.Entity<Machine>().HasIndex(x => x.IsActive);
            modelBuilder.Entity<AlarmEvent>().HasIndex(x => x.CreatedAt);
        }
    }
}
