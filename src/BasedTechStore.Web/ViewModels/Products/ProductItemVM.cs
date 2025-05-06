using Microsoft.AspNetCore.Mvc.Rendering;

namespace BasedTechStore.Web.ViewModels.Products
{
    public class ProductItemVM
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public string? Brand { get; set; }

        public string CategoryName { get; set; } = string.Empty;
        public string SubCategoryName { get; set; } = string.Empty;

        public Guid SubCategoryId { get; set; }
        public List<SelectListItem> SubCategories { get; set; } = new();
    }
}
