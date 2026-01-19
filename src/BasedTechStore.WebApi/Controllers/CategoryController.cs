using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using BasedTechStore.Application.Common.Interfaces.Services;
using BasedTechStore.Common.Constants;
using BasedTechStore.Common.ViewModels.Categories;
using BasedTechStore.Common.Models.Api;

[ApiController]
[Route(ApiRoutes.Categories.Base)]
public class CategoryController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IMapper _mapper;
    private readonly ILogger<CategoryController> _logger;

    public CategoryController(IProductService productService, IMapper mapper, ILogger<CategoryController> logger)
    {
        _productService = productService;
        _mapper = mapper;
        _logger = logger;
    }

    // [HttpGet]
    // [Route(ApiRoutes.Categories.GetById)]
    // public async Task<IActionResult> GetCategoryById(Guid categoryId)
    // {
    //     try
    //     {
    //         var category = await _productService.GetCategoryByIdAsync(categoryId);
    //         if (category == null)
    //         {
    //             _logger.LogWarning($"Category with ID {categoryId} not found");
    //             return NotFound(new ErrorResponse
    //             {
    //                 Message = $"Category with ID {categoryId} not found",
    //                 StatusCode = StatusCodes.Status404NotFound,
    //                 Timestamp = DateTime.UtcNow
    //             });
    //         }

    //         var vm = _mapper.Map<CategoryItemVM>(category);

    //         _logger.LogInformation($"Retrieved category with ID {categoryId}");

    //         return Ok(new ApiResponse<CategoryItemVM>
    //         {
    //             IsSuccess = true,
    //             Data = vm,
    //             Message = "Category retrieved successfully",
    //             StatusCode = StatusCodes.Status200OK
    //         });
    //     }
    //     catch (Exception ex)
    //     {
    //         return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
    //         {
    //             Message = "An error occurred while retrieving the category",
    //             Details = ex.StackTrace,
    //             StatusCode = StatusCodes.Status500InternalServerError,
    //             Timestamp = DateTime.UtcNow
    //         });
    //     }
    // }

    [HttpGet(ApiRoutes.Categories.GetCategoriesWithSubCategories)]
    public async Task<IActionResult> GetCategoriesWithSubcategories()
    {
        try
        {
            var categories = await _productService.GetCategoriesWithSubCategoriesAsync();
            var vm = _mapper.Map<List<CategoryItemVM>>(categories);

            _logger.LogInformation($"Retrieved {vm.Count} categories with subcategories");

            return Ok(new ApiResponse<List<CategoryItemVM>>
            {
                IsSuccess = true,
                Data = vm,
                Message = "Categories with subcategories retrieved successfully",
                StatusCode = StatusCodes.Status200OK
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Message = "An error occurred while retrieving categories with subcategories",
                Details = ex.StackTrace,
                StatusCode = StatusCodes.Status500InternalServerError,
                Timestamp = DateTime.UtcNow
            });
        }
    }

}