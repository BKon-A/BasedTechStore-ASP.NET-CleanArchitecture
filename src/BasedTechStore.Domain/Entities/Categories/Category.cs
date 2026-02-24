namespace BasedTechStore.Domain.Entities.Categories
{
    public class Category
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<SubCategory> SubCategories { get; set; } = new List<SubCategory>();
    }
}
