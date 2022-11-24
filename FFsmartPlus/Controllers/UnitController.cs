using Application.Unit;
using AutoMapper;
using Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Unit = Domain.Unit;

namespace FFsmartPlus.Controllers;


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
        // GET : api/item/{id}/Item
        [HttpGet("")]
        public async Task<ActionResult<List<UnitsDto>>> getUnitsDto(long id)
        {
            Domain.Item item = await _context.Items.FindAsync(id);
            await _context.Entry(item).Collection(i => i.Units).LoadAsync();
            var unitList = new List<UnitsDto>();
            foreach(Unit i in item.Units)
            {
                unitList.Add(new UnitsDto()
                {
                    ExpiryDate = i.ExpiryDate,
                    Quantity = i.Quantity
                });
            }

            return unitList;
        }
    }