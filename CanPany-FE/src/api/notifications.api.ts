import apiClient from './axios.config';
import type { ApiResponse } from '../types';
import type { NotificationItem } from '../types/notification.types';

export interface UnreadNotificationsResponse {
    notifications: NotificationItem[];
    unreadCount: number;
}

export const notificationsApi = {
    async getAll(params?: { isRead?: boolean; type?: string; fromDate?: string; toDate?: string }): Promise<NotificationItem[]> {
        const response = await apiClient.get<ApiResponse<NotificationItem[]>>('/notifications', {
            params,
        });
        return response.data.data || [];
    },

    async getUnread(): Promise<UnreadNotificationsResponse> {
        const response = await apiClient.get<ApiResponse<UnreadNotificationsResponse>>('/notifications/unread');
        return (response.data.data as UnreadNotificationsResponse) || { notifications: [], unreadCount: 0 };
    },

    async markAsRead(id: string): Promise<number> {
        const response = await apiClient.put<ApiResponse<{ unreadCount: number }>>(`/notifications/${id}/read`);
        return response.data.data?.unreadCount ?? 0;
    },

    async markAllAsRead(): Promise<void> {
        await apiClient.put<ApiResponse<unknown>>('/notifications/read-all');
    },
};

