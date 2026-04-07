namespace ImsPosSystem.Application.DTOs.Warehouse;

// ── StorageLocation DTOs ─────────────────────────────────────────────────────

public record StorageLocationDto(
    int LocationId,
    string? Block,
    string? Aisle,
    string? Shelf,
    string LocationCode,    // Block-Aisle-Shelf — assembled in service layer
    string? Description,
    int? MaxCapacity,
    bool IsActive,
    DateTime CreatedAt);

public record CreateStorageLocationDto(
    string? Block,
    string? Aisle,
    string? Shelf,
    string? Description,
    int? MaxCapacity,
    bool IsActive = true);

public record UpdateStorageLocationDto(
    string? Block,
    string? Aisle,
    string? Shelf,
    string? Description,
    int? MaxCapacity,
    bool IsActive);

// ── Stock Ledger (read-only joined view) ─────────────────────────────────────

public record StockLedgerDto(
    long LedgerId,
    int ProductId,
    string SerialNumber,
    string ProductName,
    int LocationId,
    string LocationCode,    // Block-Aisle-Shelf
    string? Block,
    string? Aisle,
    string? Shelf,
    int QtyOnHand,
    int ReorderLevel,
    bool IsLowStock,        // QtyOnHand < ReorderLevel
    DateTime LastMovedAt,
    DateTime UpdatedAt);

// ── Stock Transfer ────────────────────────────────────────────────────────────
public record StockTransferDto(
    int ProductId,
    int FromLocationId,
    int ToLocationId,
    int Quantity,
    string? Notes);
