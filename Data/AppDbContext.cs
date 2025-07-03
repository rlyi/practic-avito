// Data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using AvitoClone.Models;

namespace AvitoClone.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Ad> Ads { get; set; }
    }
}