using Application.Item;

namespace Application.Orders;

public class SupplierOrderDto
{
    public List<OrderItemDto> Orders { get; set; }
    public long supplierId { get; set; }
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? Email { get; set; }
    public long DoorCode { get; set; }
}