namespace BasedTechStore.Web.ViewModels.Specifications
{
    public class SpecificationCategoryVM
    {
        public Guid Id { get; set; } = Guid.Empty;
        public Guid ProductCategoryId { get; set; }
        public string Name { get; set; }
        public int DisplayOrder { get; set; }
        public string ProductCategoryName { get; set; }
    }
}
