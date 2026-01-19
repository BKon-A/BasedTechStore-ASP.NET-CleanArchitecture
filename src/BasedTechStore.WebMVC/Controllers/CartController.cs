using AutoMapper;
using BasedTechStore.Application.Common.Interfaces.Services;
using BasedTechStore.Web.Extentions;
using BasedTechStore.Common.ViewModels.Cart;
using BasedTechStore.Common.ViewModels.PendingChanges;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace BasedTechStore.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IAuthService _authService;
        private readonly IMapper _mapper;
        private readonly ILogger<CartController> _logger;

        public CartController(ICartService cartService, IMapper mapper,
            ILogger<CartController> logger, IAuthService authService)
        {
            _cartService = cartService;
            _authService = authService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var (userId, sessionId) = await GetCartIdentifiers();

                var cart = await _cartService.GetOrCreateCartAsync(userId, sessionId);
                var cartVm = _mapper.Map<CartVM>(cart);

                return View(cartVm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при отриманні кошика");
                ModelState.AddModelError("IndexCartError", "Не вдалося отримати кошик. Спробуйте пізніше.");
                return View(new CartVM { CartItems = new List<CartItemVM>() });
            }

        }

        [HttpGet]
        public async Task<IActionResult> GetCartItems()
        {
            try
            {
                var (userId, sessionId) = await GetCartIdentifiers();

                var cart = await _cartService.GetOrCreateCartAsync(userId, sessionId);

                return Json(new
                {
                    success = true,
                    items = cart.CartItems.Select(item => new {
                        id = item.Id,
                        productId = item.ProductId,
                        quantity = item.Quantity,
                        price = item.Price,
                        totalPrice = item.TotalPrice
                    }),
                    cartItemCount = cart.TotalItems,
                    cartTotalPrice = cart.TotalPrice
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cart items");
                return Json(new
                {
                    success = false,
                    message = "Помилка при отриманні вмісту кошика"
                });
            }
        }

        private async Task<(string? userId, Guid sessionId)> GetCartIdentifiers()
        {
            var sessionId = _cartService.GetOrCreateCartSessionId(HttpContext);
            var userId = _authService.GetUserId(User);

            if (!string.IsNullOrEmpty(userId))
            {
                await _cartService.TransferCartAsync(sessionId, userId);
                _logger.LogInformation($"Кошик з сесії {sessionId} перенесено до користувача {userId}");
            }

            return (userId, sessionId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(Guid productId, int quantity = 1)
        {
            try
            {
                if (quantity <= 0)
                    quantity = 1;

                var (userId, sessionId) = await GetCartIdentifiers();

                var cartDto = await _cartService.AddToCartAsync(productId, quantity, userId, sessionId);

                if (Request.IsAjaxRequest())
                {
                    return Json(new { 
                        success = true, 
                        cartItemCount = cartDto.TotalItems, 
                        cartTotalPrice = cartDto.TotalPrice,
                        message = "Товар додано до кошика. Зберігання змін відбудеться автоматично."
                    });
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Помилка при додаванні товару {productId} до кошика");

                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = ex.Message });
                }

                ModelState.AddModelError("AddToCartError", "Помилка при додаванні товару до кошика: " + ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCartItem(Guid cartItemId, int quantity)
        {
            try
            {
                var cartDto = await _cartService.UpdateCartItemAsync(cartItemId, quantity);

                if (Request.IsAjaxRequest())
                {
                    return Json(new
                    {
                        success = true,
                        itemTotalPrice = cartDto.CartItems.Find(i => i.Id == cartItemId)?.TotalPrice,
                        cartTotalPrice = cartDto.TotalPrice,
                        cartItemCount = cartDto.TotalItems,
                        message = "Кількість товару оновлено. Зберігання змін відбудеться автоматично."
                    });
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Помилка при оновленні товару {cartItemId} в кошику");

                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = ex.Message });
                }

                ModelState.AddModelError("", "Помилка при оновленні товару в кошику: " + ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(Guid cartItemId)
        {
            try
            {
                var cartDto = await _cartService.RemoveFromCartAsync(cartItemId);

                if (Request.IsAjaxRequest())
                {
                    return Json(new
                    {
                        success = true,
                        cartTotalPrice = cartDto.TotalPrice,
                        cartItemCount =cartDto.TotalItems,
                        message = "Товар видалено з кошика. Зберігання змін відбудеться автоматично."
                    });
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Помилка при видаленні товару {cartItemId} з кошика");

                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = ex.Message });
                }

                ModelState.AddModelError("", "Помилка при видаленні товару з кошика: " + ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SaveCartChangesBeacon()
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync();
                var pendingChanges = JsonSerializer.Deserialize<CartPendingChangesVM>(body, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (pendingChanges == null || pendingChanges.CartId == Guid.Empty)
                {
                    _logger.LogWarning("SaveCartChangesBeacon: Не надано даних для збереження або не вказано ID кошика");
                    return StatusCode(400, "Не надано даних для збереження або не вказано ID кошика");
                }

                _logger.LogInformation($"Початок збереження змін кошика ID: {pendingChanges.CartId}," +
                    $" створених: {pendingChanges.CreatedItems?.Count ?? 0}," +
                    $" оновлених: {pendingChanges.UpdatedItems?.Count ?? 0}," +
                    $" видалених: {pendingChanges.DeletedItems?.Count ?? 0}");

                var updatedCart = await _cartService.SaveAllCartChangesAsync(
                    pendingChanges.CreatedItems,
                    pendingChanges.UpdatedItems,
                    pendingChanges.DeletedItems,
                    pendingChanges.CartId);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Помилка при збереженні змін кошика ID");
                return StatusCode(500);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCartChanges(CartPendingChangesVM pendingChanges)
        {
            try
            {
                if (pendingChanges == null || pendingChanges.CartId == Guid.Empty)
                {
                    return BadRequest("Не надано даних для збереження або не вказано ID кошика");
                }

                _logger.LogInformation($"Початок збереження змін кошика ID: {pendingChanges.CartId}," +
                    $" створених: {pendingChanges.CreatedItems?.Count ?? 0}," +
                    $" оновлених: {pendingChanges.UpdatedItems?.Count ?? 0}," +
                    $" видалених: {pendingChanges.DeletedItems?.Count ?? 0}");

                // Єдине місце, де викликається SaveAllCartChangesAsync
                var updatedCart = await _cartService.SaveAllCartChangesAsync(
                    pendingChanges.CreatedItems,
                    pendingChanges.UpdatedItems,
                    pendingChanges.DeletedItems,
                    pendingChanges.CartId);

                var cartVm = _mapper.Map<CartVM>(updatedCart);

                if (Request.IsAjaxRequest())
                {
                    return Json(new
                    {
                        success = true,
                        message = "Зміни в кошику збережено успішно",
                        cartTotalPrice = updatedCart.TotalPrice,
                        cartItemCount = updatedCart.TotalItems,
                        cart = cartVm
                    });
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Помилка при збереженні змін кошика ID: {pendingChanges?.CartId}");

                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = ex.Message });
                }

                ModelState.AddModelError("", $"Помилка при збереженні кошика: {ex.Message}");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearCart(Guid cartId)
        {
            try
            {
                var cartDto = await _cartService.ClearCartAsync(cartId);

                if (Request.IsAjaxRequest())
                {
                    return Json(new { 
                        success = true,
                        message = "Кошик очищено. Зберігання змін відбудеться автоматично."
                    });
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Помилка при очищенні кошика {cartId}");

                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = ex.Message });
                }

                ModelState.AddModelError("", "Помилка при очищенні кошика: " + ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
