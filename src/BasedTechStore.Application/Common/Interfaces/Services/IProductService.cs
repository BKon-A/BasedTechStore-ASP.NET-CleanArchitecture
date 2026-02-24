using BasedTechStore.Application.Common.Models;
using BasedTechStore.Application.Common.Queries;
using BasedTechStore.Application.DTOs.Products;

namespace BasedTechStore.Application.Common.Interfaces.Services
{
    public interface IProductService
    {
        Task<ProductDto> GetByIdAsync(Guid productId);
        Task<IEnumerable<ProductDto>> GetAllAsync();
        Task<IEnumerable<ProductDto>> GetFeaturedAsync(int count);
        Task<IEnumerable<ProductDto>> GetRelatedAsync(Guid productId, int count);
        Task<IEnumerable<ProductDto>> GetByCategoryIdAsync(Guid categoryId);
        Task<IEnumerable<ProductDto>> GetBySubCategoryIdAsync(Guid subCategoryId);
        Task<ProductDto> CreateAsync(CreateProductDto dto);
        Task<ProductDto> UpdateAsync(Guid productId, UpdateProductDto dto);
        Task UpdateStockAsync(Guid productId, int quantity);
        Task DeleteAsync(Guid productId);

        Task<PagedResult<ProductDto>> SearchAsync(ProductSearchCriteria criteria);
        Task<ProductFiltersDto> GetAvailableFiltersAsync(Guid? categoryId = null);
    }
}
