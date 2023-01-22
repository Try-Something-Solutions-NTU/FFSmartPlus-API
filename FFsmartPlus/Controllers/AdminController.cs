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
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FFsmartPlus.Controllers;

[Authorize(Roles = UserRoles.Admin)]
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
    [HttpGet("Expiry")]
    public async Task<ActionResult<List<UnitListDto>>> GetExpiredItems()
    {
        var expiredItems = ExpiredItems();
        return _mapper.Map<List<UnitListDto>>(expiredItems);
    }
    /// <summary>
    /// Runs End of day operations
    /// </summary>
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

    [HttpGet("Audit")]
    public async Task<ActionResult> AuditGeneration(int history)
    {
        var auditUnits = _context.AuditUnits
            .Include(au => au.Item)
            .OrderBy(au => au.EventDateTime)
            .Where(au => au.ExpiryDate <= DateTime.Today.AddDays(-history))
            .ToList();

        // Generate report
        var report = new List<string>();
        report.Add("ID, Quantity, Expiry Date, Item, Activity, Event Date, User");
        foreach (var auditUnit in auditUnits)
        {
            var row = $"{auditUnit.Id}, {auditUnit.Quantity}, {auditUnit.ExpiryDate.ToShortDateString()}, {auditUnit.Item.Name}, {auditUnit.Activity}, {auditUnit.EventDateTime.ToShortDateString()}, {auditUnit.UserName}";
            report.Add(row);
        }
        return Ok(report);
        // returns as a CSV, Setup for download?
    }
    [HttpGet("Audit/{id}")]
    public async Task<ActionResult> AuditGeneration(long id, int history)
    {
        if (await _context.Items.FindAsync(id) is null)
        {
            return NotFound();
        }
        var auditUnits = _context.AuditUnits
            .Include(au => au.Item)
            .OrderBy(au => au.EventDateTime)
            .Where(au => au.Item.Id.Equals(id) && au.ExpiryDate <= DateTime.Today.AddDays(-history))
            .ToList();

        // Generate report
        var report = new List<string>();
        report.Add("ID, Quantity, Expiry Date, Item, Activity, Event Date, User");
        foreach (var auditUnit in auditUnits)
        {
            var row = $"{auditUnit.Id}, {auditUnit.Quantity}, {auditUnit.ExpiryDate.ToShortDateString()}, {auditUnit.Item.Name}, {auditUnit.Activity}, {auditUnit.EventDateTime.ToShortDateString()}, {auditUnit.UserName}";
            report.Add(row);
        }

        return Ok(report);
        // Do something with the report
    
    }

    private async Task<List<Unit>> ExpiredItems()
    {
       return _context.Units.Where(x => x.ExpiryDate <= DateTime.Today).ToList();
    }
}