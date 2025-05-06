using BasedTechStore.Application.DTOs.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        Task<List<SpecificationTypeDto>> GetSpecificationTypeBySpecCategoryIdAsync(Guid specCategoryId);
        Task<SpecificationTypeDto> GetSpecificationTypeAsync(Guid id);
        Task<SpecificationTypeDto> CreateSpecificationTypeAsync(SpecificationTypeDto specificationTypeDto);
        Task<SpecificationTypeDto> UpdateSpecificationTypeAsync(SpecificationTypeDto specificationTypeDto);
        Task<bool> DeleteSpecificationTypeAsync(Guid id);

        // Product specifications
        Task<List<ProductSpecificationDto>> GetProductSpecificationsByProductIdAsync(Guid productId);
        Task SaveProductSpecificationAsync(Guid productId, List<ProductSpecificationDto> productSpecifications);
    }   
}
