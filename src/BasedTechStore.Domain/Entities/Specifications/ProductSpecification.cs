using BasedTechStore.Domain.Entities.Products;

namespace BasedTechStore.Domain.Entities.Specifications
{
    public class ProductSpecification
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Guid SpecificationTypeId { get; set; }
        public string Value { get; set; } = string.Empty;

        // Navigation Properties
        public SpecificationType SpecificationType { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
