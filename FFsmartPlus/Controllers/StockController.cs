using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Application.Item;
using Application.Unit;
using AutoMapper;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FFsmartPlus.Controllers
{
    [Route("api/Item/{id}/[controller]")]
    [ApiController]
    public class StockController : ControllerBase
    {
        private readonly FridgeAppContext _context;
        private readonly IMapper _mapper;
        
        public StockController(FridgeAppContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        [HttpGet("")]
        public async Task<ActionResult<CurrentStockDto>> GetCurrentStock(long id)
        {
            var currentStock = new CurrentStockDto();
            Domain.Item item = await _context.Items.FindAsync(id);
            await _context.Entry(item).Collection(i => i.Units).LoadAsync();
            currentStock.currentQuantity = item.Units.Select(x => x.Quantity).Sum();
            currentStock.item = _mapper.Map<ItemDto>(item);
            return currentStock;
        }
        //POST: api/Stock/Add
        [HttpPost("Add")]
        public async Task<ActionResult<bool>> AddStock(UnitDto newUnits)
        {
            Domain.Item item = await _context.Items.FindAsync(newUnits.ItemId);
            await _context.Entry(item).Collection(i => i.Units).LoadAsync();
            Domain.Unit unit = item.Units.FirstOrDefault(x => x.ExpiryDate.Equals(newUnits.ExpiryDate));
            try
            {

                if (unit is null)
                {
                    var newUnit = _mapper.Map<Domain.Unit>(newUnits);
                    _context.Units.Add(newUnit);
                }
                else
                {
                    unit.Quantity = newUnits.Quantity + unit.Quantity;
                    _context.Entry(unit).State = EntityState.Modified;
                }
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
