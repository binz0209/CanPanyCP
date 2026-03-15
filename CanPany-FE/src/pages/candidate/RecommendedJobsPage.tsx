import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useQuery, useMutation } from '@tanstack/react-query';
import { Sparkles, Briefcase, RefreshCw, Eye, Bookmark, Send, Info } from 'lucide-react';
import { jobsApi } from '../../api/jobs.api';
import type { RecommendedJob } from '../../types';
import toast from 'react-hot-toast';

// ─── Interaction type enum (matches BE: 1=View, 2=Click, 3=Bookmark, 4=Apply) ─
const INTERACTION = { View: 1, Click: 2, Bookmark: 3, Apply: 4 } as const;
type InteractionType = (typeof INTERACTION)[keyof typeof INTERACTION];

// ─── Helper ───────────────────────────────────────────────────────────────────
function scoreColor(score: number) {
    if (score >= 80) return 'text-emerald-600 bg-emerald-50 border-emerald-200';
    if (score >= 60) return 'text-blue-600 bg-blue-50 border-blue-200';
    if (score >= 40) return 'text-amber-600 bg-amber-50 border-amber-200';
    return 'text-gray-500 bg-gray-50 border-gray-200';
}

function scoreLabel(score: number) {
    if (score >= 80) return 'Phù hợp cao';
    if (score >= 60) return 'Phù hợp tốt';
    if (score >= 40) return 'Phù hợp';
    return 'Gợi ý';
}

function formatBudget(amount?: number, type?: string) {
    if (!amount) return 'Thỏa thuận';
    const fmt = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND', maximumFractionDigits: 0 }).format(amount);
    return type === 'Hourly' ? `${fmt}/giờ` : fmt;
}

// ─── Page ─────────────────────────────────────────────────────────────────────
export function RecommendedJobsPage() {
    const [limit] = useState(12);
    const [trackedIds, setTrackedIds] = useState<Set<string>>(new Set());

    const { data: recommendations = [], isLoading, refetch, isFetching } = useQuery({
        queryKey: ['jobs', 'recommended', limit],
        queryFn: () => jobsApi.getRecommended(limit),
        staleTime: 2 * 60 * 1000, // 2 minutes
    });

    const trackMutation = useMutation({
        mutationFn: ({ jobId, type }: { jobId: string; type: InteractionType }) =>
            jobsApi.trackInteraction(jobId, type),
        onSuccess: (_, { jobId }) => {
            setTrackedIds((prev) => new Set(prev).add(jobId));
        },
    });

    function track(jobId: string, type: InteractionType) {
        trackMutation.mutate({ jobId, type });
    }

    function handleBookmarkClick(jobId: string) {
        track(jobId, INTERACTION.Bookmark);
        jobsApi.bookmark(jobId)
            .then(() => toast.success('Đã lưu việc làm'))
            .catch(() => toast.error('Không thể lưu việc làm'));
    }

    return (
        <div className="space-y-6">
            {/* ── Header ── */}
            <div className="rounded-2xl bg-gradient-to-br from-[#00b14f] to-[#009940] p-6 text-white shadow-lg">
                <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
                    <div>
                        <div className="flex items-center gap-2 text-emerald-100 text-sm">
                            <Sparkles className="h-4 w-4" />
                            <span>AI Recommendation Engine · Hybrid CF + Semantic</span>
                        </div>
                        <h1 className="mt-1 text-3xl font-bold">Việc làm phù hợp với bạn</h1>
                        <p className="mt-2 text-emerald-100 text-sm max-w-lg">
                            Được tổng hợp từ hồ sơ của bạn và hành vi của những ứng viên tương tự.
                            Tương tác nhiều hơn giúp gợi ý chính xác hơn.
                        </p>
                    </div>
                    <button
                        onClick={() => refetch()}
                        disabled={isFetching}
                        className="flex items-center gap-2 rounded-xl bg-white/20 hover:bg-white/30 px-4 py-2.5 text-sm font-medium transition-all disabled:opacity-60 self-start lg:self-auto"
                    >
                        <RefreshCw className={`h-4 w-4 ${isFetching ? 'animate-spin' : ''}`} />
                        Làm mới
                    </button>
                </div>

                {/* ── Cold-start tip ── */}
                {recommendations.length > 0 && recommendations[0].hybridScore < 20 && (
                    <div className="mt-4 flex items-start gap-2 rounded-xl bg-white/10 px-4 py-3 text-sm text-emerald-50">
                        <Info className="h-4 w-4 mt-0.5 shrink-0" />
                        <span>
                            Bạn chưa có nhiều tương tác. Hãy xem và lưu thêm việc làm để AI cải thiện gợi ý theo thời gian.
                        </span>
                    </div>
                )}
            </div>

            {/* ── Content ── */}
            {isLoading ? (
                <SkeletonList />
            ) : recommendations.length === 0 ? (
                <EmptyState />
            ) : (
                <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
                    {recommendations.map(({ job, hybridScore }) => (
                        <RecommendedJobCard
                            key={job.id}
                            item={{ job, hybridScore }}
                            viewed={trackedIds.has(job.id)}
                            onView={() => track(job.id, INTERACTION.View)}
                            onBookmark={() => handleBookmarkClick(job.id)}
                        />
                    ))}
                </div>
            )}
        </div>
    );
}

// ─── Card ─────────────────────────────────────────────────────────────────────
interface CardProps {
    item: RecommendedJob;
    viewed: boolean;
    onView: () => void;
    onBookmark: () => void;
}

function RecommendedJobCard({ item, viewed, onView, onBookmark }: CardProps) {
    const { job, hybridScore } = item;
    const score = Math.round(hybridScore);

    return (
        <div
            className={`group relative flex flex-col gap-4 rounded-2xl border bg-white p-5 shadow-sm transition-all hover:shadow-md hover:-translate-y-0.5 ${viewed ? 'border-emerald-200' : 'border-gray-100'}`}
        >
            {/* Score badge */}
            <div className={`absolute top-4 right-4 flex items-center gap-1.5 rounded-full border px-2.5 py-1 text-xs font-semibold ${scoreColor(score)}`}>
                <Sparkles className="h-3 w-3" />
                {score}% · {scoreLabel(score)}
            </div>

            {/* Score bar */}
            <div className="absolute top-0 left-0 h-1 rounded-t-2xl bg-gray-100 w-full overflow-hidden">
                <div
                    className="h-full bg-gradient-to-r from-[#00b14f] to-emerald-400 transition-all duration-700"
                    style={{ width: `${Math.min(score, 100)}%` }}
                />
            </div>

            {/* Job info */}
            <div className="pt-2 pr-20">
                <Link
                    to={`/jobs/${job.id}`}
                    onClick={onView}
                    className="text-base font-semibold text-gray-900 hover:text-[#00b14f] line-clamp-2 leading-snug transition-colors"
                >
                    {job.title}
                </Link>
                <p className="mt-1 text-sm text-gray-500 line-clamp-2">{job.description}</p>
            </div>

            {/* Meta chips */}
            <div className="flex flex-wrap gap-2 text-xs">
                {job.level && (
                    <span className="rounded-full bg-blue-50 text-blue-700 px-2.5 py-1 font-medium">{job.level}</span>
                )}
                {job.isRemote && (
                    <span className="rounded-full bg-purple-50 text-purple-700 px-2.5 py-1 font-medium">Remote</span>
                )}
                {job.location && (
                    <span className="rounded-full bg-gray-100 text-gray-600 px-2.5 py-1">{job.location}</span>
                )}
                <span className="rounded-full bg-emerald-50 text-emerald-700 px-2.5 py-1 font-medium">
                    {formatBudget(job.budgetAmount, job.budgetType)}
                </span>
            </div>

            {/* Actions */}
            <div className="flex items-center gap-2 border-t border-gray-50 pt-3 -mx-1">
                <Link
                    to={`/jobs/${job.id}`}
                    onClick={onView}
                    className="flex flex-1 items-center justify-center gap-1.5 rounded-xl bg-[#00b14f] hover:bg-[#009940] text-white text-sm font-medium py-2 transition-colors"
                >
                    <Eye className="h-3.5 w-3.5" />
                    Xem chi tiết
                </Link>
                <button
                    onClick={onBookmark}
                    title="Lưu việc làm"
                    className="flex items-center justify-center rounded-xl border border-gray-200 hover:border-[#00b14f] hover:text-[#00b14f] text-gray-500 p-2 transition-colors"
                >
                    <Bookmark className="h-4 w-4" />
                </button>
            </div>

            {/* Viewed indicator */}
            {viewed && (
                <div className="absolute bottom-2 left-4 flex items-center gap-1 text-[10px] text-emerald-500">
                    <Eye className="h-2.5 w-2.5" />
                    Đã xem
                </div>
            )}
        </div>
    );
}

// ─── Skeleton & Empty ─────────────────────────────────────────────────────────
function SkeletonList() {
    return (
        <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
            {Array.from({ length: 6 }).map((_, i) => (
                <div key={i} className="h-52 animate-pulse rounded-2xl bg-gray-100" />
            ))}
        </div>
    );
}

function EmptyState() {
    return (
        <div className="rounded-2xl border border-dashed border-gray-200 bg-white p-12 text-center">
            <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-emerald-50">
                <Sparkles className="h-8 w-8 text-[#00b14f]" />
            </div>
            <h2 className="text-lg font-semibold text-gray-900">Chưa có gợi ý</h2>
            <p className="mt-2 text-sm text-gray-500 max-w-sm mx-auto">
                Hãy hoàn thiện hồ sơ và tương tác với các tin tuyển dụng để AI có thể gợi ý việc làm phù hợp.
            </p>
            <div className="mt-6 flex gap-3 justify-center flex-wrap">
                <Link
                    to="/candidate/profile"
                    className="flex items-center gap-2 rounded-xl bg-[#00b14f] text-white px-4 py-2.5 text-sm font-medium hover:bg-[#009940] transition-colors"
                >
                    <Send className="h-4 w-4" />
                    Cập nhật hồ sơ
                </Link>
                <Link
                    to="/jobs"
                    className="flex items-center gap-2 rounded-xl border border-gray-200 text-gray-700 px-4 py-2.5 text-sm font-medium hover:border-gray-300 transition-colors"
                >
                    <Briefcase className="h-4 w-4" />
                    Khám phá việc làm
                </Link>
            </div>
        </div>
    );
}
