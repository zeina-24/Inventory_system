using ImsPosSystem.Application.Interfaces;
using ImsPosSystem.Application.Interfaces.Services;
using ImsPosSystem.Infrastructure.Persistence;
using ImsPosSystem.Infrastructure.Services;
using ImsPosSystem.Application.Mappings; // Added this
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ImsPosSystem.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register EF Core DbContext with SQL Server
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null)));

        // Register Unit of Work (scoped = one per HTTP request)
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register AutoMapper
        services.AddAutoMapper(typeof(MappingProfile).Assembly);

        // ── Catalogue services ────────────────────────────────────────────────
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ISubcategoryService, SubcategoryService>();
        services.AddScoped<IProductService, ProductService>();

        // ── Warehouse services ────────────────────────────────────────────────
        services.AddScoped<IStorageLocationService, StorageLocationService>();
        services.AddScoped<IStockLedgerService, StockLedgerService>();

        // ── Purchasing services ───────────────────────────────────────────────
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<IPurchaseService, PurchaseService>();

        return services;
    }
}
