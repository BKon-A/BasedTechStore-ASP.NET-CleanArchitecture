import { useCallback, useMemo, useState, type ReactNode } from "react";
import { Navigate, useLocation, useNavigate } from "react-router-dom";
import { useAuthContext } from "../../context/AuthContext";
import { ROUTES } from "../../constants/routes";
import { Button, Card, Col, Container, Nav, Offcanvas, Row } from "react-bootstrap";

interface AdminLayoutProps {
    children?: ReactNode;
}

type AdminDashboardTabs = 'Адмін панель' | 'Користувачі' | 'Продукти' | 'Характеристики' | 'Замовлення';

export const AdminLayout = ({ children }: AdminLayoutProps) => {
    const [ showSidebar, setShowSidebar ] = useState(true);
    const navigate = useNavigate();
    const location = useLocation();
    const { user, isAuthenticated, loading, isManager } = useAuthContext();

    const activeTab: AdminDashboardTabs = useMemo(() => {
        const path = location.pathname;
        if (path.includes('/admin/users')) return 'Користувачі';
        if (path.includes('/admin/products')) return 'Продукти';
        if (path.includes('/admin/specifications')) return 'Характеристики';
        if (path.includes('/admin/orders')) return 'Замовлення';
        return 'Адмін панель';
    }, [location.pathname]);

    const handleToggleSidebar = useCallback(() => {
        setShowSidebar(prev => !prev);
    }, []);

    const handleCloseSidebar = useCallback(() => {
        setShowSidebar(false);
    }, []);

    const handleNavigate = useCallback((path: string) => {
        navigate(path);
        setShowSidebar(false);
    }, [navigate]);

    const menuItems = useMemo(() => [
        {
            section: 'Адмін панель' as AdminDashboardTabs,
            icon: 'bi bi-speedometer2',
            label: 'Адмін панель',
            path: ROUTES.ADMIN_DASHBOARD.DASHBOARD
        },
        {
            section: 'Користувачі' as AdminDashboardTabs,
            icon: 'bi bi-people',
            label: 'Користувачі',
            path: ROUTES.ADMIN_DASHBOARD.USERS
        },
        {
            section: 'Продукти' as AdminDashboardTabs,
            icon: 'bi bi-box-seam',
            label: 'Продукти',
            path: ROUTES.ADMIN_DASHBOARD.PRODUCTS
        },
        {
            section: 'Характеристики' as AdminDashboardTabs,
            icon: 'bi bi-tags',
            label: 'Характеристики',
            path: ROUTES.ADMIN_DASHBOARD.SPECIFICATIONS
        },
        {
            section: 'Замовлення' as AdminDashboardTabs,
            icon: 'bi bi-bag-check',
            label: 'Замовлення',
            path: ROUTES.ADMIN_DASHBOARD.ORDERS
        }
    ], []);

    const renderSidebar = useMemo(() => (
        <Nav variant="pills" className="flex-column">
            {menuItems.map(item => (
                <Nav.Item key={item.section}>
                    <Nav.Link
                        active={activeTab === item.section}
                        onClick={() => handleNavigate(item.path)}
                        className="d-flex align-items-center cursor-pointer"
                    >
                        <i className={`bi ${item.icon} me-2`}></i>
                        {item.label}
                    </Nav.Link>
                </Nav.Item>
            ))}
            <hr />
            <Nav.Item>
                <Nav.Link
                    onClick={() => handleNavigate(ROUTES.HOME)}
                    className="d-flex align-items-center cursor-pointer"
                >
                    <i className="bi bi-house-door-fill me-2"></i>
                    На головну
                </Nav.Link>
            </Nav.Item>
        </Nav>
    ), [activeTab, handleNavigate, menuItems]);

    if (!loading && (!isAuthenticated || !isManager)) {
        return <Navigate to={ROUTES.HOME} replace />;
    }

    if (loading) {
        return (
            <Container className="py-5">
                <div className="text-center">
                    <div className="spinner-border text-primary" role="status">
                        <span className="visually-hidden">Завантаження...</span>
                    </div>
                </div>
            </Container>
        )
    }

    return (
        <div className="admin-layout bg-light min-vh-100">
            <div className="bg-dark text-white py-2 sticky-top shadow-sm">
                <Container fluid>
                    <Row className="align-items-center">
                        <Col>
                            <div className="d-flex align-items-center">
                                <Button variant="link" className="text-light d-lg-none me-2" onClick={handleToggleSidebar}>
                                    <i className="bi bi-list fs-4"></i>
                                </Button>
                                <i className="bi bi-shield-lock-fill me-2"></i>
                                <span className="h5 mb-0">Адмін панель</span>
                            </div>
                        </Col>
                        <Col xs="auto">
                            <div className="d-flex align-items-center gap-3">
                                <span className="d-nono d-md-inline small">
                                    {user?.userName}
                                </span>
                                <Button variant="link" className="text-light" onClick={() => navigate(ROUTES.PROFILE)} title="Профіль">
                                    <i className="bi bi-person-circle fs-5"></i>
                                </Button>
                            </div>
                        </Col>
                    </Row>
                </Container>
            </div>

            <Container fluid className="py-4">
                <Row>
                    <Col lg={2} className="d-none d-lg-block">
                        <Card className="sticky-top" style={{ top: '70px' }}>
                            <Card.Body>
                                {renderSidebar}
                            </Card.Body>
                        </Card>
                    </Col>

                    <Offcanvas show={showSidebar} onHide={handleCloseSidebar} placement="start">
                        <Offcanvas.Header closeButton>
                            <Offcanvas.Title>
                                <i className="bi bi-shield-lock me-2"></i>
                                Меню
                            </Offcanvas.Title>
                        </Offcanvas.Header>
                        <Offcanvas.Body>
                            {renderSidebar}
                        </Offcanvas.Body>
                    </Offcanvas>

                    <Col lg={10}>
                        {children}
                    </Col>
                </Row>
            </Container>
        </div>
    );
}