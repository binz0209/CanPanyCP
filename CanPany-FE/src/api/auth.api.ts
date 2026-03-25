import apiClient from './axios.config';
import type { ApiResponse, AuthResponse, LoginRequest, RegisterRequest, ForgotPasswordRequest, ResetPasswordRequest, User } from '../types';
import { normalizeAppRole } from '../lib/userRole';

function mapUserFromApi(raw: Record<string, unknown> | null | undefined): User {
    const r = raw ?? {};
    const role = normalizeAppRole(r.role ?? r.Role) ?? 'Guest';
    return {
        id: String(r.id ?? r.Id ?? ''),
        fullName: String(r.fullName ?? r.FullName ?? ''),
        email: String(r.email ?? r.Email ?? ''),
        role,
        avatarUrl: (r.avatarUrl ?? r.AvatarUrl) as string | undefined,
        isLocked: Boolean(r.isLocked ?? r.IsLocked),
        lockedUntil:
            r.lockedUntil != null || r.LockedUntil != null
                ? new Date(String(r.lockedUntil ?? r.LockedUntil))
                : undefined,
        createdAt: new Date(String(r.createdAt ?? r.CreatedAt ?? Date.now())),
        updatedAt:
            r.updatedAt != null || r.UpdatedAt != null
                ? new Date(String(r.updatedAt ?? r.UpdatedAt))
                : undefined,
    };
}

function mapAuthPayload(body: Record<string, unknown> | null | undefined): AuthResponse {
    const b = body ?? {};
    const userRaw = (b.user ?? b.User) as Record<string, unknown> | undefined;
    return {
        accessToken: String(b.accessToken ?? b.AccessToken ?? ''),
        user: mapUserFromApi(userRaw),
    };
}

export const authApi = {
    login: async (data: LoginRequest): Promise<AuthResponse> => {
        const response = await apiClient.post<ApiResponse<Record<string, unknown>>>('/auth/login', data);
        const payload = response.data.data;
        if (!payload) throw new Error('Login response missing data');
        return mapAuthPayload(payload as Record<string, unknown>);
    },

    register: async (data: RegisterRequest): Promise<AuthResponse> => {
        const response = await apiClient.post<ApiResponse<Record<string, unknown>>>('/auth/register', data);
        const payload = response.data.data;
        if (!payload) throw new Error('Register response missing data');
        return mapAuthPayload(payload as Record<string, unknown>);
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

    // Step 1 of GitHub OAuth — returns the URL to redirect the browser to
    getGitHubLinkUrl: async (): Promise<{ oauthUrl: string }> => {
        const response = await apiClient.get<ApiResponse<{ oauthUrl: string }>>('/auth/github/link');
        return response.data.data!;
    },
};
