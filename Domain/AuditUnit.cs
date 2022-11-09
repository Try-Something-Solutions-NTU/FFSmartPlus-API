namespace Domain;

public class AuditUnit
{
    public long Id { get; set; }
    public double Quantity { get; set; }
    public DateTime ExpiryDate { get; set; }
    public long ItemId { get; set; }
    public Item Item { get; set; }
    public Activity Activity { get; set; }
    public DateTime EventDateTime { get; set; }
    
    public string UserName { get; set; }
}

public enum Activity
{
    Added,
    removed,
}