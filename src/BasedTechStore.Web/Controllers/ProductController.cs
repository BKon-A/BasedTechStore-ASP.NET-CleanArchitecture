using AutoMapper;
using BasedTechStore.Application.Common.Interfaces.Services;
using BasedTechStore.Application.DTOs.Product;
using BasedTechStore.Web.ViewModels.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasedTechStore.Web.Controllers
{
    public class ProductController : BaseController
    {
        private readonly IProductService _productService;
        private readonly IMapper _mapper;

        public ProductController(IProductService productService, IMapper mapper)
        {
            _productService = productService;
            _mapper = mapper;
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
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
            {
                return NotFound("Product not found");
            }
            return View(product);
        }
    }
}
