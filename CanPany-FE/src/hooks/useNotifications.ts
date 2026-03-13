import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { notificationsApi } from '../api/notifications.api';
import { notificationKeys } from '../lib/queryKeys';
import type { NotificationItem } from '../types';

interface UseNotificationsOptions {
    enabled?: boolean;
}

export function useNotifications({ enabled = false }: UseNotificationsOptions = {}) {
    const queryClient = useQueryClient();

    const unreadQuery = useQuery({
        queryKey: notificationKeys.unread(),
        queryFn: () => notificationsApi.getUnread(),
        enabled,
        // Cho cảm giác "real-time" cơ bản mà không cần WebSocket
        refetchInterval: enabled ? 15_000 : false,
        refetchOnWindowFocus: enabled,
    });

    const notifications: NotificationItem[] = unreadQuery.data?.notifications ?? [];
    const unreadCount = unreadQuery.data?.unreadCount ?? 0;

    const markAsReadMutation = useMutation({
        mutationFn: (id: string) => notificationsApi.markAsRead(id),
        onSuccess: (newUnreadCount, id) => {
            queryClient.setQueryData(notificationKeys.unread(), (current: any) => {
                if (!current) return current;
                return {
                    ...current,
                    unreadCount: newUnreadCount,
                    notifications: (current.notifications as NotificationItem[]).map((n) =>
                        n.id === id ? { ...n, isRead: true } : n
                    ),
                };
            });
        },
    });

    const markAllAsReadMutation = useMutation({
        mutationFn: () => notificationsApi.markAllAsRead(),
        onSuccess: () => {
            queryClient.setQueryData(notificationKeys.unread(), (current: any) => {
                if (!current) return current;
                return {
                    ...current,
                    unreadCount: 0,
                    notifications: (current.notifications as NotificationItem[]).map((n) => ({
                        ...n,
                        isRead: true,
                    })),
                };
            });
        },
    });

    return {
        notifications,
        unreadCount,
        isLoading: unreadQuery.isLoading,
        isFetching: unreadQuery.isFetching,
        refetch: unreadQuery.refetch,
        markAsRead: (id: string) => markAsReadMutation.mutate(id),
        markAllAsRead: () => markAllAsReadMutation.mutate(),
        isMarkingAsRead: markAsReadMutation.isPending,
        isMarkingAllAsRead: markAllAsReadMutation.isPending,
    };
}

