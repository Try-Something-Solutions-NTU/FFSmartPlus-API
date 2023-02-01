namespace Application.Audit;

public class AuditDto
{
    public long Id { get; set; }
    public double Quantity { get; set; }
    public DateTime ExpiryDate { get; set; }
    public long ItemId { get; set; }
    public DateTime EventDateTime { get; set; }
    public string UserName { get; set; }
}