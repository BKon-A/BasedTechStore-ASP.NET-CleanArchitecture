namespace BasedTechStore.Common.Constants
{
    public static class AppConstants
    {
        // Application settings
        public const string ApplicationName = "BasedTechStore";
        public const string ApiVersion = "v1";
        
        // JWT settings
        public static class Jwt
        {
            public const string DefaultIssuer = "BasedTechStore";
            public const string DefaultAudience = "BasedTechStore-Users";
            public const int DefaultExpirationHours = 24;
            public const int RefreshTokenExpirationDays = 7;
            public const string CookieName = "access-token";
            public const string RefreshCookieName = "refresh-token";
        }

        // CORS policies
        public static class CorsPolicy
        {
            public const string AllowMultipleFrontends = "AllowMultipleFrontends";
            public const string Production = "Production";
        }

        // Cache keys
        public static class CacheKeys
        {
            public const string Products = "products";
            public const string Categories = "categories";
            public const string Specifications = "specifications";
            public const string UserCart = "user_cart_{0}";
            public const string UserWishlist = "user_wishlist_{0}";
        }

        // Response messages
        public static class Messages
        {
            public const string Success = "Operation completed successfully";
            public const string NotFound = "Resource not found";
            public const string Unauthorized = "Access denied";
            public const string BadRequest = "Invalid request data";
            public const string InternalError = "Internal server error occurred";
            
            // Auth messages
            public const string LoginSuccess = "Login successful";
            public const string LoginFailed = "Invalid email or password";
            public const string RegisterSuccess = "Registration successful";
            public const string LogoutSuccess = "Logout successful";
            public const string TokenExpired = "Token has expired";
            
            // Cart messages
            public const string ItemAddedToCart = "Item added to cart";
            public const string ItemRemovedFromCart = "Item removed from cart";
            public const string CartCleared = "Cart cleared";
            
            // Wishlist messages
            public const string ItemAddedToWishlist = "Item added to wishlist";
            public const string ItemRemovedFromWishlist = "Item removed from wishlist";
        }

        // File upload settings
        public static class FileUpload
        {
            public const int MaxFileSizeMB = 5;
            public const string AllowedImageExtensions = ".jpg,.jpeg,.png,.webp";
            public const string UploadPath = "uploads";
            public const string ProductImagesPath = "uploads/products";
        }

        // Pagination settings
        public static class Pagination
        {
            public const int DefaultPageSize = 20;
            public const int MaxPageSize = 100;
            public const int MinPageSize = 1;
        }

        // Shopping settings
        public static class Shopping
        {
            public const int MaxCartItems = 99;
            public const int MaxCompareItems = 4;
            public const int MaxWishlistItems = 1000;
        }
    }
}