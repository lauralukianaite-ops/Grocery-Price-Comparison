using backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    public DbSet<Item> Items { get; set; }
    public DbSet<Price>  Prices { get; set; }
    public DbSet<Store> Stores { get; set; }
    public DbSet<StoreLocation> StoreLocations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Store>()
            .HasIndex(s => s.Name).IsUnique();
        modelBuilder.Entity<Store>()
            .Property(s => s.Name).HasMaxLength(100);
        modelBuilder.Entity<Price>()
            .Property(p => p.Cost).HasPrecision(10, 2);
        modelBuilder.Entity<Price>()
            .Property(p => p.RetailCost).HasPrecision(10, 2);
        modelBuilder.Entity<Price>()
            .HasIndex(p => new { p.ItemId, p.StoreId, p.RecordedAt });
        modelBuilder.Entity<StoreLocation>()
            .Property(l => l.Address).HasMaxLength(300);
        
        modelBuilder.Entity<Price>()
            .HasOne(p => p.Store)
            .WithMany(s => s.Prices)
            .HasForeignKey(p => p.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Price>()
            .HasOne(p => p.Item)
            .WithMany(i => i.Prices)
            .HasForeignKey(p => p.ItemId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<StoreLocation>()
            .HasOne(l => l.Store)
            .WithMany(s => s.Locations)
            .HasForeignKey(l => l.StoreId)
            .OnDelete(DeleteBehavior.Cascade);
    }

}