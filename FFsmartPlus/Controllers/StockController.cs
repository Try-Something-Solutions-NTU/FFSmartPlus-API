using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Application.Item;
using Application.Unit;
using AutoMapper;
using Domain;
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
        //GET: api/item/{id}/Stock
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
        //POST: api/item/{id}/Stock/Add
        [HttpPost("Add")]
        public async Task<ActionResult<bool>> AddStock(long id, NewUnitDto newUnits)
        {
            Domain.Item item = await _context.Items.FindAsync(id);
            await _context.Entry(item).Collection(i => i.Units).LoadAsync();
            Domain.Unit unit = item.Units.FirstOrDefault(x => x.ExpiryDate.Equals(newUnits.ExpiryDate));
            try
            {

                if (unit is null)
                {
                     var newUnit = _mapper.Map<Domain.Unit>(newUnits);
                    newUnit.Item = item;
                    item.Units.Add(newUnit);
                    _context.Entry(item).State = EntityState.Modified;
                }
                else
                {
                    unit.Quantity = newUnits.Quantity + unit.Quantity;
                    _context.Entry(unit).State = EntityState.Modified;
                }
                await _context.SaveChangesAsync();
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        [HttpPost("Remove")] 
        public async Task<ActionResult<bool>> RemoveStock(long id, double Quantity)
         {
             Domain.Item item = await _context.Items.FindAsync(id);
             await _context.Entry(item).Collection(i => i.Units).LoadAsync();
             item.Units = item.Units.OrderBy(x => x.ExpiryDate).ToList();
             //if you try to remove too many items
             var test = item.Units.Select(x => x.Quantity).Sum();
             if (test < Quantity)
             {
                 return false;
             }
             do
             {
                 Unit unit = item.Units.OrderBy(x => x.ExpiryDate).First();
                 if (unit.Quantity <= Quantity)
                 {
                     _context.Entry(unit).State = EntityState.Deleted;
                     item.Units.Remove(unit);
                     Quantity = Quantity - unit.Quantity;
                 }
                 else
                 {
                     
                     unit.Quantity = unit.Quantity - Quantity;
                     _context.Entry(unit).State = EntityState.Modified;
                     Quantity = 0;
                 }

             } while (Quantity != 0);
             await _context.SaveChangesAsync();

             return  true;
         }
    }
}
