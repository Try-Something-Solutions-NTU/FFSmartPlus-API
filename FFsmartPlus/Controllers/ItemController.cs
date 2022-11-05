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

        // GET: api/Item
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemDto>>> GetItems()
        {
          if (_context.Items == null)
          {
              return NotFound();
          } 
          var items = await _context.Items.ToListAsync();
          return _mapper.Map<List<ItemDto>>(items);
        }

        // GET: api/Item/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemDto>> GetItem(long id)
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
            
            return _mapper.Map<ItemDto>(item);
        }

        // PUT: api/Item/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutItem(long id, ItemDto putItem)
        {
            if (id != putItem.Id)
            {
                return BadRequest();
            }

            var item = _mapper.Map<Item>(putItem);
            var validatorResult = await _itemValidator.ValidateAsync(item);
            if (!validatorResult.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, validatorResult.Errors);
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

        // POST: api/Item
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Item>> PostItem(NewItemDto newItem)
        {
          if (_context.Items == null)
          {
              return Problem("Entity set 'FridgeAppContext.Items'  is null.");
          }
          
          var item = new Item
          {
              Name = newItem.Name,
              UnitDesc = newItem.UnitDesc
          };
          var validatorResult = await _itemValidator.ValidateAsync(item);
          if (!validatorResult.IsValid)
          {
              return StatusCode(StatusCodes.Status400BadRequest, validatorResult.Errors);
          }
          _context.Items.Add(item);
          await _context.SaveChangesAsync();

            return CreatedAtAction("GetItem", new { id = item.Id }, item);
        }

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

            _context.Items.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ItemExists(long id)
        {
            return (_context.Items?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
