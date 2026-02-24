using AutoMapper;
using BasedTechStore.Application.Common.Interfaces.Authorization;
using BasedTechStore.Application.Common.Interfaces.Services;
using BasedTechStore.Application.DTOs.Orders;
using BasedTechStore.Domain.Entities.Identity;
using BasedTechStore.Domain.Entities.Orders;
using BasedTechStore.Domain.Entities.Products;
using BasedTechStore.Domain.Exceptions;
using BasedTechStore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BasedTechStore.Infrastructure.Services.Orders
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly IProductService _productService;
        private readonly IPermissionService _permissionService;
        private readonly IAuthorizationPolicy<Order> _orderPolicy;
        private readonly IMapper _mapper;

        public OrderService(AppDbContext context, IProductService productService, IPermissionService permissionService, IAuthorizationPolicy<Order> policy, IMapper mapper)
        {
            _context = context;
            _productService = productService;
            _permissionService = permissionService;
            _orderPolicy = policy;
            _mapper = mapper;
        }

        public async Task<OrderDto> GetByIdAsync(Guid id, string requestingUserId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id)
                ?? throw new NotFoundException(nameof(Order), id);

            var canView = await _orderPolicy.IsAuthorizedAsync(requestingUserId, order, "view");
            if (!canView)
            {
                throw new ForbiddenException("You don't have permission to view this order");
            }

            return _mapper.Map<OrderDto>(order);
        }

        public async Task<IEnumerable<OrderDto>> GetAllAsync()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }

        public async Task<IEnumerable<OrderDto>> GetUserOrdersAsync(string userId)
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByStatusAsync(string status)
        {
            if (!Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
            {
                throw new ValidationException("Status", $"Invalid order status: {status}");
            }

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .Where(o => o.Status == orderStatus)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
                if (!userExists)
                {
                    throw new NotFoundException(nameof(AppUser), userId);
                }

                if (!dto.Items.Any())
                {
                    throw new ValidationException("Items", "Order must contain at least one item");
                }

                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    Status = OrderStatus.Pending,
                    ShippingAddress = dto.ShippingAddress,
                    BillingAddress = dto.BillingAddress,
                    OrderItems = new List<OrderItem>()
                };

                decimal totalAmount = 0;

                foreach (var item in dto.Items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId)
                        ?? throw new NotFoundException(nameof(Product), item.ProductId);

                    if (!product.IsActive)
                    {
                        throw new ValidationException("Product", $"Product '{product.Name}' is not available");
                    }

                    if (product.Stock < item.Quantity)
                    {
                        throw new ValidationException("Stock", $"Insufficient stock for '{product.Name}'. Available: {product.Stock}");
                    }

                    var orderItem = new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = product.Price
                    };

                    order.OrderItems.Add(orderItem);
                    totalAmount += orderItem.Price * orderItem.Quantity;

                    product.Stock -= item.Quantity;
                    product.UpdatedAt = DateTime.UtcNow;
                }

                order.TotalAmount = totalAmount;

                await _context.Orders.AddAsync(order);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var createdOrder = await _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .FirstAsync(o => o.Id == order.Id);

                return _mapper.Map<OrderDto>(createdOrder);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<OrderDto> UpdateOrderStatusAsync(Guid id, string status, string requestingUserId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.Id == id)
                    ?? throw new NotFoundException(nameof(Order), id);

                var canEdit = await _orderPolicy.IsAuthorizedAsync(requestingUserId, order, "edit");
                if (!canEdit)
                {
                    throw new ForbiddenException("You don't have permission to edit this order");
                }

                if (!Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
                {
                    throw new ValidationException("Status", $"Invalid order status: {status}");
                }

                if (order.Status == OrderStatus.Cancelled && orderStatus != OrderStatus.Cancelled)
                {
                    throw new ValidationException("Status", "Cannot change status of cancelled order");
                }

                if (order.Status == OrderStatus.Delivered && orderStatus != OrderStatus.Delivered)
                {
                    throw new ValidationException("Status", "Cannot change status of delivered order");
                }

                order.Status = orderStatus;
                order.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return _mapper.Map<OrderDto>(order);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task CancelOrderAsync(Guid id, string requestingUserId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.Id == id)
                    ?? throw new NotFoundException(nameof(Order), id);

                var canCancel = await _orderPolicy.IsAuthorizedAsync(requestingUserId, order, "cancel");
                if (!canCancel)
                {
                    throw new ForbiddenException("You don't have permission to cancel this order");
                }

                if (order.Status == OrderStatus.Cancelled)
                {
                    throw new ValidationException("Status", "Order is already cancelled");
                }

                if (order.Status == OrderStatus.Delivered)
                {
                    throw new ValidationException("Status", "Cannot cancel delivered order");
                }

                order.Status = OrderStatus.Cancelled;
                order.UpdatedAt = DateTime.UtcNow;

                foreach (var item in order.OrderItems)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        product.Stock += item.Quantity;
                        product.UpdatedAt = DateTime.UtcNow;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<OrderDto> ProcessOrderAsync(Guid id, string requestingUserId)
        {
            return await UpdateOrderStatusAsync(id, OrderStatus.Processing.ToString(), requestingUserId);
        }

        public async Task<OrderDto> CompleteOrderAsync(Guid id, string requestingUserId)
        {
            return await UpdateOrderStatusAsync(id, OrderStatus.Delivered.ToString(), requestingUserId);
        }

        public async Task<OrderStatisticsDto> GetOrderStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Orders.AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt <= endDate.Value);
            }

            var orders = await query.ToListAsync();

            return new OrderStatisticsDto
            {
                TotalOrders = orders.Count,
                TotalRevenue = orders.Sum(o => o.TotalAmount),
                PendingOrders = orders.Count(o => o.Status == OrderStatus.Pending),
                ProcessingOrders = orders.Count(o => o.Status == OrderStatus.Processing),
                CompletedOrders = orders.Count(o => o.Status == OrderStatus.Delivered),
                CancelledOrders = orders.Count(o => o.Status == OrderStatus.Cancelled),
                AverageOrderValue = orders.Any() ? orders.Average(o => o.TotalAmount) : 0
            };
        }
    }
}
