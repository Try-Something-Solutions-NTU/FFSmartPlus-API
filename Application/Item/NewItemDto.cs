namespace Application.Item;

public class NewItemDto
{
    public string Name { get; set; }
    public string UnitDesc { get; set; }
    public double minimumStock { get; set; }

    
    public long SupplierId { get; set; }
}