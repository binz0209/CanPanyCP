import apiClient from './axios.config';
import type { ApiResponse } from '../types';
import type { NotificationItem } from '../types/notification.types';

interface NotificationFilters {
  type?: string;
  isRead?: boolean;
  fromDate?: string;
  toDate?: string;
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

  // Get unread notifications and count
  getUnreadCount: async (): Promise<{ notifications: NotificationItem[]; unreadCount: number }> => {
    const response = await apiClient.get<ApiResponse<{ notifications: NotificationItem[]; unreadCount: number }>>(
      '/notifications/unread'
    );
    return response.data.data || { notifications: [], unreadCount: 0 };
  },

  // Mark single notification as read
  markAsRead: async (id: string): Promise<{ unreadCount: number }> => {
    const response = await apiClient.put<ApiResponse<{ unreadCount: number }>>(`/notifications/${id}/read`);
    return response.data.data || { unreadCount: 0 };
  },

  // Mark all notifications as read
  markAllAsRead: async (): Promise<void> => {
    await apiClient.put('/notifications/read-all');
  },
};
