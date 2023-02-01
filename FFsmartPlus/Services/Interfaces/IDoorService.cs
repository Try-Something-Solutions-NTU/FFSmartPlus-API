namespace FFsmartPlus.Services;

public interface IDoorService
{
    public Task<long> GenerateNewCode(long SupplierId);
    public Task<bool> OpenDoorByCode(long code);
}