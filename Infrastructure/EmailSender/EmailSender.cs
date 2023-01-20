using System.Text;
using Application.Orders;
using System.Text.Json;

namespace Infrastructure.EmailSender;

public class EmailSender : IEmailSender
{
    private HttpClient _httpClient = new HttpClient();
    public async Task<bool> SendOrderEmails(IEnumerable<OrderEmailRequest> orders)
    {
        var url =
            "https://prod-09.uksouth.logic.azure.com:443/workflows/1b83110a720d486a86ae31e943a18e88/triggers/manual/paths/invoke?api-version=2016-10-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=spDgcdN50E4sHH8gWQ8fy1qcEUxbKxDqSMLSAIzIDJg";
        foreach (var order in orders)
        {
            var body = new EmailRequest()
                { email = order.Email, SupplierName = order.Name, body = buildMessage(order) };
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(url),
                Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
            };

            var response = await _httpClient.SendAsync(request); 
        }

        return true;
    }

    private string buildMessage(OrderEmailRequest order)
    {
        StringBuilder sb = new StringBuilder();
        
            sb.AppendLine($"Supplier ID: {order.supplierId}<br>");
            sb.AppendLine($"Supplier Name: {order.Name}<br>");
            sb.AppendLine($"Supplier Address: {order.Address}<br>");
            sb.AppendLine($"Supplier Email: {order.Email}<br>");
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
}