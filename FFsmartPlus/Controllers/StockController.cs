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

namespace FFsmartPlus.Controllers
{
    [Authorize(Roles = UserRoles.Chef)]
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
        /// <summary>
        /// Get the current stock of an Item 
        /// </summary>
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
        /// <summary>
        /// Add units to stock 
        /// </summary>
        //POST: api/item/{id}/Stock/Add
        [HttpPost("Add")]
        public async Task<ActionResult<bool>> AddStock(long id, NewUnitDto newUnits)
        {
            
            Domain.Item item = await _context.Items.FindAsync(id);
            await _context.Entry(item).Collection(i => i.Units).LoadAsync();
            Domain.Unit unit = item.Units.FirstOrDefault(x => x.ExpiryDate.Equals(newUnits.ExpiryDate));
            // try
            // {
                AuditUnit auditUnit = new AuditUnit()
                {
                    EventDateTime = DateTime.Now,
                    Activity = Activity.Added,
                    Quantity = newUnits.Quantity,
                    ExpiryDate = newUnits.ExpiryDate,
                    ItemId = id,
                    Item = item,
                    UserName = User.Identity.Name
                };
                _context.AuditUnits.Add(auditUnit);
                
                if (unit is null)
                {
                    var newUnit = _mapper.Map<Domain.Unit>(newUnits);
                    newUnit.Item = item;
                    newUnit.ItemId = id;
                    item.Units.Add(newUnit);
                }
                else
                {
                    unit.Quantity = newUnits.Quantity + unit.Quantity;
                    _context.Entry(unit).State = EntityState.Modified;
                }
                await _context.SaveChangesAsync();
                return true;
            // }
            // catch(Exception ex)
            // {
            //     return false;
            // }
        }
        /// <summary>
        /// Remove units from stock following FIFO
        /// </summary>
        [HttpPost("Remove")] 
        public async Task<ActionResult<bool>> RemoveStock(long id, double Quantity)
         {
             Domain.Item item = await _context.Items.FindAsync(id);
             await _context.Entry(item).Collection(i => i.Units).LoadAsync();
             item.Units = item.Units.ToList();
              
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
                     AuditUnit auditUnit = new AuditUnit()
                     {
                         EventDateTime = DateTime.Now,
                         Activity = Activity.removed,
                         Quantity = unit.Quantity,
                         ExpiryDate = unit.ExpiryDate,
                         ItemId = id,
                         Item = item,
                         UserName = User.Identity.Name
                     };
                     _context.AuditUnits.Add(auditUnit);
                     _context.Entry(unit).State = EntityState.Deleted;
                     item.Units.Remove(unit);
                     
                     Quantity = Quantity - unit.Quantity;
                 }
                 else
                 {
                     AuditUnit auditUnit = new AuditUnit()
                     {
                         EventDateTime = DateTime.Now,
                         Activity = Activity.removed,
                         Quantity = unit.Quantity,
                         ExpiryDate = unit.ExpiryDate,
                         ItemId = id,
                         Item = item,
                         UserName = User.Identity.Name
                     };
                     _context.AuditUnits.Add(auditUnit);

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
