import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
    Github, RefreshCw, Star, GitFork, Code2, Brain, Zap,
    CheckCircle, AlertCircle, Clock, BarChart3, BookOpen,
    Sparkles, Trophy, ChevronRight, ExternalLink, Shield, ShieldCheck
} from 'lucide-react';
import toast from 'react-hot-toast';
import { candidateApi } from '../../api/candidate.api';
import { consentApi } from '../../api/consent.api';

// ─── types ───────────────────────────────────────────────────────────────────
interface LanguageStat { language: string; percentage: number; bytes: number }
interface Repo { name: string; fullName: string; description?: string; language?: string; stars: number; forks: number; htmlUrl: string }
interface GitHubAnalysis {
    analysisId: string;
    gitHubUsername: string;
    statistics: { totalRepositories: number; totalContributions: number; totalStars: number; totalForks: number };
    languages: LanguageStat[];
    skills: { primary: string[]; expertiseLevel: string; specializations: string[]; proficiency?: Record<string, number>; recommendations?: string[] };
    topRepositories: Repo[];
    aiSummary?: string;
    analyzedAt: string;
}

// Language colour palette
const LANG_COLORS: Record<string, string> = {
    TypeScript: '#3178c6', JavaScript: '#f1e05a', Python: '#3572A5',
    'C#': '#178600', Java: '#b07219', Go: '#00ADD8', Rust: '#dea584',
    C: '#555555', 'C++': '#f34b7d', PHP: '#4F5D95', Ruby: '#701516',
    Swift: '#F05138', Kotlin: '#A97BFF', Dart: '#00B4AB',
    HTML: '#e34c26', CSS: '#563d7c', Vue: '#41b883', Svelte: '#ff3e00',
};
const langColor = (l: string) => LANG_COLORS[l] ?? '#8b949e';

// ─── page ────────────────────────────────────────────────────────────────────
export function GitHubAnalysisPage() {
    const queryClient = useQueryClient();
    const { t } = useTranslation('candidate');
    const [analysisJobId, setAnalysisJobId] = useState<string | null>(null);

    // Latest analysis result
    const { data: latestRaw, isLoading, refetch } = useQuery({
        queryKey: ['github-analysis-latest'],
        queryFn: () => candidateApi.getLatestGitHubAnalysis(),
        retry: false,
    });
    const analysis: GitHubAnalysis | null = latestRaw?.data ?? null;

    // Consent check
    const { data: hasConsent, isLoading: consentLoading, refetch: refetchConsent } = useQuery({
        queryKey: ['github-consent-check'],
        queryFn: () => consentApi.checkConsent('ExternalSync_GitHub'),
    });

    const grantConsentMutation = useMutation({
        mutationFn: () => consentApi.grantConsent('ExternalSync_GitHub', '1.0'),
        onSuccess: () => {
            refetchConsent();
            toast.success(t('githubAnalysis.consent.granted'));
        },
        onError: () => toast.error(t('githubAnalysis.consent.error')),
    });

    // Sync skill job polling
    const { data: jobProgress } = useQuery({
        queryKey: ['github-job-status', analysisJobId],
        queryFn: () => candidateApi.getGitHubJobStatus(analysisJobId!),
        enabled: !!analysisJobId,
        refetchInterval: (q) => {
            const s = q.state.data?.status;
            if (s === 'Completed' || s === 'Failed') return false;
            return 2000;
        },
    });

    // Watch for completion
    if (jobProgress?.status === 'Completed' && analysisJobId) {
        setAnalysisJobId(null);
        queryClient.invalidateQueries({ queryKey: ['github-analysis-latest'] });
        refetch();
        toast.success(t('githubAnalysis.toast.completed'));
    }

    // Re-run analysis (syncs skills from all repos)
    const refreshMutation = useMutation({
        mutationFn: () => candidateApi.syncSkillsFromRepos([]), // will trigger full sync via GitHubController
        onSuccess: (d) => {
            setAnalysisJobId(d.jobId);
            toast.success(t('githubAnalysis.toast.started'));
        },
        onError: (err: any) => {
            const msg = err?.response?.data?.message || '';
            if (msg.toLowerCase().includes('consent')) {
                toast.error(t('githubAnalysis.consent.required'));
                refetchConsent();
            } else {
                toast.error(t('githubAnalysis.toast.error'));
            }
        },
    });

    const isRunning = !!analysisJobId && jobProgress?.status !== 'Completed' && jobProgress?.status !== 'Failed';
    const needsConsent = !consentLoading && hasConsent === false;

    return (
        <div className="space-y-6">
            {/* Consent Banner */}
            {needsConsent && (
                <div className="rounded-2xl border border-amber-200 bg-gradient-to-br from-amber-50 to-orange-50 p-5 shadow-sm">
                    <div className="flex items-start gap-4">
                        <div className="rounded-xl bg-amber-100 p-3">
                            <Shield className="h-5 w-5 text-amber-600" />
                        </div>
                        <div className="flex-1">
                            <h3 className="text-sm font-semibold text-amber-900">
                                {t('githubAnalysis.consent.title', { defaultValue: 'Yêu cầu đồng ý truy cập GitHub' })}
                            </h3>
                            <p className="mt-1 text-xs text-amber-700 leading-relaxed">
                                {t('githubAnalysis.consent.description', { defaultValue: 'Để phân tích GitHub, bạn cần đồng ý cho hệ thống truy cập và xử lý dữ liệu GitHub của bạn theo Nghị định 13/2023/NĐ-CP.' })}
                            </p>
                            <button
                                onClick={() => grantConsentMutation.mutate()}
                                disabled={grantConsentMutation.isPending}
                                className="mt-3 flex items-center gap-2 rounded-xl bg-amber-600 text-white px-4 py-2 text-xs font-medium hover:bg-amber-700 disabled:opacity-50 transition-colors"
                            >
                                {grantConsentMutation.isPending ? <RefreshCw className="h-3.5 w-3.5 animate-spin" /> : <ShieldCheck className="h-3.5 w-3.5" />}
                                {t('githubAnalysis.consent.grantBtn', { defaultValue: 'Đồng ý truy cập GitHub' })}
                            </button>
                        </div>
                    </div>
                </div>
            )}
            {/* Header */}
            <div className="rounded-2xl bg-gradient-to-br from-gray-900 via-gray-800 to-[#161b22] p-6 text-white shadow-xl">
                <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
                    <div>
                        <div className="flex items-center gap-2 text-gray-300 text-sm mb-1">
                            <Github className="h-4 w-4" />
                            <span>AI-Powered GitHub Profile Analysis</span>
                        </div>
                        <h1 className="text-3xl font-bold">{t('githubAnalysis.title')}</h1>
                        <p className="mt-2 text-gray-300 text-sm max-w-lg">
                            {t('githubAnalysis.subtitle')}
                        </p>
                        {analysis && (
                            <p className="mt-1 text-xs text-gray-400">
                                {t('githubAnalysis.lastAnalyzed', { date: new Date(analysis.analyzedAt).toLocaleString('vi-VN') })}
                                {' · '}@{analysis.gitHubUsername}
                            </p>
                        )}
                    </div>
                    <button
                        onClick={() => refreshMutation.mutate()}
                        disabled={isRunning || refreshMutation.isPending || needsConsent}
                        className="flex items-center gap-2 rounded-xl bg-white/10 hover:bg-white/20 disabled:opacity-50 px-5 py-2.5 text-sm font-medium transition-all self-start lg:self-auto"
                    >
                        <RefreshCw className={`h-4 w-4 ${isRunning ? 'animate-spin' : ''}`} />
                        {isRunning ? t('githubAnalysis.actions.analyzing') : t('githubAnalysis.actions.reanalyze')}
                    </button>
                </div>

                {/* Progress bar */}
                {isRunning && jobProgress && (
                    <div className="mt-4 space-y-1">
                        <div className="flex justify-between text-xs text-gray-300">
                            <span>{jobProgress.currentStep ?? t('githubAnalysis.progress.processing')}</span>
                            <span>{jobProgress.percentComplete}%</span>
                        </div>
                        <div className="w-full bg-white/10 rounded-full h-2 overflow-hidden">
                            <div
                                className="h-full bg-gradient-to-r from-[#00b14f] to-emerald-400 transition-all duration-500"
                                style={{ width: `${jobProgress.percentComplete}%` }}
                            />
                        </div>
                    </div>
                )}
            </div>

            {isLoading ? (
                <LoadingSkeleton />
            ) : !analysis ? (
                <EmptyState onAnalyze={() => refreshMutation.mutate()} loading={refreshMutation.isPending} />
            ) : (
                <>
                    {/* Stats row */}
                    <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
                        <StatCard icon={<Code2 className="h-5 w-5 text-blue-500" />} label={t('githubAnalysis.stats.repositories')} value={analysis.statistics.totalRepositories} />
                        <StatCard icon={<Star className="h-5 w-5 text-yellow-500" />} label={t('githubAnalysis.stats.totalStars')} value={analysis.statistics.totalStars} />
                        <StatCard icon={<GitFork className="h-5 w-5 text-purple-500" />} label={t('githubAnalysis.stats.totalForks')} value={analysis.statistics.totalForks} />
                        <StatCard icon={<Zap className="h-5 w-5 text-emerald-500" />} label={t('githubAnalysis.stats.contributions')} value={analysis.statistics.totalContributions} />
                    </div>

                    <div className="grid gap-6 lg:grid-cols-2">
                        {/* Languages */}
                        <div className="rounded-2xl border border-gray-100 bg-white p-6 shadow-sm">
                            <h2 className="text-base font-semibold text-gray-900 flex items-center gap-2 mb-4">
                                <BarChart3 className="h-4 w-4 text-[#00b14f]" />
                                {t('githubAnalysis.sections.languages')}
                            </h2>
                            <div className="space-y-3">
                                {analysis.languages.map((l) => (
                                    <div key={l.language}>
                                        <div className="flex justify-between text-xs mb-1">
                                            <span className="font-medium text-gray-700">{l.language}</span>
                                            <span className="text-gray-500">{l.percentage.toFixed(1)}%</span>
                                        </div>
                                        <div className="w-full bg-gray-100 rounded-full h-2 overflow-hidden">
                                            <div
                                                className="h-full rounded-full transition-all"
                                                style={{ width: `${l.percentage}%`, backgroundColor: langColor(l.language) }}
                                            />
                                        </div>
                                    </div>
                                ))}
                            </div>
                        </div>

                        {/* Skills */}
                        <div className="rounded-2xl border border-gray-100 bg-white p-6 shadow-sm">
                            <h2 className="text-base font-semibold text-gray-900 flex items-center gap-2 mb-1">
                                <Brain className="h-4 w-4 text-[#00b14f]" />
                                {t('githubAnalysis.sections.skills')}
                            </h2>
                            {analysis.skills.expertiseLevel && (
                                <p className="text-xs text-gray-500 mb-3">
                                    {t('githubAnalysis.sections.level')} <span className="font-semibold text-gray-800">{analysis.skills.expertiseLevel}</span>
                                </p>
                            )}
                            <div className="flex flex-wrap gap-2 mb-4">
                                {analysis.skills.primary.map((s) => (
                                    <span key={s} className="px-2.5 py-1 rounded-full bg-[#00b14f]/10 text-[#00b14f] text-xs font-medium">
                                        {s}
                                    </span>
                                ))}
                            </div>
                            {analysis.skills.specializations?.length > 0 && (
                                <>
                                    <p className="text-xs font-medium text-gray-500 mb-2">{t('githubAnalysis.sections.specializations')}</p>
                                    <div className="flex flex-wrap gap-2">
                                        {analysis.skills.specializations.map((s) => (
                                            <span key={s} className="px-2.5 py-1 rounded-full bg-blue-50 text-blue-700 text-xs font-medium">
                                                {s}
                                            </span>
                                        ))}
                                    </div>
                                </>
                            )}
                        </div>
                    </div>

                    {/* AI Summary */}
                    {analysis.aiSummary && (
                        <div className="rounded-2xl border border-[#00b14f]/20 bg-gradient-to-br from-[#00b14f]/5 to-transparent p-6 shadow-sm">
                            <h2 className="text-base font-semibold text-gray-900 flex items-center gap-2 mb-3">
                                <Sparkles className="h-4 w-4 text-[#00b14f]" />
                                {t('githubAnalysis.sections.aiSummary')}
                            </h2>
                            <p className="text-sm text-gray-700 leading-relaxed whitespace-pre-wrap">{analysis.aiSummary}</p>
                        </div>
                    )}

                    {/* Skill Recommendations */}
                    {analysis.skills.recommendations && analysis.skills.recommendations.length > 0 && (
                        <div className="rounded-2xl border border-gray-100 bg-white p-6 shadow-sm">
                            <h2 className="text-base font-semibold text-gray-900 flex items-center gap-2 mb-3">
                                <Trophy className="h-4 w-4 text-amber-500" />
                                {t('githubAnalysis.sections.recommendations')}
                            </h2>
                            <ul className="space-y-2">
                                {analysis.skills.recommendations.map((r, i) => (
                                    <li key={i} className="flex items-start gap-2 text-sm text-gray-700">
                                        <ChevronRight className="h-4 w-4 text-[#00b14f] mt-0.5 shrink-0" />
                                        {r}
                                    </li>
                                ))}
                            </ul>
                        </div>
                    )}

                    {/* Top Repos */}
                    {analysis.topRepositories.length > 0 && (
                        <div className="rounded-2xl border border-gray-100 bg-white p-6 shadow-sm">
                            <h2 className="text-base font-semibold text-gray-900 flex items-center gap-2 mb-4">
                                <BookOpen className="h-4 w-4 text-[#00b14f]" />
                                {t('githubAnalysis.sections.topRepos')}
                            </h2>
                            <div className="divide-y divide-gray-50">
                                {analysis.topRepositories.map((repo) => (
                                    <div key={repo.fullName} className="flex items-start justify-between py-3 gap-4">
                                        <div className="min-w-0 flex-1">
                                            <div className="flex items-center gap-2 flex-wrap">
                                                <span className="font-medium text-sm text-gray-900 truncate">{repo.name}</span>
                                                {repo.language && (
                                                    <span
                                                        className="h-2 w-2 rounded-full shrink-0"
                                                        style={{ backgroundColor: langColor(repo.language) }}
                                                    />
                                                )}
                                                {repo.language && (
                                                    <span className="text-xs text-gray-500">{repo.language}</span>
                                                )}
                                            </div>
                                            {repo.description && (
                                                <p className="text-xs text-gray-500 mt-0.5 line-clamp-1">{repo.description}</p>
                                            )}
                                            <div className="flex items-center gap-3 mt-1 text-xs text-gray-400">
                                                <span>★ {repo.stars}</span>
                                                <span>⑂ {repo.forks}</span>
                                            </div>
                                        </div>
                                        <a
                                            href={repo.htmlUrl}
                                            target="_blank"
                                            rel="noopener noreferrer"
                                            className="shrink-0 text-gray-400 hover:text-gray-600 transition-colors"
                                        >
                                            <ExternalLink className="h-4 w-4" />
                                        </a>
                                    </div>
                                ))}
                            </div>
                        </div>
                    )}
                </>
            )}
        </div>
    );
}

// ─── sub-components ───────────────────────────────────────────────────────────
function StatCard({ icon, label, value }: { icon: React.ReactNode; label: string; value: number }) {
    return (
        <div className="rounded-2xl border border-gray-100 bg-white p-5 shadow-sm flex items-center gap-4">
            <div className="rounded-xl bg-gray-50 p-2.5">{icon}</div>
            <div>
                <p className="text-2xl font-bold text-gray-900">{value.toLocaleString()}</p>
                <p className="text-xs text-gray-500">{label}</p>
            </div>
        </div>
    );
}

function LoadingSkeleton() {
    return (
        <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
            {[...Array(4)].map((_, i) => (
                <div key={i} className="h-24 animate-pulse rounded-2xl bg-gray-100" />
            ))}
        </div>
    );
}

function EmptyState({ onAnalyze, loading }: { onAnalyze: () => void; loading: boolean }) {
    const { t } = useTranslation('candidate');
    return (
        <div className="rounded-2xl border border-dashed border-gray-200 bg-white p-12 text-center">
            <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-gray-50">
                <Github className="h-8 w-8 text-gray-400" />
            </div>
            <h2 className="text-lg font-semibold text-gray-900">{t('githubAnalysis.empty.title')}</h2>
            <p className="mt-2 text-sm text-gray-500 max-w-sm mx-auto">
                {t('githubAnalysis.empty.description')}
            </p>
            <div className="mt-6 flex gap-3 justify-center">
                <button
                    onClick={onAnalyze}
                    disabled={loading}
                    className="flex items-center gap-2 rounded-xl bg-gray-900 text-white px-4 py-2.5 text-sm font-medium hover:bg-gray-800 disabled:opacity-60 transition-colors"
                >
                    {loading ? <RefreshCw className="h-4 w-4 animate-spin" /> : <Github className="h-4 w-4" />}
                    {t('githubAnalysis.actions.start')}
                </button>
            </div>
        </div>
    );
}

// ─── status icons (used externally) ──────────────────────────────────────────
export function StatusIcon({ status }: { status: string }) {
    if (status === 'Completed') return <CheckCircle className="h-4 w-4 text-emerald-500" />;
    if (status === 'Failed') return <AlertCircle className="h-4 w-4 text-red-500" />;
    return <Clock className="h-4 w-4 text-amber-500 animate-pulse" />;
}
