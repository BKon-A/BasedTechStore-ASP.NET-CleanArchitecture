import { API_CONFIG } from '../config/api';
import type { ApiResponse } from '../types/Responces/ApiResponse';

type QueryParamValue = string | number | boolean | string[] | number[] | boolean[];
type QueryParams = Record<string, QueryParamValue | undefined | null>;

export class ApiClient {
    private baseUrl: string;
    private token: string | null = null;
    private isRefreshing: boolean = false;
    private refreshSubscribers: ((token: string) => void)[] = [];

    constructor() {
        this.baseUrl = API_CONFIG.BASE_URL || 'https://localhost:7250/api';
        this.token = sessionStorage.getItem('accessToken');
    }

    setToken(token: string | null) {
        this.token = token;
        if (token) {
            sessionStorage.setItem('accessToken', token);
        } else {
            sessionStorage.removeItem('accessToken')
        }
    }

    getToken(): string | null {
        return this.token;
    }

    private getHeaders(): HeadersInit {
        const headers: HeadersInit = {
            'Content-Type': 'application/json'
        };

        if (this.token) {
            headers['Authorization'] = `Bearer ${this.token}`;
        }

        return headers;
    }

    private async refreshToken(): Promise<string> {
        const response = await fetch(`${this.baseUrl}/auth/refresh`, {
            method: 'POST',
            credentials: 'include',
            headers: {
                'Content-Type': 'application/json'
            }
        })

        if (!response.ok) {
            throw new Error('Token refreshing failed');
        }

        const data = await response.json();
        const apiResponse = data as ApiResponse<{ token: string; tokenType: string; expiresIn: number }>;

        if (apiResponse.isSuccess && apiResponse.data) {
            return apiResponse.data.token;
        }

        throw new Error('Invalid refresh response');
    }

    private subscribeTokenRefresh(callback: (token: string) => void) {
        this.refreshSubscribers.push(callback);
    }

    private onRefreshed(token: string) {
        this.refreshSubscribers.forEach(callback => callback(token));
        this.refreshSubscribers = [];
    }

    async request<T>(endpoint: string, options: RequestInit = {}): Promise<T> {
        const url = `${this.baseUrl}${endpoint}`;

        const config: RequestInit = {
            ...options,
            headers: this.getHeaders(),
            credentials: 'include'
        };

        try {
            console.log(`Request to: ${url}`, {
                method: config.method || 'GET',
                hasToken: !!this.token
            })

            const response = await fetch(url, config);

            // token expired or invalid
            if (response.status === 401 && !endpoint.includes('/auth/')) {
                const tokenExpired = response.headers.get('Token-Expired');

                if (tokenExpired === 'true' && !this.isRefreshing) {
                    this.isRefreshing = true;

                    try {
                        const newToken = await this.refreshToken();
                        this.setToken(newToken);
                        this.isRefreshing = false;
                        this.onRefreshed(newToken);

                        config.headers = {
                            ...config.headers,
                            'Authorization': `Bearer ${newToken}`
                        };

                        return this.request<T>(endpoint, config);
                    } catch (refreshError) {
                        this.isRefreshing = false;
                        this.setToken(null);

                        window.dispatchEvent(new CustomEvent('auth:logout'));
                        throw new Error('Session expired. Please sign in again.');
                    }
                } else if (this.isRefreshing) {
                    // Wait for token refresh to complete
                    return new Promise<T>((resolve, reject) => {
                        this.subscribeTokenRefresh((token: string) => {
                            config.headers = {
                                ...config.headers,
                                'Authorization': `Bearer ${token}`
                            };
                            this.request<T>(endpoint, config).then(resolve).catch(reject);
                        });
                    });
                }

                throw new Error('Authentication required. Please sign in again.');
            }

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json();
            console.log(`Response from ${url}:`, data);

            // ApiResponse handling
            if (data && typeof data === 'object' && 'isSuccess' in data && 'data' in data) {
                const apiResponse = data as ApiResponse<T>;
                if (apiResponse.isSuccess) {
                    if (apiResponse.data === undefined) {
                        throw new Error('API response data is undefined');
                    }
                    return apiResponse.data;
                } else {
                    throw new Error(apiResponse.message || 'API request failed');
                }
            }

            return data;
        } catch (error) {
            console.error(`Error fetching ${url}:`, error);
            throw error;
        }
    }

    async get<T>(endpoint: string, params?: QueryParams): Promise<T> {
        let url = endpoint;

        if (params) {
            const searchParams = new URLSearchParams();
            Object.entries(params).forEach(([key, value]) => {
                if (value !== undefined && value !== null) {
                    if (Array.isArray(value)) {
                        value.forEach((item) => searchParams.append(key, item.toString()));
                    } else {
                        searchParams.append(key, value.toString());
                    }
                }
            });

            if (searchParams.toString()) {
                url += `?${searchParams.toString()}`;
            }
        }

        return this.request<T>(url);
    }

    async post<T, TBody = unknown>(endpoint: string, data?: TBody): Promise<T> {
        return this.request<T>(endpoint, {
            method: 'POST',
            body: JSON.stringify(data),
        });
    }

    async put<T, TBody = unknown>(endpoint: string, data?: TBody): Promise<T> {
        return this.request<T>(endpoint, {
            method: 'PUT',
            body: JSON.stringify(data),
        });
    }

    async delete<T>(endpoint: string): Promise<T> {
        return this.request<T>(endpoint, {
            method: 'DELETE',
        });
    }
}

export const apiClient = new ApiClient();