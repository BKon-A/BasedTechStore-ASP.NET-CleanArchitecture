namespace BasedTechStore.Web.ViewModels.Products
{
    public class ProductFilteringListVM
    {
        public List<ProductItemVM> Products { get; set; } = new List<ProductItemVM>();
        public ProductFilterVM ProductFilterVM { get; set; } = new ProductFilterVM();
    }
}
