import apiClient from './axios.config';
import type { ApiResponse } from '../types';

export interface JobAlertCreateDto {
    title: string;
    location?: string;
    jobType?: 'FullTime' | 'PartTime' | 'Freelance';
    minBudget?: number;
    maxBudget?: number;
    experienceLevel?: string;
    skillIds?: string[];
    categoryIds?: string[];
    frequency: 'Immediate' | 'Daily' | 'Weekly';
    emailEnabled: boolean;
    inAppEnabled: boolean;
}

export interface JobAlertUpdateDto {
    title?: string;
    location?: string;
    jobType?: string;
    minBudget?: number;
    maxBudget?: number;
    experienceLevel?: string;
    skillIds?: string[];
    categoryIds?: string[];
    frequency?: string;
    emailEnabled?: boolean;
    inAppEnabled?: boolean;
}

export interface JobAlertResponse {
    id: string;
    userId: string;
    title?: string;
    location?: string;
    jobType?: string;
    minBudget?: number;
    maxBudget?: number;
    experienceLevel?: string;
    skillIds?: string[];
    categoryIds?: string[];
    frequency: string;
    emailEnabled: boolean;
    inAppEnabled: boolean;
    isActive: boolean;
    matchCount: number;
    lastTriggeredAt?: string;
    createdAt: string;
}

export interface JobMatchPreview {
    jobId: string;
    jobTitle: string;
    companyName: string;
    location: string;
    budget: string;
    matchScore: number;
}

export interface JobAlertStats {
    totalAlerts: number;
    activeAlerts: number;
    totalMatches: number;
    recentMatches: number;
}

export const jobAlertsApi = {
    create: async (dto: JobAlertCreateDto): Promise<JobAlertResponse> => {
        const response = await apiClient.post<ApiResponse<JobAlertResponse>>('/job-alerts', dto);
        return response.data.data!;
    },

    getAll: async (): Promise<JobAlertResponse[]> => {
        const response = await apiClient.get<ApiResponse<JobAlertResponse[]>>('/job-alerts');
        return response.data.data ?? [];
    },

    getById: async (id: string): Promise<JobAlertResponse> => {
        const response = await apiClient.get<ApiResponse<JobAlertResponse>>(`/job-alerts/${id}`);
        return response.data.data!;
    },

    update: async (id: string, dto: JobAlertUpdateDto): Promise<JobAlertResponse> => {
        const response = await apiClient.put<ApiResponse<JobAlertResponse>>(`/job-alerts/${id}`, dto);
        return response.data.data!;
    },

    delete: async (id: string): Promise<void> => {
        await apiClient.delete(`/job-alerts/${id}`);
    },

    pause: async (id: string): Promise<void> => {
        await apiClient.put(`/job-alerts/${id}/pause`);
    },

    resume: async (id: string): Promise<void> => {
        await apiClient.put(`/job-alerts/${id}/resume`);
    },

    preview: async (id: string): Promise<JobMatchPreview[]> => {
        const response = await apiClient.get<ApiResponse<JobMatchPreview[]>>(`/job-alerts/${id}/preview`);
        return response.data.data ?? [];
    },

    getStats: async (): Promise<JobAlertStats> => {
        const response = await apiClient.get<ApiResponse<JobAlertStats>>('/job-alerts/stats');
        return response.data.data!;
    },
};
