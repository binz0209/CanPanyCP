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
    const response = await apiClient.get<ApiResponse<PaginatedNotifications>>(
      '/api/notifications',
      { params: filters }
    );
    return response.data.data?.items || [];
  },

  // Get unread notifications count
  getUnreadCount: async (): Promise<number> => {
    const response = await apiClient.get<ApiResponse<{ count: number }>>(
      '/api/notifications/unread-count'
    );
    return response.data.data?.count || 0;
  },

  // Mark single notification as read
  markAsRead: async (id: string): Promise<void> => {
    await apiClient.patch(`/api/notifications/${id}/read`);
  },

  // Mark all notifications as read
  markAllAsRead: async (): Promise<void> => {
    await apiClient.patch('/api/notifications/read-all');
  },

  // Delete notification
  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`/api/notifications/${id}`);
  },

  // Delete all notifications
  deleteAll: async (): Promise<void> => {
    await apiClient.delete('/api/notifications');
  },
};
