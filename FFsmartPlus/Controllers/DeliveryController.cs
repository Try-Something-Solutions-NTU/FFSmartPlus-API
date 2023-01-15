using Application.Item;
using Application.Orders;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Domain;
using FluentValidation.Results;
using Infrastructure;

namespace FFsmartPlus.Controllers;
[Route("api/[controller]")]
[ApiController]
public class DeliveryController : ControllerBase
{
    private readonly FridgeAppContext _context;
    private readonly IMapper _mapper;

    public DeliveryController(FridgeAppContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet("{id}/{date}")]
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

}