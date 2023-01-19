using Application.Item;
using Application.Orders;
using AutoMapper;
using Domain;
using FFsmartPlus.Services;
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
    public readonly IStockService _StockService;

    public OrdersController(FridgeAppContext context, IMapper mapper, IStockService stockService)
    {
        _context = context;
        _StockService = stockService;
        _mapper = mapper;
    }
    /// <summary>
    /// Get list of items below minimum stock level
    /// </summary>
    //Get: api/Orders/BelowMin
    [HttpGet("BelowMin")]
    public async Task<ActionResult<IEnumerable<CurrentStockDto>>> GetItemsBelowMinStock()
    {
        var lowStockItems = await  GetItemBelowMiniumStock();
        var list = new List<CurrentStockDto>();
        foreach (var item in lowStockItems)
        {
            await _context.Entry(item).Collection(i => i.Units).LoadAsync();
            var currentStock = item.Units.Select(x => x.Quantity).Sum();
            var itemDto = _mapper.Map<ItemDto>(item);
            list.Add(new CurrentStockDto(){item = itemDto, currentQuantity = currentStock});
        }

        return list;
    }
    /// <summary>
    /// Generates an order of items below the minimum stock level
    /// </summary>
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
                OrderQuantity = item.desiredStock - await GetCurrentStock(item)
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

    [HttpPost("ConfirmOrder")]
    public async Task<ActionResult<bool>> ConfirmOrder(IEnumerable<SupplierOrderDto> orders)
    {
        foreach (var supplier in orders)
        {
            foreach (var order in supplier.Orders)
            {
                _context.OrderLogs.Add(new OrderLog()
                {
                    ItemId = order.Id,
                    SupplierId = supplier.supplierId,
                    orderDate = DateTime.Now,
                    OrderDelivered = false,
                    Quantity = order.OrderQuantity
                });
                await _context.SaveChangesAsync();
            }
        }

        return true;
    }
    [HttpPost("ConfirmOrderByIDs")]
    public async Task<ActionResult<bool>> ConfirmOrderByIDs(OrderRequestDto orderRequest)
    {
        foreach (var order in orderRequest.Items)
            {
                _context.OrderLogs.Add(new OrderLog()
                {
                    ItemId = order.Id,
                    //SupplierId = supplier.Id,
                    orderDate = DateTime.Now,
                    OrderDelivered = false,
                    Quantity = order.quantity
                });
                await _context.SaveChangesAsync();
            }

        return true;
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