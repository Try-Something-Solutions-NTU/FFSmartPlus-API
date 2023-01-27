using Application.Item;
using Application.Orders;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Domain;
using FFsmartPlus.Services;
using FluentValidation.Results;
using Infrastructure;

namespace FFsmartPlus.Controllers;
[Route("api/[controller]")]
[ApiController]
public class DeliveryController : ControllerBase
{
    private readonly FridgeAppContext _context;
    private readonly IMapper _mapper;
    private readonly IStockService _stockService;


    public DeliveryController(FridgeAppContext context, IMapper mapper, IStockService stockService)
    {
        _context = context;
        _stockService = stockService;
        _mapper = mapper;
    }

    [HttpGet("{id}/{date}")]
    public async Task<ActionResult<ActiveOrdersDto>> GetActiveOrders(long id, DateTime date)
    {
        var orderLogs = _context.OrderLogs.Where(x => x.SupplierId == id && x.OrderDelivered ==false && x.orderDate.Date.DayOfYear == date.Date.DayOfYear && x.orderDate.Date.Year == date.Date.Year).Include(x => x.Item).ToList();
        var list = new List<ActiveOrderDto>();
        foreach (var order in orderLogs)
        {
            list.Add(new ActiveOrderDto()
                {
                    Id = order.Id,
                    Name = order.Item.Name,
                    QuantityOrdered = order.Quantity,
                    UnitDescription = order.Item.UnitDesc
                }
            );
        }

        return new ActiveOrdersDto() { orderDate = date, orders = list};
    }
    /// <summary>
    /// confirms item has been delivered and records it
    /// </summary>

    [HttpPut("Confirm")]
    public async Task<ActionResult<bool>> ConfirmDeliver(OrderConfirmationDTO confirmationDto)
    {
        //TODO change to use User.Identity.Name and error handleing 
        var username = "Nick";
        OrderLog order = await _context.OrderLogs.FindAsync(confirmationDto.OrderLogId);
        await _stockService.AddStock((long)order.ItemId, confirmationDto.unitDeliver, username);
        order.actualDelivered = confirmationDto.unitDeliver.Quantity;
        order.DeliverDate = DateTime.Now;
        order.OrderDelivered = true;
        _context.Entry(order).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return true;
    }

    [HttpGet("/OrdersByDate/{date}")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrdersByDate(DateTime date)
    {
        var orderLogs = _context.OrderLogs.Where(x => x.orderDate.Date.DayOfYear == date.Date.DayOfYear  ).Include(x => x.Item).ToList();
        if (orderLogs is null)
            return NotFound();
        var orders = new List<OrderDto>();
        foreach (var o in orderLogs)
        {
            orders.Add(_mapper.Map<OrderDto>(o));
        }
        return Ok(orders);
    }

}