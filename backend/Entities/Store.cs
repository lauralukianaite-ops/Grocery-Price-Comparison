namespace backend.Entities;

public class Store
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<Price> Prices { get; set; }
    
    //add address later for maps
}