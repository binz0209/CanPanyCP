import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { notificationsApi } from '../api/notifications.api';
import { notificationKeys } from '../lib/queryKeys';
import type { NotificationItem } from '../types/notification.types';

interface UseNotificationsOptions {
  enabled?: boolean;
  refetchInterval?: number;
}

export function useNotifications(options?: UseNotificationsOptions) {
  const queryClient = useQueryClient();
  const { enabled = true, refetchInterval = 10000 } = options || {};

  // Fetch notifications
  const { data: notifications = [], isLoading } = useQuery({
    queryKey: notificationKeys.list(),
    queryFn: () => notificationsApi.getAll(),
    enabled,
    refetchInterval,
  });

  // Fetch unread notifications and count
  const { data: unreadData, isLoading: isUnreadLoading } = useQuery({
    queryKey: notificationKeys.unread(),
    queryFn: () => notificationsApi.getUnreadCount(),
    enabled,
    refetchInterval,
  });
  
  const unreadNotifications = unreadData?.notifications || [];
  const unreadCount = unreadData?.unreadCount || 0;

  // Mark as read mutation
  const { mutate: markAsRead } = useMutation({
    mutationFn: (id: string) => notificationsApi.markAsRead(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: notificationKeys.all });
    },
  });

  // Mark all as read mutation
  const { mutate: markAllAsRead, isPending: isMarkingAllAsRead } = useMutation({
    mutationFn: () => notificationsApi.markAllAsRead(),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: notificationKeys.all });
    },
  });

  return {
    notifications: notifications as NotificationItem[],
    unreadNotifications: unreadNotifications as NotificationItem[],
    unreadCount,
    isLoading: isLoading || isUnreadLoading,
    markAsRead,
    markAllAsRead,
    isMarkingAllAsRead,
  };
}
