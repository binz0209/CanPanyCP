import { useState } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { MapPin, Clock, DollarSign, Bookmark, Building2, ArrowLeft, Sparkles } from 'lucide-react';
import toast from 'react-hot-toast';
import { useTranslation } from 'react-i18next';
import { Button, Badge, Card } from '../../components/ui';
import { ApplyModal } from '../../components/features/jobs';
import { jobsApi } from '../../api';
import { formatRelativeTime, formatCurrency, formatDate } from '../../utils';
import { cn } from '../../utils';
import { useAuthStore } from '@/stores/auth.store';
import { useBookmarks } from '@/hooks/candidate/useBookmarks';

export function JobDetailPage() {
    const { t } = useTranslation('public');
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const queryClient = useQueryClient();
    const { isAuthenticated, user } = useAuthStore();
    const [showApplyModal, setShowApplyModal] = useState(false);

    const { data, isLoading, error } = useQuery({
        queryKey: ['job', id],
        queryFn: () => jobsApi.getById(id!),
        enabled: !!id,
    });

    const { isBookmarked, toggle, isToggling } = useBookmarks();

    if (isLoading) {
        return (
            <div className="min-h-screen bg-gray-50 dark:bg-slate-950">
                <div className="mx-auto max-w-4xl px-4 py-8">
                    <div className="h-64 animate-pulse rounded-xl bg-gray-200 dark:bg-slate-800" />
                </div>
            </div>
        );
    }

    if (error || !data) {
        return (
            <div className="min-h-screen bg-gray-50 py-20 text-center dark:bg-slate-950">
                <h2 className="text-xl font-semibold text-gray-900 dark:text-slate-100">{t('jobDetail.notFound')}</h2>
                <Link to="/jobs">
                    <Button variant="outline" className="mt-4">
                        <ArrowLeft className="h-4 w-4" />
                        {t('jobDetail.backToList')}
                    </Button>
                </Link>
            </div>
        );
    }

    const { job } = data;
    const bookmarked = isBookmarked(job.id) || data.isBookmarked;

    const levelColors = {
        Junior: 'bg-green-100 text-green-800 dark:bg-green-900/40 dark:text-green-200',
        Mid: 'bg-blue-100 text-blue-800 dark:bg-blue-900/40 dark:text-blue-200',
        Senior: 'bg-purple-100 text-purple-800 dark:bg-purple-900/40 dark:text-purple-200',
        Expert: 'bg-red-100 text-red-800 dark:bg-red-900/40 dark:text-red-200',
    };

    return (
        <>
        <div className="min-h-screen bg-gray-50 dark:bg-slate-950">
            {/* Header */}
            <div className="border-b border-gray-200 bg-white dark:border-slate-800 dark:bg-slate-900">
                <div className="mx-auto max-w-4xl px-4 py-6 sm:px-6 lg:px-8">
                    <Link to="/jobs" className="mb-4 inline-flex items-center gap-1 text-sm text-gray-500 hover:text-gray-700 dark:text-slate-400 dark:hover:text-slate-200">
                        <ArrowLeft className="h-4 w-4" />
                        {t('jobDetail.back')}
                    </Link>

                    <div className="flex flex-col gap-6 lg:flex-row lg:items-start lg:justify-between">
                        <div className="flex gap-4">
                            <div className="flex h-16 w-16 shrink-0 items-center justify-center rounded-xl bg-gradient-to-br from-blue-100 to-purple-100 dark:from-slate-800 dark:to-slate-700 lg:h-20 lg:w-20">
                                <Building2 className="h-8 w-8 text-blue-600 dark:text-blue-300 lg:h-10 lg:w-10" />
                            </div>
                            <div>
                                <h1 className="text-2xl font-bold text-gray-900 dark:text-slate-100 lg:text-3xl">{job.title}</h1>
                                <p className="mt-1 text-lg text-gray-600 dark:text-slate-300">{job.companyId}</p>
                                <div className="mt-3 flex flex-wrap items-center gap-4 text-sm text-gray-500 dark:text-slate-400">
                                    {job.location && (
                                        <span className="flex items-center gap-1">
                                            <MapPin className="h-4 w-4" />
                                            {job.location}
                                        </span>
                                    )}
                                    <span className="flex items-center gap-1">
                                        <Clock className="h-4 w-4" />
                                        {formatRelativeTime(job.createdAt)}
                                    </span>
                                </div>
                            </div>
                        </div>

                        <div className="flex gap-3">
                            {isAuthenticated && user?.role === 'Candidate' && (
                                <Button
                                    variant="outline"
                                    size="lg"
                                    className="border-indigo-200 text-indigo-700 hover:bg-indigo-50"
                                    onClick={() => navigate(`/candidate/cv/ai?jobId=${job.id}&jobTitle=${encodeURIComponent(job.title)}&autoStart=true`)}
                                >
                                    <Sparkles className="h-4 w-4 mr-2" />
                                    {t('jobDetail.genCv')}
                                </Button>
                            )}
                            <Button
                                variant="outline"
                                size="lg"
                                onClick={() => toggle(job)}
                                isLoading={isToggling(job.id)}
                                aria-label={bookmarked ? t('jobDetail.saveAriaUnbookmark') : t('jobDetail.saveAriaSave')}
                            >
                                <Bookmark className={cn('h-4 w-4', bookmarked && 'fill-current text-[#00b14f]')} />
                                {bookmarked ? t('jobDetail.saved') : t('jobDetail.save')}
                            </Button>
                            <Button
                                size="lg"
                                onClick={() => isAuthenticated ? setShowApplyModal(true) : navigate('/auth/login')}
                            >
                                {t('jobDetail.applyNow')}
                            </Button>
                        </div>
                    </div>
                </div>
            </div>

            {/* Content */}
            <div className="mx-auto max-w-4xl px-4 py-8 sm:px-6 lg:px-8">
                <div className="grid gap-8 lg:grid-cols-3">
                    {/* Main Content */}
                    <div className="lg:col-span-2">
                        <Card className="p-6 dark:border-slate-800 dark:bg-slate-900">
                            <h2 className="text-lg font-semibold text-gray-900 dark:text-slate-100">{t('jobDetail.descriptionSection')}</h2>
                            <div className="prose prose-gray mt-4 max-w-none dark:prose-invert">
                                <p className="whitespace-pre-wrap text-gray-600 dark:text-slate-300">{job.description}</p>
                            </div>
                        </Card>

                        {job.skillIds && job.skillIds.length > 0 && (
                            <Card className="mt-6 p-6">
                                <h2 className="text-lg font-semibold text-gray-900 dark:text-slate-100">{t('jobDetail.skillsSection')}</h2>
                                <div className="mt-4 flex flex-wrap gap-2">
                                    {job.skillIds.map((skill) => (
                                        <Badge key={skill} variant="secondary">{skill}</Badge>
                                    ))}
                                </div>
                            </Card>
                        )}
                    </div>

                    {/* Sidebar */}
                    <div className="space-y-6">
                        <Card className="p-6 dark:border-slate-800 dark:bg-slate-900">
                            <h2 className="text-lg font-semibold text-gray-900 dark:text-slate-100">{t('jobDetail.infoSection')}</h2>
                            <dl className="mt-4 space-y-4">
                                {job.budgetAmount && (
                                    <div>
                                        <dt className="text-sm text-gray-500 dark:text-slate-400">{t('jobDetail.salary')}</dt>
                                        <dd className="mt-1 flex items-center gap-1 font-medium text-gray-900 dark:text-slate-100">
                                            <DollarSign className="h-4 w-4 text-green-600" />
                                            {formatCurrency(job.budgetAmount)}
                                            {job.budgetType === 'Hourly' && <span className="text-gray-500 dark:text-slate-400">{t('jobDetail.salaryPerHour')}</span>}
                                        </dd>
                                    </div>
                                )}
                                {job.level && (
                                    <div>
                                        <dt className="text-sm text-gray-500 dark:text-slate-400">{t('jobDetail.level')}</dt>
                                        <dd className="mt-1">
                                            <Badge className={levelColors[job.level]}>{job.level}</Badge>
                                        </dd>
                                    </div>
                                )}
                                <div>
                                    <dt className="text-sm text-gray-500 dark:text-slate-400">{t('jobDetail.workType')}</dt>
                                    <dd className="mt-1 font-medium text-gray-900 dark:text-slate-100">
                                        {job.isRemote ? t('jobDetail.remote') : t('jobDetail.onsite')}
                                    </dd>
                                </div>
                                {job.deadline && (
                                    <div>
                                        <dt className="text-sm text-gray-500 dark:text-slate-400">{t('jobDetail.deadline')}</dt>
                                        <dd className="mt-1 font-medium text-gray-900 dark:text-slate-100">{formatDate(job.deadline)}</dd>
                                    </div>
                                )}
                            </dl>
                        </Card>

                        <Card className="p-6 dark:border-slate-800 dark:bg-slate-900">
                            <h2 className="text-lg font-semibold text-gray-900 dark:text-slate-100">{t('jobDetail.statsSection')}</h2>
                            <dl className="mt-4 grid grid-cols-2 gap-4">
                                <div className="rounded-lg bg-gray-50 p-3 text-center dark:bg-slate-800">
                                    <dt className="text-2xl font-bold text-blue-600 dark:text-blue-300">{job.viewCount}</dt>
                                    <dd className="text-sm text-gray-500 dark:text-slate-400">{t('jobDetail.views')}</dd>
                                </div>
                                <div className="rounded-lg bg-gray-50 p-3 text-center dark:bg-slate-800">
                                    <dt className="text-2xl font-bold text-green-600 dark:text-green-300">{job.applicationCount}</dt>
                                    <dd className="text-sm text-gray-500 dark:text-slate-400">{t('jobDetail.applicants')}</dd>
                                </div>
                            </dl>
                        </Card>
                    </div>
                </div>
            </div>
        </div>

        <ApplyModal
            jobId={job.id}
            jobTitle={job.title}
            isOpen={showApplyModal}
            onClose={() => setShowApplyModal(false)}
            onSuccess={() => {
                setShowApplyModal(false);
                queryClient.invalidateQueries({ queryKey: ['job', id] });
                toast.success(t('jobDetail.applySuccess'), { duration: 2000, position: 'top-right' });
            }}
        />
        </>
    );
}
