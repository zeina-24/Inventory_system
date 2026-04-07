using AutoMapper;
using ImsPosSystem.Application.DTOs.Purchasing;
using ImsPosSystem.Application.Interfaces;
using ImsPosSystem.Application.Interfaces.Services;
using ImsPosSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ImsPosSystem.Infrastructure.Services;

public class PurchaseService : IPurchaseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PurchaseService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PurchaseInvoiceDTO>> GetAllPurchasesAsync()
    {
        var purchases = await _unitOfWork.PurchaseInvoices.Query()
            .Include(p => p.Supplier)
            .Include(p => p.PurchaseInvoiceLines)
                .ThenInclude(l => l.Product)
            .Include(p => p.PurchaseInvoiceLines)
                .ThenInclude(l => l.Location)
            .ToListAsync();

        return _mapper.Map<IEnumerable<PurchaseInvoiceDTO>>(purchases);
    }

    public async Task<PurchaseInvoiceDTO?> GetPurchaseByIdAsync(long id)
    {
        var purchase = await _unitOfWork.PurchaseInvoices.Query()
            .Include(p => p.Supplier)
            .Include(p => p.PurchaseInvoiceLines)
                .ThenInclude(l => l.Product)
            .Include(p => p.PurchaseInvoiceLines)
                .ThenInclude(l => l.Location)
            .FirstOrDefaultAsync(p => p.PurchaseInvoiceId == id);

        return _mapper.Map<PurchaseInvoiceDTO>(purchase);
    }

    public async Task<IEnumerable<PurchaseInvoiceDTO>> GetPurchasesBySupplierAsync(int supplierId)
    {
        var purchases = await _unitOfWork.PurchaseInvoices.Query()
            .Where(p => p.SupplierId == supplierId)
            .Include(p => p.Supplier)
            .Include(p => p.PurchaseInvoiceLines)
            .ToListAsync();

        return _mapper.Map<IEnumerable<PurchaseInvoiceDTO>>(purchases);
    }

    public async Task<PurchaseInvoiceDTO> CreatePurchaseInvoiceAsync(CreatePurchaseInvoiceDTO dto)
    {
        var invoice = _mapper.Map<PurchaseInvoice>(dto);
        invoice.Status = "CONFIRMED"; // For simplicity, we process immediately
        invoice.CreatedAt = DateTime.UtcNow;
        invoice.UpdatedAt = DateTime.UtcNow;

        // Calculate Header Totals
        decimal subTotal = 0;
        decimal totalDiscount = 0;

        foreach (var line in invoice.PurchaseInvoiceLines)
        {
            var lineSub = line.Quantity * line.UnitBuyingPrice;
            var lineDisc = (lineSub * line.DiscountPercent) / 100;
            
            subTotal += lineSub;
            totalDiscount += lineDisc;
        }

        invoice.SubTotal = subTotal;
        invoice.DiscountAmount = totalDiscount;
        invoice.TaxAmount = (subTotal - totalDiscount) * 0.15m; // Assume 15% VAT for demo
        invoice.TotalAmount = (subTotal - totalDiscount) + invoice.TaxAmount;

        // Atomic Transaction for Invoice creation + Stock Update
        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            // 1. Add the Invoice
            await _unitOfWork.PurchaseInvoices.AddAsync(invoice);
            await _unitOfWork.SaveChangesAsync(); // Get IDs

            // 2. Update Stock for each line
            foreach (var line in dto.Lines)
            {
                // Find or Create StockLedger entry
                var ledger = await _unitOfWork.StockLedgers.Query()
                    .FirstOrDefaultAsync(s => s.ProductId == line.ProductId && s.LocationId == line.LocationId);

                if (ledger == null)
                {
                    ledger = new ImsPosSystem.Domain.Entities.StockLedger
                    {
                        ProductId = line.ProductId,
                        LocationId = line.LocationId,
                        QtyOnHand = (int)line.Quantity,
                        LastMovedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.StockLedgers.AddAsync(ledger);
                }
                else
                {
                    ledger.QtyOnHand += (int)line.Quantity;
                    ledger.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.StockLedgers.Update(ledger);
                }

                // Update Product Buying Price to latest
                var product = await _unitOfWork.Products.GetByIdAsync(line.ProductId);
                if (product != null)
                {
                    product.BuyingPrice = line.UnitBuyingPrice;
                    product.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.Products.Update(product);
                }
            }

            await _unitOfWork.SaveChangesAsync();
        });

        return await GetPurchaseByIdAsync(invoice.PurchaseInvoiceId) ?? _mapper.Map<PurchaseInvoiceDTO>(invoice);
    }
}
