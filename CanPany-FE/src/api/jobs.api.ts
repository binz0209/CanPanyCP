import apiClient from './axios.config';
import type { ApiResponse, Job, JobSearchParams } from '@/types';

export const jobsApi = {
    search: async (params?: JobSearchParams): Promise<Job[]> => {
        const response = await apiClient.get<ApiResponse<Job[]>>('/jobs', { params });
        return response.data.data || [];
    },

    getById: async (id: string): Promise<{ job: Job; isBookmarked: boolean }> => {
        const response = await apiClient.get<ApiResponse<{ job: Job; isBookmarked: boolean }>>(`/jobs/${id}`);
        return response.data.data!;
    },

    getRecommended: async (limit = 10): Promise<Job[]> => {
        const response = await apiClient.get<ApiResponse<Job[]>>('/jobs/recommended', { params: { limit } });
        return response.data.data || [];
    },

    bookmark: async (id: string): Promise<void> => {
        await apiClient.post(`/jobs/${id}/bookmark`);
    },

    removeBookmark: async (id: string): Promise<void> => {
        await apiClient.delete(`/jobs/${id}/bookmark`);
    },

    getBookmarked: async (): Promise<Job[]> => {
        const response = await apiClient.get<ApiResponse<Job[]>>('/jobs/bookmarked');
        return response.data.data || [];
    },

    getByCompany: async (companyId: string): Promise<Job[]> => {
        const response = await apiClient.get<ApiResponse<Job[]>>(`/jobs/company/${companyId}`);
        return response.data.data || [];
    },
};
