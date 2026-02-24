using AutoMapper;
using BasedTechStore.Application.Common.Interfaces.Repositories;
using BasedTechStore.Application.Common.Interfaces.Services;
using BasedTechStore.Application.Common.Models;
using BasedTechStore.Application.Common.Queries;
using BasedTechStore.Application.DTOs.Products;
using BasedTechStore.Domain.Entities.Categories;
using BasedTechStore.Domain.Entities.Products;
using BasedTechStore.Domain.Entities.Specifications;
using BasedTechStore.Domain.Exceptions;
using BasedTechStore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BasedTechStore.Infrastructure.Services.Products
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public ProductService(AppDbContext context, IMapper mapper, 
            IProductRepository productRepository)
        {
            _context = context;
            _mapper = mapper;
            _productRepository = productRepository;
        }

        public async Task<ProductDto> GetByIdAsync(Guid productId)
        {
            var product = await _context.Products
                .Include(p => p.SubCategory)
                    .ThenInclude(sc => sc.Category)
                .Include(p => p.Specifications)
                .FirstOrDefaultAsync(p => p.Id == productId)
                ?? throw new NotFoundException(nameof(Product), productId);

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            var product = await _context.Products
                .Include(p => p.SubCategory)
                    .ThenInclude(sc => sc.Category)
                .Include(p => p.Specifications)
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ProductDto>>(product);
        }

        public async Task<IEnumerable<ProductDto>> GetBySubCategoryIdAsync(Guid subCategoryId)
        {
            var products = await _context.Products
                .Include(p => p.SubCategory)
                    .ThenInclude(sc => sc.Category)
                .Include(p => p.Specifications)
                .Where(p => p.SubCategoryId == subCategoryId && p.IsActive)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<IEnumerable<ProductDto>> GetByCategoryIdAsync(Guid categoryId)
        {
            var products = await _context.Products
                .Include(p => p.SubCategory)
                    .ThenInclude(sc => sc.Category)
                .Include(p => p.Specifications)
                .Where(p => p.SubCategory.CategoryId == categoryId && p.IsActive)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<IEnumerable<ProductDto>> GetFeaturedAsync(int count)
        {
            var products = await _productRepository.GetFeaturedAsync(count);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<IEnumerable<ProductDto>> GetRelatedAsync(Guid productId, int count)
        {
            var products = await _productRepository.GetRelatedAsync(productId, count);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<ProductDto> CreateAsync(CreateProductDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var subCategoryExists = await _context.SubCategories
                    .AnyAsync(sc => sc.Id == dto.SubCategoryId);

                if (!subCategoryExists)
                {
                    throw new NotFoundException(nameof(SubCategory), dto.SubCategoryId);
                }
                if (dto.Price < 0)
                {
                    throw new ValidationException("Price", "Price can`t be negative");
                }
                if (dto.Stock < 0)
                {
                    throw new ValidationException("Stock", "Stock can`t be negative");
                }

                var product = new Product
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    ImageUrl = dto.ImageUrl,
                    Brand = dto.Brand,
                    Price = dto.Price,
                    Stock = dto.Stock,
                    SubCategoryId = dto.SubCategoryId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    Specifications = new List<ProductSpecification>()
                };

                if (dto.Specifications?.Any() == true)
                {
                    foreach (var specDto in dto.Specifications)
                    {
                        product.Specifications.Add(new ProductSpecification
                        {
                            Key = specDto.Key,
                            Value = specDto.Value,
                        });
                    }
                }

                await _context.Products.AddAsync(product);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                var createdProduct = await _context.Products
                    .Include(p => p.SubCategory)
                        .ThenInclude(sc => sc.Category)
                    .Include(p => p.Specifications)
                    .FirstAsync(p => p.Id == product.Id);

                return _mapper.Map<ProductDto>(createdProduct);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<ProductDto> UpdateAsync(Guid productId, UpdateProductDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var product = await _context.Products
                    .Include(p => p.SubCategory)
                        .ThenInclude(sc => sc.Category)
                    .Include(p => p.Specifications)
                    .FirstOrDefaultAsync(p => p.Id == productId)
                    ?? throw new NotFoundException(nameof(Product), productId);

                if (dto.SubCategoryId.HasValue)
                {
                    var subCategoryExists = await _context.SubCategories
                        .AnyAsync(sc => sc.Id == dto.SubCategoryId.Value);

                    if (!subCategoryExists)
                    {
                        throw new NotFoundException(nameof(SubCategory), dto.SubCategoryId.Value);
                    }

                    product.SubCategoryId = dto.SubCategoryId.Value;
                }
                if (dto.Price.HasValue)
                {
                    if (dto.Price.Value < 0)
                    {
                        throw new ValidationException("Price", "Price can`t be negative");
                    }
                    product.Price = dto.Price.Value;
                }
                if (dto.Stock.HasValue)
                {
                    if (dto.Stock.Value < 0)
                    {
                        throw new ValidationException("Stock", "Stock can`t be negative");
                    }
                    product.Stock = dto.Stock.Value;
                }

                if (!string.IsNullOrWhiteSpace(dto.Name))
                    product.Name = dto.Name;
                if (dto.Description != null)
                    product.Description = dto.Description;
                if (dto.ImageUrl != null)
                    product.ImageUrl = dto.ImageUrl;
                if (dto.Brand != null)
                    product.Brand = dto.Brand;
                if (dto.Specifications != null)
                {
                    _context.ProductSpecifications.RemoveRange(product.Specifications);
                    product.Specifications.Clear();

                    foreach (var specDto in dto.Specifications)
                    {
                        product.Specifications.Add(new ProductSpecification
                        {
                            ProductId = product.Id,
                            Key = specDto.Key,
                            Value = specDto.Value
                        });
                    }
                }

                product.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return _mapper.Map<ProductDto>(product);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateStockAsync(Guid productId, int quantity)
        {
            var product = await _context.Products.FindAsync(productId)
                ?? throw new NotFoundException(nameof(Product), productId);

            product.Stock += quantity;

            if (product.Stock < 0)
            {
                throw new ValidationException("Stock", "Stock can`t be negative");
            }

            product.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid productId)
        {
            var product = await _context.Products.FindAsync(productId)
                ?? throw new NotFoundException(nameof(Product), productId);

            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task<PagedResult<ProductDto>> SearchAsync(ProductSearchCriteria criteria)
        {
            var searchedProducts = await _productRepository.SearchAsync(criteria);

            var totalCount = await _context.Products
                .Where(p => p.IsActive)
                .CountAsync();

            return new PagedResult<ProductDto>
            {
                Items = _mapper.Map<IEnumerable<ProductDto>>(searchedProducts),
                TotalCount = totalCount,
                Page = criteria.Page,
                PageSize = criteria.PageSize
            };
        }

        public async Task<ProductFiltersDto> GetAvailableFiltersAsync(Guid? categoryId = null)
        {
            var query = _context.Products
                .Include(p => p.SubCategory)
                .Include(p => p.Specifications)
                .Where(p => p.IsActive);

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.SubCategory.CategoryId == categoryId.Value);
            }

            var products = await query.ToListAsync();

            var filters = new ProductFiltersDto
            {
                Brands = products
                    .Where(p => !string.IsNullOrEmpty(p.Brand))
                    .Select(p => p.Brand!)
                    .Distinct()
                    .OrderBy(b => b)
                    .ToList(),
                MinPrice = products.Any() ? products.Min(p => p.Price) : 0,
                MaxPrice = products.Any() ? products.Max(p => p.Price) : 0
            };

            var specOptions = new Dictionary<string, HashSet<string>>();
            foreach (var product in products)
            {
                foreach (var spec in product.Specifications)
                {
                    if (!specOptions.ContainsKey(spec.Key))
                    {
                        specOptions[spec.Key] = new HashSet<string>();
                    }
                    specOptions[spec.Key].Add(spec.Value);
                }
            }

            filters.SpecOptions = specOptions.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.OrderBy(v => v).ToList()
            );

            return filters;
        }
    }
}
