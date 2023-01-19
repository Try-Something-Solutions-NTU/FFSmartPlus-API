namespace Application.Orders;

public class OrderRequestDto
{
    public List<ItemRequestDto> Items { get; set; }
}

public class ItemRequestDto
{
    public long Id { get; set; }
    public double quantity { get; set; }
}

