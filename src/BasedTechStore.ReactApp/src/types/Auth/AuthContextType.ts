import type { CurrentUser } from "./CurrentUser";

export interface AuthContextType {
    user: CurrentUser | null;
    isAuthenticated: boolean;
    isManager: boolean;
    loading: boolean;
    checkAuthStatus: () => Promise<void>;
    logOut: () => Promise<void>;
}