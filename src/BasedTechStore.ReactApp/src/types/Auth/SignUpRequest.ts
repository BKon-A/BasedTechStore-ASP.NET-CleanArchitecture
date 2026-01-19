export interface SignUpRequest {
    fullName: string;
    email: string;
    password: string;
    confirmPassword: string;
    userName?: string | null;
}