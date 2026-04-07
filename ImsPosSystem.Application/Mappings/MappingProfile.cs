using AutoMapper;
using ImsPosSystem.Application.DTOs.Catalogue;
using ImsPosSystem.Application.DTOs.Warehouse;
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
    }
}
