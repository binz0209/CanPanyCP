import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Sparkles, Briefcase, RefreshCw, Eye, Bookmark, Send, Info, Brain, FileText, CheckCircle2 } from 'lucide-react';
import { jobsApi } from '../../api/jobs.api';
import { cvApi } from '../../api/cv.api';
import { candidateApi } from '../../api/candidate.api';
import { paymentsApi } from '../../api/payments.api';
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

function scoreLabel(score: number, t: any) {
    if (score >= 80) return t('recommendedJobs.matchLevels.veryHigh');
    if (score >= 60) return t('recommendedJobs.matchLevels.high');
    if (score >= 40) return t('recommendedJobs.matchLevels.medium');
    return t('recommendedJobs.matchLevels.suggested');
}

function formatBudget(amount: number | undefined, type: string | undefined, t: any) {
    if (!amount) return t('recommendedJobs.budget.negotiable');
    const fmt = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND', maximumFractionDigits: 0 }).format(amount);
    return type === 'Hourly' ? `${fmt}${t('recommendedJobs.budget.perHour')}` : fmt;
}

// ─── Page ─────────────────────────────────────────────────────────────────────
export function RecommendedJobsPage() {
    const { t } = useTranslation('candidate');
    const queryClient = useQueryClient();
    const [limit] = useState(12);
    const [trackedIds, setTrackedIds] = useState<Set<string>>(new Set());
    const [syncJobId, setSyncJobId] = useState<string | null>(null);

    const { data: recommendations = [], isLoading, refetch, isFetching } = useQuery({
        queryKey: ['jobs', 'recommended', limit],
        queryFn: () => jobsApi.getRecommended(limit),
        staleTime: 2 * 60 * 1000,
    });

    const { data: premiumStatus } = useQuery({
        queryKey: ['premium-status'],
        queryFn: () => paymentsApi.getPremiumStatus(),
        staleTime: 5 * 60 * 1000,
    });

    const isPremiumRequired: boolean = premiumStatus ? !premiumStatus.isPremium : false;

    // Sync skills mutation
    const syncMutation = useMutation({
        mutationFn: () => candidateApi.syncRecommendationSkills(20),
        onSuccess: (d) => {
            setSyncJobId(d.jobId);
            toast.success(t('recommendedJobs.toast.syncStarted'));
        },
        onError: (err: any) => {
            const code = err?.response?.data?.errorCode || err?.response?.data?.ErrorCode;
            if (code === 'PremiumRequired') {
                toast(t('recommendedJobs.premium.description', 'Bạn cần nâng cấp gói Premium để mở khoá tính năng Gợi ý Việc làm Phù hợp bằng AI. 🚀'), {
                    icon: '⭐',
                    style: { maxWidth: '360px' },
                });
            } else {
                toast.error(t('recommendedJobs.toast.syncError'));
            }
        },
    });

    // Poll sync job
    const { data: syncProgress } = useQuery({
        queryKey: ['recommendation-sync-job', syncJobId],
        queryFn: () => candidateApi.getMyJobDetail(syncJobId!),
        enabled: !!syncJobId,
        refetchInterval: (q) => {
            const s = q.state.data?.status;
            if (s === 'Completed' || s === 'Failed') return false;
            return 2000;
        },
    });

    useEffect(() => {
        if (syncProgress?.status === 'Completed' && syncJobId) {
            setSyncJobId(null);
            queryClient.invalidateQueries({ queryKey: ['jobs', 'recommended'] });
            refetch();
            toast.success(t('recommendedJobs.toast.syncSuccess'));
        }
        if (syncProgress?.status === 'Failed' && syncJobId) {
            setSyncJobId(null);
            toast.error(t('recommendedJobs.toast.syncFailed'));
        }
    }, [syncProgress?.status]); // eslint-disable-line react-hooks/exhaustive-deps

    const isSyncing = !!syncJobId;

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
            .then(() => toast.success(t('recommendedJobs.toast.bookmarkSuccess')))
            .catch(() => toast.error(t('recommendedJobs.toast.bookmarkError')));
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
                        <h1 className="mt-1 text-3xl font-bold">{t('recommendedJobs.title')}</h1>
                        <p className="mt-2 text-emerald-100 text-sm max-w-lg">
                            {t('recommendedJobs.subtitle')}
                        </p>
                    </div>
                    <div className="flex flex-col gap-2 self-start lg:self-auto">
                        <button
                            onClick={() => refetch()}
                            disabled={isFetching}
                            className="flex items-center gap-2 rounded-xl bg-white/20 hover:bg-white/30 px-4 py-2.5 text-sm font-medium transition-all disabled:opacity-60"
                        >
                            <RefreshCw className={`h-4 w-4 ${isFetching ? 'animate-spin' : ''}`} />
                            {t('recommendedJobs.actions.refresh')}
                        </button>
                        <button
                            onClick={() => syncMutation.mutate()}
                            disabled={isSyncing || syncMutation.isPending || isPremiumRequired}
                            className="flex items-center gap-2 rounded-xl bg-white/10 hover:bg-white/20 border border-white/20 px-4 py-2.5 text-sm font-medium transition-all disabled:opacity-60"
                        >
                            <Brain className={`h-4 w-4 ${isSyncing ? 'animate-pulse' : ''}`} />
                            {isSyncing ? t('recommendedJobs.actions.syncing') : t('recommendedJobs.actions.syncAiSkills')}
                        </button>
                    </div>
                </div>

                {/* Sync progress */}
                {isSyncing && syncProgress && (
                    <div className="mt-4 space-y-1">
                        <div className="flex justify-between text-xs text-emerald-100">
                            <span>{syncProgress.currentStep ?? t('recommendedJobs.actions.syncing')}</span>
                            <span>{syncProgress.percentComplete}%</span>
                        </div>
                        <div className="w-full bg-white/10 rounded-full h-1.5 overflow-hidden">
                            <div
                                className="h-full bg-white transition-all duration-500"
                                style={{ width: `${syncProgress.percentComplete}%` }}
                            />
                        </div>
                    </div>
                )}

                {/* Cold-start tip */}
                {recommendations.length > 0 && recommendations[0].hybridScore < 20 && (
                    <div className="mt-4 flex items-start gap-2 rounded-xl bg-white/10 px-4 py-3 text-sm text-emerald-50">
                        <Info className="h-4 w-4 mt-0.5 shrink-0" />
                        <span>
                            {t('recommendedJobs.tips.coldStart1')} <strong>{t('recommendedJobs.tips.coldStart2')}</strong> {t('recommendedJobs.tips.coldStart3')}
                        </span>
                    </div>
                )}
            </div>

            {/* ── Content ── */}
            {isLoading ? (
                <SkeletonList />
            ) : isPremiumRequired ? (
                <PremiumLockState />
            ) : recommendations.length === 0 ? (
                <EmptyState onSync={() => syncMutation.mutate()} isSyncing={syncMutation.isPending} />
            ) : (
                <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
                    {recommendations.map(({ job, hybridScore }) => (
                        <RecommendedJobCard
                            key={job.id}
                            item={{ job, hybridScore }}
                            viewed={trackedIds.has(job.id)}
                            onView={() => track(job.id, INTERACTION.View)}
                            onBookmark={() => handleBookmarkClick(job.id)}
                            t={t}
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
    t: any;
}

function RecommendedJobCard({ item, viewed, onView, onBookmark, t }: CardProps) {
    const { job, hybridScore } = item;
    const score = Math.round(hybridScore);
    const [cvDone, setCvDone] = useState(false);
    const [cvJobId, setCvJobId] = useState<string | null>(null);

    const genCVMutation = useMutation({
        mutationFn: () => cvApi.generateCV(job.id),
        onSuccess: (d) => {
            setCvJobId(d.jobId);
            toast.success(t('recommendedJobs.toast.cvStarted'));
        },
        onError: (err: any) => {
            const code = err?.response?.data?.errorCode || err?.response?.data?.ErrorCode;
            if (code === 'PremiumRequired') {
                toast(t('recommendedJobs.toast.cvPremiumRequired'), {
                    icon: '⭐',
                    duration: 6000,
                    style: { maxWidth: '360px' },
                });
            } else {
                toast.error(t('recommendedJobs.toast.cvError'));
            }
        },
    });

    const { data: cvProgress } = useQuery({
        queryKey: ['cv-gen', cvJobId],
        queryFn: () => candidateApi.getMyJobDetail(cvJobId!),
        enabled: !!cvJobId,
        refetchInterval: (q) => {
            const s = q.state.data?.status;
            if (s === 'Completed' || s === 'Failed') return false;
            return 2000;
        },
    });

    useEffect(() => {
        if (cvProgress?.status === 'Completed' && cvJobId) {
            setCvJobId(null);
            setCvDone(true);
            toast.success(t('recommendedJobs.toast.cvSuccess', { title: job.title }));
        }
    }, [cvProgress?.status]); // eslint-disable-line react-hooks/exhaustive-deps

    const cvIsRunning = !!cvJobId;

    return (
        <div
            className={`group relative flex flex-col gap-4 rounded-2xl border bg-white p-5 shadow-sm transition-all hover:shadow-md hover:-translate-y-0.5 ${viewed ? 'border-emerald-200' : 'border-gray-100'}`}
        >
            {/* Score badge */}
            <div className={`absolute top-4 right-4 flex items-center gap-1.5 rounded-full border px-2.5 py-1 text-xs font-semibold ${scoreColor(score)}`}>
                <Sparkles className="h-3 w-3" />
                {score}% · {scoreLabel(score, t)}
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
                    <span className="rounded-full bg-purple-50 text-purple-700 px-2.5 py-1 font-medium">{t('recommendedJobs.status.remote')}</span>
                )}
                {job.location && (
                    <span className="rounded-full bg-gray-100 text-gray-600 px-2.5 py-1">{job.location}</span>
                )}
                <span className="rounded-full bg-emerald-50 text-emerald-700 px-2.5 py-1 font-medium">
                    {formatBudget(job.budgetAmount, job.budgetType, t)}
                </span>
            </div>

            {/* CV mini-progress */}
            {cvIsRunning && cvProgress && (
                <div className="rounded-lg bg-[#00b14f]/5 border border-[#00b14f]/10 p-2.5 space-y-1">
                    <div className="flex justify-between text-xs text-gray-600">
                        <span className="flex items-center gap-1">
                            <RefreshCw className="h-3 w-3 animate-spin text-[#00b14f]" />
                            {cvProgress.currentStep ?? t('recommendedJobs.actions.generatingCv')}
                        </span>
                        <span>{cvProgress.percentComplete ?? 0}%</span>
                    </div>
                    <div className="w-full bg-gray-100 rounded-full h-1 overflow-hidden">
                        <div
                            className="h-full bg-[#00b14f] transition-all"
                            style={{ width: `${cvProgress.percentComplete ?? 0}%` }}
                        />
                    </div>
                </div>
            )}

            {/* Actions */}
            <div className="flex items-center gap-2 border-t border-gray-50 pt-3 -mx-1">
                <Link
                    to={`/jobs/${job.id}`}
                    onClick={onView}
                    className="flex flex-1 items-center justify-center gap-1.5 rounded-xl bg-[#00b14f] hover:bg-[#009940] text-white text-sm font-medium py-2 transition-colors"
                >
                    <Eye className="h-3.5 w-3.5" />
                    {t('recommendedJobs.actions.viewDetails')}
                </Link>

                {/* AI CV button */}
                <button
                    onClick={() => !cvDone && genCVMutation.mutate()}
                    disabled={cvIsRunning || genCVMutation.isPending || cvDone}
                    title={cvDone ? t('recommendedJobs.actions.cvGenerated') : t('recommendedJobs.actions.generateCv')}
                    className={`flex items-center justify-center rounded-xl border p-2 transition-colors ${
                        cvDone
                            ? 'border-emerald-200 text-emerald-500 bg-emerald-50'
                            : 'border-gray-200 hover:border-[#00b14f] hover:text-[#00b14f] text-gray-500'
                    } disabled:opacity-60`}
                >
                    {cvDone ? <CheckCircle2 className="h-4 w-4" /> :
                     cvIsRunning ? <RefreshCw className="h-4 w-4 animate-spin" /> :
                     <FileText className="h-4 w-4" />}
                </button>

                <button
                    onClick={onBookmark}
                    title={t('recommendedJobs.actions.bookmark')}
                    className="flex items-center justify-center rounded-xl border border-gray-200 hover:border-[#00b14f] hover:text-[#00b14f] text-gray-500 p-2 transition-colors"
                >
                    <Bookmark className="h-4 w-4" />
                </button>
            </div>

            {/* Viewed indicator */}
            {viewed && (
                <div className="absolute bottom-2 left-4 flex items-center gap-1 text-[10px] text-emerald-500">
                    <Eye className="h-2.5 w-2.5" />
                    {t('recommendedJobs.status.viewed')}
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

function EmptyState({ onSync, isSyncing }: { onSync: () => void; isSyncing: boolean }) {
    const { t } = useTranslation('candidate');
    return (
        <div className="rounded-2xl border border-dashed border-gray-200 bg-white p-12 text-center">
            <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-emerald-50">
                <Sparkles className="h-8 w-8 text-[#00b14f]" />
            </div>
            <h2 className="text-lg font-semibold text-gray-900">{t('recommendedJobs.empty.title')}</h2>
            <p className="mt-2 text-sm text-gray-500 max-w-sm mx-auto">
                {t('recommendedJobs.empty.description')}
            </p>
            <div className="mt-6 flex gap-3 justify-center flex-wrap">
                <button
                    onClick={onSync}
                    disabled={isSyncing}
                    className="flex items-center gap-2 rounded-xl bg-[#00b14f] text-white px-4 py-2.5 text-sm font-medium hover:bg-[#009940] transition-colors disabled:opacity-60"
                >
                    {isSyncing ? <RefreshCw className="h-4 w-4 animate-spin" /> : <Brain className="h-4 w-4" />}
                    {t('recommendedJobs.actions.syncAiSkills')}
                </button>
                <Link
                    to="/candidate/profile"
                    className="flex items-center gap-2 rounded-xl border border-gray-200 text-gray-700 px-4 py-2.5 text-sm font-medium hover:border-gray-300 transition-colors"
                >
                    <Send className="h-4 w-4" />
                    {t('recommendedJobs.actions.updateProfile')}
                </Link>
                <Link
                    to="/jobs"
                    className="flex items-center gap-2 rounded-xl border border-gray-200 text-gray-700 px-4 py-2.5 text-sm font-medium hover:border-gray-300 transition-colors"
                >
                    <Briefcase className="h-4 w-4" />
                    {t('recommendedJobs.actions.exploreJobs')}
                </Link>
            </div>
        </div>
    );
}

function PremiumLockState() {
    const { t } = useTranslation('candidate');
    return (
        <div className="rounded-2xl border border-dashed border-emerald-200 bg-white p-12 text-center shadow-sm">
            <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-emerald-50">
                <Sparkles className="h-8 w-8 text-[#00b14f]" />
            </div>
            <h2 className="text-lg font-semibold text-gray-900">
                {t('recommendedJobs.premium.title', 'Tính năng Premium')}
            </h2>
            <p className="mt-2 text-sm text-gray-500 max-w-sm mx-auto">
                {t('recommendedJobs.premium.description', 'Bạn cần nâng cấp gói Premium để mở khoá tính năng Gợi ý Việc làm Phù hợp bằng AI và tạo CV độc quyền. 🚀')}
            </p>
            <div className="mt-6 flex justify-center">
                <Link
                    to="/candidate/premium"
                    className="flex items-center gap-2 rounded-xl bg-gradient-to-r from-[#00b14f] to-emerald-400 text-white px-6 py-2.5 text-sm font-medium hover:shadow-lg transition-all hover:-translate-y-0.5"
                >
                    <Sparkles className="h-4 w-4" />
                    {t('recommendedJobs.premium.upgradeAction', 'Nâng cấp ngay')}
                </Link>
            </div>
        </div>
    );
}
