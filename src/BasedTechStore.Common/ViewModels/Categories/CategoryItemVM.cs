namespace BasedTechStore.Common.ViewModels.Categories
{
    public class CategoryItemVM
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<SubCategoryItemVM> SubCategories { get; set; } = new();
    }
}
