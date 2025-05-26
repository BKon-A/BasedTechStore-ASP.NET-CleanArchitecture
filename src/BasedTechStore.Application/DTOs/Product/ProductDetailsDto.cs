using BasedTechStore.Application.DTOs.Specifications;

namespace BasedTechStore.Application.DTOs.Product
{
    public class ProductDetailsDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public string? Brand { get; set; }

        // Category information
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public Guid SubCategoryId { get; set; }
        public string SubCategoryName { get; set; } = string.Empty;

        // Specifications
        public List<SpecificationCategoryGroupDto> SpecificationGroups { get; set; } = new List<SpecificationCategoryGroupDto>();
    }
}
