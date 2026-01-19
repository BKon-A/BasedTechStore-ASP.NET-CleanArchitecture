import { createContext, useState, useContext, useCallback, useEffect, useMemo, type ReactNode } from 'react';
import type { AuthContextType } from '../types/Auth/AuthContextType';
import type { AuthStatus } from '../types/Auth/AuthStatus';
import { authService } from '../services/AuthService';

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider = ({ children }: { children: ReactNode }) => {
    const [user, setUser] = useState<AuthContextType['user']>(null);
    const [isAuthenticated, setIsAuthenticated] = useState<boolean>(false);
    const [isManager, setIsManager] = useState<boolean>(false);
    const [loading, setLoading] = useState<boolean>(false);

    const checkAuthStatus = useCallback(async () => {
        try {
            setLoading(true);

            const status: AuthStatus = await authService.checkAuth();

            console.log('Auth status:', status);

            setIsManager(status.clams?.some(clam => clam.type === 'role' && clam.value === 'manager') || false);

            if (status.isAuthenticated) {
                const userData = await authService.getCurrentUser();
                setUser(userData);
                setIsAuthenticated(true);
            } else {
                setUser(null);
                setIsAuthenticated(false);
            }

        } catch (error) {
            console.error('Error checking auth status:', error);
            setUser(null);
            setIsAuthenticated(false);
        } finally {
            setLoading(false);
        }
    }, []);

    const logOut = useCallback(async () => {
        try {
            await authService.signOut();
            setUser(null);
            setIsAuthenticated(false);
        } catch (error) {
            console.error('Error signing out:', error);
        }
    }, []);

    useEffect(() => {
        checkAuthStatus();
    }, [checkAuthStatus]);

    const value = useMemo(
        () => ({
            user,
            isAuthenticated,
            isManager,
            loading,
            checkAuthStatus,
            logOut,
        }),
        [user, isAuthenticated, isManager, loading, checkAuthStatus, logOut]
    );

    return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export const useAuthContext = () => {
    const context = useContext(AuthContext);
    if (context === undefined) {
        throw new Error('useAuthContext must be used within an AuthProvider');
    }
    return context;
}
