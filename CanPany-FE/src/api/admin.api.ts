import apiClient from './axios.config';
import type { ApiResponse, User } from '../types';

export interface AdminDashboardStats {
    totalUsers: number;
    totalJobs: number;
    totalApplications: number;
    totalCompanies: number;
    pendingVerifications: number;
    pendingPayments: number;
    totalRevenue: number;
}

function toNumber(value: unknown, fallback = 0): number {
    const n = typeof value === 'number' ? value : Number(value);
    return Number.isFinite(n) ? n : fallback;
}

function normalizeDashboardStats(raw: any): AdminDashboardStats {
    return {
        totalUsers: toNumber(raw?.totalUsers ?? raw?.TotalUsers),
        totalJobs: toNumber(raw?.totalJobs ?? raw?.TotalJobs),
        totalApplications: toNumber(raw?.totalApplications ?? raw?.TotalApplications),
        totalCompanies: toNumber(raw?.totalCompanies ?? raw?.TotalCompanies),
        pendingVerifications: toNumber(raw?.pendingVerifications ?? raw?.PendingVerifications),
        pendingPayments: toNumber(raw?.pendingPayments ?? raw?.PendingPayments),
        totalRevenue: toNumber(raw?.totalRevenue ?? raw?.TotalRevenue),
    };
}

function normalizeUser(dto: any): User {
    return {
        id: dto?.id ?? dto?.Id ?? '',
        fullName: dto?.fullName ?? dto?.FullName ?? '',
        email: dto?.email ?? dto?.Email ?? '',
        role: (dto?.role ?? dto?.Role ?? 'Candidate') as User['role'],
        avatarUrl: dto?.avatarUrl ?? dto?.AvatarUrl,
        isLocked: dto?.isLocked ?? dto?.IsLocked ?? false,
        lockedUntil: dto?.lockedUntil ?? dto?.LockedUntil,
        createdAt: new Date(dto?.createdAt ?? dto?.CreatedAt ?? Date.now()),
        updatedAt: dto?.updatedAt ?? dto?.UpdatedAt ? new Date(dto.updatedAt ?? dto.UpdatedAt) : undefined,
    };
}

export const adminApi = {
    getDashboard: async (): Promise<AdminDashboardStats> => {
        const response = await apiClient.get<ApiResponse<any>>('/admin/dashboard');
        return normalizeDashboardStats(response.data.data ?? {});
    },

    getUsers: async (): Promise<User[]> => {
        const response = await apiClient.get<ApiResponse<any[]>>('/admin/users');
        const list = response.data.data ?? [];
        return list.map(normalizeUser);
    },

    banUser: async (id: string): Promise<void> => {
        await apiClient.put(`/admin/users/${id}/ban`);
    },

    unbanUser: async (id: string): Promise<void> => {
        await apiClient.put(`/admin/users/${id}/unban`);
    },

    /** BE hiện trả về danh sách rỗng (TODO); giữ API để nối sau. */
    getVerificationRequests: async (): Promise<unknown[]> => {
        const response = await apiClient.get<ApiResponse<unknown[]>>('/admin/companies/verification-requests');
        return response.data.data ?? [];
    },

    approveVerification: async (companyId: string): Promise<void> => {
        await apiClient.put(`/admin/companies/${companyId}/verify/approve`);
    },

    rejectVerification: async (companyId: string, reason: string): Promise<void> => {
        await apiClient.put(`/admin/companies/${companyId}/verify/reject`, { reason });
    },
};
