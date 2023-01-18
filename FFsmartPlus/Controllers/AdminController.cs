using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.Item;
using Application.Unit;
using AutoMapper;
using Domain;
using Infrastructure;
using Infrastructure.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FFsmartPlus.Controllers;

[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly FridgeAppContext _context;
    private readonly IMapper _mapper;
    public AdminController(FridgeAppContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    /// <summary>
    /// Get Expired Items
    /// </summary>
    [Authorize(Roles = UserRoles.Chef)]
    [Authorize(Roles = UserRoles.Admin)]
    [Authorize(Roles = UserRoles.HeadChef)]

    [HttpGet("Expiry")]
    public async Task<ActionResult<List<UnitListDto>>> GetExpiredItems()
    {
        var expiredItems = ExpiredItems();
        return _mapper.Map<List<UnitListDto>>(expiredItems);
    }
    /// <summary>
    /// Runs End of day operations
    /// </summary>
    [Authorize(Roles = UserRoles.Admin)]
    [Authorize(Roles = UserRoles.HeadChef)]
    [HttpDelete("EndOfDay")]
    public async Task<ActionResult> EndOfDay()
    {
        var expiredItems = await ExpiredItems();
        foreach (var expiredItem in expiredItems)
        {
            AuditUnit auditUnit = new AuditUnit()
            {
                EventDateTime = DateTime.Now,
                Activity = Activity.removed,
                Quantity = expiredItem.Quantity,
                ExpiryDate = expiredItem.ExpiryDate,
                ItemId = expiredItem.ItemId,
                Item = expiredItem.Item,
                UserName = User.Identity.Name
            };
            _context.AuditUnits.Add(auditUnit);
        }

        var UnitsToRemove = _context.Units.Where(x => x.ExpiryDate <= DateTime.Today);
        _context.Units.RemoveRange(UnitsToRemove);
        _context.SaveChangesAsync();
        return NoContent();
    }

    private async Task<List<Unit>> ExpiredItems()
    {
       return _context.Units.Where(x => x.ExpiryDate <= DateTime.Today).ToList();
    }
}