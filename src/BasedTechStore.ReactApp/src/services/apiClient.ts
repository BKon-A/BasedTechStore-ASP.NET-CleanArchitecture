import { API_CONFIG } from '../config/api';
import type { ApiResponse } from '../types/Responces/ApiResponse';

type QueryParamValue = string | number | boolean | string[] | number[] | boolean[];
type QueryParams = Record<string, QueryParamValue | undefined | null>;

export class ApiClient {
    private baseUrl: string;

    constructor() {
        this.baseUrl = API_CONFIG.BASE_URL || 'https://localhost:7250/api';
    }

    async request<T>(endpoint: string, options: RequestInit = {}): Promise<T> {
        const url = `${this.baseUrl}${endpoint}`;

        const config: RequestInit = {
            headers: {
                'Content-Type': 'application/json',
                ...options.headers,
            },
            ...options,
        };

        try {
            console.log(`Request to: ${url}`)

            const response = await fetch(url, config);

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
                    //console.log(`ApiResponse call successful: ${apiResponse.data}`);
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