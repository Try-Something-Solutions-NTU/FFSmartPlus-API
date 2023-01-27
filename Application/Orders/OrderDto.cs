namespace Application.Orders;

public class OrderDto
{
    public long Id { get; set; }
    public long? ItemId { get; set; }
    public long? SupplierId { get; set; }
    public DateTime orderDate { get; set; }
    public double Quantity { get; set; }
    public double? actualDelivered { get; set; }
    public bool OrderDelivered { get; set; }
    public DateTime? DeliverDate { get; set; }
    
}