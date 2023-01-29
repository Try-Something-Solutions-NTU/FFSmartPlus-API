using Application.Unit;
using AutoMapper;
using Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unit = Domain.Unit;

namespace FFsmartPlus.Controllers;

    [Authorize]
    [Route("api/Item/{id}/[controller]")]
    [ApiController]
    public class UnitController
    {
        private readonly FridgeAppContext _context;
        private readonly IMapper _mapper;

        public UnitController(FridgeAppContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        /// <summary>
        /// Get units by item ID
        /// </summary>
        // GET : api/item/{id}/Item
        [HttpGet("")]
        public async Task<ActionResult<List<UnitsDto>>> getUnitsDto(long id)
        {
            Domain.Item item = await _context.Items.FindAsync(id);
            if (item is null)
                return  new NotFoundResult(); 
            await _context.Entry(item).Collection(i => i.Units).LoadAsync();
            var unitList = await _context.Units
                .Where(u => u.ItemId == id)
                .Select(u => new UnitsDto() { ExpiryDate = u.ExpiryDate, Quantity = u.Quantity })
                .ToListAsync();
            if (item.Units.Count == 0)
                return  new NotFoundResult();
            return unitList;
        }
    }