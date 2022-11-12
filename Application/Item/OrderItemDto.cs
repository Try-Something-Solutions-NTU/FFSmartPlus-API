namespace Application.Item;

public class OrderItemDto
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public string? UnitDesc { get; set; }
    public double OrderQuantity { get; set; }
}