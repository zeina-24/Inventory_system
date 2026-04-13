using AutoMapper;
using ImsPosSystem.Application.DTOs.POS;
using ImsPosSystem.Application.Interfaces;
using ImsPosSystem.Application.Interfaces.Services;
using ImsPosSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ImsPosSystem.Infrastructure.Services;

public class SalesService : ISalesService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SalesService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<SalesInvoiceDTO>> GetAllSalesAsync()
    {
        var sales = await _unitOfWork.SalesInvoices.Query()
            .Include(s => s.SalesInvoiceLines)
                .ThenInclude(l => l.Product)
            .Include(s => s.SalesInvoiceLines)
                .ThenInclude(l => l.Location)
            .ToListAsync();

        return _mapper.Map<IEnumerable<SalesInvoiceDTO>>(sales);
    }

    public async Task<SalesInvoiceDTO?> GetSaleByIdAsync(long id)
    {
        var sale = await _unitOfWork.SalesInvoices.Query()
            .Include(s => s.SalesInvoiceLines)
                .ThenInclude(l => l.Product)
            .Include(s => s.SalesInvoiceLines)
                .ThenInclude(l => l.Location)
            .FirstOrDefaultAsync(s => s.SalesInvoiceId == id);

        return _mapper.Map<SalesInvoiceDTO>(sale);
    }

    public async Task<SalesInvoiceDTO> CreateSaleAsync(CreateSalesInvoiceDTO dto)
    {
        var invoice = _mapper.Map<SalesInvoice>(dto);
        invoice.InvoiceDate = DateTime.UtcNow;
        invoice.Status = "COMPLETED";
        invoice.CreatedAt = DateTime.UtcNow;

        decimal subTotal = 0;
        decimal totalDiscount = 0;

        // Atomic Transaction for Sale creation + Stock Update
        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            foreach (var lineDto in dto.Lines)
            {
                // 1. Fetch Product and Location Stock
                var product = await _unitOfWork.Products.GetByIdAsync(lineDto.ProductId);
                if (product == null) throw new Exception($"Product ID {lineDto.ProductId} not found.");

                var ledger = await _unitOfWork.StockLedgers.Query()
                    .FirstOrDefaultAsync(s => s.ProductId == lineDto.ProductId && s.LocationId == lineDto.LocationId);

                // 2. Stock Availability Check
                if (ledger == null || ledger.QtyOnHand < lineDto.Quantity)
                {
                    throw new Exception($"Insufficient stock for {product.ProductName} in the selected location. Available: {(ledger?.QtyOnHand ?? 0)}, Requested: {lineDto.Quantity}");
                }

                // 3. Create Line Entity
                var line = new SalesInvoiceLine
                {
                    ProductId = lineDto.ProductId,
                    LocationId = lineDto.LocationId,
                    Quantity = lineDto.Quantity,
                    UnitSellingPrice = product.SellingPrice, // Use current product price
                    DiscountPercent = lineDto.DiscountPercent,
                    UnitCostSnapshot = product.BuyingPrice, // Capture cost for profit analysis
                    
                    // Note: LineSubTotal, LineDiscount, LineTotal are computed by SQL Server usually,
                    // but we need to calculate them if we want to update the header SubTotal here.
                };

                var lineSub = line.Quantity * line.UnitSellingPrice;
                var lineDisc = (lineSub * line.DiscountPercent) / 100;
                
                subTotal += lineSub;
                totalDiscount += lineDisc;

                invoice.SalesInvoiceLines.Add(line);

                // 4. Update Stock
                ledger.QtyOnHand -= (int)line.Quantity;
                ledger.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.StockLedgers.Update(ledger);
            }

            // 5. Finalize Header Totals
            invoice.SubTotal = subTotal;
            invoice.DiscountAmount = totalDiscount;
            invoice.TaxAmount = (subTotal - totalDiscount) * 0.15m;
            invoice.TotalAmount = (subTotal - totalDiscount) + invoice.TaxAmount;

            // 6. Save Everything
            await _unitOfWork.SalesInvoices.AddAsync(invoice);
            await _unitOfWork.SaveChangesAsync();
        });

        return await GetSaleByIdAsync(invoice.SalesInvoiceId) ?? _mapper.Map<SalesInvoiceDTO>(invoice);
    }
}
