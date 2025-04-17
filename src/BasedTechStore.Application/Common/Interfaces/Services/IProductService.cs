using BasedTechStore.Application.DTOs.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasedTechStore.Application.Common.Interfaces.Services
{
    public interface IProductService
    {
        Task<List<ProductDto>> GetAllProductsAsync();
        Task<List<ProductDto>> GetProductsBySubCategoryAsync(Guid id);
        Task<ProductDto?> GetProductByIdAsync(Guid productId);
        //Task<List<string>> GetAllCategoriesAsync();
        //Task<List<string>> GetAllSubCategoriesAsync();
        Task AddProductAsync(ProductDto productDto);
        Task UpdateProductAsync(ProductDto productDto);
        Task DeleteProductAsync(Guid productId);
    }
}
