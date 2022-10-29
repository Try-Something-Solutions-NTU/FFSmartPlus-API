using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Fridge;
using Application.Fridge.Commands;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Domain;
using FluentValidation;
using FluentValidation.Results;
using Infrastructure;

namespace FFsmartPlus.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FridgeController : ControllerBase
    {
        private readonly FridgeAppContext _context;
        private readonly IMapper _mapper;
        private readonly FridgeValidator _validator = new();

        public FridgeController(FridgeAppContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Fridge
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Fridge>>> GetFridges()
        {
          if (_context.Fridges == null)
          {
              return NotFound();
          }
            return await _context.Fridges.ToListAsync();
        }

        // GET: api/Fridge/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Fridge>> GetFridge(int id)
        {
          if (_context.Fridges == null)
          {
              return NotFound();
          }
            var fridge = await _context.Fridges.FindAsync(id);

            if (fridge == null)
            {
                return NotFound();
            }

            return fridge;
        }

        // PUT: api/Fridge/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFridge(int id, Fridge fridge)
        {
            if (id != fridge.Id)
            {
                return BadRequest();
            }

            _context.Entry(fridge).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FridgeExists(id))
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

        // POST: api/Fridge
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Fridge>> PostFridge(FridgeDto fridgeDto)
        {
          if (_context.Fridges == null)
          {
              return Problem("Entity set 'FridgeAppContext.Fridges'  is null.");
          }
          Fridge fridge = _mapper.Map<Fridge>(fridgeDto);
          
          var validatorResult = await _validator.ValidateAsync(fridge);
          if (!validatorResult.IsValid)
          {
              return StatusCode(StatusCodes.Status400BadRequest, validatorResult.Errors);
          }
          var newFridge = _context.Fridges.Add(fridge);
          await _context.SaveChangesAsync();

          return CreatedAtAction("GetFridge", new { id = newFridge.Entity.Id }, fridge);
        }

        // DELETE: api/Fridge/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFridge(int id)
        {
            if (_context.Fridges == null)
            {
                return NotFound();
            }
            var fridge = await _context.Fridges.FindAsync(id);
            if (fridge == null)
            {
                return NotFound();
            }

            _context.Fridges.Remove(fridge);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool FridgeExists(int id)
        {
            return (_context.Fridges?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
