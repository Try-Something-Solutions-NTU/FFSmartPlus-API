namespace Domain;

public class Unit
{
    public long Id { get; set; }
    public double Quantity { get; set; }
    public DateTime ExpiryDate { get; set; }
    public long ItemId { get; set; }
    public Item Item { get; set; }
}