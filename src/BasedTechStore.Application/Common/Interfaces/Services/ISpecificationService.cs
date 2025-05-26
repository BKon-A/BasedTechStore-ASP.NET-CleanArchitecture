using BasedTechStore.Application.DTOs.Specifications;
using Microsoft.EntityFrameworkCore;

namespace BasedTechStore.Application.Common.Interfaces.Services
{
    public interface ISpecificationService
    {
        // Category specifications
        Task<List<SpecificationCategoryDto>> GetSpecificationCategoriesByCategoryIdAsync(Guid categoryId);
        Task<SpecificationCategoryDto> GetSpecificationCategoryAsync(Guid id);
        Task<SpecificationCategoryDto> CreateSpecificationCategoryAsync(SpecificationCategoryDto specificationCategoryDto);
        Task<SpecificationCategoryDto> UpdateSpecificationCategoryAsync(SpecificationCategoryDto specificationCategoryDto);
        Task<bool> DeleteSpecificationCategoryAsync(Guid id);

        // Specification types
        Task<List<SpecificationTypeDto>> GetSpecificationTypesByCategoryIdAsync(Guid categoryId);
        Task<List<SpecificationTypeDto>> GetSpecificationTypesBySpecCategoryIdAsync(Guid specCategoryId);
        Task<SpecificationTypeDto> GetSpecificationTypeAsync(Guid id);
        Task<SpecificationTypeDto> CreateSpecificationTypeAsync(SpecificationTypeDto specificationTypeDto);
        Task<SpecificationTypeDto> UpdateSpecificationTypeAsync(SpecificationTypeDto specificationTypeDto);
        Task<bool> DeleteSpecificationTypeAsync(Guid id);

        // Product specifications
        Task<List<ProductSpecificationDto>> GetProductSpecificationsByProductIdAsync(Guid productId);
        Task<List<ProductSpecificationDto>> GetAllPossibleProductSpecificationsAsync(Guid productId);
        Task SaveProductSpecificationsAsync(Guid productId, List<ProductSpecificationDto> productSpecifications);
        Task SaveAllSpecificationsAsync(List<SpecificationCategoryDto> createdCategories, List<SpecificationCategoryDto> updatedCategories,
            List<SpecificationCategoryDto> deletedCategories, List<SpecificationTypeDto> createdTypes, List<SpecificationTypeDto> updatedTypes, List<SpecificationTypeDto> deletedTypes);
    }   
}
