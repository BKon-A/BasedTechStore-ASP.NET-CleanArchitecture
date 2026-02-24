namespace BasedTechStore.Domain.Constants
{
    /// <summary>
    /// Map roles to their default permissions
    /// </summary>
    public static class RolePermissions
    {
        private static readonly IReadOnlyDictionary<string, IReadOnlySet<string>> _rolePermissions = new Dictionary<string, IReadOnlySet<string>>
        {
            [Roles.Customer] = new HashSet<string>
            {
                Permissions.ProductsView,
                Permissions.CategoriesView,
                Permissions.OrdersCreate,
                Permissions.OrdersCancel,
                Permissions.CartView,
                Permissions.CartManage
            },
            [Roles.Manager] = new HashSet<string>
            {
                // Full product management
                Permissions.ProductsView,
                Permissions.ProductsCreate,
                Permissions.ProductsEdit,
                Permissions.ProductsDelete,
                Permissions.ProductsManageSpecifications,

                // Full category management
                Permissions.CategoriesView,
                Permissions.CategoriesCreate,
                Permissions.CategoriesEdit,
                Permissions.CategoriesDelete,

                // Order management
                Permissions.OrdersView,
                Permissions.OrdersViewAll,
                Permissions.OrdersEdit,
                Permissions.OrdersProcess,
                Permissions.OrdersComplete,

                // Reports
                Permissions.ReportsViewSales,
                Permissions.ReportsViewProducts,
                Permissions.ReportsViewUsers,
                Permissions.ReportsExport
            },
            [Roles.Admin] = new HashSet<string>
            {
                // All product permissions
                Permissions.ProductsView,
                Permissions.ProductsCreate,
                Permissions.ProductsEdit,
                Permissions.ProductsDelete,
                Permissions.ProductsManageSpecifications,

                // All category permissions
                Permissions.CategoriesView,
                Permissions.CategoriesCreate,
                Permissions.CategoriesEdit,
                Permissions.CategoriesDelete,

                // All order permissions
                Permissions.OrdersView,
                Permissions.OrdersViewAll,
                Permissions.OrdersCreate,
                Permissions.OrdersEdit,
                Permissions.OrdersCancel,
                Permissions.OrdersProcess,
                Permissions.OrdersComplete,

                // All user permissions
                Permissions.UsersManage,
                Permissions.UsersManageRoles,

                // All cart permissions
                Permissions.CartView,
                Permissions.CartManage,

                // All report permissions
                Permissions.ReportsViewSales,
                Permissions.ReportsViewProducts,
                Permissions.ReportsViewUsers,
                Permissions.ReportsExport,

                // All system permissions
                Permissions.SystemViewLogs,
                Permissions.SystemManageSettings,
                Permissions.SystemManageBackups
            },
            [Roles.Analyst] = new HashSet<string>
            {
                Permissions.ProductsView,
                Permissions.CategoriesView,
                Permissions.OrdersView,
                Permissions.OrdersViewAll,

                Permissions.ReportsViewSales,
                Permissions.ReportsViewProducts,
                Permissions.ReportsViewUsers,
                Permissions.ReportsExport,
            },
            [Roles.Support] = new HashSet<string>
            {
                Permissions.ProductsView,
                Permissions.CategoriesView,
                Permissions.OrdersView,
                Permissions.OrdersViewAll,
                Permissions.OrdersProcess,
                Permissions.OrdersCancel,
                Permissions.OrdersComplete,
                Permissions.UsersManage,
                Permissions.ReportsViewProducts
            }
        };

        /// <summary>
        /// Get default role permissions
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public static IReadOnlySet<string> GetPermissionsForRole(string role)
        {
            return _rolePermissions.TryGetValue(role, out var perms)
                ? perms : new HashSet<string>();
        }

        /// <summary>
        /// Check if role has specific permission
        /// </summary>
        /// <param name="role"></param>
        /// <param name="perm"></param>
        /// <returns></returns>
        public static bool RoleHasPermission(string role, string perm)
        {
            return GetPermissionsForRole(role).Contains(perm);
        }

        /// <summary>
        /// Check if user has perms (role perms + custom)
        /// </summary>
        /// <param name="role"></param>
        /// <param name="customPerms"></param>
        /// <param name="perm"></param>
        /// <returns></returns>
        public static bool UserHasPermissions(string role, IEnumerable<string>? customPerms, string perm)
        {
            return GetAllUserPermissions(role, customPerms).Contains(perm);
        }

        /// <summary>
        /// Check if user has any of the required perms
        /// </summary>
        /// <param name="role"></param>
        /// <param name="customPerms"></param>
        /// <param name="requiredPerms"></param>
        /// <returns></returns>
        public static bool UserHasAnyPermissions(string role, IEnumerable<string>? customPerms, params string[] requiredPerms)
        {
            var userPerms = GetAllUserPermissions(role, customPerms);
            return requiredPerms.Any(p => userPerms.Contains(p));
        }

        /// <summary>
        /// Check if user has all of the required perms
        /// </summary>
        /// <param name="role"></param>
        /// <param name="customPerms"></param>
        /// <param name="requiredPerms"></param>
        /// <returns></returns>
        public static bool UserHasAllPermissioms(string role, IEnumerable<string>? customPerms, params string[] requiredPerms)
        {
            var userPerms = GetAllUserPermissions(role, customPerms);
            return requiredPerms.All(p => userPerms.Contains(p));
        }

        /// <summary>
        /// Get all perms for user
        /// </summary>
        /// <param name="role"></param>
        /// <param name="customPermissions"></param>
        /// <returns></returns>
        public static IReadOnlySet<string> GetAllUserPermissions(string role, IEnumerable<string>? customPermissions)
        {
            var rolePerms = GetPermissionsForRole(role);
            if (customPermissions == null || !customPermissions.Any())
                return rolePerms;

            return rolePerms.Union(customPermissions).ToHashSet();
        }
    }
}
