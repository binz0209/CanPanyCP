import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import {
    Search, CheckCircle, XCircle, ChevronLeft, ChevronRight,
    Loader2, AlertTriangle, CreditCard
} from 'lucide-react';
import { Button, Badge, Card } from '../../components/ui';
import { adminApi } from '../../api';
import { formatRelativeTime, formatCurrency } from '../../utils';

const STATUS_OPTIONS = ['', 'Pending', 'Approved', 'Rejected'];

const statusColor: Record<string, string> = {
    Pending:  'bg-amber-50 text-amber-700 border-amber-200',
    Approved: 'bg-green-50 text-green-700 border-green-200',
    Rejected: 'bg-red-50 text-red-700 border-red-200',
};

export function AdminPaymentsPage() {
    const { t } = useTranslation('admin');
    const qc = useQueryClient();

    const [statusFilter, setStatusFilter] = useState('Pending');
    const [search, setSearch] = useState('');
    const [page, setPage] = useState(1);
    const PAGE_SIZE = 15;

    // Reject modal state
    const [rejectTarget, setRejectTarget] = useState<{ id: string } | null>(null);
    const [rejectReason, setRejectReason] = useState('');

    const { data: allPayments = [], isLoading } = useQuery({
        queryKey: ['admin-payments', statusFilter],
        queryFn: () => adminApi.getPayments(statusFilter || undefined),
    });

    const filtered = allPayments.filter((p: any) => {
        if (!search.trim()) return true;
        const q = search.toLowerCase();
        return (p.id ?? '').toLowerCase().includes(q) ||
               (p.userId ?? '').toLowerCase().includes(q) ||
               (p.packageId ?? '').toLowerCase().includes(q);
    });
    const totalPages = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE));
    const paged = filtered.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE);

    const approveMutation = useMutation({
        mutationFn: (id: string) => adminApi.approvePayment(id),
        onSuccess: () => {
            toast.success(t('payments.approve.success'));
            qc.invalidateQueries({ queryKey: ['admin-payments'] });
        },
        onError: () => toast.error(t('payments.approve.error')),
    });

    const rejectMutation = useMutation({
        mutationFn: () => adminApi.rejectPayment(rejectTarget!.id, rejectReason.trim()),
        onSuccess: () => {
            toast.success(t('payments.reject.success'));
            qc.invalidateQueries({ queryKey: ['admin-payments'] });
            setRejectTarget(null);
            setRejectReason('');
        },
        onError: () => toast.error(t('payments.reject.error')),
    });

    const fmt = (v: unknown): string => {
        if (v === null || v === undefined) return '—';
        if (typeof v === 'number') return formatCurrency(v);
        return String(v);
    };

    return (
        <div className="space-y-6">
            <div>
                <h1 className="text-2xl font-bold text-gray-900">{t('placeholders.payments.title')}</h1>
            </div>

            {/* Filters */}
            <Card className="p-4">
                <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
                    <div className="relative flex-1">
                        <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-gray-400" />
                        <input
                            value={search}
                            onChange={(e) => { setSearch(e.target.value); setPage(1); }}
                            placeholder="Search by payment ID, user ID…"
                            className="w-full rounded-lg border border-gray-200 py-2 pl-9 pr-3 text-sm outline-none focus:border-slate-900"
                        />
                    </div>
                    <select
                        value={statusFilter}
                        onChange={(e) => { setStatusFilter(e.target.value); setPage(1); }}
                        className="rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-slate-900"
                    >
                        {STATUS_OPTIONS.map((s) => (
                            <option key={s} value={s}>{s || 'All Statuses'}</option>
                        ))}
                    </select>
                    <span className="text-sm text-gray-500 whitespace-nowrap">{filtered.length} payments</span>
                </div>
            </Card>

            {/* Table */}
            <Card className="overflow-hidden p-0">
                {isLoading ? (
                    <div className="flex items-center justify-center py-20">
                        <Loader2 className="h-6 w-6 animate-spin text-gray-400" />
                    </div>
                ) : paged.length === 0 ? (
                    <div className="py-16 text-center">
                        <CreditCard className="mx-auto h-10 w-10 text-gray-300" />
                        <p className="mt-3 text-sm text-gray-500">No payments found</p>
                    </div>
                ) : (
                    <div className="overflow-x-auto">
                        <table className="w-full text-sm">
                            <thead className="border-b border-gray-100 bg-gray-50">
                                <tr>
                                    <th className="px-4 py-3 text-left font-medium text-gray-600">Payment ID</th>
                                    <th className="px-4 py-3 text-left font-medium text-gray-600">User</th>
                                    <th className="px-4 py-3 text-left font-medium text-gray-600">Amount</th>
                                    <th className="px-4 py-3 text-left font-medium text-gray-600">Method</th>
                                    <th className="px-4 py-3 text-left font-medium text-gray-600">Status</th>
                                    <th className="px-4 py-3 text-left font-medium text-gray-600">Date</th>
                                    <th className="px-4 py-3 text-right font-medium text-gray-600">Actions</th>
                                </tr>
                            </thead>
                            <tbody className="divide-y divide-gray-50">
                                {paged.map((p: any) => {
                                    const status: string = p.status ?? p.Status ?? 'Unknown';
                                    const isPending = status === 'Pending';
                                    return (
                                        <tr key={p.id} className="hover:bg-gray-50/80">
                                            <td className="px-4 py-3 font-mono text-xs text-gray-500">
                                                {(p.id ?? '').slice(0, 16)}…
                                            </td>
                                            <td className="px-4 py-3 text-gray-700 font-mono text-xs">
                                                {(p.userId ?? p.UserId ?? '—').slice(0, 12)}…
                                            </td>
                                            <td className="px-4 py-3 font-semibold text-gray-900">
                                                {fmt(p.amount ?? p.Amount)}
                                            </td>
                                            <td className="px-4 py-3 text-gray-500">
                                                {p.paymentMethod ?? p.PaymentMethod ?? '—'}
                                            </td>
                                            <td className="px-4 py-3">
                                                <Badge className={`text-xs ${statusColor[status] ?? 'bg-gray-100 text-gray-600'}`}>
                                                    {status}
                                                </Badge>
                                            </td>
                                            <td className="px-4 py-3 text-gray-500 whitespace-nowrap">
                                                {p.createdAt ? formatRelativeTime(p.createdAt) : '—'}
                                            </td>
                                            <td className="px-4 py-3">
                                                {isPending && (
                                                    <div className="flex items-center justify-end gap-1">
                                                        <button
                                                            title="Approve"
                                                            onClick={() => approveMutation.mutate(p.id)}
                                                            disabled={approveMutation.isPending}
                                                            className="rounded p-1.5 text-gray-400 hover:bg-green-50 hover:text-green-600 disabled:opacity-50"
                                                        >
                                                            <CheckCircle className="h-4 w-4" />
                                                        </button>
                                                        <button
                                                            title="Reject"
                                                            onClick={() => setRejectTarget({ id: p.id })}
                                                            className="rounded p-1.5 text-gray-400 hover:bg-red-50 hover:text-red-600"
                                                        >
                                                            <XCircle className="h-4 w-4" />
                                                        </button>
                                                    </div>
                                                )}
                                            </td>
                                        </tr>
                                    );
                                })}
                            </tbody>
                        </table>
                    </div>
                )}

                {totalPages > 1 && (
                    <div className="flex items-center justify-between border-t border-gray-100 px-4 py-3">
                        <span className="text-sm text-gray-500">Page {page} of {totalPages}</span>
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

            {/* Reject Modal */}
            {rejectTarget && (
                <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
                    <div className="w-full max-w-md rounded-2xl bg-white p-6 shadow-xl">
                        <div className="flex items-center gap-3 mb-4">
                            <div className="flex h-10 w-10 items-center justify-center rounded-full bg-red-50">
                                <AlertTriangle className="h-5 w-5 text-red-600" />
                            </div>
                            <div>
                                <h2 className="font-bold text-gray-900">Reject Payment</h2>
                                <p className="text-xs font-mono text-gray-400">{rejectTarget.id}</p>
                            </div>
                        </div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">Reason (required)</label>
                        <textarea
                            value={rejectReason}
                            onChange={(e) => setRejectReason(e.target.value)}
                            rows={3}
                            placeholder="Enter rejection reason…"
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-red-300 resize-none"
                        />
                        <div className="mt-4 flex justify-end gap-2">
                            <Button variant="outline" onClick={() => { setRejectTarget(null); setRejectReason(''); }}>Cancel</Button>
                            <Button
                                className="bg-red-600 hover:bg-red-700 text-white"
                                disabled={!rejectReason.trim() || rejectMutation.isPending}
                                onClick={() => rejectMutation.mutate()}
                            >
                                {rejectMutation.isPending ? <Loader2 className="h-4 w-4 animate-spin" /> : 'Reject'}
                            </Button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}
