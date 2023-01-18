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
using Infrastructure.Auth;
using Microsoft.AspNetCore.Authorization;

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
    [Authorize]
    public async Task<ActionResult<ActiveOrdersDto>> GetActiveOrders(long id, DateTime date)
    { 
        //TODO fix this \/
        var orderLogs = _context.OrderLogs.Where(x => x.SupplierId == id && x.OrderDelivered ==false && x.orderDate.Date.DayOfYear == date.Date.DayOfYear  ).Include(x => x.Item).ToList();
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
    [Authorize(Roles = UserRoles.Delivery)]
    [Authorize(Roles = UserRoles.Admin)]
    [Authorize(Roles = UserRoles.HeadChef)]
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

}