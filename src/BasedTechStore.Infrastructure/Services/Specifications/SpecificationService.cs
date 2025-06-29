using AutoMapper;
using BasedTechStore.Application.Common.Interfaces.Services;
using BasedTechStore.Application.DTOs.Specifications;
using BasedTechStore.Domain.Entities.Specifications;
using BasedTechStore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BasedTechStore.Infrastructure.Services.Specifications
{
    public class SpecificationService : ISpecificationService
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;
        private ILogger<SpecificationService> _logger;

        public SpecificationService(AppDbContext dbContext, IMapper mapper,
            ILogger<SpecificationService> logger)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<SpecificationCategoryDto>> GetSpecificationCategoriesByCategoryIdAsync(Guid categoryId)
        {
            var categories = await _dbContext.SpecificationCategories
                .Include(sc => sc.ProductCategory)
                .Where(sc => sc.ProductCategoryId == categoryId)
                .OrderBy(sc => sc.DisplayOrder)
                .ToListAsync();

            return _mapper.Map<List<SpecificationCategoryDto>>(categories);
        }

        public async Task<SpecificationCategoryDto> GetSpecificationCategoryAsync(Guid id)
        {
            var category = await _dbContext.SpecificationCategories
                .Include(sc => sc.ProductCategory)
                .FirstOrDefaultAsync(sc => sc.Id == id);

            return _mapper.Map<SpecificationCategoryDto>(category);
        }

        public async Task<SpecificationCategoryDto> CreateSpecificationCategoryAsync(SpecificationCategoryDto specCategoryDto)
        {
            var category = _mapper.Map<SpecificationCategory>(specCategoryDto);
            category.Id = Guid.NewGuid();  

            await EnsureUniqueDisplayOrderAsync(category);

            await _dbContext.SpecificationCategories.AddAsync(category);
            await _dbContext.SaveChangesAsync();

            return _mapper.Map<SpecificationCategoryDto>(category);
        }

        // Need change
        public async Task<SpecificationCategoryDto> UpdateSpecificationCategoryAsync(SpecificationCategoryDto specCategoryDto)
        {
            var category = await _dbContext.SpecificationCategories.FindAsync(specCategoryDto.Id);
            if (category == null)
            {
                _logger.LogWarning($"Specification category with ID {specCategoryDto.Id} not found.");
                return null;
            }

            _mapper.Map(specCategoryDto, category);

            await EnsureUniqueDisplayOrderAsync(category);

            await _dbContext.SaveChangesAsync();

            return _mapper.Map<SpecificationCategoryDto>(category);
        }

        public async Task<bool> DeleteSpecificationCategoryAsync(Guid id)
        {
            var category = await _dbContext.SpecificationCategories.FindAsync(id);
            if (category == null)
            {
                _logger.LogWarning($"Specification category with ID {id} not found.");
                return false;
            }

            _dbContext.SpecificationCategories.Remove(category);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<List<SpecificationTypeDto>> GetSpecificationTypesByCategoryIdAsync(Guid categoryId)
        {
            var types = await _dbContext.SpecificationTypes
                .Include(st => st.SpecificationCategory)
                .Where(st => st.SpecificationCategory.ProductCategoryId == categoryId)
                .OrderBy(st => st.SpecificationCategory.DisplayOrder)
                .ThenBy(st => st.DisplayOrder)
                .ToListAsync();

            return _mapper.Map<List<SpecificationTypeDto>>(types);
        }

        public async Task<List<SpecificationTypeDto>> GetSpecificationTypesBySpecCategoryIdAsync(Guid specCategoryId)
        {
            var types = await _dbContext.SpecificationTypes
                .Include(st => st.SpecificationCategory)
                .Where(st => st.SpecificationCategoryId == specCategoryId)
                .OrderBy(st => st.DisplayOrder)
                .ToListAsync();

            return _mapper.Map<List<SpecificationTypeDto>>(types);
        }

        public async Task<SpecificationTypeDto> GetSpecificationTypeAsync(Guid id)
        {
            var type = await _dbContext.SpecificationTypes
                .Include(st => st.SpecificationCategory)
                .FirstOrDefaultAsync(st => st.Id == id);

            return _mapper.Map<SpecificationTypeDto>(type);
        }

        public async Task<SpecificationTypeDto> CreateSpecificationTypeAsync(SpecificationTypeDto specTypeDto)
        {
            var type = _mapper.Map<SpecificationType>(specTypeDto);
            type.Id = Guid.NewGuid();

            type.Unit = type.Unit ?? string.Empty;

            await EnsureUniqueDisplayOrderAsync(type);

            await _dbContext.SpecificationTypes.AddAsync(type);
            await _dbContext.SaveChangesAsync();

            return _mapper.Map<SpecificationTypeDto>(type);
        }

        public async Task<SpecificationTypeDto> UpdateSpecificationTypeAsync(SpecificationTypeDto specTypeDto)
        {
            var type = await _dbContext.SpecificationTypes.FindAsync(specTypeDto.Id);
            if (type == null)
            {
                _logger.LogWarning($"Specification type with ID {specTypeDto.Id} not found.");
                return null;
            }

            _mapper.Map(specTypeDto, type);

            await EnsureUniqueDisplayOrderAsync(type);

            await _dbContext.SaveChangesAsync();

            return _mapper.Map<SpecificationTypeDto>(type);
        }

        public async Task<bool> DeleteSpecificationTypeAsync(Guid id)
        {
            var type = await _dbContext.SpecificationTypes.FindAsync(id);
            if (type == null)
            {
                _logger.LogWarning($"Specification type with ID {id} not found.");
                return false;
            }

            _dbContext.SpecificationTypes.Remove(type);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<List<ProductSpecificationDto>> GetProductSpecificationsByProductIdAsync(Guid productId)
        {
            var specs = await _dbContext.ProductSpecifications
                .Include(ps => ps.SpecificationType)
                .ThenInclude(st => st.SpecificationCategory)
                .Where(ps => ps.ProductId == productId)
                .OrderBy(ps => ps.SpecificationType.SpecificationCategory.DisplayOrder)
                .ThenBy(ps => ps.SpecificationType.DisplayOrder)
                .ToListAsync();

            return _mapper.Map<List<ProductSpecificationDto>>(specs);
        }
        public async Task<List<ProductSpecificationDto>> GetAllPossibleProductSpecificationsAsync(Guid productId)
        {
            // Отримуємо інформацію про продукт
            var product = await _dbContext.Products
                .Include(p => p.SubCategory)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
                return new List<ProductSpecificationDto>();

            // Отримуємо всі типи характеристик для категорії
            var specTypes = await _dbContext.SpecificationTypes
                .Include(st => st.SpecificationCategory)
                .Where(st => st.SpecificationCategory.ProductCategoryId == product.SubCategory.CategoryId)
                .OrderBy(st => st.SpecificationCategory.DisplayOrder)
                .ThenBy(st => st.DisplayOrder)
                .ToListAsync();

            // Отримуємо існуючі значення для продукту
            var existingSpecs = await _dbContext.ProductSpecifications
                .Where(ps => ps.ProductId == productId)
                .ToListAsync();

            // Створюємо список з усіх можливих характеристик
            var result = new List<ProductSpecificationDto>();

            foreach (var type in specTypes)
            {
                var existingSpec = existingSpecs.FirstOrDefault(s => s.SpecificationTypeId == type.Id);

                result.Add(new ProductSpecificationDto
                {
                    Id = existingSpec?.Id ?? Guid.Empty,
                    ProductId = productId,
                    SpecificationTypeId = type.Id,
                    Value = existingSpec?.Value ?? string.Empty,
                    TypeName = type.Name,
                    TypeUnit = type.Unit,
                    CategoryName = type.SpecificationCategory.Name,
                    DisplayOrder = type.DisplayOrder
                });
            }

            return result;
        }

        public async Task<List<SpecificationTypeDto>> GetFilterableSpecificationTypesAsync()
        {
            var types = await _dbContext.SpecificationTypes
                .Include(st => st.SpecificationCategory)
                .Where(st => st.IsFilterable)
                .OrderBy(st => st.SpecificationCategory.DisplayOrder)
                .ThenBy(st => st.DisplayOrder)
                .ToListAsync();

            return _mapper.Map<List<SpecificationTypeDto>>(types);
        }

        public async Task SaveAllSpecificationsAsync(
            List<SpecificationCategoryDto> createdCategories,
            List<SpecificationCategoryDto> updatedCategories,
            List<SpecificationCategoryDto> deletedCategories,
            List<SpecificationTypeDto> createdTypes,
            List<SpecificationTypeDto> updatedTypes,
            List<SpecificationTypeDto> deletedTypes)
        {
            _logger.LogInformation($"SaveAllSpecificationsAsync: " +
                $"Created categories: {createdCategories?.Count ?? 0}, " +
                $"Updated categories: {updatedCategories?.Count ?? 0}, " +
                $"Deleted categories: {deletedCategories?.Count ?? 0}, " +
                $"Created types: {createdTypes?.Count ?? 0}, " +
                $"Updated types: {updatedTypes?.Count ?? 0}, " +
                $"Deleted types: {deletedTypes?.Count ?? 0}");

            // Детальніший лог для діагностики проблем
            if (updatedCategories?.Any() == true)
            {
                _logger.LogDebug("Updated categories details: " +
                    string.Join(", ", updatedCategories.Select(c =>
                        $"Id={c.Id}, Name={c.Name}, DisplayOrder={c.DisplayOrder}")));
            }

            if (updatedTypes?.Any() == true)
            {
                _logger.LogDebug("Updated types details: " +
                    string.Join(", ", updatedTypes.Select(t =>
                        $"Id={t.Id}, Name={t.Name}, DisplayOrder={t.DisplayOrder}")));
            }

            // Видаляємо дублікати перед обробкою
            RemoveDuplicatesById(ref updatedCategories);
            RemoveDuplicatesById(ref updatedTypes);

            // Забезпечуємо унікальні порядкові номери
            EnsureUniqueDisplayOrderInCollection(createdCategories);
            EnsureUniqueDisplayOrderInCollection(updatedCategories);
            EnsureUniqueDisplayOrderInCollection(createdTypes);
            EnsureUniqueDisplayOrderInCollection(updatedTypes);

            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // 1. Видалення даних
                    // Спочатку видаляємо типи, потім категорії (через зовнішні ключі)
                    if (deletedTypes?.Any() == true)
                    {
                        var deletedTypeIds = deletedTypes.Select(t => t.Id).ToList();
                        var typesToDelete = await _dbContext.SpecificationTypes
                            .Where(t => deletedTypeIds.Contains(t.Id))
                            .ToListAsync();

                        if (typesToDelete.Any())
                        {
                            _dbContext.SpecificationTypes.RemoveRange(typesToDelete);
                            _logger.LogInformation($"Marked {typesToDelete.Count} types for deletion");
                        }
                    }

                    if (deletedCategories?.Any() == true)
                    {
                        var deletedCategoryIds = deletedCategories.Select(c => c.Id).ToList();
                        var categoriesToDelete = await _dbContext.SpecificationCategories
                            .Where(c => deletedCategoryIds.Contains(c.Id))
                            .ToListAsync();

                        if (categoriesToDelete.Any())
                        {
                            _dbContext.SpecificationCategories.RemoveRange(categoriesToDelete);
                            _logger.LogInformation($"Marked {categoriesToDelete.Count} categories for deletion");
                        }
                    }

                    // 2. Створення нових категорій
                    if (createdCategories?.Any() == true)
                    {
                        var newCategoryEntities = _mapper.Map<List<SpecificationCategory>>(createdCategories);
                        foreach (var entity in newCategoryEntities)
                        {
                            // Забезпечуємо унікальний ідентифікатор
                            if (entity.Id == Guid.Empty)
                            {
                                entity.Id = Guid.NewGuid();
                            }

                            await _dbContext.SpecificationCategories.AddAsync(entity);
                        }
                        _logger.LogInformation($"Created {newCategoryEntities.Count} new categories");
                    }

                    // 3. Оновлення існуючих категорій
                    if (updatedCategories?.Any() == true)
                    {
                        var updatedCategoryIds = updatedCategories.Select(c => c.Id).ToList();
                        var existingCategories = await _dbContext.SpecificationCategories
                            .Where(c => updatedCategoryIds.Contains(c.Id))
                            .ToListAsync();

                        foreach (var dto in updatedCategories)
                        {
                            var entity = existingCategories.FirstOrDefault(c => c.Id == dto.Id);
                            if (entity != null)
                            {
                                // Оновлюємо властивості вручну для кращого контролю
                                entity.Name = dto.Name;
                                entity.DisplayOrder = dto.DisplayOrder;
                                entity.ProductCategoryId = dto.ProductCategoryId;

                                // Відзначаємо об'єкт як змінений
                                _dbContext.Entry(entity).State = EntityState.Modified;
                            }
                        }
                        _logger.LogInformation($"Updated {existingCategories.Count} categories");
                    }

                    // 4. Створення нових типів характеристик
                    if (createdTypes?.Any() == true)
                    {
                        var newTypeEntities = _mapper.Map<List<SpecificationType>>(createdTypes);
                        foreach (var entity in newTypeEntities)
                        {
                            // Забезпечуємо унікальний ідентифікатор
                            if (entity.Id == Guid.Empty)
                            {
                                entity.Id = Guid.NewGuid();
                            }

                            entity.Unit = entity.Unit ?? string.Empty;

                            await _dbContext.SpecificationTypes.AddAsync(entity);
                        }
                        _logger.LogInformation($"Created {newTypeEntities.Count} new types");
                    }

                    // 5. Оновлення існуючих типів характеристик
                    if (updatedTypes?.Any() == true)
                    {
                        var updatedTypeIds = updatedTypes.Select(t => t.Id).ToList();
                        var existingTypes = await _dbContext.SpecificationTypes
                            .Where(t => updatedTypeIds.Contains(t.Id))
                            .ToListAsync();

                        foreach (var dto in updatedTypes)
                        {
                            var entity = existingTypes.FirstOrDefault(t => t.Id == dto.Id);
                            if (entity != null)
                            {
                                // Оновлюємо властивості вручну для кращого контролю
                                entity.Name = dto.Name;
                                entity.Unit = dto.Unit ?? string.Empty;
                                entity.IsFilterable = dto.IsFilterable;
                                entity.DisplayOrder = dto.DisplayOrder;
                                entity.SpecificationCategoryId = dto.SpecificationCategoryId;

                                // Відзначаємо об'єкт як змінений
                                _dbContext.Entry(entity).State = EntityState.Modified;
                            }
                        }
                        _logger.LogInformation($"Updated {existingTypes.Count} types");
                    }

                    // Зберігаємо всі зміни в базі даних
                    var changeCount = await _dbContext.SaveChangesAsync();
                    _logger.LogInformation($"Saved {changeCount} changes to database");

                    // Підтверджуємо транзакцію
                    await transaction.CommitAsync();
                    _logger.LogInformation("Transaction committed successfully");
                }
                catch (Exception ex)
                {
                    // Відкочуємо транзакцію у випадку помилки
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error saving specifications - transaction rolled back");
                    throw; // Перекидаємо виключення для обробки викликаючим кодом
                }
            }
        }

        public async Task SaveProductSpecificationsAsync(Guid productId, List<ProductSpecificationDto> productSpecifications)
        {
            var existingSpecs = await _dbContext.ProductSpecifications
                .Where(ps => ps.ProductId == productId)
                .ToListAsync();

            _dbContext.ProductSpecifications.RemoveRange(existingSpecs);

            if (productSpecifications != null && productSpecifications.Any())
            {
                var productSpecs = _mapper.Map<List<ProductSpecification>>(productSpecifications);
                foreach (var spec in productSpecs)
                {
                    spec.Id = Guid.NewGuid();
                    spec.ProductId = productId;

                    spec.Value = spec.Value ?? string.Empty;
                }

                await _dbContext.ProductSpecifications.AddRangeAsync(productSpecs);
            }

            await _dbContext.SaveChangesAsync();
        }

        // ====================== Ensure Unique Display Order ======================
        private async Task EnsureUniqueDisplayOrderAsync(SpecificationCategory category)
        {
            // Search all categories in this group, including the current one
            var existingCategories = await _dbContext.SpecificationCategories
                .Where(sc => sc.ProductCategoryId == category.ProductCategoryId && sc.Id != category.Id)
                .ToListAsync();

            existingCategories.Add(category);

            var sorted = existingCategories.OrderBy(sc => sc.DisplayOrder).ToList();
            for (int i = 0; i < sorted.Count; i++)
            {
                sorted[i].DisplayOrder = i + 1;
            }
        }

        private async Task EnsureUniqueDisplayOrderAsync(SpecificationType type)
        {
            // Search all types in this group, including the current one
            var existingTypes = await _dbContext.SpecificationTypes
                .Where(st => st.SpecificationCategoryId == type.SpecificationCategoryId && st.Id != type.Id)
                .ToListAsync();

            existingTypes.Add(type);

            var sorted = existingTypes.OrderBy(st => st.DisplayOrder).ToList();
            for (int i = 0; i < sorted.Count; i++)
            {
                sorted[i].DisplayOrder = i + 1;
            }
        }

        // Generic method who garantee unique display order for any collection of items
        private void EnsureUniqueDisplayOrderInCollection<T>(List<T> items) where T : class
        {
            if (items == null || items.Count <= 1) return;

            // Use reflection for access to DisplayOrder property
            var displayOrderProperty = typeof(T).GetProperty("DisplayOrder");
            if (displayOrderProperty == null) return;

            // Check for duplicates in DisplayOrder
            var displayOrders = items.Select(item => (int)displayOrderProperty.GetValue(item)).ToList();
            var hasDuplicates = displayOrders.GroupBy(x => x).Any(g => g.Count() > 1);

            if (hasDuplicates)
            {
                // Sort items by DisplayOrder
                items.Sort((a, b) =>
                {
                    var aValue = (int)displayOrderProperty.GetValue(a);
                    var bValue = (int)displayOrderProperty.GetValue(b);
                    return aValue.CompareTo(bValue);
                });

                for (int i = 0; i < items.Count; i++)
                {
                    displayOrderProperty.SetValue(items[i], i + 1);
                }
            }
        }
        // ====================== Ensure Unique Display Order ======================

        // ====================== Extension Methods ======================
        private void RemoveDuplicatesById<T>(ref List<T> items) where T : class
        {
            if (items == null || items.Count <= 1) return;

            var idProperty = typeof(T).GetProperty("Id");
            if (idProperty == null) return;

            var uniqueItems = new List<T>();
            var processedIds = new HashSet<Guid>();

            // Iterate through the list in reverse order to maintain the original order
            for (int i = items.Count - 1; i >= 0; i--)
            {
                var item = items[i];
                var idValue = (Guid)idProperty.GetValue(item);

                if (!processedIds.Contains(idValue))
                {
                    uniqueItems.Insert(0, item); // Insert at the beginning to maintain order
                    processedIds.Add(idValue);
                }
            }

            items = uniqueItems;
        }
    }
}
