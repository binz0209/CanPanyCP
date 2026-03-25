import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { notificationsApi } from '../api/notifications.api';

interface UseNotificationsOptions {
    enabled?: boolean;
    refetchInterval?: number;
}

export function useNotifications(options?: UseNotificationsOptions) {
    const queryClient = useQueryClient();
    const { enabled = true, refetchInterval = 10_000 } = options || {};

    // For the navbar panel we only need unread notifications + unreadCount.
    const unreadQueryKey = ['notifications', 'unread'];

    const {
        data: unreadData,
        isLoading,
    } = useQuery({
        queryKey: unreadQueryKey,
        enabled,
        refetchInterval,
        queryFn: () => notificationsApi.getUnreadNotifications(),
    });

    const notifications = unreadData?.notifications ?? [];
    const unreadCount = unreadData?.unreadCount ?? 0;

    const markAsReadMutation = useMutation({
        mutationFn: (id: string) => notificationsApi.markAsRead(id),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: unreadQueryKey });
        },
    });

    const markAllAsReadMutation = useMutation({
        mutationFn: () => notificationsApi.markAllAsRead(),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: unreadQueryKey });
        },
    });

    return {
        notifications,
        unreadCount,
        isLoading,
        markAsRead: markAsReadMutation.mutate,
        markAllAsRead: markAllAsReadMutation.mutate,
        isMarkingAllAsRead: markAllAsReadMutation.isPending,
    };
}

