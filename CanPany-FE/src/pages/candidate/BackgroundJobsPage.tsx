import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import {
    Activity,
    CheckCircle2,
    XCircle,
    Clock,
    RefreshCw,
    Github,
    ChevronDown,
    ChevronUp,
    AlertCircle,
} from 'lucide-react';
import { candidateApi, type JobProgressRecord, type JobStatus } from '../../api';
import { useAuthStore } from '../../stores/auth.store';

// ──────────────────── Helpers ────────────────────
function formatDuration(ms?: number): string {
    if (!ms) return '—';
    if (ms < 1000) return `${ms}ms`;
    if (ms < 60000) return `${(ms / 1000).toFixed(1)}s`;
    return `${Math.floor(ms / 60000)}m ${Math.round((ms % 60000) / 1000)}s`;
}

function formatDateTime(iso?: string): string {
    if (!iso) return '—';
    return new Date(iso).toLocaleString('vi-VN', {
        day: '2-digit', month: '2-digit', year: 'numeric',
        hour: '2-digit', minute: '2-digit', second: '2-digit',
    });
}

function statusLabel(s: JobStatus): string {
    const map: Record<JobStatus, string> = {
        Pending: 'Chờ xử lý', Running: 'Đang chạy', Completed: 'Hoàn tất',
        Failed: 'Thất bại', Cancelled: 'Đã hủy', Retrying: 'Thử lại',
    };
    return map[s] ?? s;
}

interface StatusBadgeProps { status: JobStatus; }
function StatusBadge({ status }: StatusBadgeProps) {
    const cfg: Record<JobStatus, { bg: string; text: string; icon: React.ReactNode }> = {
        Pending:   { bg: 'bg-yellow-100', text: 'text-yellow-700', icon: <Clock className="h-3.5 w-3.5" /> },
        Running:   { bg: 'bg-blue-100',   text: 'text-blue-700',   icon: <RefreshCw className="h-3.5 w-3.5 animate-spin" /> },
        Completed: { bg: 'bg-green-100',  text: 'text-green-700',  icon: <CheckCircle2 className="h-3.5 w-3.5" /> },
        Failed:    { bg: 'bg-red-100',    text: 'text-red-700',    icon: <XCircle className="h-3.5 w-3.5" /> },
        Cancelled: { bg: 'bg-gray-100',   text: 'text-gray-600',   icon: <AlertCircle className="h-3.5 w-3.5" /> },
        Retrying:  { bg: 'bg-orange-100', text: 'text-orange-700', icon: <RefreshCw className="h-3.5 w-3.5" /> },
    };
    const c = cfg[status] ?? cfg.Pending;
    return (
        <span className={`inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-xs font-semibold ${c.bg} ${c.text}`}>
            {c.icon}
            {statusLabel(status)}
        </span>
    );
}

interface JobTypeIconProps { jobType?: string; }
function JobTypeIcon({ jobType }: JobTypeIconProps) {
    if (jobType === 'SyncSkills') return <Github className="h-5 w-5 text-gray-700" />;
    return <Activity className="h-5 w-5 text-gray-400" />;
}

// ──────────────────── Detail Panel ────────────────────
interface DetailPanelProps { job: JobProgressRecord; }
function DetailPanel({ job }: DetailPanelProps) {
    const selectedRepos: string[] = job.details?.['selectedRepos'] ?? [];
    const gitHubUsername: string = job.details?.['gitHubUsername'] ?? '';
    const extractedSkills: string[] = job.result?.['primarySkills'] || job.result?.['PrimarySkills'] || [];

    return (
        <div className="border-t border-gray-100 bg-gray-50 px-5 pb-5 pt-4 space-y-4 text-sm">
            {/* Progress */}
            <div>
                <div className="flex justify-between items-center mb-1 text-xs text-gray-500">
                    <span>{job.currentStep || 'Đang chờ...'}</span>
                    <span className="font-medium">{job.percentComplete}%</span>
                </div>
                <div className="w-full bg-gray-200 rounded-full h-2">
                    <div
                        className={`h-2 rounded-full transition-all duration-500 ${
                            job.status === 'Completed' ? 'bg-green-500' :
                            job.status === 'Failed'    ? 'bg-red-500'   : 'bg-blue-500'
                        }`}
                        style={{ width: `${job.percentComplete}%` }}
                    />
                </div>
                {job.totalSteps > 0 && (
                    <p className="text-xs text-gray-400 mt-1">
                        Bước {job.completedSteps}/{job.totalSteps}
                    </p>
                )}
            </div>

            {/* For SyncSkills — repos */}
            {selectedRepos.length > 0 && (
                <div>
                    <p className="text-xs font-semibold text-gray-700 mb-2 flex items-center gap-1.5">
                        <Github className="h-4 w-4" />
                        Repositories đã chọn {gitHubUsername && <span className="text-gray-400">(@{gitHubUsername})</span>}
                    </p>
                    <div className="flex flex-wrap gap-2">
                        {selectedRepos.map(r => (
                            <span key={r} className="text-xs px-2.5 py-1 bg-gray-800 text-white rounded-full font-mono">
                                {r}
                            </span>
                        ))}
                    </div>
                </div>
            )}

            {/* Extracted skills (from result) */}
            {extractedSkills.length > 0 && (
                <div>
                    <p className="text-xs font-semibold text-gray-700 mb-2">✨ Skills đã trích xuất</p>
                    <div className="flex flex-wrap gap-1.5">
                        {extractedSkills.map(s => (
                            <span key={s} className="text-xs px-2.5 py-1 bg-[#00b14f]/10 text-[#00b14f] rounded-full font-medium">
                                {s}
                            </span>
                        ))}
                    </div>
                </div>
            )}

            {/* Error */}
            {job.errorMessage && (
                <div className="rounded-lg bg-red-50 border border-red-200 p-3">
                    <p className="text-xs font-semibold text-red-700 mb-1">Lỗi</p>
                    <p className="text-xs text-red-600 font-mono">{job.errorMessage}</p>
                </div>
            )}

            {/* Timestamps */}
            <div className="grid grid-cols-2 gap-2 text-xs text-gray-500 border-t border-gray-200 pt-3">
                <div><span className="font-medium text-gray-600">Bắt đầu:</span> {formatDateTime(job.startedAt)}</div>
                <div><span className="font-medium text-gray-600">Kết thúc:</span> {formatDateTime(job.completedAt)}</div>
                <div><span className="font-medium text-gray-600">Thời gian:</span> {formatDuration(job.durationMs)}</div>
                <div><span className="font-medium text-gray-600">Cập nhật:</span> {formatDateTime(job.updatedAt)}</div>
            </div>
        </div>
    );
}

// ──────────────────── Job Row ────────────────────
interface JobRowProps { job: JobProgressRecord; }
function JobRow({ job }: JobRowProps) {
    const isActive = job.status === 'Running' || job.status === 'Retrying' || job.status === 'Pending';
    const [expanded, setExpanded] = useState(isActive);

    return (
        <div className="border border-gray-100 rounded-xl overflow-hidden shadow-sm hover:shadow-md transition-shadow">
            <button
                className="w-full text-left"
                onClick={() => setExpanded(e => !e)}
            >
                <div className="flex items-center gap-4 p-5">
                    {/* Icon */}
                    <div className="flex-shrink-0 w-10 h-10 rounded-full bg-gray-100 flex items-center justify-center">
                        <JobTypeIcon jobType={job.jobType} />
                    </div>

                    {/* Info */}
                    <div className="flex-1 min-w-0">
                        <div className="flex items-center gap-2 mb-1 flex-wrap">
                            <span className="text-sm font-semibold text-gray-900 truncate">
                                {job.jobTitle || job.jobType || 'Background Job'}
                            </span>
                            <StatusBadge status={job.status} />
                        </div>
                        {/* Mini progress bar when running */}
                        {isActive && (
                            <div className="w-full bg-gray-200 rounded-full h-1.5 mt-1.5 max-w-xs">
                                <div
                                    className="bg-blue-500 h-1.5 rounded-full transition-all duration-500"
                                    style={{ width: `${job.percentComplete}%` }}
                                />
                            </div>
                        )}
                        <p className="text-xs text-gray-400 mt-1">
                            {isActive
                                ? (job.currentStep || 'Đang xử lý...')
                                : `Hoàn tất lúc: ${formatDateTime(job.completedAt)}`
                            }
                        </p>
                    </div>

                    {/* Right: time */}
                    <div className="flex-shrink-0 text-right hidden sm:block">
                        <p className="text-xs text-gray-500">{formatDateTime(job.startedAt || job.updatedAt)}</p>
                        {job.durationMs != null && (
                            <p className="text-xs text-gray-400 mt-0.5">⏱ {formatDuration(job.durationMs)}</p>
                        )}
                    </div>

                    {/* Chevron */}
                    <div className="flex-shrink-0 text-gray-400">
                        {expanded ? <ChevronUp className="h-4 w-4" /> : <ChevronDown className="h-4 w-4" />}
                    </div>
                </div>
            </button>

            {expanded && <DetailPanel job={job} />}
        </div>
    );
}

// ──────────────────── Page ────────────────────
export function BackgroundJobsPage() {
    const { isAuthenticated } = useAuthStore();

    const { data, isLoading, refetch, isFetching } = useQuery({
        queryKey: ['my-jobs'],
        queryFn: () => candidateApi.getMyJobs(0, 50),
        enabled: isAuthenticated,
        // Auto-refresh every 3s if any job is still running
        refetchInterval: (query) => {
            const jobs = query.state.data?.jobs ?? [];
            const hasActive = jobs.some(j =>
                j.status === 'Running' || j.status === 'Retrying' || j.status === 'Pending'
            );
            return hasActive ? 3000 : false;
        },
    });

    const jobs = data?.jobs ?? [];

    return (
        <div>
            {/* Hero */}
            <section className="relative overflow-hidden bg-gradient-to-br from-[#00b14f] via-[#00a045] to-[#008f3c] mb-8">
                <div className="relative mx-auto max-w-7xl px-4 py-12 sm:px-6 lg:px-8">
                    <div className="text-center">
                        <h2 className="text-3xl font-bold tracking-tight text-white sm:text-4xl flex items-center justify-center gap-3">
                            <Activity className="h-8 w-8" />
                            Tiến trình công việc
                        </h2>
                        <p className="mt-3 text-white/80 text-sm">
                            Theo dõi trạng thái các tác vụ nền đang chạy trong tài khoản của bạn
                        </p>
                    </div>
                </div>
            </section>

            <div className="mx-auto max-w-4xl px-4 pb-12">
                {/* Toolbar */}
                <div className="flex items-center justify-between mb-5">
                    <p className="text-sm text-gray-500">
                        {jobs.length > 0
                            ? `${jobs.length} công việc gần đây`
                            : 'Chưa có công việc nào'}
                    </p>
                    <button
                        onClick={() => refetch()}
                        disabled={isFetching}
                        className="flex items-center gap-1.5 text-sm text-gray-600 hover:text-[#00b14f] transition-colors disabled:opacity-50"
                    >
                        <RefreshCw className={`h-4 w-4 ${isFetching ? 'animate-spin' : ''}`} />
                        Làm mới
                    </button>
                </div>

                {/* Loading */}
                {isLoading && (
                    <div className="flex items-center justify-center py-20">
                        <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-[#00b14f]" />
                    </div>
                )}

                {/* Empty */}
                {!isLoading && jobs.length === 0 && (
                    <div className="text-center py-20 bg-white rounded-2xl border border-gray-100 shadow-sm">
                        <Activity className="h-12 w-12 text-gray-300 mx-auto mb-4" />
                        <h3 className="text-lg font-semibold text-gray-700 mb-1">Chưa có tác vụ nào</h3>
                        <p className="text-sm text-gray-400">
                            Khi bạn sync GitHub skills hoặc chạy các tính năng AI,<br />
                            tiến trình sẽ xuất hiện ở đây.
                        </p>
                    </div>
                )}

                {/* Job List */}
                {jobs.length > 0 && (
                    <div className="space-y-3">
                        {jobs.map(job => (
                            <JobRow key={job.jobId} job={job} />
                        ))}
                    </div>
                )}
            </div>
        </div>
    );
}
