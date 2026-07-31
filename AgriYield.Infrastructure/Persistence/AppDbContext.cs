// Entity Framework Core veritabanı bağlamı — sera, sensör ve tahmin tablolarını yönetir.
using AgriYield.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgriYield.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Sera (greenhouse) ana tablosu.
    public DbSet<Greenhouse> Greenhouses => Set<Greenhouse>();
    // IoT sensörlerinden gelen telemetri kayıtları.
    public DbSet<SensorData> SensorLogs => Set<SensorData>();
    // ML modeli tarafından üretilen verim ve hastalık riski tahminleri.
    public DbSet<YieldPrediction> Predictions => Set<YieldPrediction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // SensorData → Greenhouse bire-çok ilişkisi (Foreign Key: GreenhouseId).
        modelBuilder.Entity<SensorData>()
            .HasOne(s => s.Greenhouse)
            .WithMany(g => g.SensorLogs)
            .HasForeignKey(s => s.GreenhouseId);

        // YieldPrediction → Greenhouse bire-çok ilişkisi (Foreign Key: GreenhouseId).
        modelBuilder.Entity<YieldPrediction>()
            .HasOne(p => p.Greenhouse)
            .WithMany(g => g.Predictions)
            .HasForeignKey(p => p.GreenhouseId);
    }
}
