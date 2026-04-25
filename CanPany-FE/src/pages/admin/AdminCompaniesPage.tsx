import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import toast from 'react-hot-toast';
import {
    Search, CheckCircle, XCircle, Eye, ChevronLeft, ChevronRight,
    Loader2, Building2, AlertTriangle
} from 'lucide-react';
import { Button, Badge, Card } from '../../components/ui';
import { adminApi } from '../../api';
import { formatRelativeTime } from '../../utils';

const STATUS_OPTIONS = ['', 'Pending', 'Verified', 'Rejected'];

const statusColor: Record<string, string> = {
    Pending:  'bg-amber-50 text-amber-700 border-amber-200',
    Verified: 'bg-green-50 text-green-700 border-green-200',
    Rejected: 'bg-red-50 text-red-700 border-red-200',
    Unverified: 'bg-gray-100 text-gray-600 border-gray-200',
};

export function AdminCompaniesPage() {
    const qc = useQueryClient();

    const [search, setSearch] = useState('');
    const [statusFilter, setStatusFilter] = useState('');
    const [page, setPage] = useState(1);
    const PAGE_SIZE = 15;

    // Modals
    const [detailCompany, setDetailCompany] = useState<any | null>(null);
    const [rejectTarget, setRejectTarget] = useState<{ id: string; name: string } | null>(null);
    const [rejectReason, setRejectReason] = useState('');

    const { data: allCompanies = [], isLoading } = useQuery({
        queryKey: ['admin-companies', statusFilter],
        queryFn: () => adminApi.getCompanies(statusFilter || undefined),
    });

    // Client-side search + pagination
    const filtered = allCompanies.filter((c: any) => {
        if (!search.trim()) return true;
        const q = search.toLowerCase();
        return (
            (c.name ?? '').toLowerCase().includes(q) ||
            (c.email ?? '').toLowerCase().includes(q) ||
            (c.id ?? '').toLowerCase().includes(q)
        );
    });
    const totalPages = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE));
    const paged = filtered.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE);

    const approveMutation = useMutation({
        mutationFn: (id: string) => adminApi.approveVerification(id),
        onSuccess: () => {
            toast.success('Company approved');
            qc.invalidateQueries({ queryKey: ['admin-companies'] });
        },
        onError: () => toast.error('Failed to approve'),
    });

    const rejectMutation = useMutation({
        mutationFn: () => adminApi.rejectVerification(rejectTarget!.id, rejectReason.trim()),
        onSuccess: () => {
            toast.success('Company rejected');
            qc.invalidateQueries({ queryKey: ['admin-companies'] });
            setRejectTarget(null);
            setRejectReason('');
        },
        onError: () => toast.error('Failed to reject'),
    });

    const verificationStatus = (c: any) =>
        c.verificationStatus ?? c.VerificationStatus ?? (c.isVerified ? 'Verified' : 'Unverified');

    return (
        <div className="space-y-6">
            {/* Header */}
            <div>
                <h1 className="text-2xl font-bold text-gray-900">Companies</h1>
                <p className="mt-1 text-sm text-gray-500">Browse, verify and manage all registered companies.</p>
            </div>

            {/* Filters */}
            <Card className="p-4">
                <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
                    <div className="relative flex-1">
                        <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-gray-400" />
                        <input
                            value={search}
                            onChange={(e) => { setSearch(e.target.value); setPage(1); }}
                            placeholder="Search by name, email, or ID…"
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
                    <span className="text-sm text-gray-500 whitespace-nowrap">{filtered.length} companies</span>
                </div>
            </Card>

            {/* Table */}
            <Card className="overflow-hidden p-0">
                {isLoading ? (
                    <div className="flex items-center justify-center py-20">
                        <Loader2 className="h-6 w-6 animate-spin text-gray-400" />
                    </div>
                ) : paged.length === 0 ? (
                    <div className="py-16 text-center text-sm text-gray-500">No companies found</div>
                ) : (
                    <div className="overflow-x-auto">
                        <table className="w-full text-sm">
                            <thead className="border-b border-gray-100 bg-gray-50">
                                <tr>
                                    <th className="px-4 py-3 text-left font-medium text-gray-600">Company</th>
                                    <th className="px-4 py-3 text-left font-medium text-gray-600">Email</th>
                                    <th className="px-4 py-3 text-left font-medium text-gray-600">Verification</th>
                                    <th className="px-4 py-3 text-left font-medium text-gray-600">Registered</th>
                                    <th className="px-4 py-3 text-right font-medium text-gray-600">Actions</th>
                                </tr>
                            </thead>
                            <tbody className="divide-y divide-gray-50">
                                {paged.map((company: any) => {
                                    const status = verificationStatus(company);
                                    const isPending = status === 'Pending' || status === 'Unverified';
                                    return (
                                        <tr key={company.id} className="hover:bg-gray-50/80">
                                            <td className="px-4 py-3">
                                                <div className="flex items-center gap-3">
                                                    {company.logoUrl ? (
                                                        <img src={company.logoUrl} alt="" className="h-9 w-9 rounded-lg object-contain border border-gray-100" />
                                                    ) : (
                                                        <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-gray-100">
                                                            <Building2 className="h-4 w-4 text-gray-400" />
                                                        </div>
                                                    )}
                                                    <div>
                                                        <p className="font-medium text-gray-900">{company.name ?? '—'}</p>
                                                        <p className="text-xs text-gray-400 font-mono">{company.id}</p>
                                                    </div>
                                                </div>
                                            </td>
                                            <td className="px-4 py-3 text-gray-500">{company.email ?? '—'}</td>
                                            <td className="px-4 py-3">
                                                <Badge className={`text-xs ${statusColor[status] ?? 'bg-gray-100 text-gray-600'}`}>
                                                    {status}
                                                </Badge>
                                            </td>
                                            <td className="px-4 py-3 text-gray-500 whitespace-nowrap">
                                                {company.createdAt ? formatRelativeTime(company.createdAt) : '—'}
                                            </td>
                                            <td className="px-4 py-3">
                                                <div className="flex items-center justify-end gap-1">
                                                    <button
                                                        title="View detail"
                                                        onClick={() => setDetailCompany(company)}
                                                        className="rounded p-1.5 text-gray-400 hover:bg-blue-50 hover:text-blue-600"
                                                    >
                                                        <Eye className="h-4 w-4" />
                                                    </button>
                                                    {isPending && (
                                                        <>
                                                            <button
                                                                title="Approve"
                                                                onClick={() => approveMutation.mutate(company.id)}
                                                                disabled={approveMutation.isPending}
                                                                className="rounded p-1.5 text-gray-400 hover:bg-green-50 hover:text-green-600 disabled:opacity-50"
                                                            >
                                                                <CheckCircle className="h-4 w-4" />
                                                            </button>
                                                            <button
                                                                title="Reject"
                                                                onClick={() => setRejectTarget({ id: company.id, name: company.name })}
                                                                className="rounded p-1.5 text-gray-400 hover:bg-red-50 hover:text-red-600"
                                                            >
                                                                <XCircle className="h-4 w-4" />
                                                            </button>
                                                        </>
                                                    )}
                                                </div>
                                            </td>
                                        </tr>
                                    );
                                })}
                            </tbody>
                        </table>
                    </div>
                )}

                {/* Pagination */}
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

            {/* Detail Modal */}
            {detailCompany && (
                <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
                    <div className="w-full max-w-lg rounded-2xl bg-white p-6 shadow-xl max-h-[80vh] overflow-y-auto">
                        <div className="flex items-center gap-3 mb-4">
                            {detailCompany.logoUrl ? (
                                <img src={detailCompany.logoUrl} alt="" className="h-12 w-12 rounded-xl object-contain border border-gray-100" />
                            ) : (
                                <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-gray-100">
                                    <Building2 className="h-6 w-6 text-gray-400" />
                                </div>
                            )}
                            <div>
                                <h2 className="text-lg font-bold text-gray-900">{detailCompany.name}</h2>
                                <p className="text-sm text-gray-500">{detailCompany.email}</p>
                            </div>
                        </div>
                        <div className="space-y-2 text-sm">
                            {Object.entries(detailCompany)
                                .filter(([k]) => !['logoUrl'].includes(k))
                                .map(([k, v]) => (
                                    <div key={k} className="flex gap-2">
                                        <span className="w-36 shrink-0 font-medium text-gray-500 capitalize">{k}:</span>
                                        <span className="break-all text-gray-800">{String(v)}</span>
                                    </div>
                                ))}
                        </div>
                        <div className="mt-6 flex justify-end gap-2">
                            {(verificationStatus(detailCompany) === 'Pending' || verificationStatus(detailCompany) === 'Unverified') && (
                                <>
                                    <Button
                                        className="bg-green-600 hover:bg-green-700 text-white"
                                        onClick={() => { approveMutation.mutate(detailCompany.id); setDetailCompany(null); }}
                                    >
                                        <CheckCircle className="h-4 w-4 mr-1" /> Approve
                                    </Button>
                                    <Button
                                        variant="outline"
                                        className="border-red-200 text-red-600 hover:bg-red-50"
                                        onClick={() => { setRejectTarget({ id: detailCompany.id, name: detailCompany.name }); setDetailCompany(null); }}
                                    >
                                        <XCircle className="h-4 w-4 mr-1" /> Reject
                                    </Button>
                                </>
                            )}
                            <Button variant="outline" onClick={() => setDetailCompany(null)}>Close</Button>
                        </div>
                    </div>
                </div>
            )}

            {/* Reject Modal */}
            {rejectTarget && (
                <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
                    <div className="w-full max-w-md rounded-2xl bg-white p-6 shadow-xl">
                        <div className="flex items-center gap-3 mb-4">
                            <div className="flex h-10 w-10 items-center justify-center rounded-full bg-red-50">
                                <AlertTriangle className="h-5 w-5 text-red-600" />
                            </div>
                            <div>
                                <h2 className="font-bold text-gray-900">Reject Verification</h2>
                                <p className="text-sm text-gray-500">"{rejectTarget.name}"</p>
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
