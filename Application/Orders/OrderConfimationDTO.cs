using Application.Unit;

namespace Application.Orders;

public class OrderConfirmationDTO
{
    public long OrderLogId { get; set; }
    public NewUnitDto unitDeliver { get; set; }
    
}