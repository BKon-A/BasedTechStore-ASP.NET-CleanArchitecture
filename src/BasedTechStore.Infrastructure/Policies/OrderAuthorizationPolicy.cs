using BasedTechStore.Application.Common.Interfaces.Authorization;
using BasedTechStore.Application.Common.Interfaces.Services;
using BasedTechStore.Domain.Constants;
using BasedTechStore.Domain.Entities.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasedTechStore.Infrastructure.Policies
{
    public class OrderAuthorizationPolicy : IAuthorizationPolicy<Order>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPermissionService _permissionService;

        public OrderAuthorizationPolicy(
            IUserRepository userRepository,
            IPermissionService permissionService)
        {
            _userRepository = userRepository;
            _permissionService = permissionService;
        }

        public async Task<bool> IsAuthorizedAsync(string userId, Order order, string action)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return false;

            var permissions = _permissionService.GetAllPermissions(user.Role, user.CustomPermissions);

            return action switch
            {
                "view" => CanViewOrder(userId, order, permissions),
                "cancel" => CanCancelOrder(userId, order, permissions),
                "edit" => CanEditOrder(userId, order, permissions),
                _ => false
            };
        }

        private bool CanViewOrder(string userId, Order order, IReadOnlySet<string> permissions)
        {
            if (permissions.Contains(Permissions.OrdersViewAll))
                return true;

            if (order.UserId == userId && permissions.Contains(Permissions.OrdersView))
                return true;

            return false;
        }

        private bool CanCancelOrder(string userId, Order order, IReadOnlySet<string> permissions)
        {
            if (permissions.Contains(Permissions.OrdersCancel) && order.UserId == userId)
                return true;

            if (permissions.Contains(Permissions.OrdersEdit))
                return true;

            return false;
        }

        private bool CanEditOrder(string userId, Order order, IReadOnlySet<string> permissions)
        {
            return permissions.Contains(Permissions.OrdersEdit);
        }
    }
}
