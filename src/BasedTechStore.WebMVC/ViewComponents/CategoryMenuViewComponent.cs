using AutoMapper;
using BasedTechStore.Application.Common.Interfaces.Services;
using BasedTechStore.Common.ViewModels.Categories;
using Microsoft.AspNetCore.Mvc;

namespace BasedTechStore.Web.ViewComponents
{
    [ViewComponent(Name = "CategoryMenu")]
    public class CategoryMenuViewComponent : ViewComponent
    {
        private readonly IProductService _productService;
        private readonly IMapper _mapper;

        public CategoryMenuViewComponent(IProductService productService, IMapper mapper)
        {
            _productService = productService;
            _mapper = mapper;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categories = await _productService.GetCategoriesWithSubCategoriesAsync();
            var vm = _mapper.Map<List<CategoryItemVM>>(categories);
            return View(vm);
        }
    }
}
