import apiClient from './axios.config';
import type { ApiResponse } from '../types';

export interface Notification {
    id: string;
    type: string;
    title: string;
    content: string;
    timestamp: string; // ISO string from backend
    isRead: boolean;
}

export interface NotificationsUnreadResponse {
    notifications: Notification[];
    unreadCount: number;
}

export const notificationsApi = {
    // Normalize BE casing (Id/IsRead/Title/Content/Timestamp) -> FE camelCase fields.
    _normalize: (dto: any): Notification => ({
        id: dto?.id ?? dto?.Id ?? '',
        type: dto?.type ?? dto?.Type ?? '',
        title: dto?.title ?? dto?.Title ?? '',
        content: dto?.content ?? dto?.Content ?? '',
        timestamp: dto?.timestamp ?? dto?.Timestamp ?? new Date().toISOString(),
        isRead: dto?.isRead ?? dto?.IsRead ?? false,
    }),

    // Main FE (main worktree) compatibility:
    // - getAll() + getUnreadCount() are used by NotificationCenterPage/useNotifications
    // - We normalize casing (Id/IsRead vs id/isRead).

    // Get all notifications (for filtered list pages)
    getAll: async (filters?: {
        isRead?: boolean | null;
        type?: string | null;
        fromDate?: string | null;
        toDate?: string | null;
    }): Promise<Notification[]> => {
        const notifications = await notificationsApi.getNotifications(filters);
        return notifications.map((n: Notification) => ({ ...n, isRead: !!n.isRead }));
    },

    // Get unread notifications count + optionally list
    getUnreadCount: async (): Promise<number> => {
        const data = await notificationsApi.getUnreadNotifications();
        return data.unreadCount ?? 0;
    },

    getNotifications: async (params?: {
        isRead?: boolean | null;
        type?: string | null;
        fromDate?: string | null;
        toDate?: string | null;
    }): Promise<Notification[]> => {
        const response = await apiClient.get<ApiResponse<Notification[]>>('/notifications', {
            params: {
                // backend expects query params as nullable
                isRead: params?.isRead ?? undefined,
                type: params?.type ?? undefined,
                fromDate: params?.fromDate ?? undefined,
                toDate: params?.toDate ?? undefined,
            },
        });

        return (response.data.data ?? []).map(notificationsApi._normalize);
    },

    getUnreadNotifications: async (): Promise<NotificationsUnreadResponse> => {
        const response = await apiClient.get<ApiResponse<{ notifications: Notification[]; unreadCount: number }>>(
            '/notifications/unread'
        );

        const data = response.data.data;
        return {
            notifications: (data?.notifications ?? []).map(notificationsApi._normalize),
            unreadCount: data?.unreadCount ?? 0,
        };
    },

    markAsRead: async (id: string): Promise<void> => {
        await apiClient.put(`/notifications/${id}/read`);
    },

    markAllAsRead: async (): Promise<void> => {
        await apiClient.put('/notifications/read-all');
    },
};

