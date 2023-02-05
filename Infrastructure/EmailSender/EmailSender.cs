using System.Text;
using Application.Orders;
using System.Text.Json;

namespace Infrastructure.EmailSender;

public class EmailSender : IEmailSender
{
    private  readonly  string url =  
    "https://prod-09.uksouth.logic.azure.com:443/workflows/1b83110a720d486a86ae31e943a18e88/triggers/manual/paths/invoke?api-version=2016-10-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=spDgcdN50E4sHH8gWQ8fy1qcEUxbKxDqSMLSAIzIDJg";

    private readonly HttpClient _httpClient = new HttpClient();
    public async Task<bool> SendOrderEmails(IEnumerable<OrderEmailRequest> orders)
    {
               if (orders is null)
            return false;
        foreach (var order in orders)
        {
            if (!IsValid(order))
                return false;
                
            var body = new EmailRequest()
                { email = order.Email, SupplierName = order.Name, body = buildMessage(order) };
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(url),
                Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
            };
            // Comment out to stop from sending emails
            var response = await _httpClient.SendAsync(request); 
        }

        return true;
    }

    public async Task<bool> SendEmail(string content, string email, string name)
    {
        if (!IsValidEmail(email))
            return false;
        if (content is null || email is null || name is null)
            return false;
        var body = new EmailRequest()
            { email = email, SupplierName = name, body = content };
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(url),
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
        };

        var response = await _httpClient.SendAsync(request);
        return true;
    }
    

    private string buildMessage(OrderEmailRequest order)
    {
        StringBuilder sb = new StringBuilder();
        
            sb.AppendLine($"Supplier ID: {order.supplierId}<br>");
            sb.AppendLine($"Supplier Name: {order.Name}<br>");
            sb.AppendLine($"Supplier Address: {order.Address}<br>");
            sb.AppendLine($"Supplier Email: {order.Email}<br>");
            sb.AppendLine($"OTP: {order.DoorCode}<br>");

            sb.AppendLine("Order Details:<br>");
            sb.AppendLine("----------------<br>");
            foreach (var item in order.Orders)
            {
                sb.AppendLine($"Order Id: {item.Id}<br>");
                sb.AppendLine($"Name: {item.Name}<br>");
                sb.AppendLine($"Unit Description: {item.UnitDesc}<br>");
                sb.AppendLine($"Quantity: {item.OrderQuantity}<br>");
                sb.AppendLine();
            }
            sb.AppendLine();
        

        return sb.ToString();
    }
    
    private bool IsValid(OrderEmailRequest request)
    {
        if (!IsValidEmail(request.Email))
            return false;
        if (string.IsNullOrEmpty(request.Name ?? "") || 
            string.IsNullOrEmpty(request.Address ?? "") || 
            string.IsNullOrEmpty(request.Email ?? "")
            )
        {
            return false;
        }
        else
        {
            foreach (var item in request.Orders)
            {
                if (string.IsNullOrEmpty(item.Name ?? "") || 
                    string.IsNullOrEmpty(item.UnitDesc ?? ""))
                {
                    return false;
                }
            }
            return true;
        }
    }
    private bool IsValidEmail(string email)
    {
        var trimmedEmail = email.Trim();

        if (trimmedEmail.EndsWith(".")) {
            return false; // suggested by @TK-421
        }
        try {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == trimmedEmail;
        }
        catch {
            return false;
        }
    }
}