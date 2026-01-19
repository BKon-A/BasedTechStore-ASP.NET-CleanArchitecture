export const handleAddToCart = async (productId: string) => {
    try {
        console.log(`Adding product with ID ${productId} to cart...`);
    } catch (error) {
        console.error(`Failed to add product with ID ${productId} to cart:`, error);
    }
};

export const handleRemoveFromCart = async (productId: string) => {
    try {
        console.log(`Removing product with ID ${productId} from cart...`);
    } catch (error) {
        console.error(`Failed to remove product with ID ${productId} from cart:`, error);
    }
};

export const handleAddToWishlist = async (productId: string) => {
    try {
        console.log(`Adding product with ID ${productId} to wishlist...`);
    } catch (error) {
        console.error(`Failed to add product with ID ${productId} to wishlist:`, error);
    }
};

export const handleRemoveFromWishlist = async (productId: string) => {
    try {
        console.log(`Removing product with ID ${productId} from wishlist...`);
    } catch (error) {
        console.error(`Failed to remove product with ID ${productId} from wishlist:`, error);
    }
};

export const handleAddToCompare = async (productId: string) => {
    try {
        console.log(`Adding product with ID ${productId} to compare list...`);
    } catch (error) {
        console.error(`Failed to add product with ID ${productId} to compare list:`, error);
    }
};
