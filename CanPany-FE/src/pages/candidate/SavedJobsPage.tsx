import { Link } from 'react-router-dom';
import { Bookmark, Briefcase, RefreshCw } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { Button, Card } from '../../components/ui';
import { JobCard } from '../../components/features/jobs';
import { useBookmarks } from '../../hooks/candidate/useBookmarks';

export function SavedJobsPage() {
    const { t } = useTranslation('candidate');
    const { savedJobs, isLoading, isFetching, error, isBookmarked, isToggling, toggle, refetch } = useBookmarks();

    if (isLoading) {
        return (
            <div className="space-y-6">
                <PageHeader count={0} loading isRefreshing={false} onRefresh={() => void refetch()} />
                <div className="space-y-4">
                    {[1, 2, 3].map((i) => (
                        <div key={i} className="h-40 animate-pulse rounded-xl bg-gray-100" />
                    ))}
                </div>
            </div>
        );
    }

    if (error) {
        return (
            <div className="space-y-6">
                <PageHeader count={0} isRefreshing={isFetching} onRefresh={() => void refetch()} />
                <Card className="p-8 text-center">
                    <h2 className="text-lg font-semibold text-gray-900">{t('savedJobs.emptyTitle')}</h2>
                    <p className="mt-2 text-sm text-gray-500">{t('savedJobs.emptyDesc')}</p>
                    <div className="mt-6">
                        <Button onClick={() => void refetch()}>
                            <RefreshCw className="h-4 w-4" />
                            {t('savedJobs.findMore')}
                        </Button>
                    </div>
                </Card>
            </div>
        );
    }

    return (
        <div className="space-y-6">
            <PageHeader count={savedJobs.length} isRefreshing={isFetching} onRefresh={() => void refetch()} />

            {savedJobs.length === 0 ? (
                <EmptySavedState />
            ) : (
                <div className="space-y-4">
                    {savedJobs.map((job) => (
                        <div key={job.id} className="relative">
                            <JobCard
                                job={job}
                                isBookmarked={isBookmarked(job.id)}
                                onBookmark={(_id) => toggle(job)}
                            />
                            {isToggling(job.id) && (
                                <div className="pointer-events-none absolute inset-0 rounded-xl bg-white/40" />
                            )}
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
}

interface PageHeaderProps {
    count: number;
    loading?: boolean;
    isRefreshing?: boolean;
    onRefresh: () => void;
}

function PageHeader({ count, loading, isRefreshing, onRefresh }: PageHeaderProps) {
    const { t } = useTranslation('candidate');
    return (
        <div className="rounded-2xl bg-white p-6 shadow-sm">
            <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
                <div>
                    <div className="flex items-center gap-2 text-sm text-gray-500">
                        <Bookmark className="h-4 w-4" />
                        <span>{t('savedJobs.title')}</span>
                    </div>
                    <h1 className="mt-1 text-3xl font-bold text-gray-900">
                        {loading ? '—' : t('savedJobs.count', { count })}
                    </h1>
                    <p className="mt-2 text-sm text-gray-600">{t('savedJobs.subtitle')}</p>
                </div>
                <div className="flex items-center gap-2">
                    <Button variant="outline" onClick={onRefresh} disabled={isRefreshing}>
                        <RefreshCw className={`h-4 w-4 ${isRefreshing ? 'animate-spin' : ''}`} />
                        {t('wallet.refresh')}
                    </Button>
                    <Link to="/jobs">
                        <Button variant="outline">
                            <Briefcase className="h-4 w-4" />
                            {t('savedJobs.findMore')}
                        </Button>
                    </Link>
                </div>
            </div>
        </div>
    );
}

function EmptySavedState() {
    const { t } = useTranslation('candidate');
    return (
        <Card className="p-8 text-center">
            <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-gray-100">
                <Bookmark className="h-8 w-8 text-gray-400" />
            </div>
            <h2 className="text-lg font-semibold text-gray-900">{t('savedJobs.emptyTitle')}</h2>
            <p className="mt-2 text-sm text-gray-500">{t('savedJobs.emptyDesc')}</p>
            <div className="mt-6">
                <Link to="/jobs">
                    <Button>
                        <Briefcase className="h-4 w-4" />
                        {t('savedJobs.exploreJobs')}
                    </Button>
                </Link>
            </div>
        </Card>
    );
}
