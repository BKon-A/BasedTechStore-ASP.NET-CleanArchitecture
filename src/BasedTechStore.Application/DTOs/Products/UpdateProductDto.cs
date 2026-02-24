using BasedTechStore.Application.DTOs.Specifications;
using System.ComponentModel.DataAnnotations;

namespace BasedTechStore.Application.DTOs.Products
{
    public sealed record UpdateProductDto
    {
        [MaxLength(200)]
        public string? Name { get; set; }

        [MaxLength(2500)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        [MaxLength(100)]
        public string? Brand { get; set; }

        [Range(0, 100000000)]
        public decimal? Price { get; set; }

        [Range(0, 100000000)]
        public int? Stock { get; set; }

        public Guid? SubCategoryId { get; set; }
        public List<UpdateProductSpecificationDto> Specifications { get; set; } = new();
    }
}
