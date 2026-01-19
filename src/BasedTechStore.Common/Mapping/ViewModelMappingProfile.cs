using AutoMapper;
using BasedTechStore.Application.DTOs.Cart;
using BasedTechStore.Application.DTOs.Categories;
using BasedTechStore.Application.DTOs.Identity;
using BasedTechStore.Application.DTOs.Product;
using BasedTechStore.Application.DTOs.Specifications;
using BasedTechStore.Common.ViewModels.Auth;
using BasedTechStore.Common.ViewModels.Cart;
using BasedTechStore.Common.ViewModels.Categories;
using BasedTechStore.Common.ViewModels.Products;
using BasedTechStore.Common.ViewModels.Specifications;

namespace BasedTechStore.Common.Mapping
{
    public class ViewModelMappingProfile : Profile
    {
        public ViewModelMappingProfile()
        {
            // Identity <-> IdentityVMs
            CreateMap<SignInDto, SignInVM>().ReverseMap();
            CreateMap<SignUpDto, SignUpVM>().ReverseMap();

            // Category <-> CategoriesVMs
            CreateMap<CategoryDto, CategoryItemVM>()
                .ForMember(dest => dest.SubCategories, opt => opt.MapFrom(src => src.SubCategories));
            CreateMap<CategoryItemVM, CategoryDto>()
                .ForMember(dest => dest.SubCategories, opt => opt.MapFrom(src => src.SubCategories));

            // SubCategory <-> SubCategoriesVMs
            CreateMap<SubCategoryDto, SubCategoryItemVM>();
            CreateMap<SubCategoryItemVM, SubCategoryDto>();

            // Products <-> ProductsVMs
            CreateMap<ProductDto, ProductItemVM>();
            CreateMap<ProductItemVM, ProductDto>();

            // Categories <-> CategoriesVMs
            CreateMap<CategoryDto, CategoryItemVM>();
            CreateMap<CategoryItemVM, CategoryDto>();

            CreateMap<SubCategoryDto, SubCategoryItemVM>();
            CreateMap<SubCategoryItemVM, SubCategoryDto>();

            // Products <-> ProductsVMs
            CreateMap<ProductDto, ProductItemVM>();
            CreateMap<ProductDto, ProductDetailsDto>()
                .ForMember(dest => dest.SpecificationGroups, opt => opt.Ignore());
            CreateMap<ProductItemVM, ProductDto>();
            CreateMap<ProductDetailsDto, ProductDetailsVM>()
                .ReverseMap();

            // Specifications <-> SpecificationsVMs
            CreateMap<SpecificationCategoryDto, SpecificationCategoryVM>();
            CreateMap<SpecificationCategoryVM, SpecificationCategoryDto>();

            CreateMap<SpecificationCategoryGroupDto, SpecificationCategoryGroupVM>()
                .ReverseMap();

            CreateMap<SpecificationTypeDto, SpecificationTypeVM>();
            CreateMap<SpecificationTypeVM, SpecificationTypeDto>();

            CreateMap<SpecificationTypeDto, SpecificationFilterItemVM>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Unit, opt => opt.MapFrom(src => src.Unit))
                .ForMember(dest => dest.MinValue, opt => opt.Ignore())
                .ForMember(dest => dest.MaxValue, opt => opt.Ignore());

            CreateMap<ProductSpecificationDto, ProductSpecificationVM>();
            CreateMap<ProductSpecificationVM, ProductSpecificationDto>();

            // Cart <-> CartVMs
            CreateMap<CartDto, CartVM>()
                .ForMember(dest => dest.CartItems, opt => opt.MapFrom(src => src.CartItems))
                .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.TotalPrice))
                .ForMember(dest => dest.TotalItems, opt => opt.MapFrom(src => src.TotalItems))
                .ReverseMap()
                .ForMember(dest => dest.CartItems, opt => opt.MapFrom(src => src.CartItems))
                .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.TotalPrice))
                .ForMember(dest => dest.TotalItems, opt => opt.MapFrom(src => src.TotalItems));
            CreateMap<CartItemDto, CartItemVM>();

            // Додатковий маппінг для коректної роботи з формами
            CreateMap<string, CategoryItemVM>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.SubCategories, opt => opt.Ignore());
        }
    }
}
