using AutoMapper;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Application.Fridge.Commands;

public class FridgeCommands
{
    private readonly FridgeAppContext _context;
    private readonly IMapper _mapper;
    public FridgeCommands(FridgeAppContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    public async Task<IEnumerable<Domain.Fridge>> GetFridges()
    {
        return await _context.Fridges.ToListAsync();
    }
}