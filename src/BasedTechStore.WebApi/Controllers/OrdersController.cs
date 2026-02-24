using BasedTechStore.Application.Common.Interfaces.Services;
using BasedTechStore.Application.DTOs.Orders;
using BasedTechStore.Common.Constants;
using BasedTechStore.Common.Models.Api;
using BasedTechStore.Domain.Constants;
using BasedTechStore.WebApi.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasedTechStore.WebApi.Controllers
{
    [ApiController]
    [Route(ApiRoutes.Orders.Base)]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IAuthService _authService;

        public OrdersController(IOrderService orderService, IAuthService authService)
        {
            _orderService = orderService;
            _authService = authService;
        }

        [HttpGet(ApiRoutes.Orders.GetAll)]
        [RequirePermission(Permissions.OrdersViewAll)]
        public async Task<ActionResult<ApiResponse<IEnumerable<OrderDto>>>> GetAll()
        {
            var orders = await _orderService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<OrderDto>>.Success(orders));
        }

        [HttpGet(ApiRoutes.Orders.GetMyOrders)]
        public async Task<ActionResult<ApiResponse<IEnumerable<OrderDto>>>> GetMyOrders()
        {
            var userId = _authService.GetUserId(User);
            var orders = await _orderService.GetUserOrdersAsync(userId!);
            return Ok(ApiResponse<IEnumerable<OrderDto>>.Success(orders));
        }

        [HttpGet(ApiRoutes.Orders.GetByStatus)]
        [RequirePermission(Permissions.OrdersViewAll)]
        public async Task<ActionResult<ApiResponse<IEnumerable<OrderDto>>>> GetByStatus(string status)
        {
            var orders = await _orderService.GetOrdersByStatusAsync(status);
            return Ok(ApiResponse<IEnumerable<OrderDto>>.Success(orders));
        }

        [HttpGet(ApiRoutes.Products.GetById)]
        public async Task<ActionResult<ApiResponse<OrderDto>>> GetById(Guid id)
        {
            var userId = _authService.GetUserId(User);
            var order = await _orderService.GetByIdAsync(id, userId!);
            return Ok(ApiResponse<OrderDto>.Success(order));
        }

        [HttpPost]
        [RequirePermission(Permissions.OrdersCreate)]
        public async Task<ActionResult<ApiResponse<OrderDto>>> Create([FromBody] CreateOrderDto dto)
        {
            var userId = _authService.GetUserId(User);
            var order = await _orderService.CreateOrderAsync(dto, userId!);

            return CreatedAtAction(
                nameof(GetById),
                new { id = order.Id },
                ApiResponse<OrderDto>.Success(order, "Order created successfully")
            );
        }

        [HttpPatch(ApiRoutes.Orders.UpdateByStatus)]
        [RequirePermission(Permissions.OrdersEdit)]
        public async Task<ActionResult<ApiResponse<OrderDto>>> UpdateStatus(
            Guid id,
            [FromBody] UpdateOrderStatusDto dto)
        {
            var userId = _authService.GetUserId(User);
            var order = await _orderService.UpdateOrderStatusAsync(id, dto.Status, userId!);
            return Ok(ApiResponse<OrderDto>.Success(order, "Order status updated"));
        }

        [HttpPost(ApiRoutes.Orders.Process)]
        [RequirePermission(Permissions.OrdersProcess)]
        public async Task<ActionResult<ApiResponse<OrderDto>>> Process(Guid id)
        {
            var userId = _authService.GetUserId(User);
            var order = await _orderService.ProcessOrderAsync(id, userId!);
            return Ok(ApiResponse<OrderDto>.Success(order, "Order is being processed"));
        }

        [HttpPost(ApiRoutes.Orders.Complete)]
        [RequirePermission(Permissions.OrdersComplete)]
        public async Task<ActionResult<ApiResponse<OrderDto>>> Complete(Guid id)
        {
            var userId = _authService.GetUserId(User);
            var order = await _orderService.CompleteOrderAsync(id, userId!);
            return Ok(ApiResponse<OrderDto>.Success(order, "Order completed"));
        }

        [HttpPost(ApiRoutes.Orders.Cancel)]
        public async Task<ActionResult<ApiResponse<object>>> Cancel(Guid id)
        {
            var userId = _authService.GetUserId(User);
            await _orderService.CancelOrderAsync(id, userId!);
            return Ok(ApiResponse<object>.Success(null, "Order cancelled"));
        }

        [HttpGet(ApiRoutes.Orders.Statistics)]
        [RequirePermission(Permissions.ReportsViewSales)]
        public async Task<ActionResult<ApiResponse<OrderStatisticsDto>>> GetStatistics(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var stats = await _orderService.GetOrderStatisticsAsync(startDate, endDate);
            return Ok(ApiResponse<OrderStatisticsDto>.Success(stats));
        }
    }
}
