namespace Application.Orders;

public class OrderEmailRequest
{
    public List<OrderItem> Orders { get; set; }
    public long supplierId { get; set; }
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? Email { get; set; }
}
public class OrderItem
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public string? UnitDesc { get; set; }
    public double OrderQuantity { get; set; }
}
