import type { SignInRequest } from "../../types/Auth/SignInRequest";
import type { SignUpRequest } from "../../types/Auth/SignUpRequest";
import type { AuthTokenResponse } from "../../types/Responces/AuthTokenResponse";
import type { UseAuthReturns } from "./UseAuthReturns";
import { useAuthContext } from "../../context/AuthContext";
import { authService } from "../../services/AuthService";
import { useCallback, useState } from "react"

export const useAuth = (): UseAuthReturns => {
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const { checkAuthStatus, logOut } = useAuthContext();

    const signIn = useCallback(async (credentials: SignInRequest): Promise<AuthTokenResponse> => {
        setLoading(true);
        setError(null);
        try {
            const response = await authService.signIn(credentials);

            console.log('Sign-in successful, token received');

            await checkAuthStatus();
            return response;
        } catch (err: any) {
            const errorMessage = err?.message || 'Sign-in failed';
            setError(errorMessage);
            throw err;
        } finally {
            setLoading(false);
        }
    }, [checkAuthStatus]);

    const signUp = useCallback(async (data: SignUpRequest): Promise<AuthTokenResponse> => {
        setLoading(true);
        setError(null);
        try {
            const response = await authService.signUp(data);

            console.log('Sign-in successful, token received');

            await checkAuthStatus();
            return response;
        } catch (err: any) {
            const errorMessage = err?.message || 'Sign-up failed';
            setError(errorMessage);
            throw err;
        } finally {
            setLoading(false);
        }
    }, [checkAuthStatus]);

    const signOut = useCallback(async (): Promise<void> => {
        setLoading(true);
        setError(null);
        try {
            await authService.signOut();
            logOut(); // Clear auth context
        } catch (err: any) {
            const errorMessage = err?.message || 'Sign-out failed';
            setError(errorMessage);
            throw err;
        } finally {
            setLoading(false);
        }
    }, [logOut]);

    return {
        signIn,
        signUp,
        signOut,
        loading,
        error
    };
}