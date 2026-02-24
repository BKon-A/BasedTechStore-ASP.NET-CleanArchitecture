import type { SignInRequest } from "../../types/Auth/SignInRequest";
import type { SignUpRequest } from "../../types/Auth/SignUpRequest";
import type { AuthResponse } from "../../types/Responces/AuthTokenResponse";

export interface UseAuthReturns {
    signIn: (credentials: SignInRequest) => Promise<AuthResponse>;
    signUp: (data: SignUpRequest) => Promise<AuthResponse>;
    loading: boolean;
    error: string | null;    
}