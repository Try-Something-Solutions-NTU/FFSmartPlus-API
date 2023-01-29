namespace Testing;

using Application.Orders;
using Infrastructure.EmailSender;
using RichardSzalay.MockHttp;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

public class EmailSenderTests
{
    private readonly IEmailSender _emailSender;
    private readonly HttpClient _fakeHttpClient;

    public EmailSenderTests()
    {
        var mockHttp = new MockHttpMessageHandler(); 
        _fakeHttpClient = mockHttp.ToHttpClient();
        _emailSender = new EmailSender();
    }
    

    [Fact]
    public async Task SendOrderEmails_ReturnsFalse_IfOrdersIsMissingDetails()
    {
        // Arrange
        var orders = new List<OrderEmailRequest>
        {
            new OrderEmailRequest { Email = "test@example.com", Name = "Test Supplier" }
        };

        // Act
        var result = await _emailSender.SendOrderEmails(orders);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SendOrderEmails_ReturnsFalse_InvalidEmail()
    {
        // Arrange
        var orders = new List<OrderEmailRequest>
        {
            new OrderEmailRequest { Email = "myInvalidEmail@@@aaaaa.g.com", Name = "Test Supplier" }
        };

        // Act
        var result = await _emailSender.SendOrderEmails(orders);

        // Assert
        Assert.False(result);
    }

    [Fact]
    
    public async Task SendOrderReturnsTrueIfValidRequest()
    {
        //Arrange
        var orders = new List<OrderEmailRequest>()
        {
            new OrderEmailRequest()
            {
                Email = "test@test.com", Name = "Unit Test Supplier", Orders = new List<OrderItem>()
                {
                    new OrderItem() { Id = 1, Name = "Apple", UnitDesc = "Per Apple", OrderQuantity = 12 }
                },
                Address = "Test address",
                supplierId = 1
            }
        };
        //Act
        var result = await _emailSender.SendOrderEmails(orders);
        
        //Assert
        Assert.True(result);
    }


    [Fact]
    public async Task SendOrderEmails_Should_Return_False_If_Orders_Is_Null()
    {
        // Arrange
        var emailSender = new EmailSender();

        // Act
        var result = await emailSender.SendOrderEmails(null);

        // Assert
        Assert.False(result);
    }
    
    
}