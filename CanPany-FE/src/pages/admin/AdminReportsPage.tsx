import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import {
    Search, CheckCircle, XCircle, Eye, ChevronLeft, ChevronRight,
    Loader2, ShieldAlert, Clock, Flag
} from 'lucide-react';
import { Button, Badge, Card } from '../../components/ui';
import { adminApi } from '../../api';
import type { AdminReportDetails } from '../../api/admin.api';
import { formatRelativeTime } from '../../utils';

const statusColor: Record<string, string> = {
    Pending:  'bg-amber-50 text-amber-700 border-amber-200',
    Open:     'bg-amber-50 text-amber-700 border-amber-200',
    Resolved: 'bg-green-50 text-green-700 border-green-200',
    Rejected: 'bg-red-50 text-red-700 border-red-200',
    Closed:   'bg-gray-100 text-gray-600 border-gray-200',
};

const PAGE_SIZE = 15;

/* ─── mini detail drawer ─── */
function ReportDetailModal({
    report,
    onClose,
    onResolve,
    onReject,
}: {
    report: AdminReportDetails;
    onClose: () => void;
    onResolve: (note: string, ban: boolean) => void;
    onReject: (reason: string) => void;
}) {
    const [resolveNote, setResolveNote] = useState('');
    const [banUser, setBanUser] = useState(false);
    const [rejectReason, setRejectReason] = useState('');
    const [tab, setTab] = useState<'resolve' | 'reject'>('resolve');

    const isPending = report.status === 'Pending' || report.status === 'Open';

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
            <div className="w-full max-w-xl rounded-2xl bg-white shadow-xl max-h-[90vh] flex flex-col">
                {/* Header */}
                <div className="flex items-center gap-3 border-b border-gray-100 px-6 py-4">
                    <div className="flex h-10 w-10 items-center justify-center rounded-full bg-red-50">
                        <ShieldAlert className="h-5 w-5 text-red-600" />
                    </div>
                    <div className="flex-1 min-w-0">
                        <h2 className="font-bold text-gray-900 truncate">{t('reports.modal.title')}</h2>
                        <p className="text-xs font-mono text-gray-400">{report.id}</p>
                    </div>
                    <Badge className={`text-xs ${statusColor[report.status] ?? 'bg-gray-100 text-gray-600'}`}>
                        {report.status}
                    </Badge>
                </div>

                {/* Body */}
                <div className="overflow-y-auto flex-1 px-6 py-4 space-y-4">
                    {/* Reporter & reported */}
                    <div className="grid grid-cols-2 gap-4">
                        <div className="rounded-lg bg-gray-50 p-3">
                            <p className="text-xs font-medium text-gray-400 mb-1">{t('reports.modal.reporter')}</p>
                            <p className="text-sm font-semibold text-gray-900">{report.reporter.fullName}</p>
                            <p className="text-xs text-gray-500">{report.reporter.email}</p>
                        </div>
                        {report.reportedUser && (
                            <div className="rounded-lg bg-red-50 p-3">
                                <p className="text-xs font-medium text-red-400 mb-1">{t('reports.modal.reportedUser')}</p>
                                <p className="text-sm font-semibold text-gray-900">{report.reportedUser.fullName}</p>
                                <p className="text-xs text-gray-500">{report.reportedUser.email}</p>
                            </div>
                        )}
                    </div>

                    {/* Reason & description */}
                    <div>
                        <p className="text-xs font-medium text-gray-400 mb-1">{t('reports.modal.reason')}</p>
                        <p className="text-sm font-semibold text-gray-900">{report.reason}</p>
                    </div>
                    <div>
                        <p className="text-xs font-medium text-gray-400 mb-1">{t('reports.modal.description')}</p>
                        <p className="text-sm text-gray-700 whitespace-pre-wrap">{report.description}</p>
                    </div>

                    {/* Evidence */}
                    {report.evidence && report.evidence.length > 0 && (
                        <div>
                            <p className="text-xs font-medium text-gray-400 mb-1">{t('reports.modal.evidence')}</p>
                            <ul className="list-disc pl-5 text-sm text-gray-700 space-y-0.5">
                                {report.evidence.map((e, i) => <li key={i}>{e}</li>)}
                            </ul>
                        </div>
                    )}

                    {/* Resolution note (if already resolved) */}
                    {report.resolutionNote && (
                        <div className="rounded-lg bg-green-50 p-3">
                            <p className="text-xs font-medium text-green-600 mb-1">{t('reports.modal.resolutionNote')}</p>
                            <p className="text-sm text-gray-800">{report.resolutionNote}</p>
                        </div>
                    )}

                    {/* Actions — only for pending */}
                    {isPending && (
                        <div className="border-t border-gray-100 pt-4">
                            {/* tab switcher */}
                            <div className="flex gap-1 rounded-lg bg-gray-100 p-1 mb-3">
                                <button
                                    onClick={() => setTab('resolve')}
                                    className={`flex-1 rounded-md py-1.5 text-xs font-medium transition-all ${tab === 'resolve' ? 'bg-white text-green-700 shadow-sm' : 'text-gray-500'}`}
                                >
                                    ✅ {t('reports.modal.resolveTab')}
                                </button>
                                <button
                                    onClick={() => setTab('reject')}
                                    className={`flex-1 rounded-md py-1.5 text-xs font-medium transition-all ${tab === 'reject' ? 'bg-white text-red-700 shadow-sm' : 'text-gray-500'}`}
                                >
                                    ❌ {t('reports.modal.rejectTab')}
                                </button>
                            </div>

                            {tab === 'resolve' ? (
                                <div className="space-y-2">
                                    <textarea
                                        value={resolveNote}
                                        onChange={(e) => setResolveNote(e.target.value)}
                                        rows={3}
                                        placeholder={t('reports.modal.resolvePlaceholder')}
                                        className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-green-400 resize-none"
                                    />
                                    <label className="flex items-center gap-2 text-sm text-gray-700">
                                        <input type="checkbox" checked={banUser} onChange={(e) => setBanUser(e.target.checked)} />
                                        {t('reports.modal.banUser')}
                                    </label>
                                    <div className="flex justify-end">
                                        <Button
                                            className="bg-green-600 hover:bg-green-700 text-white"
                                            disabled={!resolveNote.trim()}
                                            onClick={() => onResolve(resolveNote.trim(), banUser)}
                                        >
                                            <CheckCircle className="h-4 w-4 mr-1" /> {t('reports.modal.resolve')}
                                        </Button>
                                    </div>
                                </div>
                            ) : (
                                <div className="space-y-2">
                                    <textarea
                                        value={rejectReason}
                                        onChange={(e) => setRejectReason(e.target.value)}
                                        rows={3}
                                        placeholder={t('reports.modal.rejectPlaceholder')}
                                        className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-red-300 resize-none"
                                    />
                                    <div className="flex justify-end">
                                        <Button
                                            className="bg-red-600 hover:bg-red-700 text-white"
                                            disabled={!rejectReason.trim()}
                                            onClick={() => onReject(rejectReason.trim())}
                                        >
                                            <XCircle className="h-4 w-4 mr-1" /> {t('reports.modal.reject')}
                                        </Button>
                                    </div>
                                </div>
                            )}
                        </div>
                    )}
                </div>

                {/* Footer */}
                <div className="border-t border-gray-100 px-6 py-3 flex justify-between items-center">
                    <span className="text-xs text-gray-400">
                        {formatRelativeTime(report.createdAt.toString())}
                        {report.resolvedAt ? ` · ${t('reports.tabs.resolved')} ${formatRelativeTime(report.resolvedAt.toString())}` : ''}
                    </span>
                    <Button variant="outline" size="sm" onClick={onClose}>{t('reports.modal.close')}</Button>
                </div>
            </div>
        </div>
    );
}

/* ─── report table ─── */
function ReportTable({
    reports,
    onView,
    t
}: {
    reports: AdminReportDetails[];
    onView: (r: AdminReportDetails) => void;
    t: any;
}) {
    const [search, setSearch] = useState('');
    const [page, setPage] = useState(1);

    const filtered = reports.filter((r) => {
        if (!search.trim()) return true;
        const q = search.toLowerCase();
        return (
            r.reason.toLowerCase().includes(q) ||
            (r.reporter?.email ?? '').toLowerCase().includes(q) ||
            (r.reportedUser?.email ?? '').toLowerCase().includes(q) ||
            r.id.toLowerCase().includes(q)
        );
    });
    const totalPages = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE));
    const paged = filtered.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE);

    return (
        <div className="space-y-3">
            <div className="relative">
                <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-gray-400" />
                <input
                    value={search}
                    onChange={(e) => { setSearch(e.target.value); setPage(1); }}
                    placeholder={t('reports.table.search')}
                    className="w-full rounded-lg border border-gray-200 py-2 pl-9 pr-3 text-sm outline-none focus:border-slate-900"
                />
            </div>

            <div className="overflow-x-auto rounded-xl border border-gray-100">
                <table className="w-full text-sm">
                    <thead className="bg-gray-50 border-b border-gray-100">
                        <tr>
                            <th className="px-4 py-3 text-left font-medium text-gray-600">{t('reports.table.reporter')}</th>
                            <th className="px-4 py-3 text-left font-medium text-gray-600">{t('reports.table.reported')}</th>
                            <th className="px-4 py-3 text-left font-medium text-gray-600">{t('reports.table.reason')}</th>
                            <th className="px-4 py-3 text-left font-medium text-gray-600">{t('reports.table.status')}</th>
                            <th className="px-4 py-3 text-left font-medium text-gray-600">{t('reports.table.date')}</th>
                            <th className="px-4 py-3 text-right font-medium text-gray-600">{t('reports.table.action')}</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-50">
                        {paged.length === 0 ? (
                            <tr><td colSpan={6} className="px-4 py-10 text-center text-sm text-gray-400">{t('reports.table.empty')}</td></tr>
                        ) : paged.map((r) => (
                            <tr key={r.id} className="hover:bg-gray-50/80">
                                <td className="px-4 py-3">
                                    <p className="text-sm font-medium text-gray-900">{r.reporter.fullName}</p>
                                    <p className="text-xs text-gray-400">{r.reporter.email}</p>
                                </td>
                                <td className="px-4 py-3">
                                    {r.reportedUser ? (
                                        <>
                                            <p className="text-sm font-medium text-gray-900">{r.reportedUser.fullName}</p>
                                            <p className="text-xs text-gray-400">{r.reportedUser.email}</p>
                                        </>
                                    ) : <span className="text-gray-400">—</span>}
                                </td>
                                <td className="px-4 py-3 max-w-[160px]">
                                    <p className="truncate text-sm text-gray-800">{r.reason}</p>
                                </td>
                                <td className="px-4 py-3">
                                    <Badge className={`text-xs ${statusColor[r.status] ?? 'bg-gray-100 text-gray-600'}`}>
                                        {r.status}
                                    </Badge>
                                </td>
                                <td className="px-4 py-3 text-xs text-gray-500 whitespace-nowrap">
                                    {formatRelativeTime(r.createdAt.toString())}
                                </td>
                                <td className="px-4 py-3">
                                    <div className="flex justify-end">
                                        <button
                                            onClick={() => onView(r)}
                                            className="rounded p-1.5 text-gray-400 hover:bg-blue-50 hover:text-blue-600"
                                            title="View & act"
                                        >
                                            <Eye className="h-4 w-4" />
                                        </button>
                                    </div>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>

            {totalPages > 1 && (
                <div className="flex items-center justify-between">
                    <span className="text-xs text-gray-400">{filtered.length} records</span>
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
        </div>
    );
}

/* ─── main page ─── */
export function AdminReportsPage() {
    const { t } = useTranslation('admin');
    const qc = useQueryClient();
    const [selected, setSelected] = useState<AdminReportDetails | null>(null);

    const { data: allReports = [], isLoading } = useQuery<AdminReportDetails[]>({
        queryKey: ['admin-reports'],
        queryFn: () => adminApi.getReports(),
        select: (data) => [...data].sort((a, b) =>
            new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
        ),
    });

    // Split
    const pending  = allReports.filter((r) => r.status === 'Pending' || r.status === 'Open');
    const resolved = allReports.filter((r) => r.status !== 'Pending' && r.status !== 'Open');

    const resolveMutation = useMutation({
        mutationFn: ({ note, ban }: { note: string; ban: boolean }) =>
            adminApi.resolveReport(selected!.id, { resolutionNote: note, banUser: ban }),
        onSuccess: () => {
            toast.success(t('reports.resolve.success'));
            qc.invalidateQueries({ queryKey: ['admin-reports'] });
            setSelected(null);
        },
        onError: () => toast.error(t('reports.resolve.error')),
    });

    const rejectMutation = useMutation({
        mutationFn: (reason: string) =>
            adminApi.rejectReport(selected!.id, { rejectionReason: reason }),
        onSuccess: () => {
            toast.success(t('reports.reject.success'));
            qc.invalidateQueries({ queryKey: ['admin-reports'] });
            setSelected(null);
        },
        onError: () => toast.error(t('reports.reject.error')),
    });

    return (
        <div className="space-y-6">
            {/* Header */}
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold text-gray-900">{t('placeholders.reports.title')}</h1>
                </div>
                <div className="flex items-center gap-3">
                    <div className="flex items-center gap-1.5 rounded-full bg-amber-50 px-3 py-1.5 text-sm font-medium text-amber-700">
                        <Clock className="h-4 w-4" />
                        {pending.length} pending
                    </div>
                    <div className="flex items-center gap-1.5 rounded-full bg-green-50 px-3 py-1.5 text-sm font-medium text-green-700">
                        <CheckCircle className="h-4 w-4" />
                        {resolved.length} resolved
                    </div>
                </div>
            </div>

            {isLoading ? (
                <div className="flex items-center justify-center py-24">
                    <Loader2 className="h-6 w-6 animate-spin text-gray-400" />
                </div>
            ) : (
                <div className="space-y-6">
                    {/* ── Pending ── */}
                    <Card className="p-5">
                        <div className="flex items-center gap-2 mb-4">
                            <Flag className="h-4 w-4 text-amber-500" />
                            <h2 className="font-semibold text-gray-900">{t('reports.tabs.pending')}</h2>
                            <span className="ml-auto text-xs text-gray-400">{pending.length} reports</span>
                        </div>
                        <ReportTable reports={pending} onView={setSelected} t={t} />
                    </Card>

                    {/* ── Resolved ── */}
                    <Card className="p-5">
                        <div className="flex items-center gap-2 mb-4">
                            <CheckCircle className="h-4 w-4 text-green-500" />
                            <h2 className="font-semibold text-gray-900">{t('reports.tabs.resolved')}</h2>
                            <span className="ml-auto text-xs text-gray-400">{resolved.length} reports</span>
                        </div>
                        <ReportTable reports={resolved} onView={setSelected} t={t} />
                    </Card>
                </div>
            )}

            {/* Detail modal */}
            {selected && (
                <ReportDetailModalWrapper
                    report={selected}
                    onClose={() => setSelected(null)}
                    onResolve={(note, ban) => resolveMutation.mutate({ note, ban })}
                    onReject={(reason) => rejectMutation.mutate(reason)}
                />
            )}
        </div>
    );
}

function ReportDetailModalWrapper({
    report,
    onClose,
    onResolve,
    onReject,
}: {
    report: AdminReportDetails;
    onClose: () => void;
    onResolve: (note: string, ban: boolean) => void;
    onReject: (reason: string) => void;
}) {
    const { t } = useTranslation('admin');
    return (
        <ReportDetailModal 
            report={report} 
            onClose={onClose} 
            onResolve={onResolve} 
            onReject={onReject} 
            t={t} 
        />
    )
}

