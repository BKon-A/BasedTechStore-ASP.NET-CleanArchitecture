using BasedTechStore.Domain.Entities.Categories;
using BasedTechStore.Domain.Entities.Specifications;
using System.Collections;

namespace BasedTechStore.Domain.Entities.Products
{
    public class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? Brand { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Guid SubCategoryId { get; set; }
        public SubCategory SubCategory { get; set; } = null!;

        public ICollection<ProductSpecification> ProductSpecifications { get; set; } = new List<ProductSpecification>();
    }
}
