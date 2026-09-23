namespace backend.Entities;

public class StoreLocation
{
    public int Id { get; set; }
    public int StoreId { get; set; }
    public Store? Store { get; set; }
    public string Address { get; set; } = string.Empty;
    public double? Latitude { get; set; } // not sure if those will be needed
    public double? Longitude { get; set; }
}
