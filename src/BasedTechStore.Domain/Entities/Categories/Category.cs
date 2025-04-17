using BasedTechStore.Domain.Entities.Products;

namespace BasedTechStore.Domain.Entities.Categories
{
    public class Category
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public ICollection<SubCategory> SubCategories { get; set; } = new List<SubCategory>();
    }
}
