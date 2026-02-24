using AutoMapper;
using BasedTechStore.Application.Common.Interfaces.Services;
using BasedTechStore.Application.Common.Queries;
using BasedTechStore.Application.DTOs.Products;
using BasedTechStore.Application.DTOs.Specifications;
using BasedTechStore.Common.Constants;
using BasedTechStore.Common.Models.Api;
using BasedTechStore.Common.Utilities;
using BasedTechStore.Common.ViewModels.Categories;
using BasedTechStore.Common.ViewModels.Products;
using BasedTechStore.Domain.Constants;
using BasedTechStore.WebApi.Attributes;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

[ApiController]
[Route(ApiRoutes.Products.Base)]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> GetAll(
        [FromQuery] string? search = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] Guid? subCategoryId = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] string? brand = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var criteria = new ProductSearchCriteria
        {
            SearchTerm = search,
            CategoryId = categoryId,
            SubCategoryId = subCategoryId,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            Brands = string.IsNullOrEmpty(brand) ? null : new List<string> { brand },
            SortBy = sortBy,
            SortDescending = sortDesc,
            Page = page,
            PageSize = pageSize
        };

        var result = await _productService.SearchAsync(criteria);

        return Ok(ApiResponse<object>.Success(new
        {
            items = result.Items,
            pagination = new
            {
                page = result.Page,
                pageSize = result.PageSize,
                totalCount = result.TotalCount,
                totalPages = result.TotalPages,
                hasPrevious = result.HasPrevious,
                hasNext = result.HasNext
            }
        }));
    }

    [HttpGet(ApiRoutes.Products.GetById)]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetById(Guid id)
    {
        var product = await _productService.GetByIdAsync(id);
        return Ok(ApiResponse<ProductDto>.Success(product));
    }

    [HttpGet(ApiRoutes.Products.GetFeatured)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetFeatured([FromQuery] int count = 10)
    {
        var products = await _productService.GetFeaturedAsync(count);
        return Ok(ApiResponse<IEnumerable<ProductDto>>.Success(products));
    }

    [HttpGet(ApiRoutes.Products.GetRelated)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetRelated(Guid id, [FromQuery] int count = 4)
    {
        var products = await _productService.GetRelatedAsync(id, count);
        return Ok(ApiResponse<IEnumerable<ProductDto>>.Success(products));
    }

    [HttpGet(ApiRoutes.Products.GetByCategory)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetByCategory(Guid categoryId)
    {
        var products = await _productService.GetByCategoryIdAsync(categoryId);
        return Ok(ApiResponse<IEnumerable<ProductDto>>.Success(products));
    }

    [HttpGet(ApiRoutes.Products.GetBySubcategory)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetBySubCategory(Guid subcategoryId)
    {
        var products = await _productService.GetBySubCategoryIdAsync(subcategoryId);
        return Ok(ApiResponse<IEnumerable<ProductDto>>.Success(products));
    }

    [HttpGet(ApiRoutes.Products.GetProductFilters)]
    public async Task<ActionResult<ApiResponse<ProductFiltersDto>>> GetFilters([FromQuery] Guid? categoryId = null)
    {
        var filters = await _productService.GetAvailableFiltersAsync(categoryId);
        return Ok(ApiResponse<ProductFiltersDto>.Success(filters));
    }

    [HttpPost]
    [RequirePermission(Permissions.ProductsCreate)]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Create([FromBody] CreateProductDto dto)
    {
        var product = await _productService.CreateAsync(dto);
        return CreatedAtAction(
            nameof(GetById),
            new { id = product.Id },
            ApiResponse<ProductDto>.Success(product, "Product created successfully")
        );
    }

    [HttpPut(ApiRoutes.Products.GetById)]
    [RequirePermission(Permissions.ProductsEdit)]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Update(Guid id, [FromBody] UpdateProductDto dto)
    {
        var product = await _productService.UpdateAsync(id, dto);
        return Ok(ApiResponse<ProductDto>.Success(product, "Product updated successfully"));
    }

    [HttpPatch(ApiRoutes.Products.UpdateStock)]
    [RequirePermission(Permissions.ProductsEdit)]
    public async Task<IActionResult> UpdateStock(Guid id, [FromBody] UpdateStockDto dto)
    {
        await _productService.UpdateStockAsync(id, dto.Quantity);
        return NoContent();
    }

    [HttpDelete(ApiRoutes.Products.GetById)]
    [RequirePermission(Permissions.ProductsDelete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _productService.DeleteAsync(id);
        return NoContent();
    }
}
