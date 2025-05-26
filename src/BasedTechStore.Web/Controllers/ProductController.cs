using AutoMapper;
using BasedTechStore.Application.Common.Interfaces.Services;
using BasedTechStore.Application.DTOs.Product;
using BasedTechStore.Web.ViewModels.Products;
using BasedTechStore.Web.ViewModels.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasedTechStore.Web.Controllers
{
    public class ProductController : BaseController
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
        public async Task<IActionResult> Index(Guid? subcategoryId)
        {
            List<ProductDto> products;

            if (subcategoryId.HasValue)
            {
                var subcategoryName = await _productService.GetSubCategoryNameByIdAsync(subcategoryId.Value);

                products = await _productService.GetProductsBySubCategoryAsync(subcategoryId);

                if (!string.IsNullOrEmpty(subcategoryName) && !products.Any(p => p.SubCategoryName == subcategoryName))
                {
                    ViewData["Message"] = $"Товару в категорії не знайдено. Показано всі продукти";
                }
            }
            else
            {
                products = await _productService.GetAllProductsAsync();
            }

            var vm = _mapper.Map<List<ProductItemVM>>(products);
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> BySubCategory(Guid subcategoryId)
        {
            var products = await _productService.GetProductsBySubCategoryAsync(subcategoryId);
            return View("Index", products);
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
    }
}
