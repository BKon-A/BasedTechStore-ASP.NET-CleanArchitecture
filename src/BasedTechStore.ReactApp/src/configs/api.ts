// API configuration
export const API_CONFIG = {
  BASE_URL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:7001/api',
  TIMEOUT: 10000,
  RETRY_ATTEMPTS: 3,
  RETRY_DELAY: 1000,
} as const;

// API endpoints
export const API_ENDPOINTS = {
  // Auth endpoints
  AUTH: {
    LOGIN: '/auth/login',
    REGISTER: '/auth/register',
    LOGOUT: '/auth/logout',
    REFRESH: '/auth/refresh-token',
    PROFILE: '/auth/profile',
  },
  
  // Products endpoints
  PRODUCTS: {
    BASE: '/products',
    BY_ID: (id: string) => `/products/${id}`,
    BY_CATEGORY: (categoryId: string) => `/products/category/${categoryId}`,
    BY_SUBCATEGORY: (subcategoryId: string) => `/products/subcategory/${subcategoryId}`,
    SEARCH: '/products/search',
    FILTER: '/products/filter',
  },
  
  // Categories endpoints
  CATEGORIES: {
    BASE: '/categories',
    BY_ID: (id: string) => `/categories/${id}`,
    WITH_SUBCATEGORIES: '/categories/with-subcategories',
    SUBCATEGORIES: (id: string) => `/categories/${id}/subcategories`,
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