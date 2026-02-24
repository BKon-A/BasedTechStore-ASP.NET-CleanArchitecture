using BasedTechStore.Application.DTOs.Categories;

namespace BasedTechStore.Application.Common.Interfaces.Services
{
    public interface ICategoryService
    {
        Task<CategoryDto> GetByIdAsync(Guid categoryId);
        Task<IEnumerable<CategoryDto>> GetAllAsync(bool includeSubCategories = true);
        Task<IEnumerable<SubCategoryDto>> GetSubCategoriesAsync(Guid categoryId);
    }
}
