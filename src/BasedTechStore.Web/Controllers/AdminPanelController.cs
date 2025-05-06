using AutoMapper;
using BasedTechStore.Application.Common.Interfaces.Services;
using BasedTechStore.Application.DTOs.Categories;
using BasedTechStore.Application.DTOs.Product;
using BasedTechStore.Web.ViewModels.AdminPanel;
using BasedTechStore.Web.ViewModels.Categories;
using BasedTechStore.Web.ViewModels.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BasedTechStore.Web.Controllers
{
    [Authorize(Roles = "Manager")]
    public class AdminPanelController : BaseController
    {
        private readonly IProductService _productService;
        private readonly IMapper _mapper;
        private readonly ILogger<AdminPanelController> _logger;

        public AdminPanelController(IProductService productService,
            IMapper mapper,
            ILogger<AdminPanelController> logger)
        {
            _productService = productService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var products = _mapper.Map<List<ProductItemVM>>(await _productService.GetAllProductsAsync());
            var categories = await _productService.GetCategoriesWithSubCategoriesAsync();
            var categoriesVM = _mapper.Map<List<CategoryItemVM>>(categories);

            var allSubCategories = categoriesVM.SelectMany(c => c.SubCategories).ToList();
            var subCategorySelectList = allSubCategories
                .Select(sc => new SelectListItem
                {
                    Value = sc.Id.ToString(),
                    Text = sc.Name
                }).ToList();

            foreach (var product in products)
            {
                product.SubCategories = subCategorySelectList;
            }

            var viewModel = new AdminPanelVM
            {
                Products = new ManageProductsVM
                {
                    Products = products,
                    Categories = categoriesVM,
                    SubCategories = allSubCategories
                },
                Categories = new ManageCategoriesVM
                {
                    Categories = categoriesVM
                }
            };

            return View(viewModel);
        }

        //[HttpPost]
        //[Authorize(Roles = "Manager")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> UploadProduct(IFormFile ImageFile, ProductDto productDto)
        //{
        //    if (ImageFile != null && ImageFile.Length > 0)
        //    {
        //        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
        //        Directory.CreateDirectory(uploadsFolder); // На всяк випадок

        //        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
        //        string filePath = Path.Combine(uploadsFolder, fileName);

        //        using (var fileStream = new FileStream(filePath, FileMode.Create))
        //        {
        //            await ImageFile.CopyToAsync(fileStream);
        //        }

        //        // Прив'язуємо URL до продукту
        //        productDto.ImageUrl = "/uploads/" + fileName;
        //    }

        //    await _productService.AddProductAsync(productDto);

        //    return RedirectToAction("Index");
        //}

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _productService.GetAllCategoriesAsync();
            return Json(categories);
        }

        [HttpGet]
        public async Task<IActionResult> GetSubCategoriesByCategoryId(Guid categoryId)
        {
            var subCategories = await _productService.GetSubCategoriesByCategoryIdAsync(categoryId);
            return Json(subCategories);
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveProducts(ManageProductsVM manageProductsVM, IFormFile? image, Guid? productIdForImage)
        {
            foreach (var product in manageProductsVM.Products)
            {
                if (string.IsNullOrWhiteSpace(product.CategoryName) || string.IsNullOrWhiteSpace(product.SubCategoryName))
                {
                    ModelState.AddModelError("", $"Будь ласка, оберіть категорію та підкатегорію для продукту '{product.Name}'");
                }
            }

            if (!ModelState.IsValid)
            {
                return PartialView("_ManageProductsPartial", manageProductsVM);
            }

            var productsDto = _mapper.Map<List<ProductDto>>(manageProductsVM.Products);

            if (image != null && image.Length > 0)
            {
                var imageUrl = await _productService.UploadProductImageAsync(image);

                if (productIdForImage.HasValue)
                {
                    var targetProduct = productsDto.FirstOrDefault(p => p.Id == productIdForImage.Value);
                    if (targetProduct != null)
                    {
                        targetProduct.ImageUrl = imageUrl;
                    }
                }
            }

            var updatedProductsDto = await _productService.SaveProductsAsync(productsDto);
            var updatedProductsVM = new ManageProductsVM
            {
                Products = _mapper.Map<List<ProductItemVM>>(updatedProductsDto)
            };
            return PartialView("_ManageProductsPartial", updatedProductsVM);
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadImage(IFormFile image)
        {
            if (image == null || image.Length == 0)
                return Content("error");

            var imageUrl = await _productService.UploadProductImageAsync(image);
            if (string.IsNullOrEmpty(imageUrl))
                return Content("error");

            return Content(imageUrl);
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUnusedImages(List<string> imageUrls)
        {
            if (imageUrls == null || !imageUrls.Any())
                return Content("No images to delete");

            // Background task to delete images
            _ = Task.Run(async () =>
            {
                try
                {
                    await _productService.DeleteUnusedImagesAsync(imageUrls);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error deleting unused images: {ex.Message}");
                }
            });
            
            return Content("Images deletion in progress");
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCategories(ManageCategoriesVM manageCategoriesVM)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_ManageCategoriesPartial", manageCategoriesVM);
            }

            var categoriesDto = _mapper.Map<List<CategoryDto>>(manageCategoriesVM.Categories);

            categoriesDto.SelectMany(c =>
                c.SubCategories.Select(sc =>
                {
                    sc.CategoryId = c.Id;
                    return sc;
                }))
                .ToList();

            var updatedCategoriesDto = await _productService.SaveCategoriesAsync(categoriesDto);
            var updatedCategoriesVM = new ManageCategoriesVM
            {
                Categories = _mapper.Map<List<CategoryItemVM>>(updatedCategoriesDto)
            };

            return PartialView("_ManageCategoriesPartial", updatedCategoriesVM);
        }

        [HttpGet]
        public async Task<IActionResult> GetSubCategoriesByCategory(Guid categoryId)
        {
            var subCategories = await _productService.GetSubCategoriesByCategoryIdAsync(categoryId);
            var subCategoryVMs = _mapper.Map<List<SubCategoryItemVM>>(subCategories);
            return Json(subCategoryVMs);
        }
    }
}
