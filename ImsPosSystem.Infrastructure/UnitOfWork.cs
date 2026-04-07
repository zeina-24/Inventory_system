using ImsPosSystem.Application.Interfaces;
using ImsPosSystem.Domain.Entities;
using ImsPosSystem.Infrastructure.Persistence;
using ImsPosSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ImsPosSystem.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    // --- Repository backing fields ---
    private IGenericRepository<Category>? _categories;
    private IGenericRepository<Subcategory>? _subcategories;
    private IGenericRepository<Product>? _products;
    private IGenericRepository<StorageLocation>? _storageLocations;
    private IGenericRepository<StockLedger>? _stockLedgers;
    private IGenericRepository<Supplier>? _suppliers;
    private IGenericRepository<PurchaseInvoice>? _purchaseInvoices;
    private IGenericRepository<PurchaseInvoiceLine>? _purchaseInvoiceLines;
    private IGenericRepository<SupplierPayment>? _supplierPayments;
    private IGenericRepository<SalesInvoice>? _salesInvoices;
    private IGenericRepository<SalesInvoiceLine>? _salesInvoiceLines;
    private IGenericRepository<User>? _users;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    // --- Lazy-initialized repositories ---
    public IGenericRepository<Category> Categories
        => _categories ??= new GenericRepository<Category>(_context);

    public IGenericRepository<Subcategory> Subcategories
        => _subcategories ??= new GenericRepository<Subcategory>(_context);

    public IGenericRepository<Product> Products
        => _products ??= new GenericRepository<Product>(_context);

    public IGenericRepository<StorageLocation> StorageLocations
        => _storageLocations ??= new GenericRepository<StorageLocation>(_context);

    public IGenericRepository<StockLedger> StockLedgers
        => _stockLedgers ??= new GenericRepository<StockLedger>(_context);

    public IGenericRepository<Supplier> Suppliers
        => _suppliers ??= new GenericRepository<Supplier>(_context);

    public IGenericRepository<PurchaseInvoice> PurchaseInvoices
        => _purchaseInvoices ??= new GenericRepository<PurchaseInvoice>(_context);

    public IGenericRepository<PurchaseInvoiceLine> PurchaseInvoiceLines
        => _purchaseInvoiceLines ??= new GenericRepository<PurchaseInvoiceLine>(_context);

    public IGenericRepository<SupplierPayment> SupplierPayments
        => _supplierPayments ??= new GenericRepository<SupplierPayment>(_context);

    public IGenericRepository<SalesInvoice> SalesInvoices
        => _salesInvoices ??= new GenericRepository<SalesInvoice>(_context);

    public IGenericRepository<SalesInvoiceLine> SalesInvoiceLines
        => _salesInvoiceLines ??= new GenericRepository<SalesInvoiceLine>(_context);

    public IGenericRepository<User> Users
        => _users ??= new GenericRepository<User>(_context);

    // --- Persistence ---
    public async Task<int> SaveChangesAsync()
        => await _context.SaveChangesAsync();

    // --- Explicit Transaction (Confirm Purchase / Complete Sale) ---
    public async Task BeginTransactionAsync()
        => _transaction = await _context.Database.BeginTransactionAsync();

    public async Task CommitTransactionAsync()
    {
        if (_transaction == null)
            throw new InvalidOperationException("No active transaction to commit.");
        await _transaction.CommitAsync();
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction == null) return;
        await _transaction.RollbackAsync();
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task ExecuteInTransactionAsync(Func<Task> operation)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await operation();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
