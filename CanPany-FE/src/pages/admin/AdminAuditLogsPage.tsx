import { useMutation } from '@tanstack/react-query';
import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import { Button, Card } from '../../components/ui';
import { adminApi } from '../../api';
import type { AdminAuditLog } from '../../api/admin.api';

export function AdminAuditLogsPage() {
    const { t } = useTranslation('admin');

    const title = t('placeholders.auditLogs.title');
    const desc = t('placeholders.auditLogs.description');

    const [userId, setUserId] = useState('');
    const [entityType, setEntityType] = useState('');
    const [fromDate, setFromDate] = useState('');
    const [toDate, setToDate] = useState('');

    const [logs, setLogs] = useState<AdminAuditLog[]>([]);

    const loadLogs = useMutation({
        mutationFn: async () => {
            const from = fromDate ? new Date(fromDate) : undefined;
            const to = toDate ? new Date(toDate) : undefined;
            return adminApi.getAuditLogs({
                userId: userId.trim() || undefined,
                entityType: entityType.trim() || undefined,
                fromDate: from,
                toDate: to,
            });
        },
        onSuccess: (data) => setLogs(data),
        onError: () => toast.error(t('auditLogs.load.error')),
    });

    return (
        <div className="space-y-6">
            <div>
                <h1 className="text-2xl font-bold text-gray-900">{title}</h1>
                <p className="mt-1 text-sm text-gray-600">{desc}</p>
            </div>

            <Card className="space-y-4 p-5">
                <h2 className="text-lg font-semibold text-gray-900">{t('auditLogs.filter.sectionTitle')}</h2>
                <div className="grid gap-4 md:grid-cols-2">
                    <div className="space-y-2">
                        <label className="block text-sm font-medium text-gray-700">{t('auditLogs.filter.userIdLabel')}</label>
                        <input
                            value={userId}
                            onChange={(e) => setUserId(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder={t('auditLogs.filter.optionalPlaceholder')}
                        />
                    </div>
                    <div className="space-y-2">
                        <label className="block text-sm font-medium text-gray-700">{t('auditLogs.filter.entityTypeLabel')}</label>
                        <input
                            value={entityType}
                            onChange={(e) => setEntityType(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder={t('auditLogs.filter.optionalPlaceholder')}
                        />
                    </div>
                    <div className="space-y-2">
                        <label className="block text-sm font-medium text-gray-700">{t('auditLogs.filter.fromDateLabel')}</label>
                        <input
                            type="date"
                            value={fromDate}
                            onChange={(e) => setFromDate(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                        />
                    </div>
                    <div className="space-y-2">
                        <label className="block text-sm font-medium text-gray-700">{t('auditLogs.filter.toDateLabel')}</label>
                        <input
                            type="date"
                            value={toDate}
                            onChange={(e) => setToDate(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                        />
                    </div>
                </div>
                <div className="flex justify-end">
                    <Button
                        className="bg-[#00b14f] hover:bg-[#00b14f]/90"
                        disabled={loadLogs.isPending}
                        onClick={() => loadLogs.mutate()}
                    >
                        {t('auditLogs.filter.loadButton')}
                    </Button>
                </div>
            </Card>

            <Card className="overflow-hidden p-0">
                <div className="overflow-x-auto">
                    <table className="w-full min-w-[820px] text-left text-sm">
                        <thead className="border-b border-gray-100 bg-gray-50 text-xs font-semibold uppercase text-gray-500">
                            <tr>
                                <th className="px-4 py-3">{t('auditLogs.table.created')}</th>
                                <th className="px-4 py-3">{t('auditLogs.table.action')}</th>
                                <th className="px-4 py-3">{t('auditLogs.table.entity')}</th>
                                <th className="px-4 py-3">{t('auditLogs.table.endpoint')}</th>
                                <th className="px-4 py-3">{t('auditLogs.table.status')}</th>
                                <th className="px-4 py-3">{t('auditLogs.table.error')}</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-gray-100">
                            {logs.length === 0 ? (
                                <tr>
                                    <td colSpan={6} className="px-4 py-12 text-center text-gray-500">
                                        {loadLogs.isPending ? t('auditLogs.table.loading') : t('auditLogs.table.empty')}
                                    </td>
                                </tr>
                            ) : (
                                logs.map((l) => (
                                    <tr key={l.id} className="hover:bg-gray-50/80">
                                        <td className="px-4 py-3 text-gray-700">
                                            {new Date(l.createdAt).toLocaleString()}
                                        </td>
                                        <td className="px-4 py-3 font-medium text-gray-900">{l.action}</td>
                                        <td className="px-4 py-3 text-gray-700">
                                            {l.entityType ? `${l.entityType}${l.entityId ? `:${l.entityId}` : ''}` : '-'}
                                        </td>
                                        <td className="px-4 py-3 text-gray-700">
                                            {l.httpMethod} {l.requestPath}
                                        </td>
                                        <td className="px-4 py-3 text-gray-700">
                                            {l.responseStatusCode ?? '-'}
                                        </td>
                                        <td className="px-4 py-3 text-gray-700">
                                            {l.errorMessage ?? '-'}
                                        </td>
                                    </tr>
                                ))
                            )}
                        </tbody>
                    </table>
                </div>
            </Card>
        </div>
    );
}

