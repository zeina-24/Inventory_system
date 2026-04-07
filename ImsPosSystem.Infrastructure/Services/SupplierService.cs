using AutoMapper;
using ImsPosSystem.Application.DTOs.Purchasing;
using ImsPosSystem.Application.Interfaces;
using ImsPosSystem.Application.Interfaces.Services;
using ImsPosSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ImsPosSystem.Infrastructure.Services;

public class SupplierService : ISupplierService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SupplierService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<SupplierDTO>> GetAllSuppliersAsync()
    {
        var suppliers = await _unitOfWork.Suppliers.GetAllAsync();
        return _mapper.Map<IEnumerable<SupplierDTO>>(suppliers);
    }

    public async Task<SupplierDTO?> GetSupplierByIdAsync(int id)
    {
        var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id);
        return _mapper.Map<SupplierDTO>(supplier);
    }

    public async Task<SupplierDTO> CreateSupplierAsync(CreateSupplierDTO dto)
    {
        var supplier = _mapper.Map<Supplier>(dto);
        supplier.IsActive = true;
        supplier.CreatedAt = DateTime.UtcNow;
        supplier.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Suppliers.AddAsync(supplier);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<SupplierDTO>(supplier);
    }

    public async Task<SupplierDTO> UpdateSupplierAsync(int id, UpdateSupplierDTO dto)
    {
        var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id);
        if (supplier == null)
            throw new Exception("Supplier not found");

        _mapper.Map(dto, supplier);
        supplier.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Suppliers.Update(supplier);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<SupplierDTO>(supplier);
    }

    public async Task DeleteSupplierAsync(int id)
    {
        var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id);
        if (supplier != null)
        {
            // Implementation of Soft Delete if needed, here just hard delete for now or follow soft delete pattern
            // If soft delete is required, supplier.IsActive = false;
            _unitOfWork.Suppliers.Remove(supplier);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
