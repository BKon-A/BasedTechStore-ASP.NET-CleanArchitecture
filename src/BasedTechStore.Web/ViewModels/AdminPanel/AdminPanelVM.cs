using BasedTechStore.Application.DTOs.Categories;
using BasedTechStore.Application.DTOs.Product;
using BasedTechStore.Web.ViewModels.Categories;
using BasedTechStore.Web.ViewModels.Products;

namespace BasedTechStore.Web.ViewModels.AdminPanel
{
    public class AdminPanelVM
    {
        public ManageProductsVM Products { get; set; } = new();
        public ManageCategoriesVM Categories { get; set; } = new();
    }
}
