using BasedTechStore.Application.DTOs.Orders;

namespace BasedTechStore.Application.Common.Interfaces.Services
{
    public interface IOrderService
    {
        Task<OrderDto> GetByIdAsync(Guid orderId, string requestingUserId);
        Task<IEnumerable<OrderDto>> GetAllAsync();
        Task<IEnumerable<OrderDto>> GetUserOrdersAsync(string userId);
        Task<IEnumerable<OrderDto>> GetOrdersByStatusAsync(string status);
        Task<OrderDto> CreateOrderAsync(CreateOrderDto dto, string userId);
        Task<OrderDto> UpdateOrderStatusAsync(Guid orderId, string status, string requestingUserId);
        Task CancelOrderAsync(Guid orderId, string requestingUserId);
        Task<OrderDto> ProcessOrderAsync(Guid orderId, string requestingUserId);
        Task<OrderDto> CompleteOrderAsync(Guid orderId, string requestingUserId);
        Task<OrderStatisticsDto> GetOrderStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    }
}
