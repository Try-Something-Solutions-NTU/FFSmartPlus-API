using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Supplier;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Domain;
using FluentValidation.Results;
using Infrastructure;
using Infrastructure.Auth;
using Microsoft.AspNetCore.Authorization;

namespace FFsmartPlus.Controllers
{
    [Route("api/Suppliers/[controller]")]
    [Authorize(Roles = UserRoles.Admin)]
    [Authorize(Roles = UserRoles.HeadChef)]
    [ApiController]
    public class SupplierController : ControllerBase
    {
        private readonly FridgeAppContext _context;
        private readonly IMapper _mapper;
        private readonly SupplierValidator _supplierValidator;

        public SupplierController(FridgeAppContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
            _supplierValidator = new SupplierValidator();
        }
        /// <summary>
        /// Get list of all suppliers
        /// </summary>
        // GET: api/Supplier
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SupplierDto>>> GetSuppliers()
        {
          if (_context.Suppliers == null)
          {
              return NotFound();
          }
          var suppliers =  await _context.Suppliers.ToListAsync();
          return _mapper.Map<List<SupplierDto>>(suppliers);
        }
        /// <summary>
        /// Get Supplier by ID
        /// </summary>

        // GET: api/Supplier/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SupplierDto>> GetSupplier(long id)
        {
          if (_context.Suppliers == null)
          {
              return NotFound();
          }
            var supplier = await _context.Suppliers.FindAsync(id);

            if (supplier == null)
            {
                return NotFound();
            }

            return _mapper.Map<SupplierDto>(supplier);
        }
        /// <summary>
        /// Update a supplier record
        /// </summary>
        // PUT: api/Supplier/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(SupplierDto), 200)]
        [ProducesResponseType(typeof(ValidationResult), 400)]
        public async Task<IActionResult> PutSupplier(long id, SupplierDto supplierDto)
        {
            if (id != supplierDto.Id)
                return BadRequest("Id does not match body");
            if (!SupplierExists(id))
            {
                return NotFound();
            }

            var supplier = _mapper.Map<Supplier>(supplierDto);
            var validatorResult = await _supplierValidator.ValidateAsync(supplier);
            if (!validatorResult.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, validatorResult);
            }
            _context.Entry(supplier).State = EntityState.Modified;
            
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SupplierExists(id))
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
        /// Create a new supplier
        /// </summary>
        // POST: api/Supplier
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [ProducesResponseType(typeof(SupplierDto), 201)]
        [ProducesResponseType(typeof(ValidationResult), 400)]
        public async Task<ActionResult<SupplierDto>> PostSupplier(NewSupplierDto supplier)
        {
          if (_context.Suppliers == null)
          {
              return Problem("Entity set 'FridgeAppContext.Suppliers'  is null.");
          }
          
          var newSupplier = new Supplier()
          {
            Name = supplier.Name,
            Address = supplier.Address,
            Email = supplier.Email
          };
          var validatorResult = await _supplierValidator.ValidateAsync(newSupplier);
          if (!validatorResult.IsValid)
          {
              return StatusCode(StatusCodes.Status400BadRequest, validatorResult);
          }
          _context.Suppliers.Add(newSupplier);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetSupplier", new { Id = newSupplier.Id }, newSupplier);
        }
        /// <summary>
        /// Delete a supplier by its ID
        /// </summary>

        // DELETE: api/Supplier/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSupplier(long id)
        {
            if (_context.Suppliers == null)
            {
                return NotFound();
            }
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
            {
                return NotFound();
            }

            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SupplierExists(long id)
        {
            return (_context.Suppliers?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
