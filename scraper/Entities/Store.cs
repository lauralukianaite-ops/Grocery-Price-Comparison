namespace backend.Entities;

public class Store
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Price> Prices { get; set; } = new();
    public List<StoreLocation> Locations { get; set; } = new();
}