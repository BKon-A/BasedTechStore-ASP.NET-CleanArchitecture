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
        }
        public static class Products
        {
            public const string Base = $"{ApiBase}/products";
        }
        public static class Categories
        {
            public const string Base = $"{ApiBase}/categories";
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