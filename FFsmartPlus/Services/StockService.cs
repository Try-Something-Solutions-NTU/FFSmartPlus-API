using Application.Unit;
using AutoMapper;
using Domain;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
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

    public async Task<bool> RemoveStock(long id,double Quantity, string UserName)
    {
        if (Quantity == 0)
            return false;
        Domain.Item item = await _context.Items.FindAsync(id);
        if (item is null || UserName is null)
            return false;
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
                         UserName = UserName
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
                         UserName = UserName
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

    public async Task<bool> AddStock(long id, NewUnitDto newUnits, string username)
    {
        Domain.Item item = await _context.Items.FindAsync(id);
        if (item is null || username is null || newUnits is null)
            return false;
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