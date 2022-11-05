using Domain;

namespace Application.Item;

public class CurrentStockDto
{
    public ItemDto item { get; set; }
    public double currentQuantity { get; set; }
}
