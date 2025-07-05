export const ROUTES = {
    // Home route
    HOME: '/',
    
    // Auth routes
    LOGIN: '/login',
    REGISTER: '/signup',
    LOGOUT: '/logout',
    
    // Products routes
    PRODUCTS: '/products',
    PRODUCT_DETAILS: (id: string) => `/products/${id}`,
    PRODUCT_SEARCH: '/products/search',
    
    // Categories routes
    CATEGORIES: '/categories',
    SUBCATEGORIES: (id: string, subId: string) => `/categories/${id}/subcategories/${subId}`,
    
    // Cart routes
    CART: '/cart',
    
    // Wishlist routes
    WISHLIST: '/wishlist',
    
    // Compare routes
    COMPARE: '/compare',
    
    // Orders routes
    ORDERS: '/orders',

    // Profile routes
    PROFILE: '/profile',

    // Admin routes
    ADMIN_PANEL: {
        PANEL: '/admin-panel',
        PRODUCTS: '/admin-panel/products',
        CATEGORIES: '/admin-panel/categories',
        SPECIFICATIONS: '/admin-panel/specifications',
        ORDERS: '/admin-panel/orders',
        USERS: '/admin-panel/users',
    }
};