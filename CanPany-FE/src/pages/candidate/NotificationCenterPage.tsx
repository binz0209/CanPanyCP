import { useState } from 'react';
import { Bell, BellOff, Check, CheckCheck, Briefcase, MessageSquare, DollarSign, Loader2, Filter } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { notificationsApi } from '../../api/notifications.api';
import { notificationKeys } from '../../lib/queryKeys';
import { Button } from '../../components/ui/Button';
import type { NotificationItem, NotificationType } from '../../types/notification.types';
import { cn } from '../../utils';

const TYPE_FILTERS = [
    { value: '', label: 'Tất cả' },
    { value: 'JobMatch', label: 'Job Match' },
    { value: 'ApplicationUpdate', label: 'Applications' },
    { value: 'NewMessage', label: 'Tin nhắn' },
    { value: 'PaymentConfirmation', label: 'Thanh toán' },
] as const;

const READ_FILTERS = [
    { value: undefined, label: 'Tất cả' },
    { value: false, label: 'Chưa đọc' },
    { value: true, label: 'Đã đọc' },
] as const;

function notificationIcon(type: NotificationType) {
    switch (type) {
        case 'JobMatch': return <Briefcase className="h-4 w-4 text-[#00b14f]" />;
        case 'ApplicationUpdate': return <CheckCheck className="h-4 w-4 text-blue-500" />;
        case 'NewMessage': return <MessageSquare className="h-4 w-4 text-purple-500" />;
        case 'PaymentConfirmation': return <DollarSign className="h-4 w-4 text-amber-500" />;
        default: return <Bell className="h-4 w-4 text-gray-400" />;
    }
}

function notificationIconBg(type: NotificationType) {
    switch (type) {
        case 'JobMatch': return 'bg-[#00b14f]/10';
        case 'ApplicationUpdate': return 'bg-blue-50';
        case 'NewMessage': return 'bg-purple-50';
        case 'PaymentConfirmation': return 'bg-amber-50';
        default: return 'bg-gray-100';
    }
}

function getNavigationPath(notification: NotificationItem): string | null {
    switch (notification.type) {
        case 'JobMatch': return '/candidate/job-alerts';
        case 'ApplicationUpdate': return '/candidate/applications/history';
        default: return null;
    }
}

function timeAgo(date: string | Date): string {
    const diff = (Date.now() - new Date(date).getTime()) / 1000;
    if (diff < 60) return 'Vừa xong';
    if (diff < 3600) return `${Math.floor(diff / 60)} phút trước`;
    if (diff < 86400) return `${Math.floor(diff / 3600)} giờ trước`;
    if (diff < 604800) return `${Math.floor(diff / 86400)} ngày trước`;
    return new Date(date).toLocaleDateString('vi-VN');
}

export function NotificationCenterPage() {
    const navigate = useNavigate();
    const queryClient = useQueryClient();
    const [typeFilter, setTypeFilter] = useState<string>('');
    const [readFilter, setReadFilter] = useState<boolean | undefined>(undefined);

    const { data: notifications = [], isLoading } = useQuery({
        queryKey: notificationKeys.list({ type: typeFilter, isRead: readFilter }),
        queryFn: () =>
            notificationsApi.getAll({
                type: typeFilter || undefined,
                isRead: readFilter,
            }),
    });

    const markAsReadMutation = useMutation({
        mutationFn: (id: string) => notificationsApi.markAsRead(id),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: notificationKeys.all });
        },
    });

    const markAllMutation = useMutation({
        mutationFn: () => notificationsApi.markAllAsRead(),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: notificationKeys.all });
        },
    });

    const handleNotificationClick = (notification: NotificationItem) => {
        if (!notification.isRead) {
            markAsReadMutation.mutate(notification.id);
        }
        const path = getNavigationPath(notification);
        if (path) navigate(path);
    };

    const unreadCount = notifications.filter((n) => !n.isRead).length;

    return (
        <div className="mx-auto max-w-2xl">
            {/* Header */}
            <div className="mb-6 flex items-center justify-between gap-4">
                <div>
                    <h1 className="text-2xl font-bold text-gray-900">Thông báo</h1>
                    {unreadCount > 0 && (
                        <p className="mt-1 text-sm text-gray-500">
                            <span className="font-semibold text-[#00b14f]">{unreadCount}</span> thông báo chưa đọc
                        </p>
                    )}
                </div>
                {unreadCount > 0 && (
                    <Button
                        variant="outline"
                        size="sm"
                        onClick={() => markAllMutation.mutate()}
                        disabled={markAllMutation.isPending}
                        className="shrink-0"
                    >
                        {markAllMutation.isPending ? (
                            <Loader2 className="h-4 w-4 animate-spin" />
                        ) : (
                            <Check className="h-4 w-4 mr-1.5" />
                        )}
                        Đánh dấu tất cả đã đọc
                    </Button>
                )}
            </div>

            {/* Filters */}
            <div className="mb-5 flex flex-wrap gap-2">
                <div className="flex items-center gap-1 rounded-lg border border-gray-200 bg-white p-1">
                    <Filter className="h-3.5 w-3.5 text-gray-400 ml-2" />
                    {TYPE_FILTERS.map((f) => (
                        <button
                            key={f.value}
                            onClick={() => setTypeFilter(f.value)}
                            className={cn(
                                'rounded-md px-3 py-1 text-xs font-medium transition-colors',
                                typeFilter === f.value
                                    ? 'bg-[#00b14f] text-white'
                                    : 'text-gray-600 hover:bg-gray-100'
                            )}
                        >
                            {f.label}
                        </button>
                    ))}
                </div>
                <div className="flex items-center gap-1 rounded-lg border border-gray-200 bg-white p-1">
                    {READ_FILTERS.map((f, i) => (
                        <button
                            key={i}
                            onClick={() => setReadFilter(f.value)}
                            className={cn(
                                'rounded-md px-3 py-1 text-xs font-medium transition-colors',
                                readFilter === f.value
                                    ? 'bg-[#00b14f] text-white'
                                    : 'text-gray-600 hover:bg-gray-100'
                            )}
                        >
                            {f.label}
                        </button>
                    ))}
                </div>
            </div>

            {/* List */}
            {isLoading ? (
                <div className="flex flex-col items-center justify-center py-20 text-gray-400">
                    <Loader2 className="h-8 w-8 animate-spin text-[#00b14f]" />
                    <p className="mt-3 text-sm">Đang tải thông báo...</p>
                </div>
            ) : notifications.length === 0 ? (
                <div className="flex flex-col items-center justify-center rounded-xl border-2 border-dashed border-gray-200 py-16 text-center">
                    <div className="mb-3 flex h-14 w-14 items-center justify-center rounded-full bg-gray-100">
                        <BellOff className="h-7 w-7 text-gray-400" />
                    </div>
                    <h3 className="font-medium text-gray-700">Không có thông báo nào</h3>
                    <p className="mt-1 text-sm text-gray-500">
                        {typeFilter || readFilter !== undefined
                            ? 'Không tìm thấy thông báo phù hợp với bộ lọc.'
                            : 'Bạn chưa có thông báo nào.'}
                    </p>
                </div>
            ) : (
                <div className="overflow-hidden rounded-xl border border-gray-200 bg-white shadow-sm divide-y divide-gray-100">
                    {notifications.map((notification) => (
                        <div
                            key={notification.id}
                            onClick={() => handleNotificationClick(notification)}
                            className={cn(
                                'flex items-start gap-4 px-5 py-4 cursor-pointer transition-colors',
                                !notification.isRead
                                    ? 'bg-[#00b14f]/[0.03] hover:bg-[#00b14f]/[0.06]'
                                    : 'hover:bg-gray-50'
                            )}
                        >
                            {/* Icon */}
                            <div
                                className={cn(
                                    'mt-0.5 flex h-9 w-9 shrink-0 items-center justify-center rounded-full',
                                    notificationIconBg(notification.type)
                                )}
                            >
                                {notificationIcon(notification.type)}
                            </div>

                            {/* Content */}
                            <div className="min-w-0 flex-1">
                                <div className="flex items-start justify-between gap-2">
                                    <p
                                        className={cn(
                                            'text-sm',
                                            !notification.isRead
                                                ? 'font-semibold text-gray-900'
                                                : 'font-medium text-gray-700'
                                        )}
                                    >
                                        {notification.title}
                                    </p>
                                    <div className="flex shrink-0 items-center gap-2">
                                        <span className="whitespace-nowrap text-xs text-gray-400">
                                            {timeAgo(notification.timestamp)}
                                        </span>
                                        {!notification.isRead && (
                                            <span className="h-2 w-2 shrink-0 rounded-full bg-[#00b14f]" />
                                        )}
                                    </div>
                                </div>
                                <p className="mt-1 text-sm text-gray-500 line-clamp-2">
                                    {notification.content}
                                </p>
                            </div>
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
}

export default NotificationCenterPage;
