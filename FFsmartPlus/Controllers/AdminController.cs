using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Application.Audit;
using Application.Item;
using Application.Unit;
using AutoMapper;
using Domain;
using Infrastructure;
using Infrastructure.Auth;
using Infrastructure.EmailSender;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FFsmartPlus.Controllers;
[Authorize(Roles = $"{UserRoles.Admin},{UserRoles.HeadChef}")]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly FridgeAppContext _context;
    private readonly IMapper _mapper;
    private readonly IEmailSender _emailSender;
    public AdminController(FridgeAppContext context, IMapper mapper, IEmailSender emailSender)
    {
        _context = context;
        _mapper = mapper;
        _emailSender = emailSender;
    }
    /// <summary>
    /// Get Expired Items
    /// </summary>
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.HeadChef}")]

    [HttpGet("Expiry")]
    public async Task<ActionResult<List<UnitListDto>>> GetExpiredItems()
    {
        var expiredItems = await ExpiredItems();
        var list = new List<UnitListDto>();
        foreach (var item in expiredItems)
        {
            list.Add(_mapper.Map<UnitListDto>(item));
        }
        return list;
    }
    /// <summary>
    /// Get Expired Items
    /// </summary>

    [HttpGet("ExpiryByDays")]
    public async Task<ActionResult<List<UnitListDto>>> GetExpiredItems(int days)
    {
        var expiredItems = await ExpiredItems(days);
        var list = new List<UnitListDto>();
        foreach (var item in expiredItems)
        {
            list.Add(_mapper.Map<UnitListDto>(item));
        }
        return list;
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
    public async Task<ActionResult<List<AuditDto>>> AuditGeneration(int history, string? email)
    {
        var auditUnits = _context.AuditUnits
            .Include(au => au.Item)
            .OrderBy(au => au.EventDateTime)
            .Where(au => au.EventDateTime <= DateTime.Today.AddDays(-history))
            .ToList();
        var list = new List<AuditDto>();
        foreach (var a in auditUnits)
        {
            list.Add(_mapper.Map<AuditDto>(a));
        }
        if (email is not null && list.Count != 0)
        {
            var data = GenerateEmail(list);
            var result = await _emailSender.SendEmail(data, email, "Audit");
            if (!result)
                return Problem("Error sending email");
        }
        return Ok(list);

    }
    [HttpGet("Audit/{id}")]
    public async Task<ActionResult<List<AuditDto>>> AuditGeneration(long id, int history)
    {
        if (await _context.Items.FindAsync(id) is null)
            return NotFound();
        var auditUnits = _context.AuditUnits
            .Include(au => au.Item)
            .OrderBy(au => au.EventDateTime)
            .Where(au => au.Item.Id.Equals(id) && au.EventDateTime <= DateTime.Today.AddDays(-history))
            .ToList();
        var list = new List<AuditDto>();
        foreach (var a in auditUnits)
        {
            list.Add(_mapper.Map<AuditDto>(a));
        }
        
        // Generate report
        return Ok(list);
    }
    [HttpGet("ExpiredNames")]
    public async Task<ActionResult<List<string>>> GetExpiredItemsNames()
    {
        var list = _context.Units.Where(x => x.ExpiryDate <= DateTime.Today).Select(x => x.Item.Name).ToList();
        if (list is null)
        {
            return NotFound();
        }
        return Ok(list);
    }

    private async Task<List<Unit>> ExpiredItems()
    { 
        return _context.Units.Where(x => x.ExpiryDate <= DateTime.Today).ToList();
    }
    private async Task<List<Unit>> ExpiredItems(int days)
    { 
        return _context.Units.Where(x => x.ExpiryDate <= DateTime.Today.AddDays(days)).ToList();
    }

    private string GenerateEmail(List<AuditDto> auditDtos)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Audit Log: <br>");
        sb.AppendLine("Expiry,EventTime,Quantity,ItemID,Username<br>");
        foreach (var a in auditDtos)
        {
            sb.AppendLine($"{a.ExpiryDate},{a.EventDateTime},{a.Quantity},{a.ItemId},{a.UserName}<br>");
        }

        return sb.ToString();
    }
}