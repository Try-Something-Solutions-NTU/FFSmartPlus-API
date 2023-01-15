using Application.Item;

namespace Application.Orders;

public class ActiveOrderDto
{
    public long Id { get; set; }
    public  string Name { get; set; }
    public string UnitDescription { get; set; }
    public double QuantityOrdered { get; set; }
}

public class ActiveOrdersDto
{
    public DateTime orderDate { get; set; }

    public List<ActiveOrderDto> orders { get; set; }
}