namespace BasedTechStore.Common.ViewModels.Products
{
    public class ProductFilteringListVM
    {
        public List<ProductItemVM> Products { get; set; } = new();
        public ProductFilterVM ProductFilterVM { get; set; } = new();
    }
}
