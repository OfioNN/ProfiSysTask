using Microsoft.EntityFrameworkCore;
using ProfiSysTask.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProfiSysTask.Infrastructure.DataAccess {
    public class AppDbContext : DbContext {

        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentItem> DocumentItems { get; set; }

        public AppDbContext() {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            optionsBuilder.UseSqlite("Data Source=ProfiSysData.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);
        }
    }
}
