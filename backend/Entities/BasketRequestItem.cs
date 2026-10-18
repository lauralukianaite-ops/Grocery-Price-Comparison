namespace backend.Entities;

public class BasketRequestItem
{
    public int Id { get; set; }
    public Guid BasketId { get; set; }
    public int Position { get; set; }
    public int ItemId { get; set; }
    public int Quantity { get; set; }
    public Basket? Basket { get; set; }
}
