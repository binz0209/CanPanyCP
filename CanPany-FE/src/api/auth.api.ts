import apiClient from './axios.config';
import type { ApiResponse, AuthResponse, LoginRequest, RegisterRequest, ForgotPasswordRequest, ResetPasswordRequest } from '@/types';

export const authApi = {
    login: async (data: LoginRequest): Promise<AuthResponse> => {
        const response = await apiClient.post<ApiResponse<AuthResponse>>('/auth/login', data);
        return response.data.data!;
    },

    register: async (data: RegisterRequest): Promise<AuthResponse> => {
        const response = await apiClient.post<ApiResponse<AuthResponse>>('/auth/register', data);
        return response.data.data!;
    },

    logout: async (): Promise<void> => {
        await apiClient.post('/auth/logout');
    },

    changePassword: async (oldPassword: string, newPassword: string): Promise<void> => {
        await apiClient.post('/auth/change-password', { oldPassword, newPassword });
    },

    forgotPassword: async (data: ForgotPasswordRequest): Promise<{ resetCode: string }> => {
        const response = await apiClient.post<ApiResponse<{ resetCode: string }>>('/auth/forgot-password', data);
        return response.data.data!;
    },

    resetPassword: async (data: ResetPasswordRequest): Promise<void> => {
        await apiClient.post('/auth/reset-password', data);
    },
};
