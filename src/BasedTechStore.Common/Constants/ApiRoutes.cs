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
            public const string SignIn = $"signIn";
            public const string SignUp = $"signUp";
            public const string SignOut = $"signOut";
            public const string RefreshToken = $"refresh";
            public const string GetCurrentUser = $"user";
            public const string CheckAuth = $"check";
        }
        public static class Products
        {
            public const string Base = $"{ApiBase}/products";
            public const string GetAll = $"all";
            public const string GetById = $"{{productId}}";
            public const string GetByCategory = $"category/{{categoryId}}";
            public const string GetBySubcategory = $"subcategory/{{subcategoryId}}";
            public const string GetProductFilters = $"filters";
            public const string GetFilteredProducts = $"filtered";
            public const string GetDetails = $"details/{{productId}}";
        }
        public static class Categories
        {
            public const string Base = $"{ApiBase}/categories";
            public const string GetById = $"{{categoryId}}";
            public const string GetCategoriesWithSubCategories = $"all";
        }
        public static class Specifications
        {
            public const string Base = $"{ApiBase}/specifications";
        }
        public static class Carts
        {
            public const string Base = $"{ApiBase}/carts";
        }
    }
}