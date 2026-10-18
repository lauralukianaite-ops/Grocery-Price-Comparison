using backend.Enums;

namespace backend.Entities;

public class Basket
{
    public Guid Id { get; set; }
    public int StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
    public BasketStrictness Strictness { get; set; }
    public List<BasketItem> Items { get; set; } = new();
    public List<BasketRequestItem> RequestItems { get; set; } = new();
}
