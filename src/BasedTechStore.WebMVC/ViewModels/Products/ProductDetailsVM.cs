using BasedTechStore.Application.DTOs.Specifications;
using BasedTechStore.Web.ViewModels.Specifications;

namespace BasedTechStore.Web.ViewModels.Products
{
    public class ProductDetailsVM
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public string? Brand { get; set; }

        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public Guid SubCategoryId { get; set; }
        public string SubCategoryName { get; set; } = string.Empty;

        public List<SpecificationCategoryGroupVM> SpecificationGroups { get; set; } = new List<SpecificationCategoryGroupVM>();
    }
}
