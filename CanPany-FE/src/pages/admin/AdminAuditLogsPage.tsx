import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import { Search, Download, ChevronLeft, ChevronRight, Loader2, ScrollText } from 'lucide-react';
import { Button, Card } from '../../components/ui';
import { adminApi } from '../../api';
import type { AdminAuditLog } from '../../api/admin.api';


function getHumanActionLabel(l: AdminAuditLog, t: any) {
    // New AOP Strategy sends I18n keys directly
    if (l.action && l.action.startsWith('auditLogs.actions.')) {
        return t(l.action);
    }

    // Legacy fallback
    const m = (l.httpMethod || '').toUpperCase();
    const entityLocal = l.entityType ? t(`auditLogs.entities.${l.entityType}`, l.entityType) : '';

    if (m === 'GET') {
        return entityLocal 
            ? t('auditLogs.actions.readEntity', { entity: entityLocal, defaultValue: `Xem chi tiết ${entityLocal}` }).trim()
            : t('auditLogs.actions.read', 'Xem chi tiết');
    }
    if (m === 'POST') {
        return entityLocal 
            ? t('auditLogs.actions.createEntity', { entity: entityLocal, defaultValue: `Tạo mới ${entityLocal}` }).trim()
            : t('auditLogs.actions.create', 'Tạo mới');
    }
    if (m === 'PUT' || m === 'PATCH') {
        return entityLocal 
            ? t('auditLogs.actions.updateEntity', { entity: entityLocal, defaultValue: `Cập nhật ${entityLocal}` }).trim()
            : t('auditLogs.actions.update', 'Cập nhật');
    }
    if (m === 'DELETE') {
        return entityLocal 
            ? t('auditLogs.actions.deleteEntity', { entity: entityLocal, defaultValue: `Xóa ${entityLocal}` }).trim()
            : t('auditLogs.actions.delete', 'Xóa');
    }

    return l.action;
}

export function AdminAuditLogsPage() {
    const { t } = useTranslation('admin');

    // Filters (applied on "Load" click to avoid hammering API on every keystroke)
    const [userId, setUserId] = useState('');
    const [entityType, setEntityType] = useState('');
    const [fromDate, setFromDate] = useState('');
    const [toDate, setToDate] = useState('');

    // Committed filters (sent to API)
    const [committed, setCommitted] = useState({ userId: '', entityType: '', fromDate: '', toDate: '' });

    // Client-side search + pagination
    const [search, setSearch] = useState('');
    const [page, setPage] = useState(1);
    const PAGE_SIZE = 20;

    const { data: logs = [], isLoading, isFetching } = useQuery<AdminAuditLog[]>({
        queryKey: ['admin-audit-logs', committed],
        queryFn: () => adminApi.getAuditLogs({
            userId: committed.userId || undefined,
            entityType: committed.entityType || undefined,
            fromDate: committed.fromDate ? new Date(committed.fromDate) : undefined,
            toDate: committed.toDate ? new Date(committed.toDate) : undefined,
        }),
    });

    const filtered = logs.filter((l) => {
        if (!search.trim()) return true;
        const q = search.toLowerCase();
        return (
            l.action.toLowerCase().includes(q) ||
            (l.userEmail ?? '').toLowerCase().includes(q) ||
            (l.requestPath ?? '').toLowerCase().includes(q) ||
            (l.entityType ?? '').toLowerCase().includes(q)
        );
    });
    const totalPages = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE));
    const paged = filtered.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE);

    const handleLoad = () => {
        setCommitted({ userId: userId.trim(), entityType: entityType.trim(), fromDate, toDate });
        setPage(1);
    };

    const handleExport = () => {
        const params = new URLSearchParams();
        if (committed.userId) params.set('userId', committed.userId);
        if (committed.entityType) params.set('entityType', committed.entityType);
        if (committed.fromDate) params.set('fromDate', new Date(committed.fromDate).toISOString());
        if (committed.toDate) params.set('toDate', new Date(committed.toDate).toISOString());
        window.open(`/api/admin/audit-logs/export?${params.toString()}`, '_blank');
    };

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <h1 className="text-2xl font-bold text-gray-900">{t('placeholders.auditLogs.title')}</h1>
                <Button variant="outline" className="gap-2" onClick={handleExport}>
                    <Download className="h-4 w-4" />
                    Export CSV
                </Button>
            </div>

            {/* Filter form */}
            <Card className="p-5">
                <h2 className="mb-4 text-sm font-semibold text-gray-700">{t('auditLogs.filter.sectionTitle')}</h2>
                <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
                    <div>
                        <label className="mb-1 block text-xs font-medium text-gray-500">{t('auditLogs.filter.userIdLabel')}</label>
                        <input
                            value={userId}
                            onChange={(e) => setUserId(e.target.value)}
                            placeholder="(optional)"
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-slate-900"
                        />
                    </div>
                    <div>
                        <label className="mb-1 block text-xs font-medium text-gray-500">{t('auditLogs.filter.entityTypeLabel')}</label>
                        <input
                            value={entityType}
                            onChange={(e) => setEntityType(e.target.value)}
                            placeholder="e.g. Job, User…"
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-slate-900"
                        />
                    </div>
                    <div>
                        <label className="mb-1 block text-xs font-medium text-gray-500">{t('auditLogs.filter.fromDateLabel')}</label>
                        <input
                            type="date"
                            value={fromDate}
                            onChange={(e) => setFromDate(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-slate-900"
                        />
                    </div>
                    <div>
                        <label className="mb-1 block text-xs font-medium text-gray-500">{t('auditLogs.filter.toDateLabel')}</label>
                        <input
                            type="date"
                            value={toDate}
                            onChange={(e) => setToDate(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-slate-900"
                        />
                    </div>
                </div>
                <div className="mt-3 flex items-center justify-between">
                    <span className="text-xs text-gray-400">{filtered.length} records</span>
                    <Button
                        className="bg-slate-900 hover:bg-slate-800 text-white"
                        disabled={isFetching}
                        onClick={handleLoad}
                    >
                        {isFetching ? <Loader2 className="h-4 w-4 animate-spin mr-1" /> : null}
                        {t('auditLogs.filter.loadButton')}
                    </Button>
                </div>
            </Card>

            {/* Search bar */}
            <div className="relative">
                <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-gray-400" />
                <input
                    value={search}
                    onChange={(e) => { setSearch(e.target.value); setPage(1); }}
                    placeholder="Search action, email, path…"
                    className="w-full rounded-lg border border-gray-200 py-2 pl-9 pr-4 text-sm shadow-sm outline-none focus:border-slate-900"
                />
            </div>

            {/* Table */}
            <Card className="overflow-hidden p-0">
                {isLoading ? (
                    <div className="flex items-center justify-center py-20">
                        <Loader2 className="h-6 w-6 animate-spin text-gray-400" />
                    </div>
                ) : paged.length === 0 ? (
                    <div className="py-16 text-center">
                        <ScrollText className="mx-auto h-10 w-10 text-gray-300" />
                        <p className="mt-3 text-sm text-gray-500">{t('auditLogs.table.empty')}</p>
                    </div>
                ) : (
                    <div className="overflow-x-auto">
                        <table className="w-full min-w-[860px] text-sm">
                            <thead className="border-b border-gray-100 bg-gray-50">
                                <tr>
                                    <th className="px-4 py-3 text-left font-medium text-gray-600">{t('auditLogs.table.time')}</th>
                                    <th className="px-4 py-3 text-left font-medium text-gray-600">{t('auditLogs.table.userId')}</th>
                                    <th className="px-4 py-3 text-left font-medium text-gray-600">{t('auditLogs.table.username')}</th>
                                    <th className="px-4 py-3 text-left font-medium text-gray-600">{t('auditLogs.table.action')}</th>
                                </tr>
                            </thead>
                            <tbody className="divide-y divide-gray-50">
                                {paged.map((l) => (
                                    <tr key={l.id} className="hover:bg-gray-50/80">
                                        <td className="px-4 py-3 text-xs text-gray-500 whitespace-nowrap">
                                            {new Date(l.createdAt).toLocaleString()}
                                        </td>
                                        <td className="px-4 py-3 text-xs text-gray-500 font-mono">
                                            {l.userId ?? '—'}
                                        </td>
                                        <td className="px-4 py-3 text-xs text-gray-500">
                                            {l.userEmail ?? '—'}
                                        </td>
                                        <td className="px-4 py-3 font-medium text-gray-900">
                                            {getHumanActionLabel(l, t)}
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>
                )}

                {totalPages > 1 && (
                    <div className="flex items-center justify-between border-t border-gray-100 px-4 py-3">
                        <span className="text-sm text-gray-500">Page {page} of {totalPages} · {filtered.length} records</span>
                        <div className="flex gap-1">
                            <Button variant="outline" size="sm" disabled={page === 1} onClick={() => setPage(p => p - 1)}>
                                <ChevronLeft className="h-4 w-4" />
                            </Button>
                            <Button variant="outline" size="sm" disabled={page === totalPages} onClick={() => setPage(p => p + 1)}>
                                <ChevronRight className="h-4 w-4" />
                            </Button>
                        </div>
                    </div>
                )}
            </Card>
        </div>
    );
}
