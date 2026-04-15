using Microsoft.EntityFrameworkCore;
using ProfiSysTask.Core.Models;

namespace ProfiSysTask.Infrastructure.DataAccess {
    public class AppDbContext : DbContext {

        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentItem> DocumentItems { get; set; }

        public AppDbContext() {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            string dbFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "databases");

            if (!Directory.Exists(dbFolder)) Directory.CreateDirectory(dbFolder);

            string dbPath = Path.Combine(dbFolder, "ProfiSysData.db");

            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);
        }
    }
}
