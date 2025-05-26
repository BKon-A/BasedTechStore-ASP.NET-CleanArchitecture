namespace BasedTechStore.Domain.Entities.Specifications
{
    public class SpecificationType
    {
        public Guid Id { get; set; }
        public Guid SpecificationCategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Unit { get; set; } = string.Empty;
        public bool IsFilterable { get; set; }
        public int DisplayOrder { get; set; }

        // Navigation Properties
        public SpecificationCategory SpecificationCategory { get; set; } = null!;
        public ICollection<ProductSpecification> ProductSpecifications { get; set; } = new List<ProductSpecification>();
    }
}