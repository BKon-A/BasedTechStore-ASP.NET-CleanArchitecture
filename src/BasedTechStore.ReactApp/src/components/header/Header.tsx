// import { useState, useEffect } from "react";
import { Container, Image, Dropdown, InputGroup, Nav, Navbar, Form, Button } from 'react-bootstrap';
import { ROUTES } from '../../constants/routes';
import { useCategories } from '../../hooks/Categories/useCategories';
import { useState, useCallback, useMemo } from 'react';
import { Login } from '../modals/Login';
import { Register } from '../modals/Register';
import '../../styles/catalog.css';
import { useNavigate } from 'react-router-dom';
import { useAuthContext } from '../../context/AuthContext';

export const Header = () => {
    const [showSignIn, setShowSignIn] = useState(false);
    const [showSignUp, setShowSignUp] = useState(false);

    const navigate = useNavigate();
    const { categories, loading, error } = useCategories();
    const { isAuthenticated, isManager, user, logOut } = useAuthContext();

    const handleShowSignIn = useCallback(() => setShowSignIn(true), []);
    const handleHideSignIn = useCallback(() => setShowSignIn(false), []);
    const handleShowSignUp = useCallback(() => setShowSignUp(true), []);
    const handleHideSignUp = useCallback(() => setShowSignUp(false), []);

    const handleLogOut = useCallback(async () => {
        await logOut();
        navigate(ROUTES.HOME);
    }, [logOut, navigate]);

    const handleProfile = useCallback(() => {
        navigate(ROUTES.PROFILE);
    }, [navigate]);

    const renderCategories = useMemo(() => {
        if (loading) return <li><span className="dropdown-item-text">Завантаження...</span></li>;
        if (error) return <li><span className="dropdown-item-text text-danger">Помилка завантаження категорій</span></li>;
        if (!Array.isArray(categories) || categories.length === 0) {
            return <li><span className="dropdown-item-text">Категорії відсутні</span></li>;
        }

        return categories.map((category) => (
            <li key={category.id} className="dropdown-submenu">
                <a className="dropdown-item dropdown-toggle" href={`${ROUTES.PRODUCTS}?categoryId=${category.id}`}>
                    {category.name}
                </a>

                {category.subCategories && category.subCategories.length > 0 && (
                    <ul className="dropdown-menu">
                        <li>
                            <a className="dropdown-item fw-bold" href={`${ROUTES.PRODUCTS}?categoryId=${category.id}`}>
                                Всі {category.name.toLowerCase()}
                            </a>
                        </li>
                        <li><hr className="dropdown-divider" /></li>
                        {category.subCategories.map((subcategory) => (
                            <li key={subcategory.id}>
                                <a href={`${ROUTES.PRODUCTS}?categoryId=${category.id}&subCategoryId=${subcategory.id}`}
                                    className="dropdown-item"
                                    >
                                    {subcategory.name}
                                </a>
                            </li>
                        ))}
                    </ul>
                )}
            </li>
        ));
    }, [categories, loading, error]);

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
                        <div className="dropdown menu-catalog" title="Каталог" id="dropdown-catalog" data-bs-boundary="viewport">
                            <a className="nav-link dropdown-toggle" href="#" role="button" data-bs-toggle="dropdown" aria-expanded="false" data-bs-autoclose="outside">
                                Каталог
                            </a>
                            <ul className="dropdown-menu menu-catalog-content">
                                {renderCategories}
                            </ul>
                        </div>

                        <Nav.Link href="/about">Про нас</Nav.Link>
                        <Nav.Link href="/contact">Контакти</Nav.Link>
                    </Nav>
                    
                    <div className="d-flex align-items-center gap-3">
                        <Form className="d-flex" action={ROUTES.PRODUCTS} method="GET">
                            <InputGroup>
                                <Form.Control 
                                    type="search"
                                    name="search" 
                                    placeholder="Пошук товарів..." 
                                    className="border-secondary"
                                    style={{ minWidth: '250px' }}
                                />
                                <Button variant="outline-secondary" type="submit">
                                    <i className="bi bi-search"></i>
                                </Button>
                            </InputGroup>
                        </Form>
                        
                        <Nav className="d-flex flex-row">
                            <Nav.Link href={ROUTES.COMPARE} className="position-relative px-2" title="Порівняння">
                                <i className="bi bi-arrow-left-right fs-5"></i>
                            </Nav.Link>
                            <Nav.Link href={ROUTES.WISHLIST} className="position-relative px-2" title="Обране">
                                <i className="bi bi-heart fs-5"></i>
                            </Nav.Link>
                            <Nav.Link href={ROUTES.CART} className="position-relative px-2" title="Кошик">
                                <i className="bi bi-cart fs-5"></i>
                            </Nav.Link>
                        </Nav>
                        
                        <Dropdown>
                            <Dropdown.Toggle variant="outline-light" className="border-0">
                                <i className="bi bi-person fs-5"></i>
                                {isAuthenticated && user?.userName && (
                                    <span className="ms-2 d-none d-lg-inline">{user.userName}</span>
                                )}
                            </Dropdown.Toggle>
                            <Dropdown.Menu align="end">
                                {isAuthenticated ? (
                                    <>
                                        <Dropdown.Item onClick={handleProfile}>
                                            <i className="bi bi-person-circle me-2"></i>
                                            Профіль
                                        </Dropdown.Item>
                                        {isManager && (
                                            <>
                                                <Dropdown.Divider />
                                                <Dropdown.Item href={ROUTES.ADMIN_DASHBOARD.DASHBOARD}>
                                                    <i className="bi bi-speedometer2 me-2"></i>
                                                    Панель адміністратора
                                                </Dropdown.Item>
                                            </>
                                        )}
                                        <Dropdown.Divider />
                                        <Dropdown.Item onClick={handleLogOut}>
                                            <i className="bi bi-box-arrow-right me-2"></i>
                                            Вийти
                                        </Dropdown.Item>
                                    </>
                                ) : (
                                    <>
                                        <Dropdown.Item onClick={handleShowSignIn}>
                                            <i className="bi bi-box-arrow-in-right me-2"></i>
                                            Вхід
                                        </Dropdown.Item>
                                        <Dropdown.Item onClick={handleShowSignUp}>
                                            <i className="bi bi-person-plus me-2"></i>
                                            Реєстрація
                                        </Dropdown.Item>
                                    </>
                                )}
                            </Dropdown.Menu>
                        </Dropdown>
                    </div>
                </Navbar.Collapse>
            </Container>

            {!isAuthenticated && (
                <>
                    <Login show={showSignIn} onHide={handleHideSignIn} />
                    <Register show={showSignUp} onHide={handleHideSignUp} />
                </>
            )}
        </Navbar>
    );
};