using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BasedTechStore.Application.DTOs.Identity;
using BasedTechStore.Application.DTOs.Identity.Request;
using BasedTechStore.Application.DTOs.Identity.Response;
using BasedTechStore.Application.DTOs.Product;
using BasedTechStore.Domain.Entities.Identity;
using BasedTechStore.Domain.Entities.Products;

namespace BasedTechStore.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Мапінг для реєстрації
            CreateMap<SignUpRequest, AppUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email));

            // Мапінг для AppUser -> AppUserDto
            CreateMap<AppUser, AppUserDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.Parse(src.Id)))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber));

            // Мапінг для AppUserDto -> UserProfileDto
            CreateMap<AppUserDto, UserProfileDto>()
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.PhoneNumber));
                //.ForMember(dest => dest.BirthDate, opt => opt.Ignore()) // Якщо немає BirthDate у AppUserDto
                //.ForMember(dest => dest.Address, opt => opt.Ignore()) // Якщо немає Address у AppUserDto
                //.ForMember(dest => dest.City, opt => opt.Ignore()) // Якщо немає City у AppUserDto
                //.ForMember(dest => dest.Country, opt => opt.Ignore()); // Якщо немає Country у AppUserDto

            // Мапінг для AuthenticationResponse
            CreateMap<AppUser, AuthenticationResponse>()
                .ForMember(dest => dest.IsSuccess, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.Token, opt => opt.Ignore()) // Токен генерується окремо
                .ForMember(dest => dest.Errors, opt => opt.Ignore());

            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.SubCategory.Category.Name))
                .ForMember(dest => dest.SubCategoryName, opt => opt.MapFrom(src => src.SubCategory.Name));
            CreateMap<ProductDto, Product>()
                .ForMember(dest => dest.SubCategory, opt => opt.Ignore());
        }
    }
}
