using ImsPosSystem.Application.DTOs.Warehouse;

namespace ImsPosSystem.Application.Interfaces.Services;

public interface IStorageLocationService
{
    Task<IEnumerable<StorageLocationDto>> GetAllLocationsAsync(string? block = null, bool? isActive = null);
    Task<StorageLocationDto?> GetLocationByIdAsync(int id);
    Task<StorageLocationDto> CreateLocationAsync(CreateStorageLocationDto dto);
    Task<StorageLocationDto> UpdateLocationAsync(int id, UpdateStorageLocationDto dto);
    Task DeleteLocationAsync(int id);
}

public interface IStockLedgerService
{
    Task<IEnumerable<StockLedgerDto>> GetStockAsync(
        string? search = null,
        string? block = null,
        string? aisle = null,
        bool lowStockOnly = false);

    Task<IEnumerable<StockLedgerDto>> GetStockByProductAsync(int productId);
    Task<IEnumerable<StockLedgerDto>> GetStockByLocationAsync(int locationId);
    Task TransferStockAsync(StockTransferDto dto);   // Admin transfer between bins
}
