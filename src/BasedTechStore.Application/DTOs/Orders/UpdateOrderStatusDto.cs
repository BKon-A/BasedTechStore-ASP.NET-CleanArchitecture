using System.ComponentModel.DataAnnotations;

namespace BasedTechStore.Application.DTOs.Orders
{
    public sealed record UpdateOrderStatusDto
    {
        [Required]
        public string Status { get; set; } = string.Empty;
    }
}
