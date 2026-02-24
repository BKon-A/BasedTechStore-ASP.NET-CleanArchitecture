using BasedTechStore.Domain.Entities.Categories;
using BasedTechStore.Domain.Entities.Specifications;

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
        public int Stock { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Guid SubCategoryId { get; set; }
        public SubCategory SubCategory { get; set; } = null!;

        public ICollection<ProductSpecification> Specifications { get; set; } = new List<ProductSpecification>();
    }
}
