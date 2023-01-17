using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Item;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Domain;
using FluentValidation.Results;
using Infrastructure;

namespace FFsmartPlus.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemController : ControllerBase
    {
        private readonly FridgeAppContext _context;
        private readonly IMapper _mapper;
        private readonly ItemValidator _itemValidator;
        public ItemController(FridgeAppContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
            _itemValidator = new ItemValidator();
        }
        /// <summary>
        /// Get all active items
        /// </summary>
        // GET: api/Item
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemDto>>> GetItems()
        {
          if (_context.Items == null)
          {
              return NotFound();
          } 
          var items = await _context.Items.Where(x => x.Active.Equals(true)).ToListAsync();
          return _mapper.Map<List<ItemDto>>(items);
        }
        /// <summary>
        /// Get Item by ID
        /// </summary>
        // GET: api/Item/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemDto>> GetItem(long id)
        {
          if (_context.Items == null)
          {
              return NotFound();
          }

          var item = await _context.Items.FirstOrDefaultAsync(x => x.Active.Equals(true) && x.Id == id);
            
            if (item == null)
            {
                return NotFound();
            }
            
            return _mapper.Map<ItemDto>(item);
        }
        
        /// <summary>
        /// Update an item by ID
        /// </summary>
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(ValidationResult), 400)]
        [ProducesResponseType( 404)]

        // PUT: api/Item/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutItem(long id, ItemDto putItem)
        {
            var test = ItemActive(id);
            if (id != putItem.Id || !ItemActive(id))
            {
                return BadRequest();
            }
            
            var item = _mapper.Map<Item>(putItem);
            var validatorResult = await _itemValidator.ValidateAsync(item);
            if (!validatorResult.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, validatorResult);
            }
            _context.Entry(item).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }
        /// <summary>
        /// Deactivate an item 
        /// </summary>
        [HttpPut("{id}/deactivate")]
        public async Task<IActionResult> DeactivateItem(long id)
        {
            if (_context.Items == null)
            {
                return NotFound();
            }
            var item = await _context.Items.FindAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            item.Active = false;
            _context.Entry(item).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();

        }
        /// <summary>
        /// Create a new Item
        /// </summary>
        // POST: api/Item
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [ProducesResponseType(typeof(ItemDto), 200)]
        [ProducesResponseType(typeof(ValidationResult), 400)]
        public async Task<ActionResult<Item>> PostItem(NewItemDto newItem)
        {
          if (_context.Items == null)
          {
              return Problem("Entity set 'FridgeAppContext.Items'  is null.");
          }
          
          var item = new Item
          {
              Name = newItem.Name,
              UnitDesc = newItem.UnitDesc,
              minimumStock = newItem.minimumStock,
              SupplierId = newItem.SupplierId
          };
          var validatorResult = await _itemValidator.ValidateAsync(item);
          if (!validatorResult.IsValid)
          {
              return StatusCode(StatusCodes.Status400BadRequest, validatorResult);
          }
          _context.Items.Add(item);
          await _context.SaveChangesAsync();

            return CreatedAtAction("GetItem", new { id = item.Id }, item);
        }
        /// <summary>
        /// Deletes a specific Item
        /// </summary>
        // DELETE: api/Item/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(long id)
        {
            if (_context.Items == null)
            {
                return NotFound();
            }
            var item = await _context.Items.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            item.Active = false;
            _context.Entry(item).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ItemExists(long id)
        {
            return (_context.Items?.Any(e => e.Id == id)).GetValueOrDefault();
        }
        private bool ItemActive(long id)
        {
            return (_context.Items?.Any(e => e.Id == id && e.Active == true)).GetValueOrDefault();
        }
    }
}
