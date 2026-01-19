import { apiClient } from './ApiClient';
import { API_ENDPOINTS } from '../config/api';
import type { AuthResponse } from '../types/Responces/AuthResponse';
import type { SignInRequest } from '../types/Auth/SignInRequest';
import type { SignUpRequest } from '../types/Auth/SignUpRequest';
import type { CurrentUser } from '../types/Auth/CurrentUser';
import type { AuthStatus } from '../types/Auth/AuthStatus';
import type { RefreshTokenRequest } from '../types/Auth/RefreshTokenRequest';

export class AuthService {

    async signIn(credentials: SignInRequest): Promise<AuthResponse> {
        return apiClient.post<AuthResponse>(API_ENDPOINTS.AUTH.SIGN_IN, credentials);
    }

    async signUp(data: SignUpRequest): Promise<AuthResponse> {
        return apiClient.post<AuthResponse>(API_ENDPOINTS.AUTH.SIGN_UP, data);
    }

    async signOut(): Promise<void> {
        await apiClient.post<void>(API_ENDPOINTS.AUTH.SIGN_OUT);
    }

    async refreshToken(token: RefreshTokenRequest): Promise<AuthResponse> {
        return apiClient.post<AuthResponse>(API_ENDPOINTS.AUTH.REFRESH, { token });
    }

    async getCurrentUser(): Promise<CurrentUser> {
        return apiClient.get<CurrentUser>(API_ENDPOINTS.PROFILE.HOME);
    }

    async checkAuth(): Promise<AuthStatus> {
        return apiClient.get<AuthStatus>(API_ENDPOINTS.AUTH.CHECK);
    }
}

export const authService = new AuthService();