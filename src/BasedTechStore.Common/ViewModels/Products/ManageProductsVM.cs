using BasedTechStore.Common.ViewModels.Categories;

namespace BasedTechStore.Common.ViewModels.Products
{
    public class ManageProductsVM
    {
        public List<ProductItemVM> Products { get; set; } = new();

        public List<CategoryItemVM> Categories { get; set; } = new();
        public List<SubCategoryItemVM> SubCategories { get; set; } = new();
    }
}