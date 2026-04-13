using AutoMapper;
using ImsPosSystem.Application.DTOs.Catalogue;
using ImsPosSystem.Application.DTOs.Warehouse;
using ImsPosSystem.Application.DTOs.Purchasing;
using ImsPosSystem.Application.DTOs.POS;
using ImsPosSystem.Domain.Entities;

namespace ImsPosSystem.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ── Catalogue ────────────────────────────────────────────────────────
        CreateMap<Category, CategoryDto>();
        CreateMap<Subcategory, SubcategoryDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.CategoryName));

        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.CategoryName))
            .ForMember(dest => dest.SubcategoryName, opt => opt.MapFrom(src => src.Subcategory.SubcategoryName));

        // ── Warehouse ────────────────────────────────────────────────────────
        CreateMap<StorageLocation, StorageLocationDto>()
            .ForMember(dest => dest.LocationCode, opt => opt.MapFrom(l => 
                $"{l.Block ?? ""}-{l.Aisle ?? ""}-{l.Shelf ?? ""}"));

        CreateMap<StockLedger, StockLedgerDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.ProductName))
            .ForMember(dest => dest.LocationCode, opt => opt.MapFrom(l => 
                $"{l.Location.Block ?? ""}-{l.Location.Aisle ?? ""}-{l.Location.Shelf ?? ""}"))
            .ForMember(dest => dest.Block, opt => opt.MapFrom(src => src.Location.Block))
            .ForMember(dest => dest.Aisle, opt => opt.MapFrom(src => src.Location.Aisle))
            .ForMember(dest => dest.Shelf, opt => opt.MapFrom(src => src.Location.Shelf))
            .ForMember(dest => dest.ReorderLevel, opt => opt.MapFrom(src => src.Product.ReorderLevel))
            .ForMember(dest => dest.IsLowStock, opt => opt.MapFrom(src => src.QtyOnHand < src.Product.ReorderLevel));

        // ── Purchasing ───────────────────────────────────────────────────────
        CreateMap<Supplier, SupplierDTO>();
        CreateMap<CreateSupplierDTO, Supplier>();
        CreateMap<UpdateSupplierDTO, Supplier>();

        CreateMap<PurchaseInvoice, PurchaseInvoiceDTO>()
            .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier.SupplierName))
            .ForMember(dest => dest.Lines, opt => opt.MapFrom(src => src.PurchaseInvoiceLines));

        CreateMap<PurchaseInvoiceLine, PurchaseInvoiceLineDTO>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.ProductName))
            .ForMember(dest => dest.LocationName, opt => opt.MapFrom(src => src.Location.Description));

        CreateMap<CreatePurchaseInvoiceDTO, PurchaseInvoice>()
            .ForMember(dest => dest.PurchaseInvoiceLines, opt => opt.MapFrom(src => src.Lines));

        CreateMap<CreatePurchaseInvoiceLineDTO, PurchaseInvoiceLine>();

        // ── POS ──────────────────────────────────────────────────────────────
        CreateMap<SalesInvoice, SalesInvoiceDTO>()
            .ForMember(dest => dest.Lines, opt => opt.MapFrom(src => src.SalesInvoiceLines));

        CreateMap<SalesInvoiceLine, SalesInvoiceLineDTO>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.ProductName))
            .ForMember(dest => dest.LocationName, opt => opt.MapFrom(src => src.Location.Description));

        CreateMap<CreateSalesInvoiceDTO, SalesInvoice>()
            .ForMember(dest => dest.SalesInvoiceLines, opt => opt.MapFrom(src => src.Lines));

        CreateMap<CreateSalesInvoiceLineDTO, SalesInvoiceLine>();
    }
}
