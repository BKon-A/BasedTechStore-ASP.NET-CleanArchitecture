namespace BasedTechStore.Application.DTOs.Orders
{
    public sealed record OrderDto
    {
        public Guid Id { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string? ShippingAddress { get; set; }
        public string? BillingAddress { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string UserId { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty;
        public List<OrderItemDto> OrderItems { get; set; } = new();
    }
}
