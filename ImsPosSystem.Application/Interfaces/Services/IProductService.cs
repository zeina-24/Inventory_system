using ImsPosSystem.Application.DTOs.Catalogue;

namespace ImsPosSystem.Application.Interfaces.Services;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllProductsAsync(
        string? search = null,
        int? categoryId = null,
        int? subcategoryId = null,
        bool? isActive = null);

    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<IEnumerable<ProductLookupDto>> GetProductLookupsAsync(string? search = null);

    Task<ProductDto> CreateProductAsync(CreateProductDto dto);
    Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto dto);
    Task DeactivateProductAsync(int id);    // Soft-delete: IsActive = false
    Task DeleteProductAsync(int id);        // Hard-delete only if no transactions
}
