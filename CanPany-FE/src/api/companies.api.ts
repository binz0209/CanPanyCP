import apiClient from './axios.config';
import type {
    ApiResponse,
    Company,
    CompanySearchParams,
    CompanyStatistics,
    CompanyVerificationInfo,
    CreateCompanyRequest,
    Job,
    UpdateCompanyRequest,
    VerificationRequest,
} from '../types';

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

    getMe: async (): Promise<Company> => {
        const response = await apiClient.get<ApiResponse<Company>>('/companies/me');
        return response.data.data!;
    },

    create: async (payload: CreateCompanyRequest): Promise<Company> => {
        const response = await apiClient.post<ApiResponse<Company>>('/companies', payload);
        return response.data.data!;
    },

    updateMe: async (payload: UpdateCompanyRequest): Promise<void> => {
        await apiClient.put('/companies/me', payload);
    },

    requestVerification: async (payload: VerificationRequest): Promise<void> => {
        await apiClient.post('/companies/verification', payload);
    },

    uploadVerificationDocuments: async (files: File[]): Promise<string[]> => {
        const formData = new FormData();
        files.forEach((file) => formData.append('files', file));
        const response = await apiClient.post<ApiResponse<{ urls: string[] }>>('/companies/verification/upload-document', formData, {
            headers: { 'Content-Type': 'multipart/form-data' },
        });
        return response.data.data?.urls ?? [];
    },

    getVerificationDocumentDownloadUrl: async (url: string): Promise<{ url: string; fileName: string }> => {
        const response = await apiClient.get<ApiResponse<{ url: string; fileName: string }>>('/companies/verification/download-document', {
            params: { url },
        });
        return response.data.data!;
    },

    getVerificationStatus: async (companyId: string): Promise<CompanyVerificationInfo> => {
        const response = await apiClient.get<ApiResponse<CompanyVerificationInfo>>(`/companies/${companyId}/verification`);
        return response.data.data!;
    },

    getStatistics: async (companyId: string): Promise<CompanyStatistics> => {
        const response = await apiClient.get<ApiResponse<CompanyStatistics>>(`/companies/${companyId}/statistics`);
        return response.data.data!;
    },

    getJobs: async (id: string, status?: string): Promise<Job[]> => {
        const response = await apiClient.get<ApiResponse<Job[]>>(`/companies/${id}/jobs`, {
            params: status ? { status } : undefined,
        });
        return response.data.data || [];
    },
};
