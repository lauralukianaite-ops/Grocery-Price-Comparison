namespace backend.Entities;

public class Price
{
    public int Id { get; set; }
    public int StoreId { get; set; }
    public int ItemId { get; set; }
    public decimal Amount { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    public Store Store { get; set; }
    public Item Item { get; set; }
}