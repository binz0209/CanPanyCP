import apiClient from './axios.config';
import type { ApiResponse } from '../types';
import type { NotificationItem, PaginatedNotifications } from '../types/notification.types';

interface NotificationFilters {
  type?: string;
  isRead?: boolean;
  page?: number;
  pageSize?: number;
}

export const notificationsApi = {
  // Get all notifications with filters
  getAll: async (filters?: NotificationFilters): Promise<NotificationItem[]> => {
    const response = await apiClient.get<ApiResponse<NotificationItem[]>>(
      '/notifications',
      { params: filters }
    );
    return response.data.data || [];
  },

  // Get unread notifications count
  getUnreadCount: async (): Promise<number> => {
    const response = await apiClient.get<ApiResponse<{ notifications: unknown[]; unreadCount: number }>>(
      '/notifications/unread'
    );
    return response.data.data?.unreadCount || 0;
  },

  // Mark single notification as read
  markAsRead: async (id: string): Promise<void> => {
    await apiClient.put(`/notifications/${id}/read`);
  },

  // Mark all notifications as read
  markAllAsRead: async (): Promise<void> => {
    await apiClient.put('/notifications/read-all');
  },

  // Delete notification (not implemented in backend - disabled)
  // delete: async (id: string): Promise<void> => {
  //   await apiClient.delete(`/notifications/${id}`);
  // },

  // Delete all notifications (not implemented in backend - disabled)
  // deleteAll: async (): Promise<void> => {
  //   await apiClient.delete('/notifications');
  // },
};
