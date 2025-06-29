using BasedTechStore.Application.DTOs.Cart;
using Microsoft.AspNetCore.Http;

namespace BasedTechStore.Application.Common.Interfaces.Services
{
    public interface ICartService
    {
        Guid GetOrCreateCartSessionId(HttpContext httpContext);
        Task<CartDto> GetOrCreateCartAsync(string? userId = null, Guid? sessionId = null);
        Task<CartDto> AddToCartAsync(Guid productId, int quantity, string? userId = null, Guid? sessionId = null);
        Task<CartDto> UpdateCartItemAsync(Guid cartItemId, int quantity);
        Task<CartDto> RemoveFromCartAsync(Guid cartItemId);
        Task<CartDto> SaveAllCartChangesAsync(List<CartItemDto> createdItems, List<CartItemDto> updatedItems, List<CartItemDto> deletedItems, Guid cartId);
        Task<CartDto> ClearCartAsync(Guid cartId);
        Task<CartDto> MergeCartsAsync(Guid sourceCartId, Guid targetCartId);
        Task<CartDto> TransferCartAsync(Guid sessionId, string userId);
    }
}
