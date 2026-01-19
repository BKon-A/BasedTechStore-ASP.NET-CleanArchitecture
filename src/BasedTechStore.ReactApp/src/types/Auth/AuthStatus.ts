export interface AuthStatus {
    isAuthenticated: boolean;
    userName?: string;
    clams?: Array<{type: string; value: string;}>;
}