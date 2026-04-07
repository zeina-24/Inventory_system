using ImsPosSystem.Application.DTOs.Purchasing;

namespace ImsPosSystem.Application.Interfaces.Services;

public interface ISupplierService
{
    Task<IEnumerable<SupplierDTO>> GetAllSuppliersAsync();
    Task<SupplierDTO?> GetSupplierByIdAsync(int id);
    Task<SupplierDTO> CreateSupplierAsync(CreateSupplierDTO dto);
    Task<SupplierDTO> UpdateSupplierAsync(int id, UpdateSupplierDTO dto);
    Task DeleteSupplierAsync(int id);
}
