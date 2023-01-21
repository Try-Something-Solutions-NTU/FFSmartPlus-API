using Application.Item;
using Application.Orders;
using Application.Supplier;
using AutoMapper;
using Domain;
using FFsmartPlus.Services;
using Infrastructure;
using Infrastructure.EmailSender;
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
    public readonly IEmailSender _EmailSender;

    public OrdersController(FridgeAppContext context, IMapper mapper, IStockService stockService, IEmailSender emailSender)
    {
        _context = context;
        _StockService = stockService;
        _mapper = mapper;
        _EmailSender = emailSender;
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
        var ordersConverted = new List<OrderEmailRequest>();
        foreach (var order in orders)
        {
            var ordersList = new List<OrderItem>();
            foreach (var o in order.Orders)
            {
                ordersList.Add( new OrderItem(){Id = o.Id,Name = o.Name, OrderQuantity = o.OrderQuantity, UnitDesc = o.UnitDesc});
            }
            ordersConverted.Add( new OrderEmailRequest(){Email = order.Email, Address = order.Address, Orders = ordersList, Name = order.Name, supplierId = order.supplierId});
        }
        try
        {
            _EmailSender.SendOrderEmails(ordersConverted);
        }
        catch
        {
            return BadRequest();
        }
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
        var ordersConverted = new List<OrderEmailRequest>();
        foreach (var order in orderRequest.Items)
        {
            try
            {
                CheckForDuplicateIds(orderRequest);
            }
            catch
            {
                return BadRequest("Duplicate IDs");
            }
            var item = await _context.Items.FindAsync(order.Id);
            if (item is null)
            {
                return BadRequest("Item not found");
            }

            var supplierOrder = ordersConverted.FirstOrDefault(x => x.supplierId == item.SupplierId);
            await _context.Entry(item).Reference(i => i.Supplier).LoadAsync();
            if (supplierOrder is null)
            {
                var neworder = new OrderEmailRequest()
                {
                    Address = item.Supplier.Address, 
                    Email = item.Supplier.Email, 
                    Name = item.Supplier.Name,
                    supplierId = item.SupplierId,
                    Orders = new List<OrderItem>
                    {
                        new OrderItem()
                        {
                            Id = item.Id,
                            OrderQuantity = order.quantity,
                            Name = item.Name,
                            UnitDesc = item.UnitDesc
                        }
                    }
                };
                ordersConverted.Add(neworder);
            }
            else
            {
                supplierOrder.Orders.Add(new OrderItem()
                {
                    Id = item.Id,
                    OrderQuantity = order.quantity,
                    Name = item.Name,
                    UnitDesc = item.UnitDesc
                });
            }
            try
            {
                _EmailSender.SendOrderEmails(ordersConverted);
            }
            catch
            {
                return BadRequest();
            }

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
    private void CheckForDuplicateIds(OrderRequestDto list)
    {
        var ids = new HashSet<long>();
        foreach (var obj in list.Items)
        {
            if (!ids.Add(obj.Id))
            {
                throw new Exception($"Duplicate ID found: {obj.Id}");
            }
        }
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