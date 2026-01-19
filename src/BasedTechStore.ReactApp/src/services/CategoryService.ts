import { apiClient } from './ApiClient';
import type { Category } from '../types/Categories/Category';
import type { SubCategory } from '../types/Categories/SubCategory';
import { API_ENDPOINTS } from '../config/api';

export class CategoryService {
    async getCategories(): Promise<Category[]> {
        console.log('CategoryService.getCategories called');
        console.log('Endpoint:', API_ENDPOINTS.CATEGORIES.ALL);
        return apiClient.get<Category[]>(API_ENDPOINTS.CATEGORIES.ALL);
    }
    async getCategoryById(id: string): Promise<Category> {
        return apiClient.get<Category>(API_ENDPOINTS.CATEGORIES.BY_ID(id));
    }
    async getSubcategoriesByCategoryId(categoryId: string): Promise<SubCategory[]> {
        return apiClient.get<SubCategory[]>(API_ENDPOINTS.CATEGORIES.SUBCATEGORIES_BY_CATEGORYID(categoryId));
    }
}

export const categoryService = new CategoryService();