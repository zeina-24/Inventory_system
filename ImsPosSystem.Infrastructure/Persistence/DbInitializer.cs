using ImsPosSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ImsPosSystem.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task Initialize(ApplicationDbContext context)
    {
        // 1. Ensure any pending migrations are applied
        // await context.Database.MigrateAsync();

        // 2. Check if seeding is already done (looking for our specific ELEC category)
        if (await context.Categories.AnyAsync(c => c.CategoryCode == "ELEC"))
        {
            return; // DB has been seeded with our samples
        }

        // 3. Seed Categories
        var categories = new List<Category>
        {
            new Category { CategoryCode = "ELEC", CategoryName = "Electronics", IsActive = true },
            new Category { CategoryCode = "GROC", CategoryName = "Groceries", IsActive = true },
            new Category { CategoryCode = "CLOT", CategoryName = "Clothing", IsActive = true }
        };
        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();

        // 4. Seed Subcategories
        var electronics = categories.First(c => c.CategoryCode == "ELEC");
        var groceries = categories.First(c => c.CategoryCode == "GROC");
        
        var subcategories = new List<Subcategory>
        {
            new Subcategory { CategoryId = electronics.CategoryId, SubcategoryCode = "ELEC-MOB", SubcategoryName = "Mobile Phones", IsActive = true },
            new Subcategory { CategoryId = electronics.CategoryId, SubcategoryCode = "ELEC-LAP", SubcategoryName = "Laptops", IsActive = true },
            new Subcategory { CategoryId = groceries.CategoryId, SubcategoryCode = "GROC-DAI", SubcategoryName = "Dairy Products", IsActive = true }
        };
        await context.Subcategories.AddRangeAsync(subcategories);
        await context.SaveChangesAsync();

        // 5. Seed Suppliers
        var suppliers = new List<Supplier>
        {
            new Supplier { SupplierCode = "SUP-001", SupplierName = "Global Tech Source", Email = "contact@globaltech.com", Phone = "123456789", Address = "Tech City, Silicon Valley", IsActive = true },
            new Supplier { SupplierCode = "SUP-002", SupplierName = "Fresh Farms Ltd", Email = "sales@freshfarms.com", Phone = "987654321", Address = "Green Valley, Farm Lands", IsActive = true }
        };
        await context.Suppliers.AddRangeAsync(suppliers);
        await context.SaveChangesAsync();

        // 6. Seed Storage Locations
        var locations = new List<StorageLocation>
        {
            new StorageLocation { Description = "Main Warehouse - Section A", Aisle = "A1", Block = "B1", Shelf = "S1", IsActive = true },
            new StorageLocation { Description = "Cold Storage - Section C", Aisle = "C1", Block = "B1", Shelf = "S2", IsActive = true }
        };
        await context.StorageLocations.AddRangeAsync(locations);
        await context.SaveChangesAsync();

        // 7. Seed Products
        var mobileSub = subcategories.First(s => s.SubcategoryCode == "ELEC-MOB");
        var dairySub = subcategories.First(s => s.SubcategoryCode == "GROC-DAI");

        var products = new List<Product>
        {
            new Product 
            { 
                CategoryId = electronics.CategoryId, 
                SubcategoryId = mobileSub.SubcategoryId, 
                ProductName = "iPhone 15 Pro", 
                Barcode = "190198451234", 
                SerialNumber = "SN-IP15-001",
                BuyingPrice = 900.00m, 
                SellingPrice = 1199.00m, 
                Unit = "Piece",
                IsActive = true 
            },
            new Product 
            { 
                CategoryId = groceries.CategoryId, 
                SubcategoryId = dairySub.SubcategoryId, 
                ProductName = "Fresh Milk 1L", 
                Barcode = "500012345678", 
                SerialNumber = "SN-MILK-001",
                BuyingPrice = 1.20m, 
                SellingPrice = 1.99m, 
                Unit = "Bottle",
                IsActive = true 
            }
        };
        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();

        // 8. Seed Stock Ledger (Initial Quantity)
        var iphone = products.First(p => p.ProductName == "iPhone 15 Pro");
        var milk = products.First(p => p.ProductName == "Fresh Milk 1L");
        var mainLoc = locations.First(l => l.Description.Contains("Main"));
        var coldLoc = locations.First(l => l.Description.Contains("Cold"));

        var ledgerEntries = new List<StockLedger>
        {
            new StockLedger { ProductId = iphone.ProductId, LocationId = mainLoc.LocationId, QtyOnHand = 10, LastMovedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new StockLedger { ProductId = milk.ProductId, LocationId = coldLoc.LocationId, QtyOnHand = 50, LastMovedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };
        await context.StockLedgers.AddRangeAsync(ledgerEntries);
        await context.SaveChangesAsync();
    }
}
