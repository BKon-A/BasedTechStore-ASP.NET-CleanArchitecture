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
        Task<List<string>> GetAllCategoriesAsync();
        Task<List<string>> GetAllSubCategoriesAsync();
        Task<List<string>> GetSubCategoriesByCategoryIdAsync(Guid id);
        Task<List<CategoryDto>> GetCategoriesWithSubCategoriesAsync();
        Task<List<CategoryDto>> SaveCategoriesAsync(List<CategoryDto> categories);

        Task<List<ProductDto>> GetAllProductsAsync();
        Task<List<ProductDto>> GetProductsBySubCategoryAsync(Guid? id);
        Task<ProductDto?> GetProductByIdAsync(Guid productId);
        Task<List<ProductDto>> SaveProductsAsync(List<ProductDto> productDtos);

        Task<string?> GetSubCategoryNameByIdAsync(Guid subcategoryId);

        Task<string?> UploadProductImageAsync(IFormFile image);
        Task<int> DeleteUnusedImagesAsync(List<string> imageUrls);

        Task UpdateProductAsync(ProductDto productDto);
        Task DeleteProductAsync(Guid productId);
    }
}
