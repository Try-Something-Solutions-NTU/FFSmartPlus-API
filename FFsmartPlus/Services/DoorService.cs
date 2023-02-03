using Domain;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FFsmartPlus.Services;

public class DoorService : IDoorService
{
    private readonly FridgeAppContext _context;
    public DoorService(FridgeAppContext context)
    {
        _context = context;
    }
    public async Task<long> GenerateNewCode(long SupplierId)
    {
       var supplier = await _context.Suppliers.FindAsync(SupplierId);
       if (supplier is null)
       {
           throw new Exception("No Supplier found");
       }

       var code = GenerateUniqueCode();
       _context.DoorCodes.Add(new DoorCode()
        {
            Code = await code,
            SupplierId = SupplierId,
            Supplier = supplier
        });
       await _context.SaveChangesAsync();
       return await code;
    }

    public async Task<bool> OpenDoorByCode(long code)
    {
        var codeRecord = await _context.DoorCodes.FirstOrDefaultAsync(x => x.Code == code);
        if (codeRecord is null)
        {
            return false;
        }
        _context.DoorCodes.Remove(codeRecord);
        await LogEvent(codeRecord.SupplierId);
        await _context.SaveChangesAsync();
        //Call to door lock api here
        return true;
    }

    private async Task<long> GenerateUniqueCode()
    {
        if (_context.DoorCodes.Count() >= 3000)
        {
            throw new Exception("too many codes are active");
        }
        while (true)
        {
            var code =  GenerateRandomNo();
            var codeRecord = await _context.DoorCodes.FirstOrDefaultAsync(x => x.Code == code);
            if (codeRecord is null)
            {
                return code;
            }
        }
            
    }
    private long GenerateRandomNo()
    {
        int _min = 1000;
        int _max = 9999;
        Random _rdm = new Random();
        return _rdm.Next(_min, _max);
    }

    private async Task LogEvent(long id)
    {
        await _context.AuditDoors.AddAsync(new AuditDoor
        {
            SupplierId = id,
            EventTime = DateTime.Now
        });

    }
}