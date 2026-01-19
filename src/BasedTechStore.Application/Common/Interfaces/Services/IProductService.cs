using BasedTechStore.Application.DTOs.Categories;
using BasedTechStore.Application.DTOs.Product;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasedTechStore.Application.Common.Interfaces.Services
{
    public interface IProductService
    {
        Task<List<CategoryDto>> GetAllCategoriesAsync();
        Task<List<CategoryDto>> GetCategoriesWithSubCategoriesAsync();
        Task<List<CategoryDto>> SaveCategoriesAsync(List<CategoryDto> categories);
        Task<List<SubCategoryDto>> GetAllSubCategoriesAsync();
        Task<List<SubCategoryDto>> GetSubCategoriesByCategoryIdAsync(Guid categoryId);
        Task<SubCategoryDto?> GetSubCategoryByIdAsync(Guid subcategoryId);
        Task<string?> GetSubCategoryNameByIdAsync(Guid subcategoryId);

        Task<List<ProductDto>> GetAllProductsAsync();
        Task<List<ProductDto>> GetProductsBySubCategoryAsync(Guid? id);
        Task<List<ProductDto>> GetProductsByCategoryIdAsync(Guid id);
        Task<List<ProductDto>> SaveProductsAsync(List<ProductDto> productDtos);
        Task<ProductDto?> GetProductByIdAsync(Guid productId);
        Task<ProductDetailsDto> GetProductDetailsByProductIdAsync(Guid productId);
        Task<List<ProductDto>> GetFilteredProductsAsync(decimal? minPrice, decimal? maxPrice,
            List<Guid>? categoryIds, List<string>? brands, Dictionary<Guid, (string Min, string Max)>? specificationFilters,
            List<Guid>? subcategoryIds = null);

        Task<string?> UploadProductImageAsync(IFormFile image);

        Task<int> DeleteUnusedImagesAsync(List<string> imageUrls);
        Task<int> CleanupAllUnusedImagesAsync();

        Task UpdateProductAsync(ProductDto productDto);
        Task DeleteProductAsync(Guid productId);
    }
}
