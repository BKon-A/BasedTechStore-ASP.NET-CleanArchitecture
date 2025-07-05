//import 'bootstrap/dist/js/bootstrap.bundle.min.js';
import { Container, Row, Col } from "react-bootstrap"
import { ROUTES } from "../../constants/routes";

export const Footer = () => {
    return (
        <footer className="bg-dark text-light py-4 mt-auto">
            <Container>
                <Row>
                    <Col md={6}>
                        <h5>Про BasedTechStore</h5>
                        <p>&copy; {new Date().getFullYear()} BasedTechStore. Всі права захищені.</p>
                        <p>Розроблено з ❤️ BasedTech</p>
                    </Col>
                    <Col md={3}>
                        <h5>Швидкі посилання</h5>
                        <ul className="list-unstyled">
                            <li><a href={ROUTES.HOME} className="text-light text-decoration-none">Головна</a></li>
                            <li><a href={ROUTES.PRODUCTS} className="text-light text-decoration-none">Продукти</a></li>
                            <li><a href="/about" className="text-light text-decoration-none">Про нас</a></li>
                            <li><a href="/contact" className="text-light text-decoration-none">Контакти</a></li>
                        </ul>
                    </Col>
                    <Col md={3}>
                        <h5>Підтримка</h5>
                        <ul className="list-unstyled">
                            <li><a href="/faq" className="text-light text-decoration-none">Часті запитання</a></li>
                            <li><a href="/returns" className="text-light text-decoration-none">Повернення товарів</a></li>
                            <li><a href="/shipping" className="text-light text-decoration-none">Доставка</a></li>
                            <li><a href="/privacy-policy" className="text-light text-decoration-none">Політика конфіденційності</a></li>
                        </ul>
                    </Col>
                </Row>
            </Container>
        </footer>
    );
}