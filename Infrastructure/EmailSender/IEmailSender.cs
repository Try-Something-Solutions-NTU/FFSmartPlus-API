
using Application.Orders;

namespace Infrastructure.EmailSender;

public interface IEmailSender
{
    public Task<bool> SendOrderEmails(IEnumerable<OrderEmailRequest> orders);
    public Task<bool> SendEmail(string content, string email, string name);
}