export interface User {
    id: string;
    fullName: string;
    email: string;
    userName: string | null;
    roles: string[] | null;
}