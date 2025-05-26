using AutoMapper;
using BasedTechStore.Application.DTOs.Categories;
using BasedTechStore.Application.DTOs.Identity;
using BasedTechStore.Application.DTOs.Identity.Request;
using BasedTechStore.Application.DTOs.Identity.Response;
using BasedTechStore.Application.DTOs.Product;
using BasedTechStore.Application.DTOs.Specifications;
using BasedTechStore.Domain.Entities.Categories;
using BasedTechStore.Domain.Entities.Identity;
using BasedTechStore.Domain.Entities.Products;
using BasedTechStore.Domain.Entities.Specifications;

namespace BasedTechStore.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Mapping registration
            CreateMap<SignUpRequest, AppUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email));

            // Mapping AppUser -> AppUserDto
            CreateMap<AppUser, AppUserDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.Parse(src.Id)))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber));

            // Mapping AppUserDto -> UserProfileDto
            CreateMap<AppUserDto, UserProfileDto>()
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.PhoneNumber));
                //.ForMember(dest => dest.BirthDate, opt => opt.Ignore()) // Якщо немає BirthDate у AppUserDto
                //.ForMember(dest => dest.Address, opt => opt.Ignore()) // Якщо немає Address у AppUserDto
                //.ForMember(dest => dest.City, opt => opt.Ignore()) // Якщо немає City у AppUserDto
                //.ForMember(dest => dest.Country, opt => opt.Ignore()); // Якщо немає Country у AppUserDto

            // Mapping AuthenticationResponse
            CreateMap<AppUser, AuthenticationResponse>()
                .ForMember(dest => dest.IsSuccess, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.Token, opt => opt.Ignore()) // Токен генерується окремо
                .ForMember(dest => dest.Errors, opt => opt.Ignore());

            // Mapping Product <-> ProductDto
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.SubCategory.Category.Name))
                .ForMember(dest => dest.SubCategoryName, opt => opt.MapFrom(src => src.SubCategory.Name))
                .ReverseMap()
                .ForMember(dest => dest.SubCategory, opt => opt.Ignore()) // бо обробляємо вручну
                .ForMember(dest => dest.SubCategoryId, opt => opt.Ignore()) // встановлюємо вручну
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            // Mapping Product <-> ProductDetailsDto
            CreateMap<Product, ProductDetailsDto>()
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.SubCategory.CategoryId))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.SubCategory.Category.Name))
                .ForMember(dest => dest.SubCategoryName, opt => opt.MapFrom(src => src.SubCategory.Name))
                .ForMember(dest => dest.SpecificationGroups, opt => opt.Ignore());
            //CreateMap<ProductDto, Product>()
            //    .ForMember(dest => dest.SubCategory, opt => opt.Ignore());

            // Mapping Category <-> CategoryDto
            CreateMap<Category, CategoryDto>()
                .ForMember(dest => dest.SubCategories, opt => opt.MapFrom(src => src.SubCategories));
            CreateMap<CategoryDto, Category>()
                .ForMember(dest => dest.SubCategories, opt => opt.Ignore());

            // Mapping SubCategory <-> SubCategoryDto
            CreateMap<SubCategory, SubCategoryDto>();
            CreateMap<SubCategoryDto, SubCategory>()
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.Products, opt => opt.Ignore());

            // Mapping SpecificationCategory <-> SpecificationCategoryDto
            CreateMap<SpecificationCategory, SpecificationCategoryDto>()
                .ForMember(dest => dest.ProductCategoryName, opt => opt.MapFrom(src => src.ProductCategory.Name));
            CreateMap<SpecificationCategoryDto, SpecificationCategory>();

            // Mapping SpecificationType <-> SpecificationTypeDto
            CreateMap<SpecificationType, SpecificationTypeDto>()
                .ForMember(dest => dest.SpecificationCategoryName, opt => opt.MapFrom(src => src.SpecificationCategory.Name));
            CreateMap<SpecificationTypeDto, SpecificationType>();

            // Mapping ProductSpecification <-> ProductSpecificationDto
            CreateMap<ProductSpecification, ProductSpecificationDto>()
                .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src => src.SpecificationType.Name))
                .ForMember(dest => dest.TypeUnit, opt => opt.MapFrom(src => src.SpecificationType.Unit))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.SpecificationType.SpecificationCategory.Name))
                .ForMember(dest => dest.DisplayOrder, opt => opt.MapFrom(src => src.SpecificationType.DisplayOrder));
            CreateMap<ProductSpecificationDto, ProductSpecification>();
        }
    }
}
