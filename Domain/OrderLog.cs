namespace Domain;

public class OrderLog
{
    public long Id { get; set; }
    public long? ItemId { get; set; }
    public Item? Item { get; set; }
    public long? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public DateTime orderDate { get; set; }
    public double Quantity { get; set; }
    public double? actualDelivered { get; set; }
    public bool OrderDelivered { get; set; }
    public DateTime DeliverDate { get; set; }
    
}