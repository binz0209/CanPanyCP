import { useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import toast from 'react-hot-toast';
import { useTranslation } from 'react-i18next';
import { Button, Badge, Card, CardContent, CardHeader, CardTitle } from '../../../components/ui';
import { notificationsApi } from '../../../api';
import { useAuthStore } from '../../../stores/auth.store';
import { formatRelativeTime } from '../../../utils';
import type { Notification } from '../../../api/notifications.api';

type NotificationsView = 'unread' | 'all';

export function NotificationsPage() {
    const { t } = useTranslation('candidate');
    const queryClient = useQueryClient();
    const { token } = useAuthStore();

    const [view, setView] = useState<NotificationsView>('unread');

    const notificationsQuery = useQuery({
        queryKey: ['notifications', view],
        enabled: !!token,
        queryFn: async (): Promise<{ notifications: Notification[]; unreadCount?: number }> => {
            if (view === 'unread') {
                const data = await notificationsApi.getUnreadNotifications();
                return { notifications: data.notifications, unreadCount: data.unreadCount };
            }

            const notifications = await notificationsApi.getNotifications({ isRead: null });
            return { notifications };
        },
        placeholderData: (previous) => previous,
    });

    const unreadCount = notificationsQuery.data?.unreadCount ?? 0;
    const notifications = notificationsQuery.data?.notifications ?? [];

    const markAsReadMutation = useMutation({
        mutationFn: (id: string) => notificationsApi.markAsRead(id),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['notifications'] });
            toast.success(t('notificationsPage.toastMarked'));
        },
        onError: () => {
            toast.error(t('notificationsPage.toastMarkFailed'));
        },
    });

    const markAllAsReadMutation = useMutation({
        mutationFn: () => notificationsApi.markAllAsRead(),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['notifications'] });
            toast.success(t('notificationsPage.toastAllMarked'));
            setView('unread');
        },
        onError: () => {
            toast.error(t('notificationsPage.toastAllMarkFailed'));
        },
    });

    const title = useMemo(
        () => (view === 'unread' ? t('notificationsPage.titleUnread') : t('notificationsPage.titleAll')),
        [view, t]
    );

    const subtitle = useMemo(
        () =>
            view === 'unread'
                ? t('notificationsPage.subtitleUnread', { count: unreadCount })
                : t('notificationsPage.subtitleAll'),
        [view, unreadCount, t]
    );

    return (
        <div className="space-y-4">
            <div className="flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
                <div>
                    <h1 className="text-2xl font-bold text-gray-900">{title}</h1>
                    <p className="text-sm text-gray-600">{subtitle}</p>
                </div>

                <div className="flex flex-wrap gap-2">
                    <Button
                        variant={view === 'unread' ? 'default' : 'outline'}
                        size="sm"
                        onClick={() => setView('unread')}
                        disabled={notificationsQuery.isPending}
                    >
                        {t('notificationsPage.tabUnread')}{' '}
                        {view === 'unread' && unreadCount > 0 ? <Badge>{unreadCount}</Badge> : null}
                    </Button>
                    <Button
                        variant={view === 'all' ? 'default' : 'outline'}
                        size="sm"
                        onClick={() => setView('all')}
                        disabled={notificationsQuery.isPending}
                    >
                        {t('notificationsPage.tabAll')}
                    </Button>
                </div>
            </div>

            {view === 'unread' && unreadCount > 0 && (
                <div className="flex items-center justify-end">
                    <Button
                        variant="secondary"
                        size="sm"
                        isLoading={markAllAsReadMutation.isPending}
                        onClick={() => markAllAsReadMutation.mutate()}
                    >
                        {t('notificationsPage.markAllRead')}
                    </Button>
                </div>
            )}

            <Card>
                <CardHeader className="pb-3">
                    <CardTitle className="text-base text-gray-800">{t('notificationsPage.listTitle')}</CardTitle>
                </CardHeader>
                <CardContent className="space-y-3">
                    {notificationsQuery.isLoading ? (
                        <div className="flex items-center justify-center py-10">
                            <div className="h-8 w-8 animate-spin rounded-full border-2 border-[#00b14f]/30 border-t-[#00b14f]" />
                        </div>
                    ) : notifications.length === 0 ? (
                        <div className="py-8 text-center">
                            <p className="text-sm text-gray-500">{t('notificationsPage.empty')}</p>
                        </div>
                    ) : (
                        notifications.map((n) => (
                            <div
                                key={n.id}
                                className={[
                                    'rounded-lg border border-gray-200 p-4',
                                    !n.isRead ? 'bg-[#00b14f]/5' : 'bg-white',
                                ].join(' ')}
                            >
                                <div className="flex items-start justify-between gap-3">
                                    <div className="min-w-0">
                                        <div className="flex flex-wrap items-center gap-2">
                                            <h3 className="truncate text-sm font-semibold text-gray-900">
                                                {n.title || t('notificationsPage.noTitle')}
                                            </h3>
                                            {!n.isRead ? (
                                                <Badge variant="secondary">{t('notificationsPage.badgeUnread')}</Badge>
                                            ) : null}
                                        </div>
                                        <p className="mt-2 whitespace-pre-wrap text-sm text-gray-700">
                                            {n.content || ''}
                                        </p>
                                        <p className="mt-2 text-xs text-gray-500">{formatRelativeTime(n.timestamp)}</p>
                                    </div>

                                    <div className="flex-shrink-0">
                                        {!n.isRead ? (
                                            <Button
                                                variant="outline"
                                                size="sm"
                                                disabled={markAsReadMutation.isPending}
                                                onClick={() => markAsReadMutation.mutate(n.id)}
                                            >
                                                {t('notificationsPage.markRead')}
                                            </Button>
                                        ) : (
                                            <Button variant="ghost" size="sm" disabled>
                                                {t('notificationsPage.readState')}
                                            </Button>
                                        )}
                                    </div>
                                </div>
                            </div>
                        ))
                    )}
                </CardContent>
            </Card>
        </div>
    );
}
