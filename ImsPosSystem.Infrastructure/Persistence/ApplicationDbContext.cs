using System;
using System.Collections.Generic;
using ImsPosSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ImsPosSystem.Infrastructure.Persistence;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<PurchaseInvoice> PurchaseInvoices { get; set; }

    public virtual DbSet<PurchaseInvoiceLine> PurchaseInvoiceLines { get; set; }

    public virtual DbSet<SalesInvoice> SalesInvoices { get; set; }

    public virtual DbSet<SalesInvoiceLine> SalesInvoiceLines { get; set; }

    public virtual DbSet<StockLedger> StockLedgers { get; set; }

    public virtual DbSet<StorageLocation> StorageLocations { get; set; }

    public virtual DbSet<Subcategory> Subcategories { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<SupplierPayment> SupplierPayments { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Connection string is configured via DI in Program.cs / InfrastructureServiceExtensions.
        // Do NOT add a hardcoded connection string here.
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasIndex(e => e.CategoryCode, "UQ_Categories_Code").IsUnique();

            entity.HasIndex(e => e.CategoryName, "UQ_Categories_Name").IsUnique();

            entity.Property(e => e.CategoryId)
                .HasColumnName("CategoryID")
                .ValueGeneratedOnAdd();
            entity.Property(e => e.CategoryCode)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CategoryName).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.HasIndex(e => e.Barcode, "UQ_Products_Barcode").IsUnique();

            entity.HasIndex(e => e.SerialNumber, "UQ_Products_Serial").IsUnique();

            entity.Property(e => e.ProductId)
                .HasColumnName("ProductID")
                .ValueGeneratedOnAdd();
            entity.Property(e => e.Barcode)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.BuyingPrice).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .HasColumnName("ImageURL");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.ProductName).HasMaxLength(200);
            entity.Property(e => e.SellingPrice).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.SerialNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SubcategoryId).HasColumnName("SubcategoryID");
            entity.Property(e => e.Unit)
                .HasMaxLength(30)
                .HasDefaultValue("Piece");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Products_Category");

            entity.HasOne(d => d.Subcategory).WithMany(p => p.Products)
                .HasForeignKey(d => d.SubcategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Products_Subcategory");
        });

        modelBuilder.Entity<PurchaseInvoice>(entity =>
        {
            entity.HasIndex(e => e.InvoiceNumber, "UQ_PurchaseInvoices_Number").IsUnique();

            entity.Property(e => e.PurchaseInvoiceId).HasColumnName("PurchaseInvoiceID");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.InvoiceDate).HasDefaultValueSql("(CONVERT([date],getdate()))");
            entity.Property(e => e.InvoiceNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("DRAFT");
            entity.Property(e => e.SubTotal).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.SupplierId).HasColumnName("SupplierID");
            entity.Property(e => e.TaxAmount).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Supplier).WithMany(p => p.PurchaseInvoices)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PurchaseInvoices_Supplier");
        });

        modelBuilder.Entity<PurchaseInvoiceLine>(entity =>
        {
            entity.HasKey(e => e.LineId);

            entity.Property(e => e.LineId).HasColumnName("LineID");
            entity.Property(e => e.DiscountPercent).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.LineDiscount)
                .HasComputedColumnSql("((([Quantity]*[UnitBuyingPrice])*[DiscountPercent])/(100))", true)
                .HasColumnType("decimal(38, 6)");
            entity.Property(e => e.LineSubTotal)
                .HasComputedColumnSql("([Quantity]*[UnitBuyingPrice])", true)
                .HasColumnType("decimal(37, 8)");
            entity.Property(e => e.LineTotal)
                .HasComputedColumnSql("(([Quantity]*[UnitBuyingPrice])*((1)-[DiscountPercent]/(100)))", true)
                .HasColumnType("decimal(38, 6)");
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.Notes).HasMaxLength(200);
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.PurchaseInvoiceId).HasColumnName("PurchaseInvoiceID");
            entity.Property(e => e.Quantity).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.UnitBuyingPrice).HasColumnType("decimal(18, 4)");

            entity.HasOne(d => d.Location).WithMany(p => p.PurchaseInvoiceLines)
                .HasForeignKey(d => d.LocationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PurchaseInvoiceLines_Location");

            entity.HasOne(d => d.Product).WithMany(p => p.PurchaseInvoiceLines)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PurchaseInvoiceLines_Product");

            entity.HasOne(d => d.PurchaseInvoice).WithMany(p => p.PurchaseInvoiceLines)
                .HasForeignKey(d => d.PurchaseInvoiceId)
                .HasConstraintName("FK_PurchaseInvoiceLines_Invoice");
        });

        modelBuilder.Entity<SalesInvoice>(entity =>
        {
            entity.HasIndex(e => e.InvoiceNumber, "UQ_SalesInvoices_Number").IsUnique();

            entity.Property(e => e.SalesInvoiceId).HasColumnName("SalesInvoiceID");
            entity.Property(e => e.CashierName).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.CustomerName).HasMaxLength(200);
            entity.Property(e => e.CustomerPhone)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.InvoiceDate)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.InvoiceNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("CASH");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("COMPLETED");
            entity.Property(e => e.SubTotal).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.TaxAmount).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 4)");
        });

        modelBuilder.Entity<SalesInvoiceLine>(entity =>
        {
            entity.HasKey(e => e.LineId);

            entity.Property(e => e.LineId).HasColumnName("LineID");
            entity.Property(e => e.DiscountPercent).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.LineDiscount)
                .HasComputedColumnSql("((([Quantity]*[UnitSellingPrice])*[DiscountPercent])/(100))", true)
                .HasColumnType("decimal(38, 6)");
            entity.Property(e => e.LineSubTotal)
                .HasComputedColumnSql("([Quantity]*[UnitSellingPrice])", true)
                .HasColumnType("decimal(37, 8)");
            entity.Property(e => e.LineTotal)
                .HasComputedColumnSql("(([Quantity]*[UnitSellingPrice])*((1)-[DiscountPercent]/(100)))", true)
                .HasColumnType("decimal(38, 6)");
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.Quantity).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.SalesInvoiceId).HasColumnName("SalesInvoiceID");
            entity.Property(e => e.UnitCostSnapshot).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.UnitSellingPrice).HasColumnType("decimal(18, 4)");

            entity.HasOne(d => d.Location).WithMany(p => p.SalesInvoiceLines)
                .HasForeignKey(d => d.LocationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SalesInvoiceLines_Location");

            entity.HasOne(d => d.Product).WithMany(p => p.SalesInvoiceLines)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SalesInvoiceLines_Product");

            entity.HasOne(d => d.SalesInvoice).WithMany(p => p.SalesInvoiceLines)
                .HasForeignKey(d => d.SalesInvoiceId)
                .HasConstraintName("FK_SalesInvoiceLines_Invoice");
        });

        modelBuilder.Entity<StockLedger>(entity =>
        {
            entity.HasKey(e => e.LedgerId);

            entity.ToTable("StockLedger");

            entity.HasIndex(e => new { e.ProductId, e.LocationId }, "UQ_StockLedger_ProductLoc").IsUnique();

            entity.Property(e => e.LedgerId).HasColumnName("LedgerID");
            entity.Property(e => e.LastMovedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Location).WithMany(p => p.StockLedgers)
                .HasForeignKey(d => d.LocationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StockLedger_Location");

            entity.HasOne(d => d.Product).WithMany(p => p.StockLedgers)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StockLedger_Product");
        });

        modelBuilder.Entity<StorageLocation>(entity =>
        {
            entity.ToTable("StorageLocations");
            entity.HasKey(e => e.LocationId);

            entity.Property(e => e.LocationId)
                .HasColumnName("LocationID")
                .ValueGeneratedOnAdd();
            entity.Property(e => e.Aisle)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Block)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Shelf)
                .HasMaxLength(10)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Subcategory>(entity =>
        {
            entity.ToTable("Subcategories");
            entity.HasIndex(e => e.SubcategoryCode, "UQ_Subcategories_Code").IsUnique();

            entity.Property(e => e.SubcategoryId)
                .HasColumnName("SubcategoryID")
                .ValueGeneratedOnAdd();
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SubcategoryCode)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.SubcategoryName).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Category).WithMany(p => p.Subcategories)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Subcategories_Category");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasIndex(e => e.SupplierCode, "UQ_Suppliers_Code").IsUnique();

            entity.HasIndex(e => e.Email, "UQ_Suppliers_Email").IsUnique();

            entity.Property(e => e.SupplierId).HasColumnName("SupplierID");
            entity.Property(e => e.Address).HasMaxLength(400);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Phone)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.SupplierCode)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.SupplierName).HasMaxLength(200);
            entity.Property(e => e.TaxNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
        });

        modelBuilder.Entity<SupplierPayment>(entity =>
        {
            entity.HasKey(e => e.PaymentId);

            entity.Property(e => e.PaymentId).HasColumnName("PaymentID");
            entity.Property(e => e.AmountPaid).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.PaymentDate).HasDefaultValueSql("(CONVERT([date],getdate()))");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("BANK_TRANSFER");
            entity.Property(e => e.PurchaseInvoiceId).HasColumnName("PurchaseInvoiceID");
            entity.Property(e => e.ReferenceNumber)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.SupplierId).HasColumnName("SupplierID");

            entity.HasOne(d => d.PurchaseInvoice).WithMany(p => p.SupplierPayments)
                .HasForeignKey(d => d.PurchaseInvoiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SupplierPayments_PurchaseInvoice");

            entity.HasOne(d => d.Supplier).WithMany(p => p.SupplierPayments)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SupplierPayments_Supplier");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC07EB255FAD");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E4CD2F02A7").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.RefreshToken).HasMaxLength(500);
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasDefaultValue("Admin");
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
