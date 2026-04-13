using ImsPosSystem.Application.DTOs.POS;

namespace ImsPosSystem.Application.Interfaces.Services;

public interface ISalesService
{
    Task<IEnumerable<SalesInvoiceDTO>> GetAllSalesAsync();
    Task<SalesInvoiceDTO?> GetSaleByIdAsync(long id);
    Task<SalesInvoiceDTO> CreateSaleAsync(CreateSalesInvoiceDTO dto);
}
