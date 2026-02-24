using System.ComponentModel.DataAnnotations;

namespace BasedTechStore.Application.DTOs.Orders
{
    public sealed record CreateOrderDto
    {
        [Required]
        [MinLength(1)]
        public List<CreateOrderItemDto> Items { get; set; } = new();

        [MaxLength(500)]
        public string? ShippingAddress { get; set; }

        [MaxLength(500)]
        public string? BillingAddress { get; set; }
    }
}
