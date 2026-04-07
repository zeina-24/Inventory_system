using AutoMapper;
using ImsPosSystem.Application.DTOs.Catalogue;
using ImsPosSystem.Application.Interfaces;
using ImsPosSystem.Application.Interfaces.Services;
using ImsPosSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ImsPosSystem.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public ProductService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    // ── Queries ───────────────────────────────────────────────────────────────
    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync(
        string? search = null, int? categoryId = null,
        int? subcategoryId = null, bool? isActive = null)
    {
        // EF Core projection query with joins for display names and stock total
        var query = _uow.Products.Query()
            .Include(p => p.Category)
            .Include(p => p.Subcategory)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p =>
                p.ProductName.Contains(search) ||
                p.SerialNumber.Contains(search) ||
                (p.Barcode != null && p.Barcode.Contains(search)));

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (subcategoryId.HasValue)
            query = query.Where(p => p.SubcategoryId == subcategoryId.Value);

        if (isActive.HasValue)
            query = query.Where(p => p.IsActive == isActive.Value);

        var products = await query.ToListAsync();

        // Get all stock quantities in one query
        var productIds = products.Select(p => p.ProductId).ToList();
        var stockQtys = await _uow.StockLedgers.Query()
            .Where(sl => productIds.Contains(sl.ProductId))
            .GroupBy(sl => sl.ProductId)
            .Select(g => new { ProductId = g.Key, Qty = g.Sum(sl => sl.QtyOnHand) })
            .ToListAsync();

        var stockDict = stockQtys.ToDictionary(x => x.ProductId, x => (int)x.Qty);

        return products.Select(p =>
        {
            var dto = _mapper.Map<ProductDto>(p);
            dto = dto with { QtyOnHand = stockDict.GetValueOrDefault(p.ProductId, 0) };
            return dto;
        });
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var p = await _uow.Products.Query()
            .Include(p => p.Category)
            .Include(p => p.Subcategory)
            .FirstOrDefaultAsync(p => p.ProductId == id);

        if (p == null) return null;

        var qty = (int)await _uow.StockLedgers.Query()
            .Where(sl => sl.ProductId == id)
            .SumAsync(sl => (decimal?)sl.QtyOnHand ?? 0);

        var dto = _mapper.Map<ProductDto>(p);
        return dto with { QtyOnHand = qty };
    }

    public async Task<IEnumerable<ProductLookupDto>> GetProductLookupsAsync(string? search = null)
    {
        var query = _uow.Products.Query().Where(p => p.IsActive);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p =>
                p.ProductName.Contains(search) ||
                p.SerialNumber.Contains(search) ||
                (p.Barcode != null && p.Barcode.Contains(search)));

        return await query.Select(p => new ProductLookupDto(
            p.ProductId, p.SerialNumber, p.ProductName,
            p.BuyingPrice, p.SellingPrice, p.Barcode))
            .ToListAsync();
    }

    // ── Commands ──────────────────────────────────────────────────────────────
    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
    {
        if (await _uow.Products.AnyAsync(p => p.SerialNumber == dto.SerialNumber))
            throw new InvalidOperationException($"Serial number '{dto.SerialNumber}' already exists.");

        if (dto.Barcode != null && await _uow.Products.AnyAsync(p => p.Barcode == dto.Barcode))
            throw new InvalidOperationException($"Barcode '{dto.Barcode}' already exists.");

        var entity = new Product
        {
            SerialNumber  = dto.SerialNumber,
            ProductName   = dto.ProductName,
            CategoryId    = dto.CategoryId,
            SubcategoryId = dto.SubcategoryId,
            BuyingPrice   = dto.BuyingPrice,
            SellingPrice  = dto.SellingPrice,
            Barcode       = dto.Barcode,
            Unit          = dto.Unit,
            ReorderLevel  = dto.ReorderLevel,
            ImageUrl      = dto.ImageUrl,
            Notes         = dto.Notes,
            IsActive      = dto.IsActive,
            CreatedAt     = DateTime.UtcNow,
            UpdatedAt     = DateTime.UtcNow
        };

        await _uow.Products.AddAsync(entity);
        await _uow.SaveChangesAsync();
        return (await GetProductByIdAsync(entity.ProductId))!;
    }

    public async Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto dto)
    {
        var entity = await _uow.Products.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Product {id} not found.");

        if (await _uow.Products.AnyAsync(p => p.SerialNumber == dto.SerialNumber && p.ProductId != id))
            throw new InvalidOperationException($"Serial number '{dto.SerialNumber}' already used.");

        if (dto.Barcode != null &&
            await _uow.Products.AnyAsync(p => p.Barcode == dto.Barcode && p.ProductId != id))
            throw new InvalidOperationException($"Barcode '{dto.Barcode}' already used.");

        entity.SerialNumber  = dto.SerialNumber;
        entity.ProductName   = dto.ProductName;
        entity.CategoryId    = dto.CategoryId;
        entity.SubcategoryId = dto.SubcategoryId;
        entity.BuyingPrice   = dto.BuyingPrice;
        entity.SellingPrice  = dto.SellingPrice;
        entity.Barcode       = dto.Barcode;
        entity.Unit          = dto.Unit;
        entity.ReorderLevel  = dto.ReorderLevel;
        entity.ImageUrl      = dto.ImageUrl;
        entity.Notes         = dto.Notes;
        entity.IsActive      = dto.IsActive;
        entity.UpdatedAt     = DateTime.UtcNow;

        _uow.Products.Update(entity);
        await _uow.SaveChangesAsync();
        return (await GetProductByIdAsync(id))!;
    }

    public async Task DeactivateProductAsync(int id)
    {
        var entity = await _uow.Products.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Product {id} not found.");

        entity.IsActive  = false;
        entity.UpdatedAt = DateTime.UtcNow;
        _uow.Products.Update(entity);
        await _uow.SaveChangesAsync();
    }

    public async Task DeleteProductAsync(int id)
    {
        var entity = await _uow.Products.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Product {id} not found.");

        bool hasSales     = await _uow.SalesInvoiceLines.AnyAsync(l => l.ProductId == id);
        bool hasPurchases = await _uow.PurchaseInvoiceLines.AnyAsync(l => l.ProductId == id);
        bool hasStock     = await _uow.StockLedgers.AnyAsync(sl => sl.ProductId == id);

        if (hasSales || hasPurchases || hasStock)
            throw new InvalidOperationException(
                "Cannot hard-delete a product with existing transactions or stock. Use Deactivate instead.");

        _uow.Products.Remove(entity);
        await _uow.SaveChangesAsync();
    }
}
