import apiClient from './axios.config';
import type { ApiResponse } from '../types';

export type CreateReportPayload = {
    reportedUserId?: string;
    reportedCompanyId?: string;
    reportedJobId?: string;
    reason: string;
    description: string;
    evidence?: string[];
};

export const reportsApi = {
    createReport: async (payload: CreateReportPayload): Promise<void> => {
        await apiClient.post<ApiResponse<unknown>>('/reports', payload);
    },
};

