using AutoMapper;
using BasedTechStore.Application.Common.Interfaces.Services;
using BasedTechStore.Application.DTOs.Product;
using BasedTechStore.Application.DTOs.Specifications;
using BasedTechStore.Web.ViewModels.Categories;
using BasedTechStore.Web.ViewModels.Products;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace BasedTechStore.Web.Controllers
{
    public class ProductController : Controller
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

        [HttpGet]
        public async Task<IActionResult> Index(ProductFilterVM productFilters, Guid? categoryId = null, List<Guid> subcategoryIds = null)
        {
            if (categoryId.HasValue && (productFilters.SelectedCategoryIds == null || productFilters.SelectedCategoryIds.Any()))
            {
                productFilters.SelectedCategoryIds = new List<Guid> { categoryId.Value };
            }
            if (subcategoryIds != null && subcategoryIds.Any())
            {
                productFilters.SelectedSubCategoryIds = subcategoryIds;
            }

            var categories = await _productService.GetAllCategoriesAsync();

            var filterableSpecficationTypes = await _specificationService.GetFilterableSpecificationTypesAsync();

            var groupedSpecTypes = filterableSpecficationTypes
                .GroupBy(st => new { st.SpecificationCategoryId, st.SpecificationCategoryName })
                .Select(g => new SpecificationFilterGroupVM
                {
                    CategoryId = g.Key.SpecificationCategoryId,
                    CategoryName = g.Key.SpecificationCategoryName,
                    Specifications = g.Select(s => _mapper.Map<SpecificationFilterItemVM>(s)).ToList()
                })
                .ToList();

            var allProducts = await _productService.GetAllProductsAsync();
            var brands = allProducts.Where(p => !string.IsNullOrEmpty(p.Brand))
                                    .Select(p => p.Brand)
                                    .Distinct()
                                    .OrderBy(b => b)
                                    .ToList();

            var filterdProducts = await _productService.GetFilteredProductsAsync(productFilters.MinPrice,
                productFilters.MaxPrice, productFilters.SelectedCategoryIds, productFilters.Brands, 
                ExtractSpecificationFilters(productFilters, filterableSpecficationTypes), 
                productFilters.SelectedSubCategoryIds);

            var vm = new ProductFilteringListVM
            {
                Products = _mapper.Map<List<ProductItemVM>>(filterdProducts),
                ProductFilterVM = new ProductFilterVM
                {
                    MinPrice = productFilters.MinPrice,
                    MaxPrice = productFilters.MaxPrice,
                    Categories = _mapper.Map<List<CategoryItemVM>>(categories),
                    SelectedCategoryIds = productFilters.SelectedCategoryIds ?? new List<Guid>(),
                    Brands = brands,
                    SelectedBrands = productFilters.SelectedBrands ?? new List<string>(),
                    FilterableSpecifications = groupedSpecTypes
                }
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> BySubCategory(Guid subcategoryId)
        {
            try
            {
                if (subcategoryId == Guid.Empty)
                {
                    return BadRequest("Subcategory ID cannot be empty");
                }
                var productFilters = new ProductFilterVM()
                {
                    SelectedSubCategoryIds = new List<Guid> { subcategoryId }
                };

                var subcategory = await _productService.GetSubCategoryByIdAsync(subcategoryId);

                if (subcategory == null)
                {
                    return NotFound("Subcategory not found");
                }

                productFilters.SelectedCategoryIds = new List<Guid> { subcategory.CategoryId };

                return RedirectToAction(nameof(Index), productFilters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching subcategory details for subcategoryId: {SubCategoryId}", subcategoryId);
                return StatusCode(500, "Internal server error while fetching subcategory details");
            }
                
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid productId)
        {
            try
            {
                var productDetails = await _productService.GetProductDetailsByProductIdAsync(productId);
                if (productDetails == null)
                {
                    return NotFound("Product not found");
                }

                var vm = _mapper.Map<ProductDetailsVM>(productDetails);

                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching product details for productId: {ProductId}", productId);
                return StatusCode(500, "Internal server error while fetching product details");
            }
        }

        private Dictionary<Guid, (string Min, string Max)> ExtractSpecificationFilters(ProductFilterVM filters, List<SpecificationTypeDto> specTypesDto)
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
                    if (decimal.TryParse(minValue.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out _) ||
                        decimal.TryParse(maxValue.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                    {
                        minValue = string.IsNullOrWhiteSpace(minValue) ? "" : 
                            decimal.Parse(minValue.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
                        maxValue = string.IsNullOrWhiteSpace(maxValue) ? "" :
                            decimal.Parse(maxValue.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
                    }

                    specFilters[specTypeDto.Id] = (minValue, maxValue);
                    _logger.LogInformation($"Added spec filter: {specTypeDto.Name} , Min: {minValue}, Max: {maxValue}");
                }
            }

            return specFilters;
        }
    }
}
