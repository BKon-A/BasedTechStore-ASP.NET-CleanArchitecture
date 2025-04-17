using BasedTechStore.Domain.Entities.Products;

namespace BasedTechStore.Domain.Entities.Categories
{
    public class SubCategory
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public Guid CategoryId { get; set; }
        public Category Category { get; set; }

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
