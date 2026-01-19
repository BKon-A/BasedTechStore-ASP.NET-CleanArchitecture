export const ROUTES = {
    // Home route
    HOME: '/',
    
    // Auth routes
    LOGIN: '/login',
    REGISTER: '/signup',
    LOGOUT: '/logout',
    
    // ==== Start product routes section ====
    GET_ALL_PRODUCTS: '/products/all',
    GET_PRODUCT_BY_ID: (id: string) => `/products/${id}`,
    GET_PRODUCTS_BY_CATEGORY: (categoryId: string) => `/products/category/${categoryId}`,
    GET_PRODUCTS_BY_SUBCATEGORY: (subcategoryId: string) => `/products/subcategory/${subcategoryId}`,
    ADD_PRODUCT: '/products/add',
    UPDATE_PRODUCT: (id: string) => `/products/update/${id}`,
    DELETE_PRODUCT: (id: string) => `/products/delete/${id}`,
    PRODUCTS: '/products',
    PRODUCT_DETAILS: (id: string) => `/products/${id}`,
    PRODUCT_SEARCH: '/products/search',
    CATEGORIES: '/products/categories',
    SUBCATEGORIES: (id: string, subId: string) => `products/categories/${id}/subcategories/${subId}`,
    // ---- End product routes section ----

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
    ADMIN_DASHBOARD: {
        DASHBOARD: '/admin-panel',
        PRODUCTS: '/admin-panel/products',
        CATEGORIES: '/admin-panel/categories',
        SPECIFICATIONS: '/admin-panel/specifications',
        ORDERS: '/admin-panel/orders',
        USERS: '/admin-panel/users',
    }
};