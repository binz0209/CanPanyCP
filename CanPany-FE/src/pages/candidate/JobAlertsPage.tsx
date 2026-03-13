import { useState } from 'react';
import { Bell, Plus, TrendingUp, AlertCircle, CheckCircle, Loader2, BellOff } from 'lucide-react';
import { Button } from '../../components/ui/Button';
import { useJobAlerts } from '../../hooks/useJobAlerts';
import { JobAlertCard } from '../../components/features/job-alerts/JobAlertCard';
import { JobAlertForm } from '../../components/features/job-alerts/JobAlertForm';
import { JobAlertPreview } from '../../components/features/job-alerts/JobAlertPreview';
import type { JobAlertResponse, JobAlertCreateDto, JobAlertUpdateDto } from '../../api/jobAlerts.api';

export function JobAlertsPage() {
    const [isFormOpen, setIsFormOpen] = useState(false);
    const [editingAlert, setEditingAlert] = useState<JobAlertResponse | null>(null);
    const [previewAlert, setPreviewAlert] = useState<JobAlertResponse | null>(null);
    const [deletingId, setDeletingId] = useState<string | null>(null);

    const { alerts, stats, isLoading, isStatsLoading, create, update, remove, pause, resume, isCreating, isUpdating } =
        useJobAlerts();

    const handleOpenCreate = () => {
        setEditingAlert(null);
        setIsFormOpen(true);
    };

    const handleOpenEdit = (alert: JobAlertResponse) => {
        setEditingAlert(alert);
        setIsFormOpen(true);
    };

    const handleFormSubmit = async (dto: JobAlertCreateDto | JobAlertUpdateDto) => {
        if (editingAlert) {
            await update(editingAlert.id, dto as JobAlertUpdateDto);
        } else {
            await create(dto as JobAlertCreateDto);
        }
    };

    const handleDelete = (id: string) => {
        setDeletingId(id);
        remove(id);
        setTimeout(() => setDeletingId(null), 1000);
    };

    const handleToggle = (id: string, isActive: boolean) => {
        if (isActive) pause(id);
        else resume(id);
    };

    const activeCount = alerts.filter((a) => a.isActive).length;

    return (
        <div className="mx-auto max-w-3xl">
            {/* Page header */}
            <div className="mb-8 flex items-start justify-between gap-4">
                <div>
                    <h1 className="text-2xl font-bold text-gray-900">Job Alerts</h1>
                    <p className="mt-1 text-sm text-gray-500">
                        Nhận thông báo ngay khi có công việc phù hợp với tiêu chí của bạn.
                    </p>
                </div>
                <Button
                    onClick={handleOpenCreate}
                    className="shrink-0 bg-[#00b14f] hover:bg-[#009940] text-white"
                >
                    <Plus className="h-4 w-4 mr-1.5" />
                    Tạo Alert
                </Button>
            </div>

            {/* Stats */}
            {!isStatsLoading && (
                <div className="mb-6 grid grid-cols-2 gap-3 sm:grid-cols-4">
                    {[
                        {
                            label: 'Đang hoạt động',
                            value: activeCount,
                            icon: <CheckCircle className="h-4 w-4 text-[#00b14f]" />,
                            color: 'text-[#00b14f]',
                        },
                        {
                            label: 'Tổng alerts',
                            value: alerts.length,
                            icon: <Bell className="h-4 w-4 text-gray-400" />,
                            color: 'text-gray-700',
                        },
                        {
                            label: 'Tổng kết quả',
                            value: stats?.totalMatches ?? 0,
                            icon: <TrendingUp className="h-4 w-4 text-blue-400" />,
                            color: 'text-blue-600',
                        },
                        {
                            label: 'Kết quả gần đây',
                            value: stats?.recentMatches ?? 0,
                            icon: <AlertCircle className="h-4 w-4 text-amber-400" />,
                            color: 'text-amber-600',
                        },
                    ].map((s) => (
                        <div key={s.label} className="rounded-xl border bg-white p-4 shadow-sm">
                            <div className="flex items-center gap-2 text-xs text-gray-500 mb-1">
                                {s.icon}
                                {s.label}
                            </div>
                            <div className={`text-2xl font-bold ${s.color}`}>{s.value}</div>
                        </div>
                    ))}
                </div>
            )}

            {/* Alerts list */}
            {isLoading ? (
                <div className="flex flex-col items-center justify-center py-20 text-gray-400">
                    <Loader2 className="h-8 w-8 animate-spin text-[#00b14f]" />
                    <p className="mt-3 text-sm">Đang tải job alerts...</p>
                </div>
            ) : alerts.length === 0 ? (
                <div className="flex flex-col items-center justify-center rounded-xl border-2 border-dashed border-gray-200 py-16 text-center">
                    <div className="mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-gray-100">
                        <BellOff className="h-8 w-8 text-gray-400" />
                    </div>
                    <h3 className="text-base font-semibold text-gray-700">Chưa có job alert nào</h3>
                    <p className="mt-1.5 text-sm text-gray-500 max-w-xs">
                        Tạo alert để nhận thông báo tự động khi có công việc phù hợp với bạn.
                    </p>
                    <Button
                        onClick={handleOpenCreate}
                        className="mt-5 bg-[#00b14f] hover:bg-[#009940] text-white"
                    >
                        <Plus className="h-4 w-4 mr-1.5" />
                        Tạo Alert đầu tiên
                    </Button>
                </div>
            ) : (
                <div className="space-y-3">
                    {alerts.map((alert) => (
                        <JobAlertCard
                            key={alert.id}
                            alert={alert}
                            onEdit={handleOpenEdit}
                            onDelete={handleDelete}
                            onToggle={handleToggle}
                            onPreview={setPreviewAlert}
                            isDeleting={deletingId === alert.id}
                        />
                    ))}
                </div>
            )}

            {/* Create/Edit form modal */}
            <JobAlertForm
                isOpen={isFormOpen}
                onClose={() => setIsFormOpen(false)}
                onSubmit={handleFormSubmit}
                initialData={editingAlert}
                isSubmitting={isCreating || isUpdating}
            />

            {/* Preview modal */}
            {previewAlert && (
                <JobAlertPreview alert={previewAlert} onClose={() => setPreviewAlert(null)} />
            )}
        </div>
    );
}

export default JobAlertsPage;
