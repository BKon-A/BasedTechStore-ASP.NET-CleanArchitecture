import { useCallback, useMemo, useState } from "react";
import { useAuthContext } from "../../context/AuthContext";
import { Alert, Button, Card, Col, Container, Form, Nav, Row } from "react-bootstrap";
import { ROUTES } from "../../constants/routes";
import { Navigate } from "react-router-dom";

type VerticalMenuTab = 'Загальне' | 'Безпека' | 'Мої замовлення' | 'Сповіщення' | 'Налаштування' | 'Адмін-панель';

export const ProfilePage = () => {
    const { user, isAuthenticated, loading, isManager } = useAuthContext();
    const [activeTab, setActiveTab] = useState<VerticalMenuTab>('Загальне');
    const [updateSuccess, setUpdateSuccess] = useState(false);

    const handleTabChange = useCallback((tab: VerticalMenuTab) => {
        setActiveTab(tab);
        setUpdateSuccess(false);
    }, []);

    const profileMenu = useMemo(() => (
        <Nav variant="pills" className="flex-column">
            <Nav.Item>
                <Nav.Link className="d-flex align-items-center" active={activeTab === 'Загальне'} onClick={() => handleTabChange('Загальне')}>
                    <i className="bi bi-person-circle me-2"></i>
                    Загальне
                    </Nav.Link>
            </Nav.Item>
            <Nav.Item>
                <Nav.Link className="d-flex align-items-center" active={activeTab === 'Безпека'} onClick={() => handleTabChange('Безпека')}>
                    <i className="bi bi-shield-lock me-2"></i>
                    Безпека
                    </Nav.Link>
            </Nav.Item>
            <Nav.Item>
                <Nav.Link className="d-flex align-items-center" active={activeTab === 'Мої замовлення'} onClick={() => handleTabChange('Мої замовлення')}>
                    <i className="bi bi-bag-check me-2"></i>
                    Мої замовлення
                    </Nav.Link>
            </Nav.Item>
            <Nav.Item>
                <Nav.Link className="d-flex align-items-center" active={activeTab === 'Сповіщення'} onClick={() => handleTabChange('Сповіщення')}>
                    <i className="bi bi-bell me-2"></i>
                    Сповіщення
                    </Nav.Link>
            </Nav.Item>
            <Nav.Item>
                <Nav.Link className="d-flex align-items-center" active={activeTab === 'Налаштування'} onClick={() => handleTabChange('Налаштування')}>
                    <i className="bi bi-gear me-2"></i>
                    Налаштування
                    </Nav.Link>
            </Nav.Item>
            {isManager && (
                <Nav.Item>
                    <Nav.Link href={ROUTES.ADMIN_DASHBOARD.DASHBOARD} className="d-flex align-items-center text-primary-emphasis" active={activeTab === 'Адмін-панель'} onClick={() => handleTabChange('Адмін-панель')}>
                        <i className="bi bi-speedometer2 me-2"></i>
                        Адмін-панель
                    </Nav.Link>
                </Nav.Item>
            )}
        </Nav>
    ), [activeTab, handleTabChange, isManager]);

    const renderOverviewTabContent = useMemo(() => (
        <Card>
            <Card.Header>
                <h5 className="mb-0">Огляд профілю</h5>
            </Card.Header>
            <Card.Body>
                <Row className="mb-3">
                    <Col sm={3}>
                        <strong>Ім'я користувача:</strong>
                    </Col>
                    <Col sm={9}>{user?.userName || 'Не вказано'}</Col>
                </Row>
                <Row className="mb-3">
                    <Col sm={3}>
                        <strong>Повне ім'я:</strong>
                    </Col>
                    <Col sm={9}>{user?.fullName || 'Не вказано'}</Col>
                </Row>
                <Row className="mb-3">
                    <Col sm={3}>
                        <strong>Email:</strong>
                    </Col>
                    <Col sm={9}>{user?.email || 'Не вказано'}</Col>
                </Row>
                <Row>
                    <Col sm={3}>
                        <strong>ID користувача:</strong>
                    </Col>
                    <Col sm={9}>
                        <code className="text-muted">{user?.userId}</code>
                    </Col>
                </Row>
            </Card.Body>
        </Card>
    ), [user]);

    const renderSecurityTabContent = useMemo(() => (
        <Card>
            <Card.Header>
                <h5 className="mb-0">Налаштування безпеки</h5>
            </Card.Header>
            <Card.Body>
                <p>Тут будуть налаштування безпеки, такі як зміна пароля, двофакторна автентифікація тощо.</p>
            </Card.Body>
        </Card>
    ), []);

    const renderOrdersTabContent = useMemo(() => (
        <Card>
            <Card.Header>
                <h5 className="mb-0">Мої замовлення</h5>
            </Card.Header>
            <Card.Body>
                <div className="text-center py-5">
                    <i className="bi bi-bag-x display-1 text-muted"></i>
                    <p className="mt-3 text-muted">У вас поки немає замовлень</p>
                    <Button variant="primary" href={ROUTES.PRODUCTS}>
                        Почати покупки
                    </Button>
                </div>
            </Card.Body>
        </Card>
    ), []);

    const renderNotificationsTabContent = useMemo(() => (
        <Card>
            <Card.Header>
                <h5 className="mb-0">Сповіщення</h5>
            </Card.Header>
            <Card.Body>
                <p>Тут будуть налаштування сповіщень.</p>
            </Card.Body>
        </Card>
    ), []);

    const renderSettingsTabContent = useMemo(() => (
         <Card>
            <Card.Header>
                <h5 className="mb-0">Налаштування профілю</h5>
            </Card.Header>
            <Card.Body>
                {updateSuccess && (
                    <Alert variant="success" dismissible onClose={() => setUpdateSuccess(false)}>
                        Профіль успішно оновлено!
                    </Alert>
                )}
                <Form>
                    <Form.Group className="mb-3">
                        <Form.Label>Ім'я користувача</Form.Label>
                        <Form.Control 
                            type="text" 
                            defaultValue={user?.userName || ''} 
                            disabled
                        />
                        <Form.Text className="text-muted">
                            Ім'я користувача не можна змінити
                        </Form.Text>
                    </Form.Group>

                    <Form.Group className="mb-3">
                        <Form.Label>Повне ім'я</Form.Label>
                        <Form.Control 
                            type="text" 
                            defaultValue={user?.fullName || ''} 
                            placeholder="Введіть повне ім'я"
                        />
                    </Form.Group>

                    <Form.Group className="mb-3">
                        <Form.Label>Email</Form.Label>
                        <Form.Control 
                            type="email" 
                            defaultValue={user?.email || ''} 
                            placeholder="Введіть email"
                        />
                    </Form.Group>

                    <hr />

                    <h6 className="mb-3">Зміна пароля</h6>
                    
                    <Form.Group className="mb-3">
                        <Form.Label>Поточний пароль</Form.Label>
                        <Form.Control type="password" placeholder="Введіть поточний пароль" />
                    </Form.Group>

                    <Form.Group className="mb-3">
                        <Form.Label>Новий пароль</Form.Label>
                        <Form.Control type="password" placeholder="Введіть новий пароль" />
                    </Form.Group>

                    <Form.Group className="mb-3">
                        <Form.Label>Підтвердіть новий пароль</Form.Label>
                        <Form.Control type="password" placeholder="Підтвердіть новий пароль" />
                    </Form.Group>

                    <div className="d-flex gap-2">
                        <Button variant="primary" type="submit">
                            Зберегти зміни
                        </Button>
                        <Button variant="outline-secondary" type="reset">
                            Скасувати
                        </Button>
                    </div>
                </Form>
            </Card.Body>
        </Card>
    ), [user, updateSuccess]);

    // Redirect to home if not authenticated
    if (!loading && !isAuthenticated) {
        return <Navigate to={ROUTES.HOME} replace />;
    }

    // Show loading spinner while checking auth status
    if (loading) {
        return (
            <Container className="py-5">
                <div className="text-center">
                    <div className="spinner-border text-primary" role="status">
                        <span className="visually-hidden">Завантаження...</span>
                    </div>
                </div>
            </Container>
        );
    }

    return (
        <Container className="py-4">
            <Row>
                <Col md={3} className="mb-4">
                    <Card>
                        <Card.Body>
                            <div className="text-center mb-3">
                                <div className="bg-secondary text-white rounded-circle d-inline-flex align-items-center justify-content-center" style={{ width: '80px', height: '80px', fontSize: '32px' }}>
                                    <i className="bi bi-person-fill"></i>
                                </div>
                                <h5 className="mt-2 mb-0">{user?.userName}</h5>
                                <small className="text-muted">{user?.email}</small>
                            </div>
                            <hr />
                            {profileMenu}
                        </Card.Body>
                    </Card>
                </Col>

                <Col md={9}>
                    {activeTab === 'Загальне' && renderOverviewTabContent}
                    {activeTab === 'Безпека' && renderSecurityTabContent}
                    {activeTab === 'Мої замовлення' && renderOrdersTabContent}
                    {activeTab === 'Сповіщення' && renderNotificationsTabContent}
                    {activeTab === 'Налаштування' && renderSettingsTabContent}
                </Col>
            </Row>
        </Container>
    );
}