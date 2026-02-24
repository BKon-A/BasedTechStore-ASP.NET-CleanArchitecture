using BasedTechStore.Application.Common.Queries;
using BasedTechStore.Domain.Entities.Products;

namespace BasedTechStore.Application.Common.Interfaces.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> SearchAsync(ProductSearchCriteria criteria);
        Task<IEnumerable<Product>> GetFeaturedAsync(int count);
        Task<IEnumerable<Product>> GetRelatedAsync(Guid productId, int count);
        Task<bool> IsSlugUniqueAsync(string slug, Guid? excludeId = null);
    }
}
