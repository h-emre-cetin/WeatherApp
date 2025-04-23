using Microsoft.EntityFrameworkCore;
using WeatherApp.Core.Models;

namespace WeatherApp.Infrastructure.Data
{
    public class WeatherDbContext : DbContext
    {
        public WeatherDbContext(DbContextOptions<WeatherDbContext> options)
            : base(options)
        {
        }

        public DbSet<WeatherData> WeatherData { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WeatherData>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CityName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.ZipCode).HasMaxLength(20);
                entity.Property(e => e.Description).HasMaxLength(255);
                entity.Property(e => e.Icon).HasMaxLength(50);

                // Create indexes for faster queries
                entity.HasIndex(e => e.CityName);
                entity.HasIndex(e => e.ZipCode);
                entity.HasIndex(e => e.RetrievedAt);
            });
        }
    }
}
