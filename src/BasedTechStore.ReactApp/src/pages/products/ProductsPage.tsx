import { useState, useEffect } from 'react'
import { Button, Alert, Container, Spinner, Row, Col, Pagination } from 'react-bootstrap';
import { useSearchParams } from 'react-router-dom';
import type { ProductFilters } from '../../types/Products/ProductFilters';
import { useProducts } from '../../hooks/Products/useProducts';
import {} from '../../utills/currency/currencyFormating';
import { handleAddToCart, handleAddToCompare, handleAddToWishlist} from '../../utills/handlers/productActions';
import { ProductCard } from '../../components/cards/ProductCard';
import '../../styles/product-card.css';

export const ProductsPage = () => {
    const [searchParams] = useSearchParams();
    const [filters, setFilters] = useState<ProductFilters>({});
    const { products, loading, error, pagination, refetch } = useProducts(filters);

    const handlePageChange = (page: number) => {
        const newParams = new URLSearchParams(searchParams);
        newParams.set('page', page.toString());
        window.history.pushState({}, '', `${window.location.pathname}?${newParams}`);

        const newFilters = { ...filters, page };
        setFilters(newFilters);
        refetch(newFilters);
    };

    useEffect(() => {
        const newFilters: ProductFilters = {
            categoryId: searchParams.get('categoryId') || undefined,
            subCategoryId: searchParams.get('subCategoryId') || undefined,
            minPrice: searchParams.get('minPrice') ? Number(searchParams.get('minPrice')) : undefined,
            maxPrice: searchParams.get('maxPrice') ? Number(searchParams.get('maxPrice')) : undefined,
            brands: searchParams.getAll('brands') || undefined,
            searchTerm: searchParams.get('search') || undefined,
            page: searchParams.get('page') ? Number(searchParams.get('page')) : 1,
            pageSize: 12
        };

        console.log('Updating filters from URL params:', newFilters);
        setFilters(newFilters);
    }, [searchParams]);

    const getPageTitle = () => {
        if (filters.searchTerm) {
            return `Результати пошуку: "${filters.searchTerm}"`;
        }
        if (filters.subCategoryId && filters.categoryId) {
            return 'Продукти за підкатегорією';
        }
        if (filters.categoryId) {
            return 'Продукти за категорією';
        }
        return 'Всі продукти';
    };

    if (loading) {
        return (
            <Container className="d-flex justify-content-center align-items-center" style={{ minHeight: '400px' }}>
                <Spinner animation="border" role="status">
                    <span className="visually-hidden">Завантаження...</span>
                </Spinner>
            </Container>
        );
    }

    if (error) {
        return (
            <Container className="mt-4">
                <Alert variant="danger">
                    <Alert.Heading>Помилка завантаження</Alert.Heading>
                    <p>{error}</p>
                    <Button variant="outline-danger" onClick={() => refetch()}>Спробувати знову</Button>
                </Alert>
            </Container>
        );
    }

    return (
        <Container className="product-page mt-4">
            <Row>
                <Col>
                    <h1 className="mb-4">{getPageTitle()}</h1>

                    {(filters.categoryId || filters.subCategoryId || filters.searchTerm) && (
                        <div className='mb-3'>
                            <small className="text-muted">
                                <strong>Фільтри:</strong>{' '}
                                {filters.categoryId && `Категорія: ${filters.categoryId}`}
                                {filters.subCategoryId && `| Підкатегорія: ${filters.subCategoryId}`}
                                {filters.searchTerm && `| Пошук: "${filters.searchTerm}"`}
                            </small>
                        </div>
                    )}

                    {products.length === 0 ? (
                        <Alert variant="info">
                            <Alert.Heading>Немає продуктів</Alert.Heading>
                            <p>Наразі немає продуктів, що відповідають вашим критеріям пошуку.</p>
                        </Alert>
                    ) : (
                        <>
                            <p className="text-muted mb-4">
                                Знайдено {pagination.totalItems} продуктів
                                (сторінка {pagination.currentPage} з {pagination.totalPages})
                            </p>

                            <Row xs={1} sm={2} md={3} lg={4} className="g-4">
                                {products.map((product) => (
                                    <Col key={product.id}>
                                        <ProductCard product={product}
                                            onAddToCart={handleAddToCart}
                                            onAddToWishlist={handleAddToWishlist}
                                            onCompare={handleAddToCompare} />
                                    </Col>
                                ))}
                            </Row>

                            {pagination.totalPages > 1 && (
                                <div className="d-flex justify-content-center mt-4">
                                    <Pagination>
                                        <Pagination.First
                                            disabled={pagination.currentPage === 1}
                                            onClick={() => handlePageChange(1)}
                                        />
                                        <Pagination.Prev
                                            disabled={pagination.currentPage === 1}
                                            onClick={() => handlePageChange(pagination.currentPage - 1)}
                                        />
                                        {Array.from({ length: Math.min(pagination.totalPages, 10) }, (_, i) => {
                                            const page = i + 1;
                                            return (
                                                <Pagination.Item
                                                    key={page}
                                                    active={page === pagination.currentPage}
                                                    onClick={() => handlePageChange(page)}
                                                >
                                                    {page}
                                                </Pagination.Item>
                                            );
                                        })}
                                        <Pagination.Next
                                            disabled={pagination.currentPage === pagination.totalPages}
                                            onClick={() => handlePageChange(pagination.currentPage + 1)}
                                        />
                                        <Pagination.Last
                                            disabled={pagination.currentPage === pagination.totalPages}
                                            onClick={() => handlePageChange(pagination.totalPages)}
                                        />
                                    </Pagination>
                                </div>
                            )}
                        </>
                    )}
                </Col>
            </Row>
        </Container>
    );
};