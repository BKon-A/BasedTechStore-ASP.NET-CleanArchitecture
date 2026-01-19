import { useEffect, useState } from 'react';
import type { ProductFilters } from '../../types/Products/ProductFilters';
import type { Product } from '../../types/Products/Product';
import { productService } from '../../services/ProductService';

export const useProducts = (filters: ProductFilters = {}) => {
    const [products, setProducts] = useState<Product[]>([]);
    const [loading, setLoading] = useState(true);
    const [pagination, setPagination] = useState({totalItems: 0, totalPages: 0,
        currentPage: 1, pageSize: 12});
    const [error, setError] = useState<string | null>(null);

    const fetchProducts = async (newFilters?: ProductFilters) => {
        try {
            setLoading(true);
            setError(null);

            const finalFilters = { ...filters, ...newFilters };
            let response;

            if (finalFilters.subCategoryId) {
                console.log('Fetching by subcategory:', finalFilters.subCategoryId);
                response = await productService.getProductsBySubcategory(finalFilters.subCategoryId, finalFilters);
            } else if (finalFilters.categoryId) {
                console.log('Fetching by category:', finalFilters.categoryId);
                response = await productService.getProductsByCategory(finalFilters.categoryId, finalFilters);
            } else if (finalFilters.searchTerm) {
                console.log('Searching products with term:', finalFilters.searchTerm);
                response = await productService.searchProducts(finalFilters.searchTerm, finalFilters);
            } else {
                console.log('Fetching all products with filters:', finalFilters);
                response = await productService.getProducts(finalFilters);
            }

            console.log('Fetched products:', response);

            if (response && typeof response === 'object') { // api response is an object
                if (Array.isArray(response.products)) {
                    setProducts(response.products);
                    setPagination({
                        totalItems: response.totalProducts || response.products.length,
                        totalPages: response.totalPages || 1,
                        currentPage: response.currentPage || 1,
                        pageSize: response.pageSize || response.products.length
                    });
                } else if (Array.isArray(response)) { // api response is an array of products
                    setProducts(response as Product[]);
                    setPagination({
                        totalItems: response.length,
                        totalPages: 1,
                        currentPage: 1,
                        pageSize: response.length
                    });
                } else if (response.products && Array.isArray(response.products)) { // api response has products property and it's an array
                    const productsArray = response.products as Product[];
                    setProducts(productsArray);
                    setPagination({
                        totalItems: productsArray.length,
                        totalPages: 1,
                        currentPage: 1,
                        pageSize: productsArray.length
                    });
                } else {
                    console.warn('Unexpected response structure:', response);
                    setProducts([]);
                }
            } else {
                setProducts([]);
            }
        } catch (ex) {
            console.error('Error fetching products:', ex);
            setError(ex instanceof Error ? ex.message : 'An error occurred while fetching products');
            setProducts([]);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => { 
        console.log('useProducts effect triggered with filters:', filters);
        fetchProducts();
    }, [JSON.stringify(filters)]); // Deep compare filters

    const refetch = (newFilters?: ProductFilters) => {
        console.log('Refetching products with new filters:', newFilters);
        fetchProducts(newFilters);
    };

    return {
        products,
        loading,
        pagination,
        refetch,
        error,
        fetchByCategory: (categoryId: string, additionalFilters?: ProductFilters) =>
            fetchProducts({ ...filters, ...additionalFilters, categoryId }),
        fetchBySubcategory: (subcategoryId: string, additionalFilters?: ProductFilters) =>
            fetchProducts({ ...filters, ...additionalFilters, subCategoryId: subcategoryId })
    }
}