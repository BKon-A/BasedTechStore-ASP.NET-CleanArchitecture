import { apiClient } from './ApiClient';
import { API_ENDPOINTS } from '../config/api';
import type { AuthTokenResponse } from '../types/Responces/AuthTokenResponse';
import type { SignInRequest } from '../types/Auth/SignInRequest';
import type { SignUpRequest } from '../types/Auth/SignUpRequest';
import type { CurrentUserResponse } from '../types/Responces/CurrentUserResponse';
import type { AuthStatusResponse } from '../types/Responces/AuthStatusResponse';

export class AuthService {

    async signIn(credentials: SignInRequest): Promise<AuthTokenResponse> {
        const response = await apiClient.post<AuthTokenResponse>(
            API_ENDPOINTS.AUTH.SIGN_IN,
            credentials
        );

        apiClient.setToken(response.token);

        return response;
    }

    async signUp(data: SignUpRequest): Promise<AuthTokenResponse> {
        const response = await apiClient.post<AuthTokenResponse>(
            API_ENDPOINTS.AUTH.SIGN_UP,
            data
        );

        apiClient.setToken(response.token);

        return response;
    }

    async signOut(): Promise<void> {
        try {
            await apiClient.post<void>(API_ENDPOINTS.AUTH.SIGN_OUT);
        } finally {
            apiClient.setToken(null);
        }
    }

    async refreshToken(token: string): Promise<AuthTokenResponse> {
        const response = await apiClient.post<AuthTokenResponse>(
            API_ENDPOINTS.AUTH.REFRESH,
            { token }
        );

        apiClient.setToken(response.token);

        return response;
    }

    async getCurrentUser(): Promise<CurrentUserResponse> {
        return apiClient.get<CurrentUserResponse>(API_ENDPOINTS.AUTH.GET_CURRENT_USER);
    }

    async checkAuth(): Promise<AuthStatusResponse> {
        return apiClient.get<AuthStatusResponse>(API_ENDPOINTS.AUTH.CHECK);
    }

    getStoredToken(): string | null {
        return apiClient.getToken();
    }

    isAuthenticated(): boolean {
        return !!this.getStoredToken();
    }
}

export const authService = new AuthService();