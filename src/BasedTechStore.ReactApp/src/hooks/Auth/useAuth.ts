import type { SignInRequest } from "../../types/Auth/SignInRequest";
import type { SignUpRequest } from "../../types/Auth/SignUpRequest";
import type { AuthResponse } from "../../types/Responces/AuthResponse";
import type { UseAuthReturns } from "./UseAuthReturns";
import { useAuthContext } from "../../context/AuthContext";
import { authService } from "../../services/AuthService";
import { useCallback, useState } from "react"

export const useAuth = (): UseAuthReturns => {
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const { checkAuthStatus } = useAuthContext();

    const signIn = useCallback(async (credentials: SignInRequest): Promise<AuthResponse> => {
        setLoading(true);
        setError(null);
        try {
            const response = await authService.signIn(credentials);

            console.log('Sign-in response:', response);

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

    const signUp = useCallback(async (data: SignUpRequest): Promise<AuthResponse> => {
        setLoading(true);
        setError(null);
        try {
            const response = await authService.signUp(data);

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

    return {
        signIn,
        signUp,
        loading,
        error
    };
}