using System.Text.RegularExpressions;
using Application.Unit;
using AutoMapper;
using Domain;
using FFsmartPlus.Services;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Testing;

public class DoorsTesting : IClassFixture<WebApplicationFactory<Program>>
{
    private IDoorService _doorService;
    private readonly IMapper _mapper;
    private FridgeAppContext _context;

    public DoorsTesting(WebApplicationFactory<Program> factory)
    {
        _mapper = factory.Services.GetService<IMapper>();
         _context = GetDatabaseContext().Result;
         _doorService = new DoorService(_context);
    }

    [Fact]
    public async Task invalidSupplierIDReturnsexception()
    {
        Func<Task> act = () => _doorService.GenerateNewCode(0);
        //Assert
        var exception = await Assert.ThrowsAsync<Exception>(act);
    }

    [Fact]
    public async Task validRequestRequestGets4DigitNumber()
    {
        //Check 100 different random codes
        for (int i = 0; i < 100; i++)
        {
            var code = await _doorService.GenerateNewCode(1);
            Assert.Equal(code.ToString().Length, 4);
        }
    }

    [Fact]
    public async Task NewCodeWorks()
    {
        var code = await _doorService.GenerateNewCode(1);
        var doorOpens = _doorService.OpenDoorByCode(code);
        Assert.True(await doorOpens);
    }
    [Fact]
    public async Task NewCodeCreatesAuditLog()
    {
        var code = await _doorService.GenerateNewCode(1);
        var doorOpens = _doorService.OpenDoorByCode(code);
        Assert.True(await doorOpens);
        var log = await _context.AuditDoors.FirstOrDefaultAsync();
        Assert.Equal(log.SupplierId, 1);
        Assert.True(AreDatesWithinOneSecond(log.EventTime, DateTime.Now));
    }
    [Fact]
    public async Task NewCodeRemovesCode()
    {
        var code = await _doorService.GenerateNewCode(1);
        Assert.Equal(_context.DoorCodes.Count(), 1);
        var doorOpens = _doorService.OpenDoorByCode(code);
        Assert.True(await doorOpens); 
        Assert.Equal(_context.DoorCodes.Count(), 0);
    }

    [Fact]
    public async Task NewCodeLogsCode()
    {
        var code = await _doorService.GenerateNewCode(1);
        Assert.Equal(_context.DoorCodes.Count(), 1);
    }
    
    [Fact]
    public async Task InvalidCodeFails()
    {
        var doorOpens = _doorService.OpenDoorByCode(0000);
        Assert.False(await doorOpens);
    }

    

    private async Task<FridgeAppContext> GetDatabaseContext()
    {
        var options = new DbContextOptionsBuilder<FridgeAppContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var databaseContext = new FridgeAppContext(options);
        await databaseContext.Database.EnsureCreatedAsync();
        return databaseContext;
    }
    static bool AreDatesWithinOneSecond(DateTime date1, DateTime date2)
    {
        return Math.Abs((date1 - date2).TotalSeconds) <= 1;
    }
    
}