export interface Product {
    id: string;
    name: string;
    description: string;
    price: number;
    imageUrl: string;
    brand: string;
    createdAt: string;
    updatedAt: string;
    categoryName: string;
    subCategoryName: string;

    categoryId: string;
    subCategoryId: string;

    inStock: boolean;
}