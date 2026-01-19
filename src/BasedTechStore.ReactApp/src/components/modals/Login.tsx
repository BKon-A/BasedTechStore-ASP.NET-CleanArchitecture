import { useState } from 'react';
import { Modal, Button, Form, InputGroup, Alert, Spinner } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import { Eye, EyeSlash } from 'react-bootstrap-icons';
import { useAuth } from '../../hooks/Auth/useAuth';

interface LoginProps {
    show: boolean;
    onHide: () => void;
}

export const Login = ({ show, onHide }: LoginProps) => {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [showPassword, setShowPassword] = useState(false);
    const [validationErrors, setValidationErrors] = useState<string[]>([]);

    const navigate = useNavigate();
    const { signIn, loading, error } = useAuth();

    const validateForm = (): boolean => {
        const errors: string[] = [];

        if (!email) {
            errors.push('Email is required.');
        } else if (!/\S+@\S+\.\S+/.test(email)) {
            errors.push('Email is invalid.');
        }
        if (!password) {
            errors.push('Password is required.');
        } else if (password.length < 6) {
            errors.push('Password must be at least 6 characters long.');
        }
        setValidationErrors(errors);
        return errors.length === 0;
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setValidationErrors([]);

        if (!validateForm()) {
            return;
        }

        try {
            await signIn({ email, password });
            handleClose();
            navigate('/');
        } catch (error) {
            console.error('Login error:', error);
        }
    };

    const handleClose = () => {
        setEmail('');
        setPassword('');
        setShowPassword(false);
        setValidationErrors([]);
        onHide();
    };

    return (
        <Modal show={show} onHide={handleClose} centered>
            <Modal.Header closeButton>
                <Modal.Title>Вхід до акаунту</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                {(error && validationErrors.length > 0) && (
                    <Alert variant="danger">
                        {error}
                        {validationErrors.length > 0 && (
                            <ul className="mb-0">
                                {validationErrors.map((err, idx) => (
                                    <li key={idx}>{err}</li>
                                ))}
                            </ul>
                        )}
                    </Alert>
                )}
                <Form onSubmit={handleSubmit}>
                    <Form.Group className="mb-3" controlId="signInEmail">
                        <Form.Label>Email</Form.Label>
                        <Form.Control
                            type="email"
                            placeholder="Введіть email"
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                            disabled={loading}
                            isInvalid={validationErrors.some(e => e.includes('Email'))}
                        />
                    </Form.Group>

                    <Form.Group className="mb-3" controlId="signInPassword">
                        <Form.Label>Пароль</Form.Label>
                        <InputGroup>
                            <Form.Control
                                type={showPassword ? 'text' : 'password'}
                                placeholder="Введіть пароль"
                                value={password}
                                onChange={(e) => setPassword(e.target.value)}
                                disabled={loading}
                                isInvalid={validationErrors.some(e => e.includes('Password'))}
                            />
                            <Button
                                variant="outline-secondary"
                                type="button"
                                onClick={() => setShowPassword(!showPassword)}
                                disabled={loading}
                            >
                                {showPassword ? <EyeSlash /> : <Eye />}
                            </Button>
                        </InputGroup>
                    </Form.Group>

                    <Button variant="primary" type="submit" className="w-100" disabled={loading}>
                        {loading ? (
                            <>
                                <Spinner as="span" animation="border" size="sm" role="status" aria-hidden="true" className="me-2">
                                    Завантаження...
                                </Spinner>
                            </>
                        ) : ('Увійти')}
                    </Button>
                </Form>
            </Modal.Body>
        </Modal>
    );
};