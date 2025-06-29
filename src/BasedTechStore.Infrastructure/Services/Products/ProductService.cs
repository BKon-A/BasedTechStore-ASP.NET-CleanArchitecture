using AutoMapper;
using AutoMapper.QueryableExtensions;
using BasedTechStore.Application.Common.Interfaces.Services;
using BasedTechStore.Application.DTOs.Categories;
using BasedTechStore.Application.DTOs.Product;
using BasedTechStore.Application.DTOs.Specifications;
using BasedTechStore.Domain.Entities.Categories;
using BasedTechStore.Domain.Entities.Products;
using BasedTechStore.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;

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

        // ============================= Get Data ==============================
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

        public async Task<List<ProductDto>> GetProductsByCategoryIdAsync(Guid categoryId)
        {
            var products = await _context.Products
                .Where(p => p.SubCategory.CategoryId == categoryId)
                .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return _mapper.Map<List<ProductDto>>(products);
        }

        public async Task<ProductDetailsDto?> GetProductDetailsByProductIdAsync(Guid productId)
        {
            var product = await _context.Products
                .AsNoTracking()
                .Include(p => p.SubCategory)
                    .ThenInclude(sc => sc.Category)
                .Include(p => p.ProductSpecifications)
                    .ThenInclude(ps => ps.SpecificationType)
                        .ThenInclude(st => st.SpecificationCategory)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                return null;
            }

            var productDto = _mapper.Map<ProductDetailsDto>(product);

            var specificationGroups = product.ProductSpecifications
                .GroupBy(ps => new
                {
                    CategoryId = ps.SpecificationType.SpecificationCategoryId,
                    CategoryName = ps.SpecificationType.SpecificationCategory.Name,
                    DisplayOrder = ps.SpecificationType.SpecificationCategory.DisplayOrder
                })
                .Select(g => new SpecificationCategoryGroupDto
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.CategoryName,
                    DisplayOrder = g.Key.DisplayOrder,
                    Specifications = g
                        .OrderBy(ps => ps.SpecificationType.DisplayOrder)
                        .Select(ps => _mapper.Map<ProductSpecificationDto>(ps))
                        .ToList()
                })
                .OrderBy(g => g.DisplayOrder)
                .ToList();

            productDto.SpecificationGroups = specificationGroups;

            return productDto;
        }

        public async Task<List<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            return _mapper.Map<List<CategoryDto>>(categories);
        }

        public async Task<List<SubCategoryDto>> GetAllSubCategoriesAsync()
        {
            var subcategories = await _context.SubCategories
                .Include(sc => sc.Category)
                .OrderBy(sc => sc.Name)
                .ToListAsync();

            return _mapper.Map<List<SubCategoryDto>>(subcategories);
        }

        public async Task<List<SubCategoryDto>> GetSubCategoriesByCategoryIdAsync(Guid categoryId)
        {
            var subcategories = await _context.SubCategories
                .Include(sc => sc.Category)
                .Where(sc => sc.CategoryId == categoryId)
                .OrderBy(sc => sc.Name)
                .ToListAsync();

            return _mapper.Map<List<SubCategoryDto>>(subcategories);
        }

        public async Task<SubCategoryDto?> GetSubCategoryByIdAsync(Guid subcategoryId)
        {
            var subcategory = await _context.SubCategories
                .Include(sc => sc.Category)
                .FirstOrDefaultAsync(sc => sc.Id == subcategoryId);

            return _mapper.Map<SubCategoryDto>(subcategory);
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

        public async Task<List<ProductDto>> GetFilteredProductsAsync(decimal? minPrice, decimal? maxPrice,
            List<Guid> categoryIds, List<string> brands, Dictionary<Guid, (string Min, string Max)> specificationFilters, 
            List<Guid> subcategoryIds = null)
        {
            var query = _context.Products.AsQueryable();

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }
            if (categoryIds != null && categoryIds.Count > 0)
            {
                query = query.Where(p => categoryIds.Contains(p.SubCategory.CategoryId));
            }
            if (subcategoryIds != null && subcategoryIds.Any())
            {
                query = query.Where(p => subcategoryIds.Contains(p.SubCategoryId));
            }
            if (brands != null && brands.Any())
            {
                query = query.Where(p => brands.Contains(p.Brand));
            }

            var products = await query.ToListAsync();

            if (specificationFilters != null && specificationFilters.Any())
            {
                var productIds = products.Select(p => p.Id).ToList();
                var specifications = await _context.ProductSpecifications
                    .Where(ps => productIds.Contains(ps.ProductId))
                    .ToListAsync();

                foreach (var filter in specificationFilters)
                {
                    var specTypeId = filter.Key;
                    var specTypeName = (await _context.SpecificationTypes.FindAsync(specTypeId))?.Name ?? "Unknown";
                    var minValue = filter.Value.Min;
                    var maxValue = filter.Value.Max;

                    _logger.LogInformation($"Filtering by spec type: {specTypeName}, Min: {minValue}, Max: {maxValue}");

                    if (!string.IsNullOrEmpty(minValue))
                    {
                        var filteredProducIds = specifications
                            .Where(ps => ps.SpecificationTypeId == specTypeId &&
                                decimal.TryParse(ps.Value?.Replace(',', '.'), NumberStyles.Any, 
                                    CultureInfo.InvariantCulture, out var value) &&
                                decimal.TryParse(minValue.Replace(',', '.'), NumberStyles.Any, 
                                    CultureInfo.InvariantCulture, out var minVal) &&
                                value >= minVal)
                            .Select(ps => ps.ProductId)
                            .ToList();

                        _logger.LogInformation($"After min filter, found {filteredProducIds.Count} products for spec type: {specTypeName}");
                        products = products.Where(p => filteredProducIds.Contains(p.Id)).ToList();
                    }

                    if (!string.IsNullOrEmpty(maxValue))
                    {
                        var filteredProducIds = specifications
                            .Where(ps => ps.SpecificationTypeId == specTypeId &&
                                decimal.TryParse(ps.Value?.Replace(',', '.'), NumberStyles.Any, 
                                    CultureInfo.InvariantCulture, out var value) &&
                                decimal.TryParse(maxValue.Replace(',', '.'), NumberStyles.Any, 
                                    CultureInfo.InvariantCulture, out var maxVal) &&
                                value <= maxVal)
                            .Select(ps => ps.ProductId)
                            .ToList();

                        _logger.LogInformation($"After max filter, found {filteredProducIds.Count} products for spec type: {specTypeName}");
                        products = products.Where(p => filteredProducIds.Contains(p.Id)).ToList();
                    }
                }
            }

            return _mapper.Map<List<ProductDto>>(products);
        }

        // ============================= End get data ==============================

        // ============================= Start update data =========================
        public async Task<string?> UploadProductImageAsync(IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                _logger.LogWarning("No image file provided for upload");
                return null;
            }
            try
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileName = Path.GetFileNameWithoutExtension(image.FileName);
                var fileExtension = Path.GetExtension(image.FileName);
                var uniqueFileName = $"{fileName}_{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }

                if (File.Exists(filePath))
                {
                    _logger.LogInformation($"File uploaded successfully: {filePath}");
                    return $"/uploads/{uniqueFileName}";
                }
                else
                {
                    _logger.LogError($"File not found after upload attempt: {filePath}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"File not be uploaded: {ex.Message}");
                return null;
            }
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
        // ============================= End update data ==========================

        // ============================= Start save data ==========================
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

            // Delete unused images after saving products
            try
            {
                int deletedCount = await CleanupAllUnusedImagesAsync();
                _logger.LogInformation($"Cleanup completed. Total unused images deleted: {deletedCount}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cleanup of unused images");
            }

            return _mapper.Map<List<ProductDto>>(updatedProducts);
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
        // ============================= End save data ==============================

        // ============================= Start delete data ==========================
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
                            _logger.LogInformation($"Deleting unused image file: {imageUrl}");
                            File.Delete(filePath);
                            deletedCount++;
                        }
                        else
                        {
                            _logger.LogInformation($"Image file is still in use: {imageUrl}");
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"Image file not found: {imageUrl}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error deleting image file: {imageUrl}");
                }
            }

            _logger.LogInformation($"Total unused images deleted: {deletedCount}");
            return deletedCount;
        }

        public async Task<int> CleanupAllUnusedImagesAsync()
        {
            string webRootPath = _webHostEnvironment.WebRootPath;
            var uploadsFolder = Path.Combine(webRootPath, "uploads");

            if (!Directory.Exists(uploadsFolder))
            {
                _logger.LogInformation("Uploads folder does not exist, nothing to clean up.");
                return 0;
            }

            var allFiles = Directory.GetFiles(uploadsFolder)
                .Select(f => $"/uploads/{Path.GetFileName(f)}")
                .ToList();

            var usedImageUrls = await _context.Products
                .Where(p => !string.IsNullOrEmpty(p.ImageUrl))
                .Select(p => p.ImageUrl)
                .Distinct()
                .ToListAsync();

            _logger.LogInformation($"Found {allFiles.Count} total files in uploads folder.");
            _logger.LogInformation($"Found {usedImageUrls.Count} used images in the database.");

            var unusedImages = allFiles.
                Where(f => !usedImageUrls.Contains(f))
                .ToList();

            _logger.LogInformation($"Found {unusedImages.Count} unused images to delete.");

            int deletedCount = 0;
            foreach (var file in unusedImages)
            {
                try
                {
                    var filePath = Path.Combine(webRootPath, file.TrimStart('/'));
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                        deletedCount++;
                        _logger.LogInformation($"Deleted unused image file: {file}");
                    }
                    else
                    {
                        _logger.LogWarning($"Image file not found during cleanup: {file}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error deleting image file during cleanup: {file}");
                }
            }

            _logger.LogInformation($"Total unused images deleted during cleanup: {deletedCount}");
            return deletedCount;
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
        // ============================= End delete data ==============================
    }
}
