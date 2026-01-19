import type { SubCategory } from "./SubCategory";

export interface Category {
    id: string;
    name: string;
    subCategories: SubCategory[];
}