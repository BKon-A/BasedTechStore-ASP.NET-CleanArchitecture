using BasedTechStore.Domain.Entities.Products;

namespace BasedTechStore.Application.DTOs.Cart
{
    public class CartDto
    {
        public Guid Id { get; set; }
        public string? UserId { get; set; }
        public Guid? SessionId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public decimal TotalPrice => CartItems?.Sum(i => i.Price * i.Quantity) ?? 0m;
        public int TotalItems => CartItems?.Sum(i => i.Quantity) ?? 0;

        public List<CartItemDto> CartItems { get; set; } = new List<CartItemDto>();
    }
}
