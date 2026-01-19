using AutoMapper;
using BasedTechStore.Application.Common.Interfaces.Services;
using BasedTechStore.Application.DTOs.Categories;
using BasedTechStore.Application.DTOs.Product;
using BasedTechStore.Application.DTOs.Specifications;
using BasedTechStore.Common.ViewModels.AdminPanel;
using BasedTechStore.Common.ViewModels.Categories;
using BasedTechStore.Common.ViewModels.PendingChanges;
using BasedTechStore.Common.ViewModels.Products;
using BasedTechStore.Common.ViewModels.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BasedTechStore.Web.Controllers
{
    [Authorize(Roles = "Manager")]
    public class AdminPanelController : Controller
    {
        private readonly IProductService _productService;
        private readonly ISpecificationService _specificationService;
        private readonly IMapper _mapper;
        private readonly ILogger<AdminPanelController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminPanelController(IProductService productService,
            IMapper mapper, ILogger<AdminPanelController> logger, 
            ISpecificationService specificationService, IWebHostEnvironment webHostEnvironment)
        {
            _productService = productService;
            _mapper = mapper;
            _logger = logger;
            _specificationService = specificationService;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = new AdminPanelVM
            {
                Products = await GetManageProductsVMAsync(),
                Categories = await GetManageCategoriesVMAsync(),
                Specifications = await GetManageSpecificationsVMAsync()
            };

            return View(model);
        }

        // =========================== START Get Data ============================
        private async Task<ManageProductsVM> GetManageProductsVMAsync()
        {
            var productsVM = _mapper.Map<List<ProductItemVM>>(await _productService.GetAllProductsAsync());
            var categories = await _productService.GetCategoriesWithSubCategoriesAsync();
            var categoriesVM = _mapper.Map<List<CategoryItemVM>>(categories);

            var allSubCategories = categoriesVM.SelectMany(c => c.SubCategories).ToList();
            var subCategorySelectList = allSubCategories
                .Select(sc => new SelectListItem
                {
                    Value = sc.Id.ToString(),
                    Text = sc.Name
                }).ToList();

            foreach (var product in productsVM)
            {
                product.SubCategories = subCategorySelectList;
            }

            return new ManageProductsVM
            {
                Products = productsVM,
                Categories = categoriesVM,
                SubCategories = allSubCategories
            };
        }
        private async Task<ManageCategoriesVM> GetManageCategoriesVMAsync()
        {
            var categories = await _productService.GetCategoriesWithSubCategoriesAsync();
            var categoriesVM = _mapper.Map<List<CategoryItemVM>>(categories);

            return new ManageCategoriesVM
            {
                Categories = categoriesVM
            };
        }

        private async Task<ManageSpecificationsVM> GetManageSpecificationsVMAsync()
        {
            var categories = await _productService.GetAllCategoriesAsync();
            var categoriesVM = _mapper.Map<List<CategoryItemVM>>(categories);

            return new ManageSpecificationsVM
            {
                Categories = categoriesVM
            };
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _productService.GetAllCategoriesAsync();
            var categoriesVM = _mapper.Map<List<CategoryItemVM>>(categories);
            return Json(categoriesVM);
        }

        [HttpGet]
        public async Task<IActionResult> GetSubCategoriesByCategoryId(Guid categoryId)
        {
            var subCategories = await _productService.GetSubCategoriesByCategoryIdAsync(categoryId);
            var subCategoryVMs = _mapper.Map<List<SubCategoryItemVM>>(subCategories);
            return Json(subCategoryVMs);
        }

        [HttpGet]
        public async Task<IActionResult> GetSubCategoriesByCategory(Guid categoryId)
        {
            var subCategories = await _productService.GetSubCategoriesByCategoryIdAsync(categoryId);
            var subCategoryVMs = _mapper.Map<List<SubCategoryItemVM>>(subCategories);
            return Json(subCategoryVMs);
        }

        [HttpGet]
        public async Task<IActionResult> GetSpecificationCategories(Guid categoryId)
        {
            try
            {
                var categories = await _specificationService.GetSpecificationCategoriesByCategoryIdAsync(categoryId);
                var categoriesVM = _mapper.Map<List<SpecificationCategoryVM>>(categories);

                return Json(categoriesVM);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting specification categories");
                return Json(new List<SpecificationCategoryVM>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSpecificationCategory(Guid id)
        {
            try
            {
                var category = await _specificationService.GetSpecificationCategoryAsync(id);
                if (category == null)
                {
                    return NotFound();
                }

                var categoryVM = _mapper.Map<SpecificationCategoryVM>(category);
                return Json(categoryVM);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting specification category");
                return StatusCode(500, "Error while getting specification category");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSpecificationTypes(Guid specCategoryId)
        {
            try
            {
                var types = await _specificationService.GetSpecificationTypesBySpecCategoryIdAsync(specCategoryId);
                var typesVM = _mapper.Map<List<SpecificationTypeVM>>(types);

                return Json(typesVM);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting specification types");
                return Json(new List<SpecificationTypeVM>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSpecificationType(Guid id)
        {
            try
            {
                var type = await _specificationService.GetSpecificationTypeAsync(id);
                if (type == null)
                {
                    return NotFound();
                }

                var typeVM = _mapper.Map<SpecificationTypeVM>(type);

                return Json(typeVM);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting specification type");
                return StatusCode(500, "Error while getting specification type");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProductSpecifications(Guid productId, Guid categoryId)
        {
            try
            {
                var types = await _specificationService.GetSpecificationTypesByCategoryIdAsync(categoryId);
                var specifications = await _specificationService.GetProductSpecificationsByProductIdAsync(productId);

                var model = new ProductSpecificationsVM
                {
                    ProductId = productId,
                    CategoryId = categoryId,
                    SpecificationTypes = _mapper.Map<List<SpecificationTypeVM>>(types),
                    ProductSpecifications = _mapper.Map<List<ProductSpecificationVM>>(specifications)
                };

                return Json(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting product specifications");
                return StatusCode(500, "Error while getting product specifications");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProductsByCategoryId(Guid categoryId)
        {
            try
            {
                var products = await _productService.GetProductsByCategoryIdAsync(categoryId);
                var productsVM = _mapper.Map<List<ProductItemVM>>(products);
                return Json(productsVM);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting products by category ID");
                return StatusCode(500, new { error = "Error while getting products by category ID" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProductSpecificationsForm(Guid productId)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(productId);
                if (product == null)
                {
                    return NotFound("Продукт не знайдено");
                }

                var subCategories = await _productService.GetAllSubCategoriesAsync();
                var subCategory = subCategories.FirstOrDefault(sc => sc.Id.ToString() == product.SubCategoryId.ToString());

                if (subCategory == null)
                {
                    return BadRequest("Не вдалося визначити категорію продукту");
                }

                var allTypes = await _specificationService.GetSpecificationTypesByCategoryIdAsync(subCategory.CategoryId);
                var allPossibleSpecs = await _specificationService.GetProductSpecificationsByProductIdAsync(productId);

                var model = new ProductSpecificationsVM
                {
                    ProductId = productId,
                    CategoryId = subCategory.CategoryId,
                    SpecificationTypes = _mapper.Map<List<SpecificationTypeVM>>(allTypes),
                    ProductSpecifications = _mapper.Map<List<ProductSpecificationVM>>(allPossibleSpecs)
                };

                return PartialView("_ProductSpecificationsPartial", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting product specifications form");
                return StatusCode(500, "Error while getting product specifications form");
            }
        }
        // =========================== END Get Data ============================

        // =========================== START Update Data =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSpecificationCategory(SpecificationCategoryVM specificationCategoryVM)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    foreach (var state in ModelState)
                    {
                        foreach (var error in state.Value.Errors)
                        {
                            _logger.LogError($"Validation error for {state.Key}: {error.ErrorMessage}");
                        }
                    }

                    return BadRequest(ModelState);
                }

                return Json(specificationCategoryVM);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while saving specification category");
                return StatusCode(500, "Error while saving specification category");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSpecificationType(SpecificationTypeVM specificationTypeVM)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Json(specificationTypeVM);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while saving specification type");
                return StatusCode(500, "Error while saving specification type");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadImage(IFormFile image)
        {
            try
            {
                if (image == null || image.Length == 0)
                {
                    return Content("Не надано файл зображення");
                }

                string? imageUrl = await _productService.UploadProductImageAsync(image);

                if (string.IsNullOrEmpty(imageUrl))
                {
                    return Content("Не вдалося завантажити зображення");
                }

                return Content(imageUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при завантаженні зображення");
                return Content("Помилка при завантаженні зображення");
            }
        }
        // =========================== END Update Data =========================

        // =========================== START Save Data ===========================
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveProductSpecifications(Guid productId, Dictionary<string, string> specifications) // Use ManageSpecificationsVM or ProductSpecificationsVM
        {
            try
            {
                if (productId == Guid.Empty)
                {
                    return BadRequest("Не вказано ID продукту");
                }

                var productSpecificationsDto = new List<ProductSpecificationDto>();

                foreach (var spec in specifications)
                {
                    if (!string.IsNullOrWhiteSpace(spec.Value) && Guid.TryParse(spec.Key, out var specTypeId))
                    {
                        productSpecificationsDto.Add(new ProductSpecificationDto
                        {
                            ProductId = productId,
                            SpecificationTypeId = specTypeId,
                            Value = spec.Value
                        });
                    }
                }
                await _specificationService.SaveProductSpecificationsAsync(productId, productSpecificationsDto);

                return Json(new { success = true, message = "Характеристики збережено успішно" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while saving product specifications");
                return Json(new { success = false, message = "Error while saving product specifications" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAllSpecifications(ManageSpecificationsVM manageSpecificationsVM)
        {
            try
            {
                if (manageSpecificationsVM == null || manageSpecificationsVM.PendingChanges == null)
                {
                    return BadRequest("Не надано змін для збереження");
                }

                var pendingChanges = manageSpecificationsVM.PendingChanges;

                EnsureUniqueDisplayOrders(pendingChanges);

                var createdCategoriesDto = _mapper.Map<List<SpecificationCategoryDto>>(pendingChanges.CreatedCategories);
                var updatedCategoriesDto = _mapper.Map<List<SpecificationCategoryDto>>(pendingChanges.UpdatedCategories);
                var deletedCategoriesDto = _mapper.Map<List<SpecificationCategoryDto>>(pendingChanges.DeletedCategories);

                var createdTypesDto = _mapper.Map<List<SpecificationTypeDto>>(pendingChanges.CreatedTypes);
                var updatedTypesDto = _mapper.Map<List<SpecificationTypeDto>>(pendingChanges.UpdatedTypes);
                var deletedTypesDto = _mapper.Map<List<SpecificationTypeDto>>(pendingChanges.DeletedTypes);

                // Викликаємо метод сервісу з DTO-параметрами
                await _specificationService.SaveAllSpecificationsAsync(
                    createdCategoriesDto,
                    updatedCategoriesDto,
                    deletedCategoriesDto,
                    createdTypesDto,
                    updatedTypesDto,
                    deletedTypesDto
                );

                return Json(new { success = true, message = "Усі зміни характеристик успішно збережено" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при збереженні всіх характеристик");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        private void EnsureUniqueDisplayOrders(SpecsPendingChangesVM pendingChanges)
        {
            var allCategories = new List<SpecificationCategoryVM>();
            if (pendingChanges.CreatedCategories != null) allCategories.AddRange(pendingChanges.CreatedCategories);
            if (pendingChanges.UpdatedCategories != null) allCategories.AddRange(pendingChanges.UpdatedCategories);

            // Group categories by ProductCategoryId
            var categoriesByProductId = allCategories
                .GroupBy(c => c.ProductCategoryId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Reindex display orders for each group
            foreach (var group in categoriesByProductId.Values)
            {
                var displayOrders = group.Select(c => c.DisplayOrder).Distinct().ToList();
                var duplicates = displayOrders.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key);

                if (duplicates.Any())
                {
                    // If there are duplicates, reindex the display orders
                    var sortedCategories = group.OrderBy(c => c.DisplayOrder).ToList();
                    for (int i = 0; i < sortedCategories.Count; i++)
                    {
                        sortedCategories[i].DisplayOrder = i + 1;
                    }
                }
            }

            var allTypes = new List<SpecificationTypeVM>();
            if (pendingChanges.CreatedTypes != null) allTypes.AddRange(pendingChanges.CreatedTypes);
            if (pendingChanges.UpdatedTypes != null) allTypes.AddRange(pendingChanges.UpdatedTypes);

            var typesBySpecCategoryId = allTypes
                .GroupBy(t => t.SpecificationCategoryId)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var group in typesBySpecCategoryId.Values)
            {
                var displayOrders = group.Select(t => t.DisplayOrder).Distinct().ToList();
                var duplicates = displayOrders.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key);

                if (duplicates.Any())
                {
                    // If there are duplicates, reindex the display orders
                    var sortedTypes = group.OrderBy(t => t.DisplayOrder).ToList();
                    for (int i = 0; i < sortedTypes.Count; i++)
                    {
                        sortedTypes[i].DisplayOrder = i + 1;
                    }
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSpecifications()
        {
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSpecificationCategory(SpecificationCategoryVM model)
        {
            return await UpdateSpecificationCategory(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSpecificationType(SpecificationTypeVM model)
        {
            return await UpdateSpecificationType(model);
        }
        // =========================== END Save Data ===========================

        // =========================== START Delete Data =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUnusedImages(List<string> imageUrls)
        {
            try
            {
                if (imageUrls == null || !imageUrls.Any())
                {
                    return Content("Не надано URL-адреси зображень для видалення");
                }
                var deletedCount = await _productService.DeleteUnusedImagesAsync(imageUrls);
                return Content(deletedCount.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting unused images");
                return Content("Помилка при спробі видалення невикористовуваних зображень");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSpecificationCategory(Guid id, Guid productCategoryId)
        {
            try
            {
                var result = await _specificationService.DeleteSpecificationCategoryAsync(id);
                if (!result)
                {
                    return NotFound();
                }

                var categories = await _specificationService.GetSpecificationCategoriesByCategoryIdAsync(productCategoryId);
                var categoriesVM = _mapper.Map<List<SpecificationCategoryVM>>(categories);

                return Json(categoriesVM);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting specification category");
                return StatusCode(500, "Error while deleting specification category");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSpecificationType(Guid id, Guid specCategoryId)
        {
            try
            {
                var result = await _specificationService.DeleteSpecificationTypeAsync(id);
                if (!result)
                {
                    return NotFound();
                }

                var types = await _specificationService.GetSpecificationTypesBySpecCategoryIdAsync(specCategoryId);
                var typesVM = _mapper.Map<List<SpecificationTypeVM>>(types);

                return Json(typesVM);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting specification type");
                return StatusCode(500, "Error while deleting specification type");
            }
        }
        // =========================== END Delete Data =========================
    }
}
