namespace backend.Entities;

public class Item
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Price> Prices { get; set; } = new();
}