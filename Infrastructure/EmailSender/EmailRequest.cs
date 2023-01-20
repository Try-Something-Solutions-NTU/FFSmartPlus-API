namespace Infrastructure.EmailSender;

public class EmailRequest
{
    public string email { get; set; }
    public string SupplierName { get; set; }
    public string body { get; set; }
}