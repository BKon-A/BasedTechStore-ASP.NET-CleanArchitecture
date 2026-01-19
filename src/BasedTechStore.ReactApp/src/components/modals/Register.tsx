import { useState } from 'react';
import { Modal, Button, Form, InputGroup, Alert, Spinner } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import { Eye, EyeSlash } from 'react-bootstrap-icons';
import { useAuth } from '../../hooks/Auth/useAuth';
import { ROUTES } from '../../constants/routes';

interface RegisterProps {
    show: boolean;
    onHide: () => void;
}

export const Register = ({ show, onHide }: RegisterProps) => {
    const [fullName, setFullName] = useState('');
    const [userName, setUserName] = useState('');
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [showPassword, setShowPassword] = useState(false);
    const [showConfirmPassword, setShowConfirmPassword] = useState(false);
    const [validationErrors, setValidationErrors] = useState<string[]>([]);

    const navigate = useNavigate();
    const { signUp, loading, error } = useAuth();

    const validateForm = (): boolean => {
        const errors: string[] = [];

        if (!fullName) {
            errors.push('Full name is required.');
        } else if (!/^[a-zA-Z\s]+$/.test(fullName)) {
            errors.push('Full name can only contain letters and spaces.');
        }
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
        if (!confirmPassword) {
            errors.push('Password confirmation is required.');
        } else if (password !== confirmPassword) {
            errors.push('Passwords do not match.');
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
            await signUp({ fullName, email, password, userName });
            handleClose();
            navigate(ROUTES.HOME);
        } catch (error) {
            console.error('Registration error:', error);
        }
    };

    const handleClose = () => {
        setUserName('');
        setFullName('');
        setEmail('');
        setPassword('');
        setConfirmPassword('');
        setShowPassword(false);
        setValidationErrors([]);
        onHide();
    };

    return (
        <Modal show={show} onHide={handleClose} centered>
            <Modal.Header closeButton>
                <Modal.Title>Реєстрація</Modal.Title>
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
                    <Form.Group className="mb-3" controlId="signUpFullName">
                        <Form.Label>ПІБ</Form.Label>
                        <Form.Control
                            type="text"
                            placeholder="Введіть повне ім'я"
                            value={fullName}
                            onChange={(e) => setFullName(e.target.value)}
                            disabled={loading}
                            isInvalid={validationErrors.some(e => e.includes('Full name'))}
                        />
                    </Form.Group>
                    <Form.Group className="mb-3" controlId="signUpEmail">
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
                    <Form.Group className="mb-3" controlId="signUpPassword">
                        <Form.Label>Пароль</Form.Label>
                        <InputGroup>
                            <Form.Control
                                type={showPassword ? 'text' : 'password'}
                                placeholder="Введіть пароль"
                                value={password}
                                onChange={(e) => setPassword(e.target.value)}
                                disabled={loading}
                                isInvalid={validationErrors.some(e => e.includes('Password') && !e.includes('confirmation'))}
                            />
                            <Button
                                variant="outline-secondary"
                                type="button"
                                onClick={() => setShowPassword(!showPassword)}
                            >
                                {showPassword ? <EyeSlash /> : <Eye />}
                            </Button>
                        </InputGroup>
                    </Form.Group>
                    <Form.Group className="mb-3" controlId="signUpConfirmPassword">
                        <Form.Label>Підтвердження пароля</Form.Label>
                        <InputGroup>
                            <Form.Control
                                type={showConfirmPassword ? 'text' : 'password'}
                                placeholder="Підтвердіть пароль"
                                value={confirmPassword}
                                onChange={(e) => setConfirmPassword(e.target.value)}
                                disabled={loading}
                                isInvalid={validationErrors.some(e => e.includes('match') && !e.includes('confirmation'))}
                            />
                            <Button
                                variant="outline-secondary"
                                type="button"
                                onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                            >
                                {showConfirmPassword ? <EyeSlash /> : <Eye />}
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
                        ) : ('Зареєструватися')}
                    </Button>
                </Form>
            </Modal.Body>
        </Modal>
    );
};