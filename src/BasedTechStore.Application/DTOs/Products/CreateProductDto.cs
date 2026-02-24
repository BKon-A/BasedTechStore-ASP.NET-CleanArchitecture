using BasedTechStore.Domain.Entities.Specifications;
using System.ComponentModel.DataAnnotations;

namespace BasedTechStore.Application.DTOs.Products
{
    public sealed record CreateProductDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(2500)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        [MaxLength(100)]
        public string? Brand { get; set; }

        [Required]
        [Range(0, 100000000)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, 1000000)]
        public int Stock { get; set; }

        [Required]
        public Guid SubCategoryId { get; set; }
        public List<ProductSpecification> Specifications { get; set; } = new();
    }
}
