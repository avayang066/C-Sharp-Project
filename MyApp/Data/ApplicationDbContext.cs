using Microsoft.EntityFrameworkCore;
using MyApp.Models;

namespace MyApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // 無參數建構函式供 migration 使用
        public ApplicationDbContext() { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // 使用 SQLite
                optionsBuilder.UseSqlite("Data Source=MyAppDB.db");
            }
        }

        public DbSet<Machine> Machines { get; set; }
        public DbSet<AlarmEvent> AlarmEvents { get; set; }
        public DbSet<ProductionLog> ProductionLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder
                .Entity<MyApp.Models.AlarmEvent>()
                .HasOne(a => a.ProductionLog)
                .WithMany()
                .HasForeignKey(a => a.ProductionLogId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
