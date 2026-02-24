using System.Reflection;

namespace BasedTechStore.Domain.Constants
{
    /// <summary>
    /// Canonical permission keys
    /// </summary>
    public static class Permissions
    {
        public const string ProductsView = "products:view";
        public const string ProductsCreate = "products:create";
        public const string ProductsEdit = "products:edit";
        public const string ProductsDelete = "products:delete";
        public const string ProductsManageSpecifications = "products:manage_specs";

        public const string CategoriesManage = "categories:manage";
        public const string CategoriesView = "categories:view";
        public const string CategoriesCreate = "categories:create";
        public const string CategoriesEdit = "categories:edit";
        public const string CategoriesDelete = "categories:delete";

        public const string UsersManage = "users:manage";
        public const string UsersManageRoles = "users:manage_roles";
        public const string UsersView = "users:view";
        public const string UsersCreate = "users:create";
        public const string UsersEdit = "users:edit";
        public const string UsersDelete = "users:delete";

        public const string OrdersView = "orders:view";
        public const string OrdersViewAll = "orders:view_all";
        public const string OrdersCreate = "orders:create";
        public const string OrdersEdit = "orders:edit";
        public const string OrdersCancel = "orders:cancel";
        public const string OrdersProcess = "orders:process";
        public const string OrdersComplete = "orders:complete";

        // Cart permissions
        public const string CartView = "cart:view";
        public const string CartManage = "cart:manage";

        // Reports permissions
        public const string ReportsViewSales = "reports:view_sales";
        public const string ReportsViewProducts = "reports:view_products";
        public const string ReportsViewUsers = "reports:view_users";
        public const string ReportsExport = "reports:export";

        // System permissions
        public const string SystemViewLogs = "system:view_logs";
        public const string SystemManageSettings = "system:manage_settings";
        public const string SystemManageBackups = "system:manage_backups";

        public static IReadOnlyList<string> GetAll()
        {
            return typeof(Permissions)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.IsLiteral && f.FieldType == typeof(string))
                .Select(f => (string)f.GetValue(null)!)
                .ToList();
        }
    }
}
