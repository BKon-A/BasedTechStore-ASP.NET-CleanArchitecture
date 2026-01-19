import { apiClient } from './ApiClient';
import type { ProductResponse } from '../types/Responces/ProductResponse';
import type { ProductFilters } from '../types/Products/ProductFilters';
import type { Product } from '../types/Products/Product'
import { API_ENDPOINTS } from '../config/api';

export class ProductService {
  async getProducts(filters: ProductFilters = {}): Promise<ProductResponse> {
    return apiClient.get<ProductResponse>(API_ENDPOINTS.PRODUCTS.BASE, filters);
  }
  async getProductById(id: string): Promise<Product> {
    return apiClient.get<Product>(API_ENDPOINTS.PRODUCTS.BY_ID(id));
  }
  async getProductsByCategory(categoryId: string, filters: ProductFilters = {}): Promise<ProductResponse> {
    return apiClient.get<ProductResponse>(API_ENDPOINTS.PRODUCTS.BY_CATEGORY(categoryId), filters);
  }
  async getProductsBySubcategory(subcategoryId: string, filters: ProductFilters = {}): Promise<ProductResponse> {
    return apiClient.get<ProductResponse>(API_ENDPOINTS.PRODUCTS.BY_SUBCATEGORY(subcategoryId), filters);
  }
  async searchProducts(searchTerm: string, filters: ProductFilters = {}): Promise<ProductResponse> {
    return apiClient.get<ProductResponse>(API_ENDPOINTS.PRODUCTS.SEARCH, { ...filters, searchTerm });
  }
}

export const productService = new ProductService();