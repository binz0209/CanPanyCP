import { Bell, BellOff, MapPin, Clock, DollarSign, Trash2, Edit2, Eye, Play, Pause } from 'lucide-react';
import type { JobAlertResponse } from '../../../api/jobAlerts.api';
import { cn } from '../../../utils';

const frequencyLabel: Record<string, string> = {
    Immediate: 'Ngay lập tức',
    Daily: 'Hàng ngày',
    Weekly: 'Hàng tuần',
};

const jobTypeLabel: Record<string, string> = {
    FullTime: 'Full-time',
    PartTime: 'Part-time',
    Freelance: 'Freelance',
};

interface JobAlertCardProps {
    alert: JobAlertResponse;
    onEdit: (alert: JobAlertResponse) => void;
    onDelete: (id: string) => void;
    onToggle: (id: string, isActive: boolean) => void;
    onPreview: (alert: JobAlertResponse) => void;
    isDeleting?: boolean;
}

export function JobAlertCard({ alert, onEdit, onDelete, onToggle, onPreview, isDeleting }: JobAlertCardProps) {
    const formatBudget = () => {
        if (alert.minBudget && alert.maxBudget)
            return `${alert.minBudget.toLocaleString()} – ${alert.maxBudget.toLocaleString()} VND`;
        if (alert.minBudget) return `Từ ${alert.minBudget.toLocaleString()} VND`;
        if (alert.maxBudget) return `Đến ${alert.maxBudget.toLocaleString()} VND`;
        return null;
    };

    const budget = formatBudget();

    return (
        <div
            className={cn(
                'rounded-xl border bg-white p-5 shadow-sm transition-shadow hover:shadow-md',
                !alert.isActive && 'opacity-60 bg-gray-50'
            )}
        >
            {/* Header */}
            <div className="flex items-start justify-between gap-3">
                <div className="flex items-center gap-2 min-w-0">
                    <div
                        className={cn(
                            'flex h-9 w-9 shrink-0 items-center justify-center rounded-lg',
                            alert.isActive ? 'bg-[#00b14f]/10 text-[#00b14f]' : 'bg-gray-100 text-gray-400'
                        )}
                    >
                        {alert.isActive ? <Bell className="h-4 w-4" /> : <BellOff className="h-4 w-4" />}
                    </div>
                    <div className="min-w-0">
                        <h3 className="truncate font-semibold text-gray-900">{alert.title || 'Job Alert'}</h3>
                        <span
                            className={cn(
                                'inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-xs font-medium',
                                alert.isActive
                                    ? 'bg-[#00b14f]/10 text-[#00b14f]'
                                    : 'bg-gray-100 text-gray-500'
                            )}
                        >
                            {alert.isActive ? 'Đang hoạt động' : 'Tạm dừng'}
                        </span>
                    </div>
                </div>

                {/* Match count badge */}
                {alert.matchCount > 0 && (
                    <div className="shrink-0 rounded-full bg-blue-50 px-2.5 py-1 text-xs font-semibold text-blue-600">
                        {alert.matchCount} kết quả
                    </div>
                )}
            </div>

            {/* Criteria */}
            <div className="mt-3 space-y-1.5 text-sm text-gray-600">
                {alert.location && (
                    <div className="flex items-center gap-1.5">
                        <MapPin className="h-3.5 w-3.5 text-gray-400" />
                        <span>{alert.location}</span>
                    </div>
                )}
                {budget && (
                    <div className="flex items-center gap-1.5">
                        <DollarSign className="h-3.5 w-3.5 text-gray-400" />
                        <span>{budget}</span>
                    </div>
                )}
                {alert.jobType && (
                    <div className="flex items-center gap-1.5">
                        <Clock className="h-3.5 w-3.5 text-gray-400" />
                        <span>{jobTypeLabel[alert.jobType] ?? alert.jobType}</span>
                        {alert.experienceLevel && (
                            <span className="text-gray-400">• {alert.experienceLevel}</span>
                        )}
                    </div>
                )}
                {!alert.location && !budget && !alert.jobType && (
                    <span className="text-gray-400 text-xs">Tất cả công việc</span>
                )}
            </div>

            {/* Footer */}
            <div className="mt-4 flex items-center justify-between border-t border-gray-100 pt-3">
                <div className="flex items-center gap-3 text-xs text-gray-500">
                    <span>{frequencyLabel[alert.frequency] ?? alert.frequency}</span>
                    {alert.emailEnabled && <span>📧 Email</span>}
                    {alert.inAppEnabled && <span>🔔 In-app</span>}
                    {alert.lastTriggeredAt && (
                        <span>
                            Lần cuối: {new Date(alert.lastTriggeredAt).toLocaleDateString('vi-VN')}
                        </span>
                    )}
                </div>

                {/* Actions */}
                <div className="flex items-center gap-1">
                    <button
                        onClick={() => onPreview(alert)}
                        className="rounded-md p-1.5 text-gray-400 transition-colors hover:bg-gray-100 hover:text-gray-700"
                        title="Xem trước kết quả"
                    >
                        <Eye className="h-4 w-4" />
                    </button>
                    <button
                        onClick={() => onEdit(alert)}
                        className="rounded-md p-1.5 text-gray-400 transition-colors hover:bg-gray-100 hover:text-gray-700"
                        title="Chỉnh sửa"
                    >
                        <Edit2 className="h-4 w-4" />
                    </button>
                    <button
                        onClick={() => onToggle(alert.id, alert.isActive)}
                        className={cn(
                            'rounded-md p-1.5 transition-colors',
                            alert.isActive
                                ? 'text-amber-400 hover:bg-amber-50 hover:text-amber-600'
                                : 'text-[#00b14f] hover:bg-[#00b14f]/10'
                        )}
                        title={alert.isActive ? 'Tạm dừng' : 'Kích hoạt lại'}
                    >
                        {alert.isActive ? <Pause className="h-4 w-4" /> : <Play className="h-4 w-4" />}
                    </button>
                    <button
                        onClick={() => onDelete(alert.id)}
                        disabled={isDeleting}
                        className="rounded-md p-1.5 text-gray-400 transition-colors hover:bg-red-50 hover:text-red-500"
                        title="Xóa"
                    >
                        <Trash2 className="h-4 w-4" />
                    </button>
                </div>
            </div>
        </div>
    );
}
