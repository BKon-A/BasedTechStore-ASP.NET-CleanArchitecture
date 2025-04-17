using AutoMapper;
using AutoMapper.QueryableExtensions;
using BasedTechStore.Application.Common.Interfaces.Services;
using BasedTechStore.Application.DTOs.Product;
using BasedTechStore.Domain.Entities.Products;
using BasedTechStore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BasedTechStore.Infrastructure.Services.Products
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ProductService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task AddProductAsync(ProductDto productDto)
        {
            var subcategory = await _context.SubCategories
                .Include(sc => sc.Category)
                .FirstOrDefaultAsync(sc =>
                    sc.Name == productDto.CategoryName &&
                    sc.Category.Name == productDto.CategoryName);

            if (subcategory == null)
            {
                throw new InvalidOperationException("Subcategory not found");
            }

            var product = _mapper.Map<Product>(productDto);
            product.SubCategoryId = subcategory.Id;

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateProductAsync(ProductDto productDto)
        {
            var existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.Id == productDto.Id);

            if (existingProduct == null)
            {
                throw new InvalidOperationException("Product not found");
            }

            var subcategory = await _context.SubCategories
                .Include(sc => sc.Category)
                .FirstOrDefaultAsync(sc =>
                    sc.Name == productDto.CategoryName &&
                    sc.Category.Name == productDto.CategoryName);

            if (subcategory == null)
            {
                throw new InvalidOperationException("Subcategory not found");
            }

            _mapper.Map(productDto, existingProduct);
            existingProduct.SubCategoryId = subcategory.Id;
            //existingProduct.UpdatedAt = DateTime.UtcNow;

            _context.Products.Update(existingProduct);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteProductAsync(Guid productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                throw new InvalidOperationException("Product not found");
            }
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ProductDto>> GetAllProductsAsync()
        {
            return await _context.Products
                .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<List<ProductDto>> GetProductsBySubCategoryAsync(Guid subcategoryId)
        {
            return await _context.Products
                .Where(p => p.SubCategoryId == subcategoryId)
                .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<ProductDto?> GetProductByIdAsync(Guid productId)
        {
            return await _context.Products
                .Where(p => p.Id == productId)
                .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
        }
    }
}
