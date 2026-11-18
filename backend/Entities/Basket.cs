using backend.Enums;

namespace backend.Entities;

public class Basket
{
    public Guid Id { get; set; }
    public int StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public decimal GroceriesTotal { get; set; }
    public decimal TravelCost { get; set; }
    public double? DistanceKm { get; set; }
    public string? LocationAddress { get; set; }
    public DateTime CreatedAt { get; set; }
    public BasketStrictness Strictness { get; set; }
    public List<BasketItem> Items { get; set; } = new();
    public List<BasketRequestItem> RequestItems { get; set; } = new();
    public List<BannedItem> BannedItems { get; set; } = new();
}
