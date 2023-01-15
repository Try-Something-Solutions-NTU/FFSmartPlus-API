using Application.Unit;
using AutoMapper;
using Domain;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FFsmartPlus.Services;

public class StockService : IStockService
{
    private readonly FridgeAppContext _context;
    private readonly IMapper _mapper;
    public StockService(FridgeAppContext context, IMapper mapper) 
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<bool> AddStock(long id, NewUnitDto newUnits, string username)
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
            UserName = username
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
    }
}