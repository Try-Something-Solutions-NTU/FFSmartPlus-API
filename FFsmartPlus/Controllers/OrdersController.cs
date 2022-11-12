using Application.Item;
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
        var items = await _context.Items.Where(x => x.Active.Equals(true)).ToListAsync();
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
        return _mapper.Map<List<ItemDto>>(lowStockItems);
    }
    
}