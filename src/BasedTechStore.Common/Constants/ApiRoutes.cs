namespace BasedTechStore.Common.Constants
{
    public static class ApiRoutes
    {
        public const string ApiBase = "api";

        public static class Home
        {
            public const string Base = $"{ApiBase}/home";
        }
        public static class Auth
        {
            public const string Base = $"{ApiBase}/auth";
            public const string SignIn = "signin";
            public const string SignUp = "signup";
            public const string SignOut = "signout";
            public const string SignOutAll = "signout-all";
            public const string RefreshToken = "refresh";
            public const string GetCurrentUser = "user";
            public const string CheckAuth = "check";
        }
        public static class Products
        {
            public const string Base = $"{ApiBase}/products";
            public const string GetById = "{id:guid}";
            public const string GetByCategory = "category/{categoryId:guid}";
            public const string GetBySubcategory = "subcategory/{subcategoryId:guid}";
            public const string GetProductFilters = "filters";
            public const string GetFeatured = "featured";
            public const string GetRelated = "{id:guid}/related";
            public const string UpdateStock = "{id:guid}/stock";
        }
        public static class Categories
        {
            public const string Base = $"{ApiBase}/categories";
            public const string GetById = "{id:guid}";
            public const string GetAll = "";
            public const string GetSubCategories = "{id:guid}/subcategories";
        }
        public static class Orders
        {
            public const string Base = $"{ApiBase}/orders";
            public const string GetById = "{id:guid}";
            public const string GetAll = "";
            public const string GetMyOrders = "my";
            public const string GetByStatus = "status/{status}";
            public const string UpdateByStatus = "{id:guid}/status";
            public const string Process = "{id:guid}/process";
            public const string Complete = "{id:guid}/complete";
            public const string Cancel = "{id:guid}/cancel";
            public const string Statistics = "statistics";
        }
        public static class Users
        {
            public const string Base = $"{ApiBase}/users";
            public const string GetById = "{id}";
            public const string GetAll = "";
            public const string Update = "{id}";
            public const string Delete = "{id}";
            public const string ChangeRole = "{id}/role";
            public const string GetRoles = "roles";
            public const string GrantPermissions = "{id}/permissions/grant";
            public const string RevokePermissions = "{id}/permissions/revoke";
            public const string GetPermissions = "{id}/permissions";
            public const string GetAvailablePermissions = "permissions";
            public const string ToggleStatus = "{id}/toggle-status";
        }
        public static class Profile
        {
            public const string Base = $"{ApiBase}/profile";
            public const string GetProfile = "";
            public const string UpdateProfile = "";
            public const string ChangePassword = "password";
            public const string GetMyOrders = "orders";
        }
    }
}