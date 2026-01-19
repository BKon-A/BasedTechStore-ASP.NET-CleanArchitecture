import { useState, useEffect } from 'react';
import type { Category } from '../../types/Categories/Category';
import { categoryService } from '../../services/CategoryService';

export const useCategories = () => {
    const [categories, setCategories] = useState<Category[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const fetchCategories = async () => {
        try {
            console.log('Starting to fetch categories...');
            setLoading(true);
            setError(null);

            const response = await categoryService.getCategories();
            console.log('Categories fetched successfully:', response);
            setCategories(response);
        }
        catch (ex) {
            setError(ex instanceof Error ? ex.message : 'An error occurred while fetching categories');
        }
        finally {
            setLoading(false);
        }
    }

    useEffect(() => {
        console.log('useCategories hook initialized, fetching categories...');
        fetchCategories();
    }, []);

    return { categories, loading, error, refetch: fetchCategories };
}