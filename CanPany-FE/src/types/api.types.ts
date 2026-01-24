// API Response Types
export interface ApiResponse<T = unknown> {
    succeeded: boolean;
    message?: string;
    errorCode?: string;
    data?: T;
}

export interface PaginatedResponse<T> {
    items: T[];
    total: number;
    page: number;
    pageSize: number;
    totalPages: number;
}
