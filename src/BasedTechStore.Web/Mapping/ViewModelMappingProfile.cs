using AutoMapper;
using BasedTechStore.Application.DTOs.Categories;
using BasedTechStore.Application.DTOs.Product;
using BasedTechStore.Application.DTOs.Specifications;
using BasedTechStore.Web.ViewModels.Categories;
using BasedTechStore.Web.ViewModels.Products;
using BasedTechStore.Web.ViewModels.Specifications;

namespace BasedTechStore.Application.Mapping
{
    public class ViewModelMappingProfile : Profile
    {
        public ViewModelMappingProfile()
        {
            // Category Mapping
            CreateMap<CategoryDto, CategoryItemVM>()
                .ForMember(dest => dest.SubCategories, opt => opt.MapFrom(src => src.SubCategories));
            CreateMap<CategoryItemVM, CategoryDto>()
                .ForMember(dest => dest.SubCategories, opt => opt.MapFrom(src => src.SubCategories));

            // SubCategory Mapping
            CreateMap<SubCategoryDto, SubCategoryItemVM>();
            CreateMap<SubCategoryItemVM, SubCategoryDto>();

            // Product Mapping
            CreateMap<ProductDto, ProductItemVM>();
            CreateMap<ProductItemVM, ProductDto>();

            // Categories <-> ViewModels
            CreateMap<CategoryDto, CategoryItemVM>();
            CreateMap<CategoryItemVM, CategoryDto>();

            CreateMap<SubCategoryDto, SubCategoryItemVM>();
            CreateMap<SubCategoryItemVM, SubCategoryDto>();

            // Products <-> ViewModels
            CreateMap<ProductDto, ProductItemVM>();
            CreateMap<ProductDto, ProductDetailsDto>()
                .ForMember(dest => dest.SpecificationGroups, opt => opt.Ignore());
            CreateMap<ProductItemVM, ProductDto>();
            CreateMap<ProductDetailsDto, ProductDetailsVM>()
                .ReverseMap();

            // Specifications <-> ViewModels
            CreateMap<SpecificationCategoryDto, SpecificationCategoryVM>();
            CreateMap<SpecificationCategoryVM, SpecificationCategoryDto>();

            CreateMap<SpecificationCategoryGroupDto, SpecificationCategoryGroupVM>()
                .ReverseMap();

            CreateMap<SpecificationTypeDto, SpecificationTypeVM>();
            CreateMap<SpecificationTypeVM, SpecificationTypeDto>();

            CreateMap<ProductSpecificationDto, ProductSpecificationVM>();
            CreateMap<ProductSpecificationVM, ProductSpecificationDto>();

            // Додатковий маппінг для коректної роботи з формами
            CreateMap<string, CategoryItemVM>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.SubCategories, opt => opt.Ignore());
        }
    }
}
