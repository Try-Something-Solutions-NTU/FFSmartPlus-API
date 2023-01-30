using Application.Unit;
using AutoMapper;
using Domain;
using FFsmartPlus.Services;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Testing;

public class StockControllerTesting : IClassFixture<WebApplicationFactory<Program>>
{
    private StockService _stockService;
    private readonly IMapper _mapper;
    private FridgeAppContext _context;

    public StockControllerTesting(WebApplicationFactory<Program> factory)
    {
        _mapper = factory.Services.GetService<IMapper>();
         _context = GetDatabaseContext().Result;
        _stockService = new StockService(_context, _mapper);
    }
    [Fact]
    public async Task AddStockWithItemNoItemReturnsFalse()
    {
     var resp = await _stockService.AddStock(3, new NewUnitDto(),"Nick");
     Assert.False(resp);
    }
    [Fact]
    public async Task AddStockWithItemNoUserNameReturnsFalse()
    {
        
        var resp = await _stockService.AddStock(1, new NewUnitDto(),null);
        Assert.False(resp);
    }
    [Fact]
    public async Task AddStockWithItemNoUnitsDtoReturnsFalse()
    {
        var resp = await _stockService.AddStock(1, null,"Nick");
        Assert.False(resp);
    }
    [Fact]
    public async Task AddStockWithItemReturnsTrue()
    {
        var resp = await _stockService.AddStock(1, new NewUnitDto(){Quantity = 2, ExpiryDate = DateTime.Today.Add(TimeSpan.FromDays(2))},"Nick");
        Assert.True(resp);
        var units =  _context.Units.Where(x => x.ExpiryDate == DateTime.Today.Add(TimeSpan.FromDays(2)) && x.Quantity == 2).ToList();
        Assert.NotEmpty(units);
        Assert.Equal(units.Count, 1);
        Assert.Equal(units[0].Quantity, 2);
        var audit = _context.AuditUnits.Where(x => x.Activity == Activity.Added && x.Quantity == 2 && x.ItemId == 1).ToList();
        Assert.NotEmpty(audit);
        Assert.Equal(audit[0].Quantity, 2);
        Assert.Equal(audit[0].Id, 1);
        Assert.Equal(audit[0].Activity, Activity.Added);
    }
    
    [Fact]
    public async Task AddStockTwiceAddsToOneUnit()
    {
        var resp = await _stockService.AddStock(1, new NewUnitDto(){Quantity = 2, ExpiryDate = DateTime.Today.Add(TimeSpan.FromDays(2))},"Nick");
        var resp2 = await _stockService.AddStock(1, new NewUnitDto(){Quantity = 2, ExpiryDate = DateTime.Today.Add(TimeSpan.FromDays(2))},"Nick");
        Assert.True(resp);
        Assert.True(resp2);
        var units =  _context.Units.Where(x => x.ExpiryDate == DateTime.Today.Add(TimeSpan.FromDays(2))).ToList();
        Assert.NotEmpty(units);
        Assert.Equal(units.Count, 1);
        Assert.Equal(units[0].Quantity, 4);
    }

    [Fact]
    public async Task RemoveStockInvalidIdReturnsFalse()
    {
        var resp = await _stockService.RemoveStock(400, 1, "Nick");
        Assert.False(resp);
    }
    [Fact]
    public async Task RemoveStockZeroQuantityReturnsFalse()
    {
        var resp = await _stockService.RemoveStock(1, 0, "Nick");
        Assert.False(resp);
    }
    [Fact]
    public async Task RemoveStockNullUserReturnsFalse()
    {
        var resp = await _stockService.RemoveStock(1, 1, null);
        Assert.False(resp);
    }
    [Fact]
    public async Task RemoveStockTooMuchReturnsFalse()
    {
        var resp = await _stockService.RemoveStock(1, 10000, "Nick");
        Assert.False(resp);
    }
    [Fact]
    public async Task RemoveStockValidReturnsTrue()
    {
        var resp = await _stockService.RemoveStock(1, 3, "Nick");
        Assert.True(resp);
        var units = _context.Units.Where(x => x.ItemId == 1).ToList();
        Assert.Equal(units.Count(), 0);
        var audit = _context.AuditUnits.Where(x => x.Activity == Activity.removed && x.Quantity == 3 && x.ItemId == 1).ToList();
        Assert.NotEmpty(audit);
        Assert.Equal(audit[0].Quantity, 3);
        Assert.Equal(audit[0].Id, 1);
        Assert.Equal(audit[0].Activity, Activity.removed);
    }
    [Fact]
    public async Task RemoveStockTwiceHas2Audit()
    {
        var resp = await _stockService.RemoveStock(1, 1, "Nick");
        var resp2 = await _stockService.RemoveStock(1, 2, "Nick");

        Assert.True(resp);
        Assert.True(resp2);
        var units = _context.Units.Where(x => x.ItemId == 1).ToList();
        Assert.Equal(units.Count(), 0);
        var audit = _context.AuditUnits.Where(x => x.Activity == Activity.removed && x.ItemId == 1).ToList();
        Assert.NotEmpty(audit);
        Assert.Equal(audit.Count(), 2);
        Assert.Equal(audit[0].Quantity, 1);
        Assert.Equal(audit[0].ItemId, 1);
        Assert.Equal(audit[0].Activity, Activity.removed);
        Assert.Equal(audit[1].Quantity, 2);
        Assert.Equal(audit[1].ItemId, 1);
        Assert.Equal(audit[1].Activity, Activity.removed);
    }
    [Fact]
    public async Task RemoveStockValidReturnsTrueCountCorrect()
    {
        var resp = await _stockService.RemoveStock(1, 1, "Nick");
        Assert.True(resp);
        var units = _context.Units.Where(x => x.ItemId == 1).ToList();
        Assert.Equal(units.Count(), 1);
        Assert.Equal(units[0].Quantity, 2);
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
    
}