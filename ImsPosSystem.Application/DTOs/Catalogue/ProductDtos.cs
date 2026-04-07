namespace ImsPosSystem.Application.DTOs.Catalogue;

// ── Product list / detail ─────────────────────────────────────────────────────

public record ProductDto(
    int ProductId,
    string SerialNumber,
    string ProductName,
    int CategoryId,
    string CategoryName,
    int SubcategoryId,
    string SubcategoryName,
    decimal BuyingPrice,
    decimal SellingPrice,
    string? Barcode,
    string Unit,
    int ReorderLevel,
    string? ImageUrl,
    string? Notes,
    bool IsActive,
    int QtyOnHand,          // SUM from StockLedger
    DateTime CreatedAt,
    DateTime UpdatedAt);

// ── Create / Update ───────────────────────────────────────────────────────────

public record CreateProductDto(
    string SerialNumber,
    string ProductName,
    int CategoryId,
    int SubcategoryId,
    decimal BuyingPrice,
    decimal SellingPrice,
    string? Barcode,
    string Unit,
    int ReorderLevel,
    string? ImageUrl,
    string? Notes,
    bool IsActive = true);

public record UpdateProductDto(
    string SerialNumber,
    string ProductName,
    int CategoryId,
    int SubcategoryId,
    decimal BuyingPrice,
    decimal SellingPrice,
    string? Barcode,
    string Unit,
    int ReorderLevel,
    string? ImageUrl,
    string? Notes,
    bool IsActive);

// ── Dropdown (used by POS / Purchase line pickers) ───────────────────────────
public record ProductLookupDto(
    int ProductId,
    string SerialNumber,
    string ProductName,
    decimal BuyingPrice,
    decimal SellingPrice,
    string? Barcode);
