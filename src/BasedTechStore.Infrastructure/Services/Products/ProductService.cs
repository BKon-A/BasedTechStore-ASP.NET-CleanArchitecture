using AutoMapper;
using AutoMapper.QueryableExtensions;
using BasedTechStore.Application.Common.Interfaces.Services;
using BasedTechStore.Application.DTOs.Categories;
using BasedTechStore.Application.DTOs.Product;
using BasedTechStore.Domain.Entities.Categories;
using BasedTechStore.Domain.Entities.Products;
using BasedTechStore.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

namespace BasedTechStore.Infrastructure.Services.Products
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private IWebHostEnvironment _webHostEnvironment;
        private ILogger<ProductService> _logger;

        public ProductService(AppDbContext context, IMapper mapper, 
            IWebHostEnvironment webHostEnvironment, ILogger<ProductService> logger)
        {
            _context = context;
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        public async Task<List<ProductDto>> SaveProductsAsync(List<ProductDto> productDtos)
        {
            var updatedProducts = new List<Product>();

            foreach (var productDto in productDtos)
            {
                var subCategory = await _context.SubCategories
                    .Include(sc => sc.Category)
                    .FirstOrDefaultAsync(sc =>
                        sc.Name == productDto.SubCategoryName &&
                        sc.Category.Name == productDto.CategoryName);

                if (subCategory == null)
                {
                    continue;
                }

                if (productDto.Id == Guid.Empty || !await _context.Products.AnyAsync(p => p.Id == productDto.Id))
                {
                    var newProduct = _mapper.Map<Product>(productDto);
                    newProduct.SubCategoryId = subCategory.Id;
                    newProduct.SubCategory = subCategory;

                    _context.Products.Add(newProduct);
                    updatedProducts.Add(newProduct);
                }
                else
                {
                    var exitstingProduct = await _context.Products
                        .FirstOrDefaultAsync(p => p.Id == productDto.Id);

                    if (exitstingProduct != null)
                    {
                        // Check if the image URL has changed
                        if (!string.IsNullOrEmpty(productDto.ImageUrl) && productDto.ImageUrl != exitstingProduct.ImageUrl)
                        {
                            // Delete the old image file if it exists
                            if (!string.IsNullOrEmpty(exitstingProduct.ImageUrl))
                            {
                                var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", exitstingProduct.ImageUrl.TrimStart('/'));
                                if (File.Exists(oldImagePath))
                                {
                                    File.Delete(oldImagePath);
                                }

                                exitstingProduct.ImageUrl = productDto.ImageUrl;
                            }
                        }
                        _mapper.Map(productDto, exitstingProduct);
                        exitstingProduct.SubCategoryId = subCategory.Id;
                        exitstingProduct.UpdatedAt = DateTime.UtcNow;

                        updatedProducts.Add(exitstingProduct);
                    }
                }
            }

            await _context.SaveChangesAsync();
            return _mapper.Map<List<ProductDto>>(updatedProducts);
        }

        public async Task<string?> UploadProductImageAsync(IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                return null;
            }

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var fileName = Path.GetFileNameWithoutExtension(image.FileName);
            var fileExtension = Path.GetExtension(image.FileName);
            var uniqueFileName = $"{fileName}_{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            if (File.Exists(filePath))
            {
                return $"/uploads/{uniqueFileName}";
            }

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            return $"/uploads/{uniqueFileName}";
        }

        public async Task<int> DeleteUnusedImagesAsync(List<string> imageUrls)
        {
            if (imageUrls == null || !imageUrls.Any())
            {
                return 0;
            }

            var deletedCount = 0;
            string webRootPath = _webHostEnvironment.WebRootPath;

            foreach (var imageUrl in imageUrls)
            {
                try
                {
                    if (string.IsNullOrEmpty(imageUrl))
                        continue;

                    if (!imageUrl.StartsWith("/uploads"))
                        continue;

                    var filePath = Path.Combine(webRootPath, imageUrl.TrimStart('/'));
                    if (File.Exists(filePath))
                    {
                        bool isUsed = await _context.Products.AnyAsync(p => p.ImageUrl == imageUrl);

                        if (!isUsed)
                        {
                            File.Delete(filePath);
                            deletedCount++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error deleting image file: {imageUrl}");
                }
            }
            return deletedCount;
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

        public async Task<List<string>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .Select(c => c.Name)
                .Distinct()
                .ToListAsync();
        }

        public async Task<List<string>> GetAllSubCategoriesAsync()
        {
            return await _context.SubCategories
                .Select(sc => sc.Name)
                .Distinct()
                .ToListAsync();
        }

        public async Task<List<string>> GetSubCategoriesByCategoryIdAsync(Guid categoryId)
        {
            return await _context.SubCategories
                .Where(sc => sc.CategoryId == categoryId)
                .Select(sc => sc.Name)
                .Distinct()
                .ToListAsync();
        }

        public async Task<string?> GetSubCategoryNameByIdAsync(Guid subcategoryId)
        {
            return await _context.SubCategories
                .Where(sc => sc.Id == subcategoryId)
                .Select(sc => sc.Name)
                .FirstOrDefaultAsync();
        }

        public async Task<List<CategoryDto>> GetCategoriesWithSubCategoriesAsync()
        {
            var categories = await _context.Categories
                .Include(c => c.SubCategories)
                .ToListAsync();

            return _mapper.Map<List<CategoryDto>>(categories);
        }

        public async Task<List<CategoryDto>> SaveCategoriesAsync(List<CategoryDto> categoryDtos)
        {
            foreach (var categoryDto in categoryDtos)
            {

                if (categoryDto.Id == Guid.Empty)
                {
                    var newCategory = _mapper.Map<Category>(categoryDto);
                    newCategory.Id = Guid.NewGuid();

                    foreach (var subCategory in newCategory.SubCategories)
                    {
                        subCategory.Id = Guid.NewGuid();
                        subCategory.CategoryId = newCategory.Id;
                    }
                    await _context.Categories.AddAsync(newCategory);
                    continue;
                }
                else
                {
                    var existingCategory = await _context.Categories
                        .FirstOrDefaultAsync(c => c.Id == categoryDto.Id);

                    if (existingCategory == null)
                        continue;

                    existingCategory.Name = categoryDto.Name;

                    var existingSubCategories = await _context.SubCategories
                        .Where(sc => sc.CategoryId == categoryDto.Id)
                        .ToListAsync();

                    var dtoSubIds = categoryDto.SubCategories.Select(x => x.Id).ToHashSet();

                    // Remove subcategories that are not in the DTO
                    var toRemove = existingSubCategories.Where(sc => !dtoSubIds.Contains(sc.Id)).ToList();
                    _context.SubCategories.RemoveRange(toRemove);

                    // Renew existing subcategories / add new ones
                    foreach (var subDto in categoryDto.SubCategories)
                    {
                        if (subDto.Id == Guid.Empty)
                        {
                            var newSubCategory = _mapper.Map<SubCategory>(subDto);
                            newSubCategory.Id = Guid.NewGuid();
                            newSubCategory.CategoryId = categoryDto.Id;
                            _context.SubCategories.Add(newSubCategory);
                        }
                        else
                        {
                            var existingSubCategory = existingSubCategories.FirstOrDefault(sc => sc.Id == subDto.Id);

                            if (existingSubCategory != null)
                            {
                                existingSubCategory.Name = subDto.Name;
                            }
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();

            var updatedCategories = await _context.Categories
                .Include(c => c.SubCategories)
                .ToListAsync();

            return _mapper.Map<List<CategoryDto>>(updatedCategories);
        }

        public async Task<List<ProductDto>> GetAllProductsAsync()
        {
            return await _context.Products
                .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<List<ProductDto>> GetProductsBySubCategoryAsync(Guid? subcategoryId)
        {
            if (!subcategoryId.HasValue)
            {
                return await GetAllProductsAsync();
            }

            var query = await _context.Products
                .Where(p => p.SubCategoryId == subcategoryId)
                .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            if (query == null || !query.Any())
            {
                return await GetAllProductsAsync();
            }

            return query;
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
