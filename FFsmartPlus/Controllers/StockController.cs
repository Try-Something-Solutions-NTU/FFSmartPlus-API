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
using FFsmartPlus.Services;
using Infrastructure;
using Infrastructure.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FFsmartPlus.Controllers
{
    [Authorize]
    [Route("api/Item/{id}/[controller]")]
    [ApiController]
    public class StockController : ControllerBase
    {
        private readonly FridgeAppContext _context;
        private readonly IMapper _mapper;
        private readonly IStockService _stockService;
        
        public StockController(FridgeAppContext context, IMapper mapper, IStockService stockService)
        {
            _context = context;
            _stockService = stockService;
            _mapper = mapper;
        }
        /// <summary>
        /// Get the current stock of an Item 
        /// </summary>
        //GET: api/item/{id}/Stock
        [HttpGet("")]
        [ProducesResponseType(typeof(CurrentStockDto), 200)]
        [ProducesResponseType( 404)]
        public async Task<ActionResult<CurrentStockDto>> GetCurrentStock(long id)
        {
            var currentStock = new CurrentStockDto();
            var item = await _context.Items.FindAsync(id);
            if (item is null)
            {
                return NotFound();
                
            }
            
            await _context.Entry(item).Collection(i => i.Units).LoadAsync();
            currentStock.currentQuantity = item.Units.Select(x => x.Quantity).Sum();
            currentStock.item = _mapper.Map<ItemDto>(item);
            return currentStock;
        }
        /// <summary>
        /// Add units to stock 
        /// </summary>
        //POST: api/item/{id}/Stock/Add
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType( 404)]
        [ProducesResponseType(401)]
        [ProducesResponseType(400)]
        [Authorize]
        [HttpPost("Add")]
        public async Task<ActionResult<bool>> AddStockRequest(long id, NewUnitDto newUnits)
        {
            var UserName = User.Identity.Name;
            if (UserName is null)
            {
                return BadRequest("User not Found");
            }

            try
            {
                var result = await _stockService.AddStock(id, newUnits, UserName);
                if (result == false)
                {
                    return BadRequest();
                }

                return new OkObjectResult(result);
            }
            catch
            {
                return BadRequest();
            }
            // }
            // catch(Exception ex)
            // {
            //     return false;
            // }
        }
        /// <summary>
        /// Remove units from stock following FIFO
        /// </summary>
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType( 404)]
        [ProducesResponseType( 400)]

        [HttpPost("Remove")] 
        public async Task<ActionResult<bool>> RemoveStock(long id, double Quantity)
         {
             if (User.Identity.Name is null)
             {
                 return BadRequest();
             }

             try
             {
                 return await _stockService.RemoveStock(id, Quantity, User.Identity.Name);
             }
             catch
             {
                 return NotFound();
             }
         }

        
    }
}
