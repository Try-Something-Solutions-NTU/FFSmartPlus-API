namespace Domain;

public class AuditDoor
{
    public long Id { get; set; }
    public long? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public DateTime EventTime { get; set; }
}