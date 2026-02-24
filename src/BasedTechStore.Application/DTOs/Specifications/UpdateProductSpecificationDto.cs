using System.ComponentModel.DataAnnotations;

namespace BasedTechStore.Application.DTOs.Specifications
{
    public sealed record UpdateProductSpecificationDto
    {
        public Guid? Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Key { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Value { get; set; } = string.Empty;
    }
}
