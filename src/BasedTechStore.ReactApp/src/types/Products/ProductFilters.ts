export interface ProductFilters {
    categoryId?: string;
    subCategoryId?: string;
    minPrice?: number;
    maxPrice?: number;
    brands?: string[];
    searchTerm?: string;
    page?: number;
    pageSize?: number;
    [key: string]: string | number | string[] | undefined;
}