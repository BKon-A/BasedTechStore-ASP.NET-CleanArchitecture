using AutoMapper;
using BasedTechStore.Application.DTOs.Categories;
using BasedTechStore.Application.DTOs.Product;
using BasedTechStore.Web.ViewModels.Categories;
using BasedTechStore.Web.ViewModels.Products;

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
            //CreateMap<ProductDto, ProductsItemVM>()
            //    .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.CategoryName))
            //    .ForMember(dest => dest.SubCategoryName, opt => opt.MapFrom(src => src.SubCategoryName));
        }
    }
}
