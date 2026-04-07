using ImsPosSystem.Application.DTOs.Purchasing;

namespace ImsPosSystem.Application.Interfaces.Services;

public interface IPurchaseService
{
    Task<IEnumerable<PurchaseInvoiceDTO>> GetAllPurchasesAsync();
    Task<PurchaseInvoiceDTO?> GetPurchaseByIdAsync(long id);
    Task<IEnumerable<PurchaseInvoiceDTO>> GetPurchasesBySupplierAsync(int supplierId);
    Task<PurchaseInvoiceDTO> CreatePurchaseInvoiceAsync(CreatePurchaseInvoiceDTO dto);
}
