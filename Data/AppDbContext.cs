// Data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using AvitoClone.Models;


namespace AvitoClone.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

        // Таблицы в БД
        public DbSet<Ad> Ads { get; set; }
        public DbSet<Category> Categories { get; set; }

        // Настройка связей (опционально)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Ad>()
                .HasOne(a => a.Category)
                .WithMany(c => c.Ads)
                .HasForeignKey(a => a.CategoryId);
        }
    }
}