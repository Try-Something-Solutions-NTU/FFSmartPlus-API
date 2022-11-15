using Application.Item;
using Application.Orders;
using AutoMapper;
using Domain;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FFsmartPlus.Controllers;

[Route("api/Orders/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly FridgeAppContext _context;
    private readonly IMapper _mapper;

    public OrdersController(FridgeAppContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    //Get: api/Orders/BelowMin
    [HttpGet("BelowMin")]
    public async Task<ActionResult<IEnumerable<ItemDto>>> GetItemsBelowMinStock()
    {
        var lowStockItems = await  GetItemBelowMiniumStock();
        return _mapper.Map<List<ItemDto>>(lowStockItems);
    }
    
    [HttpGet("BelowMin/GenerateOrder")]
    public async Task<ActionResult<IEnumerable<SupplierOrderDto>>> GetMinimumOrder()
    {
        var Orders = new List<SupplierOrderDto>();
        var lowStockItems =  await GetItemBelowMiniumStock();
        foreach (var item in lowStockItems)
        {
            var newOrderItem = new OrderItemDto()
            {
                Id = item.Id,
                Name = item.Name,
                UnitDesc = item.UnitDesc,
                OrderQuantity = item.minimumStock - await GetCurrentStock(item)
            };
            var exisitingSupplier = Orders.FirstOrDefault(x => x.supplierId.Equals(item.SupplierId));
            if (exisitingSupplier != null)
            {
                
                exisitingSupplier.Orders.Add(newOrderItem);
            }
            else
            {
                var newSupplier = new SupplierOrderDto()
                {
                    Orders = new List<OrderItemDto>(),
                    supplierId = item.SupplierId,
                    Name = item.Supplier.Name,
                    Address = item.Supplier.Address,
                    Email = item.Supplier.Email
                };
                newSupplier.Orders.Add(newOrderItem);
                Orders.Add(newSupplier);
            }
        }

        return Orders;
    }

    private async Task<double> GetCurrentStock(Item item)
    {
        await _context.Entry(item).Collection(i => i.Units).LoadAsync();
        return item.Units.Select(x => x.Quantity).Sum();

    }

    private async Task<List<Item>> GetItemBelowMiniumStock()
    {
        var items = await _context.Items.Where(x => x.Active.Equals(true)).Include(i => i.Supplier).ToListAsync();
        var lowStockItems = new List<Item>();
        foreach (var item in items)
        {
            
            await _context.Entry(item).Collection(i => i.Units).LoadAsync();
            var currentStock = item.Units.Select(x => x.Quantity).Sum();
            if (item.minimumStock >= currentStock)
            {
                lowStockItems.Add(item);
            }
            
        }
        return lowStockItems;
    }

}