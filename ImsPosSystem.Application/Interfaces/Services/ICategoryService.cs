using ImsPosSystem.Application.DTOs.Catalogue;

namespace ImsPosSystem.Application.Interfaces.Services;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
    Task<CategoryDto?> GetCategoryByIdAsync(int id);
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto);
    Task<CategoryDto> UpdateCategoryAsync(int id, UpdateCategoryDto dto);
    Task DeleteCategoryAsync(int id);   // Soft-delete guard enforced in implementation
}

public interface ISubcategoryService
{
    Task<IEnumerable<SubcategoryDto>> GetAllSubcategoriesAsync();
    Task<IEnumerable<SubcategoryDto>> GetSubcategoriesByCategoryAsync(int categoryId);
    Task<SubcategoryDto?> GetSubcategoryByIdAsync(int id);
    Task<SubcategoryDto> CreateSubcategoryAsync(CreateSubcategoryDto dto);
    Task<SubcategoryDto> UpdateSubcategoryAsync(int id, UpdateSubcategoryDto dto);
    Task DeleteSubcategoryAsync(int id);
}
