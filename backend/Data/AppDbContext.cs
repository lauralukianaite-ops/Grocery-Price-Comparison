using backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Item> Items { get; set; }
    public DbSet<Price> Prices { get; set; }
    public DbSet<Store> Stores { get; set; }
    public DbSet<StoreLocation> StoreLocations { get; set; }
    public DbSet<Basket> Baskets { get; set; }
    public DbSet<BasketItem> BasketItems { get; set; }
    public DbSet<BasketRequestItem> BasketRequestItems { get; set; }
    public DbSet<BannedItem> BannedItems { get; set; }

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
            .Property(l => l.Address).HasMaxLength(300);
        modelBuilder.Entity<StoreLocation>()
            .HasOne(l => l.Store)
            .WithMany(s => s.Locations)
            .HasForeignKey(l => l.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Basket>()
            .Property(b => b.StoreName).HasMaxLength(100);
        modelBuilder.Entity<Basket>()
            .Property(b => b.Total).HasPrecision(10, 2);

        modelBuilder.Entity<BasketItem>()
            .Property(i => i.UnitCost).HasPrecision(10, 2);
        modelBuilder.Entity<BasketItem>()
            .Property(i => i.RowTotal).HasPrecision(10, 2);
        modelBuilder.Entity<BasketItem>()
            .HasOne(i => i.Basket)
            .WithMany(b => b.Items)
            .HasForeignKey(i => i.BasketId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BasketRequestItem>()
            .HasOne(i => i.Basket)
            .WithMany(b => b.RequestItems)
            .HasForeignKey(i => i.BasketId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BannedItem>()
            .HasOne(i => i.Basket)
            .WithMany(b => b.BannedItems)
            .HasForeignKey(i => i.BasketId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
