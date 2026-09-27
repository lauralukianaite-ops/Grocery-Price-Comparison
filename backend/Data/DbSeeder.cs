using backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public static class DbSeeder
{
    public static void Initialize(AppDbContext database)
    {
        database.Database.EnsureCreated();

        if (database.Stores.Any() || database.Items.Any() || database.Prices.Any())
        {
            return;
        }

        var stores = new List<Store>
        {
            new Store { Name = "Barbora" },
            new Store { Name = "Iki" },
        };

        database.Stores.AddRange(stores);
        database.SaveChanges();

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

        var prices = new List<Price>
        {
            new Price { StoreId = stores[0].Id, ItemId = items[0].Id, Cost = 1.49m, RetailCost = 1.80m, RecordedAt = DateTime.UtcNow },
            new Price { StoreId = stores[0].Id, ItemId = items[1].Id, Cost = 2.29m, RetailCost = 2.80m, RecordedAt = DateTime.UtcNow },
            new Price { StoreId = stores[0].Id, ItemId = items[2].Id, Cost = 0.89m, RetailCost = 1.20m, RecordedAt = DateTime.UtcNow },
            
            new Price { StoreId = stores[1].Id, ItemId = items[0].Id, Cost = 1.39m, RetailCost = null, RecordedAt = DateTime.UtcNow },
            new Price { StoreId = stores[1].Id, ItemId = items[1].Id, Cost = 2.19m, RetailCost = 2.80m, RecordedAt = DateTime.UtcNow },
            new Price { StoreId = stores[1].Id, ItemId = items[2].Id, Cost = 0.99m, RetailCost = null, RecordedAt = DateTime.UtcNow }
        };

        database.Prices.AddRange(prices);
        database.SaveChanges();
    }
}
