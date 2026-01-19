import type { Product } from '../../types/Products/Product';
import { Card, Button } from 'react-bootstrap';
import { formatCurrency } from '../../utills/currency/currencyFormating';
import { ROUTES } from '../../constants/routes';

interface ProductCardProps {
    product: Product;
    onAddToCart?: (productId: string) => void;
    onAddToWishlist?: (productId: string) => void;
    onCompare?: (productId: string) => void;
}

export const ProductCard = ({ product, onAddToCart, onAddToWishlist, onCompare }: ProductCardProps) => {
    return (
        <Card className="h-100 product-card">
            <Card.Img variant="top" 
                src={product.imageUrl || 'placeholder.png'} 
                alt={product.name} 
                style={{ height: '200px', objectFit: 'cover' }} />
            <Card.Body className="d-flex flex-column">
                <Card.Title className="h6">{product.name}</Card.Title>
                <Card.Text className="text-muted small">{product.brand} - {product.subCategoryName}</Card.Text>
                <Card.Text className="flex-grow-1">{product.description?.substring(0, 100)}
                    {product.description && product.description.length > 100 && '...'}</Card.Text>
                <div className="mt-auto">
                    <div className="d-flex justify-content-between align-items-center">
                        <span className="h5 text-primary mb-0">
                            {formatCurrency(product.price)}
                        </span>
                        <Button variant="outline-primary"
                            size="sm"
                            href={ROUTES.PRODUCT_DETAILS(product.id)}
                        > Детальніше </Button>
                    </div>
                    <div className="mt-2 d-flex gap-2">
                        {onAddToCart && (
                            <Button
                                variant="primary"
                                size="sm"
                                className="flex-grow-1"
                                onClick={() => onAddToCart(product.id)}
                                disabled={!product.inStock}
                                >
                                <i className="bi bi-cart-plus me-1" /> {product.inStock ? 'Додати до кошика' : 'Немає в наявності' }
                            </Button>
                        )}
                        {onAddToWishlist && (
                            <Button
                                variant="outline-danger"
                                size="sm"
                                className="flex-grow-1"
                                onClick={() => onAddToWishlist(product.id)}
                                >
                                <i className="bi bi-heart me-1" /> Додати до обраного
                            </Button>
                        )}
                        {onCompare && (
                            <Button
                                variant="outline-secondary"
                                size="sm"
                                className="flex-grow-1"
                                onClick={() => onCompare(product.id)}
                                >
                                <i className="bi bi-arrow-right me-1" /> Порівняти
                            </Button>
                        )}
                    </div>
                </div>
            </Card.Body>
        </Card>
    );
};