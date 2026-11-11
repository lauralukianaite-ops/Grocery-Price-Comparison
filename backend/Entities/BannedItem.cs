namespace backend.Entities;

public class BannedItem
{
    public int Id { get; set; }
    public int ItemId { get; set; } // the banned item id
    public Guid BasketId { get; set; }
    public Basket? Basket { get; set; }
}