using System.Globalization;
using System.Text.Json;
using backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public static class StoreLocationSeeder
{
    private static readonly List<(string Prefix, string StoreName)> Chains = new() 
    { 
        ("Maxima", "Maxima"), ("IKI", "Iki"), ("Rimi", "Rimi"),
        ("Promo", "PROMO Cash&Carry"), ("Livin", "Livin"), ("Assorti", "Assorti"),
        ("Ekomarket", "EkoMarket"), ("Mon ami", "Mon Ami"), ("Alkava", "ALKAVÀ"),
        ("Vyno klubas", "Vyno klubas"), ("Vyno kambarys", "Vyno kambarys"),
        ("Šerno pėda", "Šerno Pėda"), ("Steak Supply", "Steak Supply"),
        ("Tarandės", "Tarandės žuvų rūkykla")
    };
    
    public static async Task SeedAsync(AppDbContext context)
    { 
        if (await context.StoreLocations.AnyAsync()) return; 
        
        var stores = await context.Stores.ToListAsync(); 
        var locations = new List<StoreLocation>();
        
        using var document = JsonDocument.Parse(File.ReadAllText("Data/StoreLocations.geojson"));
        foreach (var item in document.RootElement.GetProperty("features").EnumerateArray()) 
        {
            var location = ToLocation(item, stores); //take location from json and add to locations
            if (location is not null) locations.Add(location); 
        }
        context.StoreLocations.AddRange(locations);
        await context.SaveChangesAsync();
    }
    
    private static StoreLocation? ToLocation(JsonElement feature, List<Store> stores)
    {
        var properties = feature.GetProperty("properties");
        
        var name = properties.GetProperty("name").GetString();
        if (name is null) return null;
        
        var coordinates = feature.GetProperty("geometry").GetProperty("coordinates");
        var longitude = coordinates[0].GetDouble();
        var latitude = coordinates[1].GetDouble();
        
        var city = properties.TryGetProperty("addr:city", out var cityElement) ? cityElement.GetString() : null;
        var street = properties.TryGetProperty("addr:street",  out var streetElement) ? streetElement.GetString() : null;
        var houseNumber = properties.TryGetProperty("addr:housenumber",  out var houseNumberElement) ? houseNumberElement.GetString() : null;
        
        var storeName = Chains
            .FirstOrDefault(c => name.StartsWith(c.Prefix, StringComparison.OrdinalIgnoreCase));
        if (storeName.StoreName is null) return null;
        
        var store = stores.FirstOrDefault(s => s.Name == storeName.StoreName);
        if (store is null) return null;
        
        return new StoreLocation()
        {
            Latitude = latitude,
            Longitude = longitude,
            Address = (houseNumber is null || street is null || city is null) ? 
                latitude.ToString(CultureInfo.InvariantCulture) + ", " + longitude.ToString(CultureInfo.InvariantCulture) : 
                street + ", " + houseNumber + ", " + city ,
            StoreId = store.Id
        };

    }
}