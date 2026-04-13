using AutoMapper;
using ImsPosSystem.Application.DTOs.Purchasing;
using ImsPosSystem.Application.Interfaces;
using ImsPosSystem.Application.Mappings;
using ImsPosSystem.Domain.Entities;
using ImsPosSystem.Infrastructure.Services;
using ImsPosSystem.Tests.TestHelpers;
using Moq;
using NUnit.Framework;
using System.Linq.Expressions;

namespace ImsPosSystem.Tests.Services;

/// <summary>
/// Unit tests for <see cref="PurchaseService"/>.
/// These tests validate the invoice total calculation logic (SubTotal,
/// DiscountAmount, TaxAmount, TotalAmount) without a real database.
/// </summary>
[TestFixture]
public class PurchaseServiceTests
{
    // ── SUT & collaborator mocks ────────────────────────────────────────────
    private Mock<IUnitOfWork> _uowMock;
    private Mock<IGenericRepository<PurchaseInvoice>> _invoiceRepoMock;
    private Mock<IGenericRepository<PurchaseInvoiceLine>> _lineRepoMock;
    private Mock<IGenericRepository<StockLedger>> _ledgerRepoMock;
    private Mock<IGenericRepository<Product>> _productRepoMock;
    private Mock<IGenericRepository<Supplier>> _supplierRepoMock;
    private IMapper _mapper;
    private PurchaseService _sut;

    [SetUp]
    public void SetUp()
    {
        _uowMock          = new Mock<IUnitOfWork>();
        _invoiceRepoMock  = new Mock<IGenericRepository<PurchaseInvoice>>();
        _lineRepoMock     = new Mock<IGenericRepository<PurchaseInvoiceLine>>();
        _ledgerRepoMock   = new Mock<IGenericRepository<StockLedger>>();
        _productRepoMock  = new Mock<IGenericRepository<Product>>();
        _supplierRepoMock = new Mock<IGenericRepository<Supplier>>();

        _uowMock.Setup(u => u.PurchaseInvoices).Returns(_invoiceRepoMock.Object);
        _uowMock.Setup(u => u.PurchaseInvoiceLines).Returns(_lineRepoMock.Object);
        _uowMock.Setup(u => u.StockLedgers).Returns(_ledgerRepoMock.Object);
        _uowMock.Setup(u => u.Products).Returns(_productRepoMock.Object);
        _uowMock.Setup(u => u.Suppliers).Returns(_supplierRepoMock.Object);

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();

        _sut = new PurchaseService(_uowMock.Object, _mapper);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  Invoice total calculation — core business logic
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Given an invoice with two lines (no discount), verify that
    /// SubTotal, DiscountAmount, TaxAmount (15%), and TotalAmount
    /// are all computed correctly by CreatePurchaseInvoiceAsync.
    ///
    /// Line 1: Qty=10, UnitPrice=100  → lineSub=1000, discount=0
    /// Line 2: Qty= 5, UnitPrice= 80  → lineSub= 400, discount=0
    ///
    /// Expected:
    ///   SubTotal      = 1400
    ///   DiscountAmount = 0
    ///   TaxAmount      = 1400 × 0.15 = 210
    ///   TotalAmount    = 1400 + 210   = 1610
    /// </summary>
    [Test]
    public async Task CreatePurchaseInvoiceAsync_CalculatesInvoiceTotals_Correctly()
    {
        // Arrange
        var dto = new CreatePurchaseInvoiceDTO
        {
            SupplierId    = 1,
            InvoiceNumber = "INV-001",
            InvoiceDate   = DateOnly.FromDateTime(DateTime.UtcNow),
            Lines         = new List<CreatePurchaseInvoiceLineDTO>
            {
                new() { ProductId = 1, LocationId = 1, Quantity = 10, UnitBuyingPrice = 100m, DiscountPercent = 0m },
                new() { ProductId = 2, LocationId = 1, Quantity =  5, UnitBuyingPrice =  80m, DiscountPercent = 0m }
            }
        };

        // Stub SaveChangesAsync so the service can set invoice IDs after AddAsync
        _uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Capture the invoice entity added by the service
        PurchaseInvoice? capturedInvoice = null;
        _invoiceRepoMock
            .Setup(r => r.AddAsync(It.IsAny<PurchaseInvoice>()))
            .Callback<PurchaseInvoice>(inv => capturedInvoice = inv)
            .Returns(Task.CompletedTask);

        // ExecuteInTransactionAsync must run the action for the calculation to happen
        _uowMock
            .Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(action => action());

        // StockLedgers.Query() — return an EF-async-compatible empty queryable (no existing ledger entry)
        var emptyLedgers = new List<StockLedger>().AsAsyncQueryable();
        _ledgerRepoMock
            .Setup(r => r.Query())
            .Returns(emptyLedgers);

        // Products.GetByIdAsync — return a dummy product so the price update path runs
        _productRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<object>()))
            .ReturnsAsync(new Product { ProductId = 1, ProductName = "Test Product", BuyingPrice = 50m });

        // StockLedgers.AddAsync — no-op
        _ledgerRepoMock
            .Setup(r => r.AddAsync(It.IsAny<StockLedger>()))
            .Returns(Task.CompletedTask);

        // The final re-fetch (GetPurchaseByIdAsync) queries PurchaseInvoices.Query()
        // Return an EF-async-compatible empty queryable so it falls back to the already-mapped capturedInvoice
        var emptyInvoices = new List<PurchaseInvoice>().AsAsyncQueryable();
        _invoiceRepoMock
            .Setup(r => r.Query())
            .Returns(emptyInvoices);

        // Act
        var result = await _sut.CreatePurchaseInvoiceAsync(dto);

        // Assert — verify captured invoice totals
        Assert.That(capturedInvoice, Is.Not.Null, "Invoice entity should have been added");
        Assert.Multiple(() =>
        {
            Assert.That(capturedInvoice!.SubTotal,        Is.EqualTo(1400m),  "SubTotal must be 1400");
            Assert.That(capturedInvoice.DiscountAmount,   Is.EqualTo(0m),     "DiscountAmount must be 0");
            Assert.That(capturedInvoice.TaxAmount,        Is.EqualTo(210m),   "TaxAmount (15%) must be 210");
            Assert.That(capturedInvoice.TotalAmount,      Is.EqualTo(1610m),  "TotalAmount must be 1610");
            Assert.That(capturedInvoice.Status,           Is.EqualTo("CONFIRMED"));
        });
    }

    /// <summary>
    /// With a 20% line discount:
    ///   Line: Qty=10, UnitPrice=100, Discount=20%
    ///   lineSub  = 1000
    ///   lineDisc = 1000 × 0.20 = 200
    ///
    /// Expected:
    ///   SubTotal      = 1000
    ///   DiscountAmount = 200
    ///   TaxAmount      = (1000 - 200) × 0.15 = 120
    ///   TotalAmount    = 800 + 120 = 920
    /// </summary>
    [Test]
    public async Task CreatePurchaseInvoiceAsync_WithDiscount_CalculatesDiscountedTotals()
    {
        // Arrange
        var dto = new CreatePurchaseInvoiceDTO
        {
            SupplierId    = 1,
            InvoiceNumber = "INV-002",
            InvoiceDate   = DateOnly.FromDateTime(DateTime.UtcNow),
            Lines         = new List<CreatePurchaseInvoiceLineDTO>
            {
                new() { ProductId = 1, LocationId = 1, Quantity = 10, UnitBuyingPrice = 100m, DiscountPercent = 20m }
            }
        };

        _uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        PurchaseInvoice? capturedInvoice = null;
        _invoiceRepoMock
            .Setup(r => r.AddAsync(It.IsAny<PurchaseInvoice>()))
            .Callback<PurchaseInvoice>(inv => capturedInvoice = inv)
            .Returns(Task.CompletedTask);

        _uowMock
            .Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(action => action());

        _ledgerRepoMock.Setup(r => r.Query()).Returns(new List<StockLedger>().AsAsyncQueryable());
        _ledgerRepoMock.Setup(r => r.AddAsync(It.IsAny<StockLedger>())).Returns(Task.CompletedTask);
        _productRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<object>()))
            .ReturnsAsync(new Product { ProductId = 1, ProductName = "Test", BuyingPrice = 50m });
        _invoiceRepoMock.Setup(r => r.Query()).Returns(new List<PurchaseInvoice>().AsAsyncQueryable());

        // Act
        await _sut.CreatePurchaseInvoiceAsync(dto);

        // Assert
        Assert.That(capturedInvoice, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(capturedInvoice!.SubTotal,       Is.EqualTo(1000m));
            Assert.That(capturedInvoice.DiscountAmount,  Is.EqualTo(200m));
            Assert.That(capturedInvoice.TaxAmount,       Is.EqualTo(120m));
            Assert.That(capturedInvoice.TotalAmount,     Is.EqualTo(920m));
        });
    }

    // ════════════════════════════════════════════════════════════════════════
    //  GetAllPurchasesAsync / GetPurchasesBySupplierAsync — basic routing
    // ════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task GetAllPurchasesAsync_ReturnsEmptyList_WhenNoPurchasesExist()
    {
        // Arrange — BuildMock() gives an EF-Core async-compatible queryable
        var mockInvoices = new List<PurchaseInvoice>().AsAsyncQueryable();
        _invoiceRepoMock
            .Setup(r => r.Query())
            .Returns(mockInvoices);

        // Act
        var result = await _sut.GetAllPurchasesAsync();

        // Assert
        Assert.That(result, Is.Empty);
    }
}
