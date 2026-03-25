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

export interface AdminAuditLog {
    id: string;
    userId?: string;
    userEmail?: string;
    action: string;
    entityType?: string;
    entityId?: string;
    endpoint: string;
    httpMethod: string;
    requestPath: string;
    queryString?: string | null;
    requestBody?: string | null;
    responseStatusCode?: number | null;
    responseBody?: string | null;
    ipAddress?: string | null;
    userAgent?: string | null;
    duration?: number | null;
    errorMessage?: string | null;
    createdAt: Date;
}

export interface AdminUserBasicInfo {
    id: string;
    fullName: string;
    email: string;
    avatarUrl?: string | null;
}

export interface AdminReportDetails {
    id: string;
    reporter: AdminUserBasicInfo;
    reportedUser?: AdminUserBasicInfo | null;
    reason: string;
    description: string;
    evidence?: string[] | null;
    status: string;
    resolutionNote?: string | null;
    createdAt: Date;
    resolvedAt?: Date | null;
}

export interface AdminPaymentLike {
    // BE chưa expose schema ổn định cho /admin/payments (TODO).
    // FE dùng "any-ish" để render/approve/reject theo id.
    id: string;
    [key: string]: unknown;
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

    // ===== UC-46: Job moderation =====
    hideJob: async (jobId: string, reason: string): Promise<void> => {
        await apiClient.put(`/admin/jobs/${jobId}/hide`, { reason });
    },

    deleteJob: async (jobId: string): Promise<void> => {
        await apiClient.delete(`/admin/jobs/${jobId}`);
    },

    // ===== UC-47: Catalog CRUD =====
    createCategory: async (name: string): Promise<void> => {
        await apiClient.post(`/admin/categories`, { name });
    },

    updateCategory: async (id: string, name: string): Promise<void> => {
        await apiClient.put(`/admin/categories/${id}`, { name });
    },

    deleteCategory: async (id: string): Promise<void> => {
        await apiClient.delete(`/admin/categories/${id}`);
    },

    createSkill: async (name: string, categoryId?: string): Promise<void> => {
        await apiClient.post(`/admin/skills`, { name, categoryId });
    },

    updateSkill: async (id: string, name: string, categoryId?: string): Promise<void> => {
        await apiClient.put(`/admin/skills/${id}`, { name, categoryId });
    },

    deleteSkill: async (id: string): Promise<void> => {
        await apiClient.delete(`/admin/skills/${id}`);
    },

    createBanner: async (payload: {
        title: string;
        imageUrl: string;
        linkUrl?: string;
        order?: number;
        isActive?: boolean;
    }): Promise<void> => {
        await apiClient.post(`/admin/banners`, payload);
    },

    updateBanner: async (id: string, payload: Partial<{
        title: string;
        imageUrl: string;
        linkUrl: string;
        order: number;
        isActive: boolean;
    }>): Promise<void> => {
        await apiClient.put(`/admin/banners/${id}`, payload);
    },

    deleteBanner: async (id: string): Promise<void> => {
        await apiClient.delete(`/admin/banners/${id}`);
    },

    updatePackagePrice: async (id: string, price: number): Promise<void> => {
        await apiClient.put(`/admin/premium-packages/${id}/price`, { price });
    },

    // ===== UC-ADM-23/24: Payments =====
    getPayments: async (status?: string): Promise<AdminPaymentLike[]> => {
        const qs = status ? `?status=${encodeURIComponent(status)}` : '';
        const response = await apiClient.get<ApiResponse<any[]>>(`/admin/payments${qs}`);
        const list = response.data.data ?? [];
        return list as AdminPaymentLike[];
    },

    approvePayment: async (id: string): Promise<void> => {
        await apiClient.put(`/admin/payments/${id}/approve`);
    },

    rejectPayment: async (id: string, reason: string): Promise<void> => {
        await apiClient.put(`/admin/payments/${id}/reject`, { reason });
    },

    // ===== UC-ADM-25: Audit logs =====
    getAuditLogs: async (params: {
        userId?: string;
        entityType?: string;
        fromDate?: Date;
        toDate?: Date;
    }): Promise<AdminAuditLog[]> => {
        const search = new URLSearchParams();
        if (params.userId) search.set('userId', params.userId);
        if (params.entityType) search.set('entityType', params.entityType);
        if (params.fromDate) search.set('fromDate', params.fromDate.toISOString());
        if (params.toDate) search.set('toDate', params.toDate.toISOString());

        const response = await apiClient.get<ApiResponse<any[]>>(
            `/admin/audit-logs?${search.toString()}`
        );
        const list = response.data.data ?? [];
        return list.map((raw) => ({
            id: String(raw?.id ?? raw?.Id ?? ''),
            userId: raw?.userId ?? raw?.UserId,
            userEmail: raw?.userEmail ?? raw?.UserEmail,
            action: String(raw?.action ?? raw?.Action ?? ''),
            entityType: raw?.entityType ?? raw?.EntityType,
            entityId: raw?.entityId ?? raw?.EntityId,
            endpoint: String(raw?.endpoint ?? raw?.Endpoint ?? ''),
            httpMethod: String(raw?.httpMethod ?? raw?.HttpMethod ?? ''),
            requestPath: String(raw?.requestPath ?? raw?.RequestPath ?? ''),
            queryString: raw?.queryString ?? raw?.QueryString ?? null,
            requestBody: raw?.requestBody ?? raw?.RequestBody ?? null,
            responseStatusCode: raw?.responseStatusCode ?? raw?.ResponseStatusCode ?? null,
            responseBody: raw?.responseBody ?? raw?.ResponseBody ?? null,
            ipAddress: raw?.ipAddress ?? raw?.IpAddress ?? null,
            userAgent: raw?.userAgent ?? raw?.UserAgent ?? null,
            duration: raw?.duration ?? raw?.Duration ?? null,
            errorMessage: raw?.errorMessage ?? raw?.ErrorMessage ?? null,
            createdAt: new Date(String(raw?.createdAt ?? raw?.CreatedAt ?? Date.now())),
        }));
    },

    // ===== UC-ADM-26: Broadcast =====
    sendBroadcastNotification: async (payload: {
        title: string;
        message: string;
        targetRole?: string;
    }): Promise<void> => {
        await apiClient.post(`/admin/notifications/broadcast`, payload);
    },

    // ===== Admin reports moderation =====
    getReports: async (filter?: { status?: string; reportType?: string; fromDate?: Date; toDate?: Date }): Promise<AdminReportDetails[]> => {
        const search = new URLSearchParams();
        if (filter?.status) search.set('status', filter.status);
        if (filter?.reportType) search.set('reportType', filter.reportType);
        if (filter?.fromDate) search.set('fromDate', filter.fromDate.toISOString());
        if (filter?.toDate) search.set('toDate', filter.toDate.toISOString());

        const response = await apiClient.get<ApiResponse<any[]>>(
            `/admin/reports${search.toString() ? `?${search.toString()}` : ''}`
        );
        const list = response.data.data ?? [];
        return list.map((raw) => ({
            ...raw,
            reporter: raw?.reporter,
            reportedUser: raw?.reportedUser,
            createdAt: new Date(String(raw?.createdAt ?? raw?.CreatedAt ?? Date.now())),
            resolvedAt: raw?.resolvedAt ? new Date(String(raw.resolvedAt)) : null,
        })) as AdminReportDetails[];
    },

    getReportDetails: async (id: string): Promise<AdminReportDetails> => {
        const response = await apiClient.get<ApiResponse<any>>(`/admin/reports/${id}`);
        const raw = response.data.data;
        return {
            id: String(raw?.id ?? raw?.Id ?? id),
            reporter: raw?.reporter,
            reportedUser: raw?.reportedUser,
            reason: String(raw?.reason ?? raw?.Reason ?? ''),
            description: String(raw?.description ?? raw?.Description ?? ''),
            evidence: raw?.evidence ?? raw?.Evidence ?? null,
            status: String(raw?.status ?? raw?.Status ?? ''),
            resolutionNote: raw?.resolutionNote ?? raw?.ResolutionNote ?? null,
            createdAt: new Date(String(raw?.createdAt ?? raw?.CreatedAt ?? Date.now())),
            resolvedAt: raw?.resolvedAt ? new Date(String(raw.resolvedAt)) : null,
        } as AdminReportDetails;
    },

    resolveReport: async (id: string, payload: { resolutionNote: string; banUser: boolean }): Promise<void> => {
        await apiClient.post(`/admin/reports/${id}/resolve`, payload);
    },

    rejectReport: async (id: string, payload: { rejectionReason: string }): Promise<void> => {
        await apiClient.post(`/admin/reports/${id}/reject`, payload);
    },
};
