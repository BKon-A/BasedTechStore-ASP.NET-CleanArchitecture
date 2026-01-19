namespace BasedTechStore.Common.ViewModels.Specifications
{
    public class ProductSpecificationsVM
    {
        public Guid ProductId { get; set; }
        public Guid CategoryId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public List<SpecificationTypeVM> SpecificationTypes { get; set; } = new();
        public List<ProductSpecificationVM> ProductSpecifications { get; set; } = new();
    }
}
