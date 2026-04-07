using ImsPosSystem.Domain.Entities;

namespace ImsPosSystem.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    // --- Repositories ---
    IGenericRepository<Category> Categories { get; }
    IGenericRepository<Subcategory> Subcategories { get; }
    IGenericRepository<Product> Products { get; }
    IGenericRepository<StorageLocation> StorageLocations { get; }
    IGenericRepository<StockLedger> StockLedgers { get; }
    IGenericRepository<Supplier> Suppliers { get; }
    IGenericRepository<PurchaseInvoice> PurchaseInvoices { get; }
    IGenericRepository<PurchaseInvoiceLine> PurchaseInvoiceLines { get; }
    IGenericRepository<SupplierPayment> SupplierPayments { get; }
    IGenericRepository<SalesInvoice> SalesInvoices { get; }
    IGenericRepository<SalesInvoiceLine> SalesInvoiceLines { get; }
    IGenericRepository<User> Users { get; }

    // --- Persistence ---
    Task<int> SaveChangesAsync();

    // --- Explicit Transaction support (for Confirm Purchase & Complete Sale) ---
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();

    Task ExecuteInTransactionAsync(Func<Task> operation);
}
