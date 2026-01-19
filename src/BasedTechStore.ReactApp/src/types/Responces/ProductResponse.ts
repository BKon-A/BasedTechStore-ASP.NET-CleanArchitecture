import type { Product } from '../Products/Product';

export interface ProductResponse {
    products: Product[];
    totalProducts: number;
    totalPages: number;
    currentPage: number;
    pageSize: number;
}