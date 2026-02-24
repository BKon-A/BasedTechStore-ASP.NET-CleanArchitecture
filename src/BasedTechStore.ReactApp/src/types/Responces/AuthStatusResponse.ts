export interface AuthStatusResponse {
    isAuthenticated: boolean;
    userId?: string;
    userName?: string;
    email?: string;
}