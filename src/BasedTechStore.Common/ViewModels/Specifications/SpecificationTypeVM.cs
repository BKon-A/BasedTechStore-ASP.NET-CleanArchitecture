namespace BasedTechStore.Common.ViewModels.Specifications
{
    public class SpecificationTypeVM
    {
        public Guid Id { get; set; }
        public Guid SpecificationCategoryId { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public bool IsFilterable { get; set; }
        public int DisplayOrder { get; set; }
        public string SpecificationCategoryName { get; set; }
    }
}
