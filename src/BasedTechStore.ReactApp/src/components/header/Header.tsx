import { Container, Image, Dropdown, InputGroup, Nav, NavDropdown, Navbar, Form, Button } from 'react-bootstrap';
import { ROUTES } from '../../constants/routes';

export const Header = () => {
    return (
        <Navbar bg="dark" variant="dark" expand="lg" className="py-3">
            <Container>
                <Navbar.Brand href={ROUTES.HOME} className="d-flex align-items-center">
                    <Image src="/basedtech_logo.png" alt="BasedTechStore Logo" width={40} height={40} className="me-2" />
                    <span className="fw-bold">BasedTechStore</span>
                </Navbar.Brand>
                
                <Navbar.Toggle aria-controls="header-navbar-nav" />
                <Navbar.Collapse id="header-navbar-nav">
                    <Nav className="me-auto">
                        <NavDropdown title="Категорії" id="dropdown-categories">
                            <NavDropdown.Header>Електроніка</NavDropdown.Header>
                            <NavDropdown.Item href="/categories/laptops">Ноутбуки</NavDropdown.Item>
                            <NavDropdown.Item href="/categories/accessories">Аксесуари</NavDropdown.Item>
                            <NavDropdown.Divider />
                            <NavDropdown.Header>Комплектуючі</NavDropdown.Header>
                            <NavDropdown.Item href="/categories/ram">Оперативна пам'ять</NavDropdown.Item>
                            <NavDropdown.Item href="/categories/ssd">SSD</NavDropdown.Item>
                            <NavDropdown.Divider />
                            <NavDropdown.Header>Запчастини</NavDropdown.Header>
                            <NavDropdown.Item href="/categories/mainboards">Материнські плати</NavDropdown.Item>
                            <NavDropdown.Item href="/categories/videocards">Відеокарти</NavDropdown.Item>
                            <NavDropdown.Item href="/categories/processors">Процесори</NavDropdown.Item>
                        </NavDropdown>
                        <Nav.Link href="/about">Про нас</Nav.Link>
                        <Nav.Link href="/contact">Контакти</Nav.Link>
                    </Nav>
                    
                    <div className="d-flex align-items-center gap-3">
                        <Form className="d-flex">
                            <InputGroup>
                                <Form.Control 
                                    type="search" 
                                    placeholder="Пошук товарів..." 
                                    className="border-secondary"
                                    style={{ minWidth: '250px' }}
                                />
                                <Button variant="outline-secondary">
                                    <i className="bi bi-search"></i>
                                </Button>
                            </InputGroup>
                        </Form>
                        
                        <Nav className="d-flex flex-row">
                            <Nav.Link href={ROUTES.COMPARE} className="position-relative px-2" title="Порівняння">
                                <i className="bi bi-arrow-left-right fs-5"></i>
                            </Nav.Link>
                            <Nav.Link href={ROUTES.WISHLIST} className="position-relative px-2" title="Бажання">
                                <i className="bi bi-heart fs-5"></i>
                            </Nav.Link>
                            <Nav.Link href={ROUTES.CART} className="position-relative px-2" title="Кошик">
                                <i className="bi bi-cart fs-5"></i>
                            </Nav.Link>
                        </Nav>
                        
                        <Dropdown>
                            <Dropdown.Toggle variant="outline-light" className="border-0">
                                <i className="bi bi-person fs-5"></i>
                            </Dropdown.Toggle>
                            <Dropdown.Menu align="end">
                                <Dropdown.Item href={ROUTES.REGISTER}>Реєстрація</Dropdown.Item>
                                <Dropdown.Item href={ROUTES.LOGIN}>Вхід</Dropdown.Item>
                                <Dropdown.Divider />
                                <Dropdown.Item href={ROUTES.PROFILE}>Профіль</Dropdown.Item>
                            </Dropdown.Menu>
                        </Dropdown>
                    </div>
                </Navbar.Collapse>
            </Container>
        </Navbar>
    );
}