using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using BasedTechStore.Application.Common.Interfaces.Services;
using BasedTechStore.Common.Constants;
using BasedTechStore.Common.ViewModels.Categories;
using BasedTechStore.Common.Models.Api;
using BasedTechStore.Application.DTOs.Categories;

[ApiController]
[Route(ApiRoutes.Categories.Base)]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet(ApiRoutes.Categories.GetAll)]
    public async Task<ActionResult<ApiResponse<IEnumerable<CategoryDto>>>> GetAll([FromQuery] bool includeSubCategories = true)
    {
        var categories = await _categoryService.GetAllAsync(includeSubCategories);
        return Ok(ApiResponse<IEnumerable<CategoryDto>>.Success(categories));
    }

    [HttpGet(ApiRoutes.Categories.GetById)]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> GetById(Guid categoryId)
    {
        var category = await _categoryService.GetByIdAsync(categoryId);
        return Ok(ApiResponse<CategoryDto>.Success(category));
    }

    [HttpGet(ApiRoutes.Categories.GetSubCategories)]
    public async Task<ActionResult<ApiResponse<IEnumerable<SubCategoryDto>>>> GetSubCategories(Guid id)
    {
        var subCategories = await _categoryService.GetSubCategoriesAsync(id);
        return Ok(ApiResponse<IEnumerable<SubCategoryDto>>.Success(subCategories));
    }
}