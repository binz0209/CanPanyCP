import apiClient from './axios.config';
import type { ApiResponse, Company, CompanySearchParams, PaginatedResponse } from '@/types';

interface CompaniesListResponse {
    companies: Company[];
    total: number;
    page: number;
    pageSize: number;
    totalPages: number;
}

export const companiesApi = {
    getAll: async (params?: CompanySearchParams): Promise<CompaniesListResponse> => {
        const response = await apiClient.get<ApiResponse<CompaniesListResponse>>('/companies', { params });
        return response.data.data!;
    },

    search: async (params?: CompanySearchParams): Promise<CompaniesListResponse> => {
        const response = await apiClient.get<ApiResponse<CompaniesListResponse>>('/companies/search', { params });
        return response.data.data!;
    },

    getById: async (id: string): Promise<Company> => {
        const response = await apiClient.get<ApiResponse<Company>>(`/companies/${id}`);
        return response.data.data!;
    },

    getJobs: async (id: string, status?: string): Promise<import('@/types').Job[]> => {
        const response = await apiClient.get<ApiResponse<import('@/types').Job[]>>(`/companies/${id}/jobs`, {
            params: status ? { status } : undefined,
        });
        return response.data.data || [];
    },
};
