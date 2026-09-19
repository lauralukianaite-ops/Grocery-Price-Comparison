using backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public static class DbSeeder
{
    public static void Initialize(AppDbContext database)
    {
        database.Database.EnsureCreated();

        // If data already exists, don't seed again
        if (database.Stores.Any() || database.Items.Any() || database.Prices.Any())
        {
            return;
        }

        // Create stores
        var stores = new List<Store>
        {
            new Store { Name = "Barbora" },
            new Store { Name = "Iki" },
        };

        database.Stores.AddRange(stores);
        database.SaveChanges();

        // Create items
        var items = new List<Item>
        {
            new Item { Name = "Pienas 2.5% (1L)" },
            new Item { Name = "Pienas 2.5% (2L kartoninis pakelis)" },
            new Item { Name = "Duona šviesi" },
            new Item { Name = "Kefyras 2.5% (0.5L)" },
            new Item { Name = "Grietinė 30% (0.2L)" },
            new Item { Name = "Švieži kopūstai" },
            new Item { Name = "Vištienos šlaunelės" }
        };

        database.Items.AddRange(items);
        database.SaveChanges();

        // Create prices
        var prices = new List<Price>
        {
            new Price { StoreId = stores[0].Id, ItemId = items[0].Id, Amount = 1.49m, RecordedAt = DateTime.UtcNow },
            new Price { StoreId = stores[0].Id, ItemId = items[1].Id, Amount = 2.29m, RecordedAt = DateTime.UtcNow },
            new Price { StoreId = stores[0].Id, ItemId = items[2].Id, Amount = 0.89m, RecordedAt = DateTime.UtcNow },
            
            new Price { StoreId = stores[1].Id, ItemId = items[0].Id, Amount = 1.39m, RecordedAt = DateTime.UtcNow },
            new Price { StoreId = stores[1].Id, ItemId = items[1].Id, Amount = 2.19m, RecordedAt = DateTime.UtcNow },
            new Price { StoreId = stores[1].Id, ItemId = items[2].Id, Amount = 0.99m, RecordedAt = DateTime.UtcNow }
        };

        database.Prices.AddRange(prices);
        database.SaveChanges();
    }
}
