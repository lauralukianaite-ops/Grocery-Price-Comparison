using backend.DTOs;

namespace backend.Entities;

public class BasketItem
{
    public int Id { get; set; }
    public Guid BasketId { get; set; }
    public int Position { get; set; }
    public int ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal RowTotal { get; set; }
    public DiscountVerdict Verdict { get; set; }
    public Basket? Basket { get; set; }
}
