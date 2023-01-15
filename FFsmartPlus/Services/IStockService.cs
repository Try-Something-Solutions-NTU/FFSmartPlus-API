using Application.Unit;

namespace FFsmartPlus.Services;

public interface IStockService
{
    public Task<bool> AddStock(long id, NewUnitDto newUnits, string username);

}