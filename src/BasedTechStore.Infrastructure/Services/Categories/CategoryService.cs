using AutoMapper;
using BasedTechStore.Application.Common.Interfaces.Services;
using BasedTechStore.Application.DTOs.Categories;
using BasedTechStore.Domain.Entities.Categories;
using BasedTechStore.Domain.Exceptions;
using BasedTechStore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasedTechStore.Infrastructure.Services.Categories
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public CategoryService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<CategoryDto> GetByIdAsync(Guid categoryId)
        {
            var category = await _context.Categories
                .Include(c => c.SubCategories.Where(sc => sc.IsActive))
                .FirstOrDefaultAsync(c => c.Id == categoryId)
                ?? throw new NotFoundException(nameof(Category), categoryId);

            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<IEnumerable<CategoryDto>> GetAllAsync(bool includeSubCategories = true)
        {
            var query = _context.Categories
                .Where(c => c.IsActive);

            if (includeSubCategories)
            {
                query = query.Include(c => c.SubCategories.Where(sc => sc.IsActive));
            }

            var categories = await query.OrderBy(c => c.DisplayOrder).ToListAsync();
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }

        public async Task<IEnumerable<SubCategoryDto>> GetSubCategoriesAsync(Guid categoryId)
        {
            var categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == categoryId);

            if (!categoryExists)
                throw new NotFoundException(nameof(Category), categoryId);

            var subCategories = await _context.SubCategories
                .Where(sc => sc.CategoryId == categoryId && sc.IsActive)
                .OrderBy(sc => sc.DisplayOrder)
                .ToListAsync();

            return _mapper.Map<IEnumerable<SubCategoryDto>>(subCategories);
        }
    }
}
