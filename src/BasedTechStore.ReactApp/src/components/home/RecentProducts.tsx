import { Row, Col, Card, Button } from 'react-bootstrap';
import { ROUTES } from '../../constants/routes';

export const RecentProducts = () => {
    // Mock data for demonstration
    const recentProducts = [
        {
            id: '1',
            name: 'Ноутбук ASUS ROG',
            price: 45000,
            image: '/placeholder-product.jpg',
            category: 'Ноутбуки'
        },
        {
            id: '2',
            name: 'Відеокарта RTX 4070',
            price: 28000,
            image: '/placeholder-product.jpg',
            category: 'Відеокарти'
        },
        {
            id: '3',
            name: 'Процесор Intel i7',
            price: 15000,
            image: '/placeholder-product.jpg',
            category: 'Процесори'
        },
        {
            id: '4',
            name: 'SSD Samsung 1TB',
            price: 3500,
            image: '/placeholder-product.jpg',
            category: 'SSD'
        }
    ];

    return (
        <div className="mb-5">
            <h2 className="mb-4">Останні переглянуті товари</h2>
            <Row>
                {recentProducts.map((product) => (
                    <Col key={product.id} md={3} className="mb-4">
                        <Card className="h-100">
                            <Card.Img 
                                variant="top" 
                                src={product.image} 
                                alt={product.name}
                                style={{ height: '200px', objectFit: 'cover' }}
                            />
                            <Card.Body className="d-flex flex-column">
                                <Card.Title className="fs-6">{product.name}</Card.Title>
                                <Card.Text className="text-muted small">{product.category}</Card.Text>
                                <div className="mt-auto">
                                    <div className="fw-bold text-primary mb-2">{product.price.toLocaleString()} ₴</div>
                                    <Button 
                                        variant="outline-primary" 
                                        size="sm" 
                                        href={ROUTES.PRODUCT_DETAILS(product.id)}
                                        className="w-100"
                                    >
                                        Переглянути
                                    </Button>
                                </div>
                            </Card.Body>
                        </Card>
                    </Col>
                ))}
            </Row>
        </div>
    );
}
