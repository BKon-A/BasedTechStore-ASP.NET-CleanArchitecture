using BasedTechStore.Domain.Entities.Products;

namespace BasedTechStore.Domain.Entities.Specifications
{
    public class ProductSpecification
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public string Key { get; set; } = null!;
        public string Value { get; set; } = null!;
    }
}
