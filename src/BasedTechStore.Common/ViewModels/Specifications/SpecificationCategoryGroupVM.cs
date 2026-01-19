namespace BasedTechStore.Common.ViewModels.Specifications
{
    public class SpecificationCategoryGroupVM
    {
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public List<ProductSpecificationVM> Specifications { get; set; } = new List<ProductSpecificationVM>();
    }
}
