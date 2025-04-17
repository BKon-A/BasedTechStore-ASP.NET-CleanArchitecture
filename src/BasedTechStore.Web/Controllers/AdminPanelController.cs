using BasedTechStore.Application.Common.Interfaces.Services;
using BasedTechStore.Application.DTOs.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasedTechStore.Web.Controllers
{
    [Authorize(Roles = "Manager")]
    public class AdminPanelController : BaseController
    {
        private readonly IProductService _productService;

        public AdminPanelController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllProductsAsync();
            return View(products);
        }

        [Authorize(Roles = "Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductDto productDto)
        {
            if (!ModelState.IsValid)
            {
                // Якщо модель невалідна, перезавантажуємо сторінку з продуктами
                var products = await _productService.GetAllProductsAsync();
                ViewData["Error"] = "Будь ласка, заповніть усі обов’язкові поля.";
                return View("Index", products);
            }

            await _productService.CreateProductAsync(productDto);

            // Після створення редирект на Index
            return RedirectToAction("Index");
        }
    }
}
