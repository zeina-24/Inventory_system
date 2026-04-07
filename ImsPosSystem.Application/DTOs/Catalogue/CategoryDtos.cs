namespace ImsPosSystem.Application.DTOs.Catalogue;

// ── Category DTOs ────────────────────────────────────────────────────────────

public record CategoryDto(
    int CategoryId,
    string CategoryCode,
    string CategoryName,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record CreateCategoryDto(
    string CategoryCode,
    string CategoryName,
    bool IsActive = true);

public record UpdateCategoryDto(
    string CategoryCode,
    string CategoryName,
    bool IsActive);

// ── Subcategory DTOs ─────────────────────────────────────────────────────────

public record SubcategoryDto(
    int SubcategoryId,
    int CategoryId,
    string CategoryName,
    string SubcategoryCode,
    string SubcategoryName,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record CreateSubcategoryDto(
    int CategoryId,
    string SubcategoryCode,
    string SubcategoryName,
    bool IsActive = true);

public record UpdateSubcategoryDto(
    int CategoryId,
    string SubcategoryCode,
    string SubcategoryName,
    bool IsActive);
