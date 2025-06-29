using AutoMapper;
using BasedTechStore.Application.Common.Interfaces.Services;
using BasedTechStore.Application.DTOs.Cart;
using BasedTechStore.Domain.Entities.Cart;
using BasedTechStore.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace BasedTechStore.Infrastructure.Services.Carts
{
    public class CartService : ICartService
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<CartService> _logger;
        private const string SessionKey = "SessionId";

        public CartService(AppDbContext context, IMapper mapper,
            ILogger<CartService> logger)
        {
            _dbContext = context;
            _mapper = mapper;
            _logger = logger;
        }

        public Guid GetOrCreateCartSessionId(HttpContext httpContext)
        {
            try
            {
                if (httpContext.Session == null)
                {
                    _logger.LogError("Session is null. Cannot create or retrieve session ID.");
                    return Guid.NewGuid();
                }
                if (!httpContext.Session.TryGetValue(SessionKey, out var sessionId))
                {
                    var newSessionId = Guid.NewGuid();
                    httpContext.Session.SetString(SessionKey, newSessionId.ToString());
                    _logger.LogInformation($"New session created with ID: {newSessionId}");
                    return newSessionId;
                }

                var sessionIdString = Encoding.UTF8.GetString(sessionId);
                var result = Guid.TryParse(sessionIdString, out var parsedSessionId) ? parsedSessionId : Guid.Empty;
                _logger.LogDebug($"Retrieved session ID: {result} from session storage.");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving or creating session ID.");
                return Guid.NewGuid(); // Fallback to a new GUID in case of error
            }
        }

        public async Task<CartDto> GetOrCreateCartAsync(string? userId = null, Guid? sessionId = null)
        {
            var cart = await GetCartAsync(userId, sessionId);
            return _mapper.Map<CartDto>(cart);
        }

        public async Task<CartDto> AddToCartAsync(Guid productId, int quantity, string? userId = null, Guid? sessionId = null)
        {
            var cart = await GetCartAsync(userId, sessionId);
            var product = await _dbContext.Products.FindAsync(productId);

            if (product == null)
                throw new ArgumentException("Продукт не знайдено", nameof(productId));

            var cartItem = cart.CartItems.FirstOrDefault(i => i.ProductId == productId);

            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
            }
            else
            {
                cartItem = new CartItem
                {
                    Id = Guid.NewGuid(),
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = quantity,
                    Price = product.Price,
                    AddedAt = DateTime.UtcNow
                };

                cart.CartItems.Add(cartItem);
            }

            cart.UpdatedAt = DateTime.UtcNow;

            return _mapper.Map<CartDto>(cart);
        }

        public async Task<CartDto> UpdateCartItemAsync(Guid cartItemId, int quantity)
        {
            var cartItem = await _dbContext.CartItems
                .Include(ci => ci.Cart)
                .ThenInclude(c => c.CartItems)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId);

            if (cartItem == null)
                throw new ArgumentException("Товар не знайдено в кошику", nameof(cartItemId));

            if (quantity <= 0)
            {
                _dbContext.CartItems.Remove(cartItem);
            }
            else
            {
                cartItem.Quantity = quantity;
            }

            cartItem.Cart.UpdatedAt = DateTime.UtcNow;

            return _mapper.Map<CartDto>(cartItem.Cart);
        }

        public async Task<CartDto> RemoveFromCartAsync(Guid cartItemId)
        {
            var cartItem = await _dbContext.CartItems
                .Include(ci => ci.Cart)
                .ThenInclude(c => c.CartItems)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId);

            if (cartItem == null)
                throw new ArgumentException("Товар не знайдено в кошику", nameof(cartItemId));

            _dbContext.CartItems.Remove(cartItem);
            cartItem.Cart.UpdatedAt = DateTime.UtcNow;

            return _mapper.Map<CartDto>(cartItem.Cart);
        }

        public async Task<CartDto> ClearCartAsync(Guid cartId)
        {
            var cart = await _dbContext.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.Id == cartId);

            if (cart == null)
                throw new ArgumentException("Кошик не знайдено", nameof(cartId));

            _dbContext.CartItems.RemoveRange(cart.CartItems);
            cart.UpdatedAt = DateTime.UtcNow;

            return _mapper.Map<CartDto>(cart);
        }

        public async Task<CartDto> MergeCartsAsync(Guid sourceCartId, Guid targetCartId)
        {
            var sourceCart = await _dbContext.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.Id == sourceCartId);

            var targetCart = await _dbContext.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.Id == targetCartId);

            if (sourceCart == null || targetCart == null)
                throw new ArgumentException("Кошик не знайдено");

            foreach (var sourceItem in sourceCart.CartItems)
            {
                var targetItem = targetCart.CartItems.FirstOrDefault(i => i.ProductId == sourceItem.ProductId);

                if (targetItem != null)
                {
                    targetItem.Quantity += sourceItem.Quantity;
                }
                else
                {
                    var newItem = new CartItem
                    {
                        Id = Guid.NewGuid(),
                        CartId = targetCart.Id,
                        ProductId = sourceItem.ProductId,
                        Quantity = sourceItem.Quantity,
                        Price = sourceItem.Price,
                        AddedAt = DateTime.UtcNow
                    };

                    targetCart.CartItems.Add(newItem);
                }
            }

            _dbContext.Carts.Remove(sourceCart);
            targetCart.UpdatedAt = DateTime.UtcNow;

            return _mapper.Map<CartDto>(targetCart);
        }

        public async Task<CartDto> TransferCartAsync(Guid sessionId, string userId)
        {
            var sessionCart = await _dbContext.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.SessionId == sessionId);

            if (sessionCart == null)
            {
                _logger.LogInformation($"No cart found for session ID: {sessionId}");
                return null;
            }

            var userCart = await _dbContext.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            // If cart not exists, create a new one
            if (userCart == null)
            {
                sessionCart.UserId = userId;
                sessionCart.UpdatedAt = DateTime.UtcNow;

                _logger.LogInformation($"Creating new cart {sessionCart.Id} for user ID: {userId} with session ID: {sessionId}");

                return _mapper.Map<CartDto>(sessionCart);
            }

            _logger.LogInformation($"Merging session cart {sessionCart.Id} into user cart {userCart.Id} for user ID: {userId}");
            return await MergeCartsAsync(sessionCart.Id, userCart.Id);
        }

        public async Task<CartDto> SaveAllCartChangesAsync(List<CartItemDto> createdItems, List<CartItemDto> updatedItems, List<CartItemDto> deletedItems, Guid cartId)
        {
            _logger.LogInformation($"SaveAllCartChangesAsync: " +
                $"Created items: {createdItems?.Count ?? 0}, " +
                $"Updated items: {updatedItems?.Count ?? 0}, " +
                $"Deleted items: {deletedItems?.Count ?? 0}");

            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var cart = await _dbContext.Carts
                        .Include(c => c.CartItems)
                        .ThenInclude(i => i.Product)
                        .FirstOrDefaultAsync(c => c.Id == cartId);

                    if (cart == null)
                    {
                        throw new ArgumentException($"Кошик з ID {cartId} не знайдено");
                    }

                    // 1. Обробка видалених елементів
                    if (deletedItems?.Any() == true)
                    {
                        var deletedItemIds = deletedItems.Select(i => i.Id).ToList();
                        var itemsToDelete = cart.CartItems.Where(i => deletedItemIds.Contains(i.Id)).ToList();

                        foreach (var item in itemsToDelete)
                        {
                            _dbContext.CartItems.Remove(item);
                        }
                        _logger.LogInformation($"Marked {itemsToDelete.Count} items for deletion");
                    }

                    // 2. Обробка оновлених елементів
                    if (updatedItems?.Any() == true)
                    {
                        foreach (var updatedItem in updatedItems)
                        {
                            var existingItem = cart.CartItems.FirstOrDefault(i => i.Id == updatedItem.Id);
                            if (existingItem != null)
                            {
                                existingItem.Quantity = updatedItem.Quantity;
                                _dbContext.Entry(existingItem).State = EntityState.Modified;
                            }
                        }
                        _logger.LogInformation($"Updated {updatedItems.Count} items");
                    }

                    // 3. Обробка доданих елементів
                    if (createdItems?.Any() == true)
                    {
                        foreach (var newItemDto in createdItems)
                        {
                            var product = await _dbContext.Products.FindAsync(newItemDto.ProductId);
                            if (product == null)
                            {
                                _logger.LogWarning($"Product with ID {newItemDto.ProductId} not found during cart update");
                                continue;
                            }

                            var newItem = new CartItem
                            {
                                Id = Guid.NewGuid(),
                                CartId = cartId,
                                ProductId = newItemDto.ProductId,
                                Quantity = newItemDto.Quantity,
                                Price = product.Price,
                                AddedAt = DateTime.UtcNow
                            };

                            await _dbContext.CartItems.AddAsync(newItem);
                        }
                        _logger.LogInformation($"Added {createdItems.Count} new items");
                    }

                    // Оновлення даних кошика
                    cart.UpdatedAt = DateTime.UtcNow;

                    // Збереження всіх змін
                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation($"Cart ID: {cartId} successfully saved with all changes");

                    // Оновлений кошик
                    var refreshedCart = await _dbContext.Carts
                        .Include(c => c.CartItems)
                        .ThenInclude(i => i.Product)
                        .FirstOrDefaultAsync(c => c.Id == cartId);

                    return _mapper.Map<CartDto>(refreshedCart);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, $"Error saving cart changes for Cart ID: {cartId}");
                    throw;
                }
            }
        }

        private async Task<Cart> GetCartAsync(string? userId, Guid? sessionId)
        {
            Cart? cart = null;

            _logger.LogDebug($"GetCartAsync called with UserId: {userId}, SessionId: {sessionId}");

            // For authrized users, try to get the cart by UserId first
            if (!string.IsNullOrWhiteSpace(userId))
            {
                cart = await _dbContext.Carts
                    .Include(c => c.CartItems)
                    .ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart != null)
                {
                    _logger.LogDebug($"Cart found for UserId: {userId}, CartId: {cart.Id}");
                }
            }

            // For unauthenticated users(mostly), try to get the cart by SessionId
            if (cart == null && sessionId.HasValue && sessionId != Guid.Empty)
            {
                cart = await _dbContext.Carts
                    .Include(c => c.CartItems)
                    .ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(c => c.SessionId == sessionId);

                if (cart != null)
                {
                    _logger.LogDebug($"Cart found for SessionId: {sessionId}, CartId: {cart.Id}");
                }
            }

            // If no cart found, create a new one
            if (cart == null)
            {
                cart = new Cart
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    SessionId = sessionId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _dbContext.Carts.AddAsync(cart);

                _logger.LogDebug($"New cart created with ID: {cart.Id}, UserId: {userId}, SessionId: {sessionId}");
            }

            return cart;
        }
    }
}
