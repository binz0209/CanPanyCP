import { Link } from 'react-router-dom';
import { Bookmark, Briefcase } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { Button, Card } from '../../components/ui';
import { JobCard } from '../../components/features/jobs';
import { useBookmarks } from '../../hooks/candidate/useBookmarks';

export function SavedJobsPage() {
    const { savedJobs, isLoading, isBookmarked, toggle } = useBookmarks();

    if (isLoading) {
        return (
            <div className="space-y-6">
                <PageHeader count={0} loading />
                <div className="space-y-4">
                    {[1, 2, 3].map((i) => (
                        <div key={i} className="h-40 animate-pulse rounded-xl bg-gray-100" />
                    ))}
                </div>
            </div>
        );
    }

    return (
        <div className="space-y-6">
            <PageHeader count={savedJobs.length} />

            {savedJobs.length === 0 ? (
                <EmptySavedState />
            ) : (
                <div className="space-y-4">
                    {savedJobs.map((job) => (
                        <JobCard
                            key={job.id}
                            job={job}
                            isBookmarked={isBookmarked(job.id)}
                            onBookmark={(_id) => toggle(job)}
                        />
                    ))}
                </div>
            )}
        </div>
    );
}

interface PageHeaderProps {
    count: number;
    loading?: boolean;
}

function PageHeader({ count, loading }: PageHeaderProps) {
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
                <Link to="/jobs">
                    <Button variant="outline">
                        <Briefcase className="h-4 w-4" />
                        {t('savedJobs.findMore')}
                    </Button>
                </Link>
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
