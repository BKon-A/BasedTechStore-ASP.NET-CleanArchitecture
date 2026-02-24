using BasedTechStore.Application.Common.Interfaces.Repositories;
using BasedTechStore.Application.Common.Queries;
using BasedTechStore.Domain.Entities.Products;
using BasedTechStore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BasedTechStore.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> SearchAsync(ProductSearchCriteria criteria)
        {
            var query = _context.Products
                .Include(p => p.SubCategory)
                    .ThenInclude(sc => sc.Category)
                .Include(p => p.Specifications)
                .Where(p => p.IsActive);

            if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
            {
                var searchTerm = criteria.SearchTerm.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(searchTerm) ||
                    (p.Description != null && p.Description.ToLower().Contains(searchTerm)) ||
                    (p.Brand != null && p.Brand.ToLower().Contains(searchTerm))
                );
            }

            if (criteria.CategoryId.HasValue)
            {
                query = query.Where(p => p.SubCategory.CategoryId == criteria.CategoryId.Value);
            }
            if (criteria.SubCategoryId.HasValue)
            {
                query = query.Where(p => p.SubCategoryId == criteria.SubCategoryId.Value);
            }
            if (criteria.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= criteria.MinPrice.Value);
            }
            if (criteria.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= criteria.MaxPrice.Value);
            }
            if (criteria.Brands?.Any() == true)
            {
                query = query.Where(p => p.Brand != null && criteria.Brands.Contains(p.Brand));
            }

            if (criteria.SpecificationFilters?.Any() == true)
            {
                foreach (var filter in criteria.SpecificationFilters)
                {
                    var key = filter.Key;
                    var value = filter.Value;

                    query = query.Where(p => p.Specifications.Any(s => s.Key == key && s.Value == value));
                }
            }

            query = criteria.SortBy?.ToLower() switch
            {
                "price" => criteria.SortDescending
                    ? query.OrderByDescending(p => p.Price)
                    : query.OrderBy(p => p.Price),
                "date" => criteria.SortDescending
                    ? query.OrderByDescending(p => p.CreatedAt)
                    : query.OrderBy(p => p.CreatedAt),
                "name" => criteria.SortDescending
                    ? query.OrderByDescending(p => p.Name)
                    : query.OrderBy(p => p.Name),
                _ => query.OrderByDescending(p => p.Price),
            };

            return await query
                .Skip((criteria.Page - 1) * criteria.PageSize)
                .Take(criteria.PageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetFeaturedAsync(int count)
        {
            return await _context.Products
                .Include(p => p.SubCategory)
                    .ThenInclude(sc => sc.Category)
                .Include(p => p.Specifications)
                .Where(p => p.IsActive && p.Stock > 0)
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetRelatedAsync(Guid productId, int count)
        {
            var product = await _context.Products
                .Include(p => p.SubCategory)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
                return Enumerable.Empty<Product>();

            return await _context.Products
                .Include(p => p.SubCategory)
                    .ThenInclude(sc => sc.Category)
                .Include(p => p.Specifications)
                .Where(p => p.Id != productId && p.IsActive 
                    && p.SubCategoryId == product.SubCategoryId)
                .Take(count)
                .ToListAsync();
        }

        public async Task<bool> IsSlugUniqueAsync(string slug, Guid? excludeId = null)
        {
            var query = _context.Products.Where(p => p.Name.ToLower().Replace(" ", "-") == slug.ToLower());

            if (excludeId.HasValue)
            {
                query = query.Where(p => p.Id != excludeId.Value);
            }

            return !await query.AnyAsync();
        }
    }
}
