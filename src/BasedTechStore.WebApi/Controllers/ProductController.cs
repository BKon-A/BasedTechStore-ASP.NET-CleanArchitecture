using AutoMapper;
using BasedTechStore.Application.Common.Interfaces.Services;
using BasedTechStore.Application.DTOs.Specifications;
using BasedTechStore.Common.Constants;
using BasedTechStore.Common.Models.Api;
using BasedTechStore.Common.Utilities;
using BasedTechStore.Common.ViewModels.Categories;
using BasedTechStore.Common.ViewModels.Products;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

[ApiController]
[Route(ApiRoutes.Products.Base)]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ISpecificationService _specificationService;
    private readonly ILogger<ProductController> _logger;
    private readonly IMapper _mapper;

    public ProductController(IProductService productService, IMapper mapper,
        ISpecificationService specificationService, ILogger<ProductController> logger)
    {
        _productService = productService;
        _mapper = mapper;
        _specificationService = specificationService;
        _logger = logger;
    }

    [HttpGet(ApiRoutes.Products.GetAll)]
    public async Task<IActionResult> GetAllProducts()
    {
        try
        {
            var products = await _productService.GetAllProductsAsync();
            var productItems = _mapper.Map<List<ProductItemVM>>(products);
            var result = new ProductListVM
            {
                Products = productItems,
                TotalCount = productItems.Count
            };
            return Ok(new ApiResponse<ProductListVM>
            {
                Data = result,
                Message = "Products retrieved successfully",
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
            {
                IsSuccess = false,
                Message = "An error occurred while retrieving products",
                Errors = new List<string> { ex.Message },
                StatusCode = StatusCodes.Status500InternalServerError
            });
        }
    }

    [HttpGet(ApiRoutes.Products.GetProductFilters)]
    public async Task<IActionResult> GetProductFilters(Guid? categoryId = null)
    {
        try
        {
            var categories = await _productService.GetAllCategoriesAsync();
            var allProducts = await _productService.GetAllProductsAsync();
            var brands = allProducts.Select(p => p.Brand).Distinct().ToList();

            var filterableSpecTypes = await _specificationService.GetFilterableSpecificationTypesAsync();

            var groupedSpecTypes = filterableSpecTypes
                .GroupBy(st => new { st.SpecificationCategoryId, st.SpecificationCategoryName })
                .Select(g => new SpecificationFilterGroupVM
                {
                    CategoryId = g.Key.SpecificationCategoryId,
                    CategoryName = g.Key.SpecificationCategoryName,
                    Specifications = g.Select(s => _mapper.Map<SpecificationFilterItemVM>(s)).ToList()
                })
                .ToList();

            var filterOptions = new ProductFilterOptionsVM
            {
                Categories = _mapper.Map<List<CategoryItemVM>>(categories),
                Brands = brands,
                FilterableSpecifications = groupedSpecTypes
            };

            return Ok(new ApiResponse<ProductFilterOptionsVM>
            {
                Data = filterOptions,
                Message = "Product filter options retrieved successfully",
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
            {
                IsSuccess = false,
                Message = "An error occurred while retrieving product filter options",
                Errors = new List<string> { ex.Message },
                StatusCode = StatusCodes.Status500InternalServerError
            });
        }
    }

    [HttpGet(ApiRoutes.Products.GetFilteredProducts)]
    public async Task<IActionResult> GetFilteredProducts([FromQuery] ProductFilterVM filters)
    {
        var filterableSpecTypes = await _specificationService.GetFilterableSpecificationTypesAsync();

        var filteredProducts = await _productService.GetFilteredProductsAsync(filters.MinPrice,
            filters.MaxPrice, filters.SelectedCategoryIds, filters.Brands,
            ExtractSpecificationFilters(filterableSpecTypes),
            filters.SelectedSubCategoryIds);

        var result = new ProductListVM
        {
            Products = _mapper.Map<List<ProductItemVM>>(filteredProducts),
            TotalCount = filteredProducts.Count
        };

        return Ok(new ApiResponse<ProductListVM>
        {
            Data = result,
            Message = "Filtered products retrieved successfully",
            IsSuccess = true,
            StatusCode = StatusCodes.Status200OK
        });
    }

    [HttpGet(ApiRoutes.Products.GetByCategory)]
    public async Task<IActionResult> GetProductsByCategory(Guid categoryId)
    {
        try
        {
            if (categoryId == Guid.Empty)
            {
                return BadRequest(new ApiResponse<string>
                {
                    IsSuccess = false,
                    Message = "Category ID cannot be empty",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            var filterableSpecTypes = await _specificationService.GetFilterableSpecificationTypesAsync();
            var filteredProducts = await _productService.GetFilteredProductsAsync(null, null,
                new List<Guid> { categoryId }, null, ExtractSpecificationFilters(filterableSpecTypes), null);

            var result = new ProductListVM
            {
                Products = _mapper.Map<List<ProductItemVM>>(filteredProducts),
                TotalCount = filteredProducts.Count
            };

            return Ok(new ApiResponse<ProductListVM>
            {
                Data = result,
                Message = "Products retrieved successfully",
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
            {
                IsSuccess = false,
                Message = "An error occurred while fetching category details",
                Errors = new List<string> { ex.Message },
                StatusCode = StatusCodes.Status500InternalServerError
            });
        }
    }

    [HttpGet(ApiRoutes.Products.GetBySubcategory)]
    public async Task<IActionResult> GetProductsBySubCategory(Guid subcategoryId)
    {
        try
        {
            if (subcategoryId == Guid.Empty)
            {
                return BadRequest(new ApiResponse<string>
                {
                    IsSuccess = false,
                    Message = "Subcategory ID cannot be empty",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            var subcategory = await _productService.GetSubCategoryByIdAsync(subcategoryId);

            if (subcategory == null)
            {
                return NotFound(new ApiResponse<string>
                {
                    IsSuccess = false,
                    Message = "Subcategory not found",
                    StatusCode = StatusCodes.Status404NotFound
                });
            }

            var filterableSpecTypes = await _specificationService.GetFilterableSpecificationTypesAsync();
            var filteredProducts = await _productService.GetFilteredProductsAsync(null, null,
                new List<Guid> { subcategory.CategoryId }, null, ExtractSpecificationFilters(filterableSpecTypes),
                new List<Guid> { subcategoryId });

            var result = new ProductListVM
            {
                Products = _mapper.Map<List<ProductItemVM>>(filteredProducts),
                TotalCount = filteredProducts.Count
            };

            return Ok(new ApiResponse<ProductListVM>
            {
                Data = result,
                Message = "Products retrieved successfully",
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
            {
                IsSuccess = false,
                Message = "An error occurred while fetching subcategory details",
                Errors = new List<string> { ex.Message },
                StatusCode = StatusCodes.Status500InternalServerError
            });
        }
    }

    [HttpGet(ApiRoutes.Products.GetDetails)]
    public async Task<IActionResult> GetProductDetails(Guid productId)
    {
        try
        {
            if (productId == Guid.Empty)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ApiResponse<string>
                {
                    IsSuccess = false,
                    Message = "Product ID cannot be empty",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            var productDetails = await _productService.GetProductDetailsByProductIdAsync(productId);

            if (productDetails == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new ApiResponse<string>
                {
                    IsSuccess = false,
                    Message = "Product not found",
                    StatusCode = StatusCodes.Status404NotFound
                });
            }

            var productDetailsVM = _mapper.Map<ProductItemVM>(productDetails);

            return Ok(new ApiResponse<ProductItemVM>
            {
                Data = productDetailsVM,
                Message = "Product details retrieved successfully",
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
            {
                IsSuccess = false,
                Message = "An error occurred while retrieving product details",
                Errors = new List<string> { ex.Message },
                StatusCode = StatusCodes.Status500InternalServerError
            });
        }
    }

    private Dictionary<Guid, (string Min, string Max)> ExtractSpecificationFilters(List<SpecificationTypeDto> specTypesDto)
    {
        var specFilters = new Dictionary<Guid, (string Min, string Max)>();

        foreach (var specTypeDto in specTypesDto.Where(st => st.IsFilterable))
        {
            var minValueKey = $"spec_{specTypeDto.Id}_min";
            var maxValueKey = $"spec_{specTypeDto.Id}_max";

            var minValue = Request.Query[minValueKey].FirstOrDefault() ?? "";
            var maxValue = Request.Query[maxValueKey].FirstOrDefault() ?? "";

            if (!string.IsNullOrWhiteSpace(minValue) || !string.IsNullOrWhiteSpace(maxValue))
            {
                if (decimal.TryParse(minValue.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var minDecimal))
                {
                    minValue = minDecimal.ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    minValue = "";
                }

                if (decimal.TryParse(maxValue.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var maxDecimal))
                {
                    maxValue = maxDecimal.ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    maxValue = "";
                }

                specFilters[specTypeDto.Id] = (minValue, maxValue);
                _logger.LogInformation($"Added spec filter: {specTypeDto.Name} , Min: {minValue}, Max: {maxValue}");
            }
        }

        return specFilters;
    }
}

