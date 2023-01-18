using Application.Unit;

namespace FFsmartPlus.Services;

public interface IStockService
{
    public Task<bool> AddStock(long id, NewUnitDto newUnits, string username);
    public Task<bool> RemoveStock(long id,double Quantity, string UserName);

}