using AutoMapper;
using ImsPosSystem.Application.DTOs.Catalogue;
using ImsPosSystem.Application.Interfaces;
using ImsPosSystem.Application.Interfaces.Services;
using ImsPosSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ImsPosSystem.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public CategoryService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    // ── Queries ───────────────────────────────────────────────────────────────
    public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
    {
        var list = await _uow.Categories.GetAllAsync();
        return _mapper.Map<IEnumerable<CategoryDto>>(list);
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
    {
        var c = await _uow.Categories.GetByIdAsync(id);
        return _mapper.Map<CategoryDto>(c);
    }

    // ── Commands ──────────────────────────────────────────────────────────────
    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto)
    {
        // Uniqueness guard
        bool codeExists = await _uow.Categories.AnyAsync(c => c.CategoryCode == dto.CategoryCode);
        if (codeExists)
            throw new InvalidOperationException($"Category code '{dto.CategoryCode}' already exists.");

        bool nameExists = await _uow.Categories.AnyAsync(c => c.CategoryName == dto.CategoryName);
        if (nameExists)
            throw new InvalidOperationException($"Category name '{dto.CategoryName}' already exists.");

        var entity = new Category
        {
            CategoryCode = dto.CategoryCode,
            CategoryName = dto.CategoryName,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _uow.Categories.AddAsync(entity);
        await _uow.SaveChangesAsync();
        return _mapper.Map<CategoryDto>(entity);
    }

    public async Task<CategoryDto> UpdateCategoryAsync(int id, UpdateCategoryDto dto)
    {
        var entity = await _uow.Categories.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Category {id} not found.");

        // Uniqueness guard (exclude self)
        bool codeConflict = await _uow.Categories
            .AnyAsync(c => c.CategoryCode == dto.CategoryCode && c.CategoryId != id);
        if (codeConflict)
            throw new InvalidOperationException($"Category code '{dto.CategoryCode}' already used.");

        bool nameConflict = await _uow.Categories
            .AnyAsync(c => c.CategoryName == dto.CategoryName && c.CategoryId != id);
        if (nameConflict)
            throw new InvalidOperationException($"Category name '{dto.CategoryName}' already used.");

        entity.CategoryCode = dto.CategoryCode;
        entity.CategoryName = dto.CategoryName;
        entity.IsActive     = dto.IsActive;
        entity.UpdatedAt    = DateTime.UtcNow;

        _uow.Categories.Update(entity);
        await _uow.SaveChangesAsync();
        return _mapper.Map<CategoryDto>(entity);
    }

    public async Task DeleteCategoryAsync(int id)
    {
        var entity = await _uow.Categories.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Category {id} not found.");

        // Block hard-delete if related Subcategories or Products exist
        bool hasSubcategories = await _uow.Subcategories.AnyAsync(s => s.CategoryId == id);
        bool hasProducts      = await _uow.Products.AnyAsync(p => p.CategoryId == id);
        if (hasSubcategories || hasProducts)
            throw new InvalidOperationException(
                "Cannot delete a category that has linked subcategories or products. Deactivate it instead.");

        _uow.Categories.Remove(entity);
        await _uow.SaveChangesAsync();
    }
}

public class SubcategoryService : ISubcategoryService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public SubcategoryService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<IEnumerable<SubcategoryDto>> GetAllSubcategoriesAsync()
    {
        var subs = await _uow.Subcategories.Query()
            .Include(s => s.Category)
            .ToListAsync();
        return _mapper.Map<IEnumerable<SubcategoryDto>>(subs);
    }

    public async Task<IEnumerable<SubcategoryDto>> GetSubcategoriesByCategoryAsync(int categoryId)
    {
        var subs = await _uow.Subcategories.Query()
            .Include(s => s.Category)
            .Where(s => s.CategoryId == categoryId)
            .ToListAsync();
        return _mapper.Map<IEnumerable<SubcategoryDto>>(subs);
    }

    public async Task<SubcategoryDto?> GetSubcategoryByIdAsync(int id)
    {
        var sub = await _uow.Subcategories.Query()
            .Include(s => s.Category)
            .FirstOrDefaultAsync(s => s.SubcategoryId == id);
        return _mapper.Map<SubcategoryDto>(sub);
    }

    public async Task<SubcategoryDto> CreateSubcategoryAsync(CreateSubcategoryDto dto)
    {
        bool codeExists = await _uow.Subcategories.AnyAsync(s => s.SubcategoryCode == dto.SubcategoryCode);
        if (codeExists)
            throw new InvalidOperationException($"Subcategory code '{dto.SubcategoryCode}' already exists.");

        var cat = await _uow.Categories.GetByIdAsync(dto.CategoryId)
            ?? throw new KeyNotFoundException($"Category {dto.CategoryId} not found.");

        var entity = new Domain.Entities.Subcategory
        {
            CategoryId       = dto.CategoryId,
            SubcategoryCode  = dto.SubcategoryCode,
            SubcategoryName  = dto.SubcategoryName,
            IsActive         = dto.IsActive,
            CreatedAt        = DateTime.UtcNow,
            UpdatedAt        = DateTime.UtcNow
        };

        await _uow.Subcategories.AddAsync(entity);
        await _uow.SaveChangesAsync();

        // Re-fetch to include category info for the mapping
        var result = await _uow.Subcategories.Query()
            .Include(s => s.Category)
            .FirstOrDefaultAsync(s => s.SubcategoryId == entity.SubcategoryId);

        return _mapper.Map<SubcategoryDto>(result);
    }

    public async Task<SubcategoryDto> UpdateSubcategoryAsync(int id, UpdateSubcategoryDto dto)
    {
        var entity = await _uow.Subcategories.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Subcategory {id} not found.");

        bool codeConflict = await _uow.Subcategories
            .AnyAsync(s => s.SubcategoryCode == dto.SubcategoryCode && s.SubcategoryId != id);
        if (codeConflict)
            throw new InvalidOperationException($"Subcategory code '{dto.SubcategoryCode}' already used.");

        var cat = await _uow.Categories.GetByIdAsync(dto.CategoryId)
            ?? throw new KeyNotFoundException($"Category {dto.CategoryId} not found.");

        entity.CategoryId      = dto.CategoryId;
        entity.SubcategoryCode = dto.SubcategoryCode;
        entity.SubcategoryName = dto.SubcategoryName;
        entity.IsActive        = dto.IsActive;
        entity.UpdatedAt       = DateTime.UtcNow;

        _uow.Subcategories.Update(entity);
        await _uow.SaveChangesAsync();

        // Re-fetch to include category info for the mapping
        var result = await _uow.Subcategories.Query()
            .Include(s => s.Category)
            .FirstOrDefaultAsync(s => s.SubcategoryId == id);

        return _mapper.Map<SubcategoryDto>(result);
    }

    public async Task DeleteSubcategoryAsync(int id)
    {
        var entity = await _uow.Subcategories.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Subcategory {id} not found.");

        bool hasProducts = await _uow.Products.AnyAsync(p => p.SubcategoryId == id);
        if (hasProducts)
            throw new InvalidOperationException(
                "Cannot delete a subcategory that has linked products. Deactivate it instead.");

        _uow.Subcategories.Remove(entity);
        await _uow.SaveChangesAsync();
    }
}
