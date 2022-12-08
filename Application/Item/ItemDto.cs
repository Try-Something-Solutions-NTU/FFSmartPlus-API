namespace Application.Item;

public class ItemDto
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public string? UnitDesc { get; set; }
    public double minimumStock { get; set; }
    public double desiredStock { get; set; }

    public long SupplierId { get; set; }
}