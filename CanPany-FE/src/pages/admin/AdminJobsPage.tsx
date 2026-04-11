import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import { Search, Eye, EyeOff, Trash2, ChevronLeft, ChevronRight, Loader2, AlertTriangle } from 'lucide-react';
import { Button, Badge, Card } from '../../components/ui';
import { adminApi } from '../../api';
import { formatRelativeTime } from '../../utils';

const STATUS_OPTIONS = ['', 'Open', 'Closed', 'Hidden'];

const statusColor: Record<string, string> = {
    Open: 'bg-green-50 text-green-700 border-green-200',
    Closed: 'bg-gray-100 text-gray-600 border-gray-200',
    Hidden: 'bg-red-50 text-red-700 border-red-200',
};

export function AdminJobsPage() {
    const { t } = useTranslation('admin');
    const qc = useQueryClient();

    const [search, setSearch] = useState('');
    const [statusFilter, setStatusFilter] = useState('');
    const [page, setPage] = useState(1);
    const PAGE_SIZE = 15;

    // Hide modal state
    const [hideTarget, setHideTarget] = useState<{ id: string; title: string } | null>(null);
    const [hideReason, setHideReason] = useState('');

    // Delete confirm state
    const [deleteTarget, setDeleteTarget] = useState<{ id: string; title: string } | null>(null);

    // Detail modal
    const [detailJob, setDetailJob] = useState<any | null>(null);

    const { data: allJobs = [], isLoading } = useQuery({
        queryKey: ['admin-jobs', statusFilter],
        queryFn: () => adminApi.getJobs(statusFilter || undefined),
    });

    // Client-side search + pagination
    const filtered = allJobs.filter((j: any) => {
        if (!search.trim()) return true;
        const q = search.toLowerCase();
        return (
            (j.title ?? '').toLowerCase().includes(q) ||
            (j.companyId ?? '').toLowerCase().includes(q) ||
            (j.id ?? '').toLowerCase().includes(q)
        );
    });
    const totalPages = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE));
    const paged = filtered.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE);

    const hideMutation = useMutation({
        mutationFn: () => adminApi.hideJob(hideTarget!.id, hideReason.trim()),
        onSuccess: () => {
            toast.success('Job hidden successfully');
            qc.invalidateQueries({ queryKey: ['admin-jobs'] });
            setHideTarget(null);
            setHideReason('');
        },
        onError: () => toast.error('Failed to hide job'),
    });

    const deleteMutation = useMutation({
        mutationFn: () => adminApi.deleteJob(deleteTarget!.id),
        onSuccess: () => {
            toast.success('Job deleted');
            qc.invalidateQueries({ queryKey: ['admin-jobs'] });
            setDeleteTarget(null);
        },
        onError: () => toast.error('Failed to delete job'),
    });

    return (
        <div className="space-y-6">
            {/* Header */}
            <div>
                <h1 className="text-2xl font-bold text-gray-900">{t('placeholders.jobs.title')}</h1>
            </div>

            {/* Filters */}
            <Card className="p-4">
                <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
                    <div className="relative flex-1">
                        <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-gray-400" />
                        <input
                            value={search}
                            onChange={(e) => { setSearch(e.target.value); setPage(1); }}
                            placeholder="Search by title, company ID, job ID…"
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
                    <span className="text-sm text-gray-500 whitespace-nowrap">
                        {filtered.length} jobs
                    </span>
                </div>
            </Card>

            {/* Table */}
            <Card className="overflow-hidden p-0">
                {isLoading ? (
                    <div className="flex items-center justify-center py-20">
                        <Loader2 className="h-6 w-6 animate-spin text-gray-400" />
                    </div>
                ) : paged.length === 0 ? (
                    <div className="py-16 text-center text-sm text-gray-500">No jobs found</div>
                ) : (
                    <div className="overflow-x-auto">
                        <table className="w-full text-sm">
                            <thead className="border-b border-gray-100 bg-gray-50">
                                <tr>
                                    <th className="px-4 py-3 text-left font-medium text-gray-600">Title</th>
                                    <th className="px-4 py-3 text-left font-medium text-gray-600">Company ID</th>
                                    <th className="px-4 py-3 text-left font-medium text-gray-600">Level</th>
                                    <th className="px-4 py-3 text-left font-medium text-gray-600">Status</th>
                                    <th className="px-4 py-3 text-left font-medium text-gray-600">Views</th>
                                    <th className="px-4 py-3 text-left font-medium text-gray-600">Applicants</th>
                                    <th className="px-4 py-3 text-left font-medium text-gray-600">Posted</th>
                                    <th className="px-4 py-3 text-right font-medium text-gray-600">Actions</th>
                                </tr>
                            </thead>
                            <tbody className="divide-y divide-gray-50">
                                {paged.map((job: any) => (
                                    <tr key={job.id} className="hover:bg-gray-50/80">
                                        <td className="px-4 py-3">
                                            <div className="max-w-[200px]">
                                                <p className="truncate font-medium text-gray-900">{job.title}</p>
                                                <p className="truncate text-xs text-gray-400">{job.id}</p>
                                            </div>
                                        </td>
                                        <td className="px-4 py-3 text-gray-500 font-mono text-xs">{job.companyId ?? '—'}</td>
                                        <td className="px-4 py-3">
                                            {job.level ? (
                                                <Badge variant="secondary" className="text-xs">{job.level}</Badge>
                                            ) : '—'}
                                        </td>
                                        <td className="px-4 py-3">
                                            <Badge className={`text-xs ${statusColor[job.status] ?? 'bg-gray-100 text-gray-600'}`}>
                                                {job.status ?? 'Unknown'}
                                            </Badge>
                                        </td>
                                        <td className="px-4 py-3 text-gray-500">{job.viewCount ?? 0}</td>
                                        <td className="px-4 py-3 text-gray-500">{job.applicationCount ?? 0}</td>
                                        <td className="px-4 py-3 text-gray-500 whitespace-nowrap">
                                            {job.createdAt ? formatRelativeTime(job.createdAt) : '—'}
                                        </td>
                                        <td className="px-4 py-3">
                                            <div className="flex items-center justify-end gap-1">
                                                <button
                                                    title="View detail"
                                                    onClick={() => setDetailJob(job)}
                                                    className="rounded p-1.5 text-gray-400 hover:bg-blue-50 hover:text-blue-600"
                                                >
                                                    <Eye className="h-4 w-4" />
                                                </button>
                                                {job.status !== 'Hidden' && (
                                                    <button
                                                        title="Hide job"
                                                        onClick={() => setHideTarget({ id: job.id, title: job.title })}
                                                        className="rounded p-1.5 text-gray-400 hover:bg-amber-50 hover:text-amber-600"
                                                    >
                                                        <EyeOff className="h-4 w-4" />
                                                    </button>
                                                )}
                                                <button
                                                    title="Delete job"
                                                    onClick={() => setDeleteTarget({ id: job.id, title: job.title })}
                                                    className="rounded p-1.5 text-gray-400 hover:bg-red-50 hover:text-red-600"
                                                >
                                                    <Trash2 className="h-4 w-4" />
                                                </button>
                                            </div>
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>
                )}

                {/* Pagination */}
                {totalPages > 1 && (
                    <div className="flex items-center justify-between border-t border-gray-100 px-4 py-3">
                        <span className="text-sm text-gray-500">
                            Page {page} of {totalPages}
                        </span>
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
            {detailJob && (
                <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
                    <div className="w-full max-w-lg rounded-2xl bg-white p-6 shadow-xl">
                        <h2 className="text-lg font-bold text-gray-900 mb-4">Job Detail</h2>
                        <div className="space-y-2 text-sm">
                            {Object.entries(detailJob).map(([k, v]) => (
                                <div key={k} className="flex gap-2">
                                    <span className="w-36 shrink-0 font-medium text-gray-500 capitalize">{k}:</span>
                                    <span className="break-all text-gray-800">{String(v)}</span>
                                </div>
                            ))}
                        </div>
                        <div className="mt-6 flex justify-end">
                            <Button variant="outline" onClick={() => setDetailJob(null)}>Close</Button>
                        </div>
                    </div>
                </div>
            )}

            {/* Hide Modal */}
            {hideTarget && (
                <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
                    <div className="w-full max-w-md rounded-2xl bg-white p-6 shadow-xl">
                        <div className="flex items-center gap-3 mb-4">
                            <div className="flex h-10 w-10 items-center justify-center rounded-full bg-amber-50">
                                <EyeOff className="h-5 w-5 text-amber-600" />
                            </div>
                            <div>
                                <h2 className="font-bold text-gray-900">Hide Job</h2>
                                <p className="text-sm text-gray-500">"{hideTarget.title}"</p>
                            </div>
                        </div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">Reason (required)</label>
                        <textarea
                            value={hideReason}
                            onChange={(e) => setHideReason(e.target.value)}
                            rows={3}
                            placeholder="Enter reason for hiding this job…"
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-amber-400 resize-none"
                        />
                        <div className="mt-4 flex justify-end gap-2">
                            <Button variant="outline" onClick={() => { setHideTarget(null); setHideReason(''); }}>Cancel</Button>
                            <Button
                                className="bg-amber-500 hover:bg-amber-600 text-white"
                                disabled={!hideReason.trim() || hideMutation.isPending}
                                onClick={() => hideMutation.mutate()}
                            >
                                {hideMutation.isPending ? <Loader2 className="h-4 w-4 animate-spin" /> : 'Hide Job'}
                            </Button>
                        </div>
                    </div>
                </div>
            )}

            {/* Delete Confirm Modal */}
            {deleteTarget && (
                <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
                    <div className="w-full max-w-md rounded-2xl bg-white p-6 shadow-xl">
                        <div className="flex items-center gap-3 mb-4">
                            <div className="flex h-10 w-10 items-center justify-center rounded-full bg-red-50">
                                <AlertTriangle className="h-5 w-5 text-red-600" />
                            </div>
                            <div>
                                <h2 className="font-bold text-gray-900">Delete Job</h2>
                                <p className="text-sm text-gray-500">This action cannot be undone.</p>
                            </div>
                        </div>
                        <p className="text-sm text-gray-700 mb-4">
                            Delete "<span className="font-semibold">{deleteTarget.title}</span>"?
                        </p>
                        <div className="flex justify-end gap-2">
                            <Button variant="outline" onClick={() => setDeleteTarget(null)}>Cancel</Button>
                            <Button
                                className="bg-red-600 hover:bg-red-700 text-white"
                                disabled={deleteMutation.isPending}
                                onClick={() => deleteMutation.mutate()}
                            >
                                {deleteMutation.isPending ? <Loader2 className="h-4 w-4 animate-spin" /> : 'Delete'}
                            </Button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}
