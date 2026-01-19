using BasedTechStore.Common.ViewModels.Categories;
using BasedTechStore.Common.ViewModels.Products;
using BasedTechStore.Common.ViewModels.Specifications;

namespace BasedTechStore.Common.ViewModels.AdminPanel
{
    public class AdminPanelVM
    {
        public ManageProductsVM Products { get; set; }
        public ManageCategoriesVM Categories { get; set; }
        public ManageSpecificationsVM Specifications { get; set; }
    }
}
