using BasedTechStore.Application.DTOs.Categories;
using BasedTechStore.Application.DTOs.Product;
using BasedTechStore.Web.ViewModels.Categories;
using BasedTechStore.Web.ViewModels.Products;
using BasedTechStore.Web.ViewModels.Specifications;

namespace BasedTechStore.Web.ViewModels.AdminPanel
{
    public class AdminPanelVM
    {
        public ManageProductsVM Products { get; set; }
        public ManageCategoriesVM Categories { get; set; }
        public ManageSpecificationsVM Specifications { get; set; }
    }
}
