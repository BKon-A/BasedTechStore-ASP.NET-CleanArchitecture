export interface ApiResponse<T> {
    isSuccess: boolean;
    data?: T;
    message?: string;
    errors?: string[];
    statusCode?: number;
}
