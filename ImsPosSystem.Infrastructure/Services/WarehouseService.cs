using AutoMapper;
using ImsPosSystem.Application.DTOs.Warehouse;
using ImsPosSystem.Application.Interfaces;
using ImsPosSystem.Application.Interfaces.Services;
using ImsPosSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ImsPosSystem.Infrastructure.Services;

public class StorageLocationService : IStorageLocationService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public StorageLocationService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<IEnumerable<StorageLocationDto>> GetAllLocationsAsync(
        string? block = null, bool? isActive = null)
    {
        var query = _uow.StorageLocations.Query();
        if (!string.IsNullOrWhiteSpace(block))
            query = query.Where(l => l.Block == block);
        if (isActive.HasValue)
            query = query.Where(l => l.IsActive == isActive.Value);
        var list = await query.ToListAsync();
        return _mapper.Map<IEnumerable<StorageLocationDto>>(list);
    }

    public async Task<StorageLocationDto?> GetLocationByIdAsync(int id)
    {
        var l = await _uow.StorageLocations.GetByIdAsync(id);
        return _mapper.Map<StorageLocationDto>(l);
    }

    public async Task<StorageLocationDto> CreateLocationAsync(CreateStorageLocationDto dto)
    {
        var entity = new StorageLocation
        {
            Block       = dto.Block,
            Aisle       = dto.Aisle,
            Shelf       = dto.Shelf,
            Description = dto.Description,
            MaxCapacity = dto.MaxCapacity,
            IsActive    = dto.IsActive,
            CreatedAt   = DateTime.UtcNow
        };
        await _uow.StorageLocations.AddAsync(entity);
        await _uow.SaveChangesAsync();
        return _mapper.Map<StorageLocationDto>(entity);
    }

    public async Task<StorageLocationDto> UpdateLocationAsync(int id, UpdateStorageLocationDto dto)
    {
        var entity = await _uow.StorageLocations.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Storage location {id} not found.");

        entity.Block       = dto.Block;
        entity.Aisle       = dto.Aisle;
        entity.Shelf       = dto.Shelf;
        entity.Description = dto.Description;
        entity.MaxCapacity = dto.MaxCapacity;
        entity.IsActive    = dto.IsActive;

        _uow.StorageLocations.Update(entity);
        await _uow.SaveChangesAsync();
        return _mapper.Map<StorageLocationDto>(entity);
    }

    public async Task DeleteLocationAsync(int id)
    {
        var entity = await _uow.StorageLocations.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Storage location {id} not found.");

        bool hasStock     = await _uow.StockLedgers.AnyAsync(sl => sl.LocationId == id);
        bool hasPurchases = await _uow.PurchaseInvoiceLines.AnyAsync(l => l.LocationId == id);
        bool hasSales     = await _uow.SalesInvoiceLines.AnyAsync(l => l.LocationId == id);

        if (hasStock || hasPurchases || hasSales)
            throw new InvalidOperationException(
                "Cannot delete a location with existing stock or transaction history. Deactivate it instead.");

        _uow.StorageLocations.Remove(entity);
        await _uow.SaveChangesAsync();
    }
}

public class StockLedgerService : IStockLedgerService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public StockLedgerService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<IEnumerable<StockLedgerDto>> GetStockAsync(
        string? search = null, string? block = null,
        string? aisle = null, bool lowStockOnly = false)
    {
        var query = _uow.StockLedgers.Query()
            .Include(sl => sl.Product)
            .Include(sl => sl.Location)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(sl =>
                sl.Product.ProductName.Contains(search) ||
                sl.Product.SerialNumber.Contains(search));

        if (!string.IsNullOrWhiteSpace(block))
            query = query.Where(sl => sl.Location.Block == block);

        if (!string.IsNullOrWhiteSpace(aisle))
            query = query.Where(sl => sl.Location.Aisle == aisle);

        if (lowStockOnly)
            query = query.Where(sl => sl.QtyOnHand < sl.Product.ReorderLevel);

        var list = await query.ToListAsync();
        return _mapper.Map<IEnumerable<StockLedgerDto>>(list);
    }

    public async Task<IEnumerable<StockLedgerDto>> GetStockByProductAsync(int productId)
    {
        var list = await _uow.StockLedgers.Query()
            .Include(sl => sl.Product)
            .Include(sl => sl.Location)
            .Where(sl => sl.ProductId == productId)
            .ToListAsync();
        return _mapper.Map<IEnumerable<StockLedgerDto>>(list);
    }

    public async Task<IEnumerable<StockLedgerDto>> GetStockByLocationAsync(int locationId)
    {
        var list = await _uow.StockLedgers.Query()
            .Include(sl => sl.Product)
            .Include(sl => sl.Location)
            .Where(sl => sl.LocationId == locationId)
            .ToListAsync();
        return _mapper.Map<IEnumerable<StockLedgerDto>>(list);
    }

    public async Task TransferStockAsync(StockTransferDto dto)
    {
        await _uow.ExecuteInTransactionAsync(async () =>
        {
            var from = await _uow.StockLedgers.Query()
                .FirstOrDefaultAsync(sl => sl.ProductId == dto.ProductId && sl.LocationId == dto.FromLocationId)
                ?? throw new KeyNotFoundException("Source stock record not found.");

            if (from.QtyOnHand < dto.Quantity)
                throw new InvalidOperationException(
                    $"Insufficient stock. Available: {from.QtyOnHand}, Requested: {dto.Quantity}.");

            from.QtyOnHand   -= dto.Quantity;
            from.LastMovedAt  = DateTime.UtcNow;
            from.UpdatedAt    = DateTime.UtcNow;
            _uow.StockLedgers.Update(from);

            var to = await _uow.StockLedgers.Query()
                .FirstOrDefaultAsync(sl => sl.ProductId == dto.ProductId && sl.LocationId == dto.ToLocationId);

            if (to == null)
            {
                to = new StockLedger
                {
                    ProductId   = dto.ProductId,
                    LocationId  = dto.ToLocationId,
                    QtyOnHand   = dto.Quantity,
                    LastMovedAt = DateTime.UtcNow,
                    UpdatedAt   = DateTime.UtcNow
                };
                await _uow.StockLedgers.AddAsync(to);
            }
            else
            {
                to.QtyOnHand  += dto.Quantity;
                to.LastMovedAt = DateTime.UtcNow;
                to.UpdatedAt   = DateTime.UtcNow;
                _uow.StockLedgers.Update(to);
            }

            await _uow.SaveChangesAsync();
        });
    }
}
