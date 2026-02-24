// API configuration
export const API_CONFIG = {
  BASE_URL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:7250/api',
  TIMEOUT: 10000,
  RETRY_ATTEMPTS: 3,
  RETRY_DELAY: 1000,
} as const;

// API endpoints
export const API_ENDPOINTS = {
  // Auth endpoints
  AUTH: {
    SIGN_IN: '/auth/signIn',
    SIGN_UP: '/auth/signUp',
    SIGN_OUT: '/auth/signOut',
    REFRESH: '/auth/refresh',
    GET_CURRENT_USER: '/auth/user',
    CHECK: '/auth/check',
  },
  
  PROFILE: {
    HOME: '/profile/home',
    ADMIN_DASHBOARD: '/profile/admin/dashboard',
  },

  // Products endpoints
  PRODUCTS: {
    BASE: '/products/all',
    BY_ID: (id: string) => `/products/${id}`,
    BY_CATEGORY: (categoryId: string) => `/products/category/${categoryId}`,
    BY_SUBCATEGORY: (subcategoryId: string) => `/products/subcategory/${subcategoryId}`,
    SEARCH: '/products/search',
    FILTER: '/products/filtered',
    // CATEGORIES: '/products/categories',
    // CATEGORY_BY_ID: (id: string) => `/products/categories/${id}`,
    // SUBCATEGORIES_BY_CATEGORYID: (categoryId: string) => `/products/categories/${categoryId}/subcategories`,
  },

  CATEGORIES: {
    BASE: '/categories',
    ALL: '/categories/all',
    BY_ID: (id: string) => `/categories/${id}`,
    SUBCATEGORIES: '/categories/subcategories',
    SUBCATEGORY_BY_ID: (id: string) => `/categories/subcategories/${id}`,
    SUBCATEGORIES_BY_CATEGORYID: (categoryId: string) => `/categories/${categoryId}/subcategories`,
  },
  
  // Cart endpoints
  CART: {
    BASE: '/cart',
    ITEMS: '/cart/items',
    ADD: '/cart/add',
    UPDATE: '/cart/update',
    REMOVE: '/cart/remove',
    CLEAR: '/cart/clear',
  },
  
  // Wishlist endpoints
  WISHLIST: {
    BASE: '/wishlist',
    ITEMS: '/wishlist/items',
    ADD: '/wishlist/add',
    REMOVE: '/wishlist/remove',
    CLEAR: '/wishlist/clear',
  },
  
  // Compare endpoints
  COMPARE: {
    BASE: '/compare',
    ITEMS: '/compare/items',
    ADD: '/compare/add',
    REMOVE: '/compare/remove',
    CLEAR: '/compare/clear',
  },
} as const;

// HTTP status codes
export const HTTP_STATUS = {
  OK: 200,
  CREATED: 201,
  BAD_REQUEST: 400,
  UNAUTHORIZED: 401,
  FORBIDDEN: 403,
  NOT_FOUND: 404,
  INTERNAL_SERVER_ERROR: 500,
} as const;