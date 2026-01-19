export const getUrlParams = (searchParams: URLSearchParams) => {
    const params: Record<string, string> = {};

    for (const [key, value] of searchParams.entries()) {
        params[key] = value;
    }

    return params;
};

export const buildUrlWithParams = (baseUrl: string, params: Record<string, string>): string => {
    const url = new URL(baseUrl, window.location.origin);

    Object.entries(params).forEach(([key, value]) => {
        if (value !== undefined && value !== null) {
            url.searchParams.set(key, value.toString());
        }
    });

    return url.pathname + url.search;
};