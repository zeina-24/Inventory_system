using System.ComponentModel.DataAnnotations;

namespace ImsPosSystem.Application.DTOs.Purchasing;

public class SupplierDTO
{
    public int SupplierId { get; set; }
    public string SupplierCode { get; set; } = null!;
    public string SupplierName { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? TaxNumber { get; set; }
    public bool IsActive { get; set; }
}

public class CreateSupplierDTO
{
    [Required]
    [StringLength(20)]
    public string SupplierCode { get; set; } = null!;

    [Required]
    [StringLength(200)]
    public string SupplierName { get; set; } = null!;

    [StringLength(30)]
    public string? Phone { get; set; }

    [EmailAddress]
    [StringLength(150)]
    public string? Email { get; set; }

    [StringLength(400)]
    public string? Address { get; set; }

    [StringLength(50)]
    public string? TaxNumber { get; set; }
}

public class UpdateSupplierDTO : CreateSupplierDTO
{
    public bool IsActive { get; set; }
}
