import { useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { isAxiosError } from 'axios';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { BriefcaseBusiness, ChevronLeft, ChevronRight, Eye, FilePenLine, Plus, RefreshCcw } from 'lucide-react';
import toast from 'react-hot-toast';
import { Button, Card } from '../../components/ui';
import { jobsApi } from '../../api';
import { formatDate, formatNumber } from '../../utils';
import type { Job, JobStatus } from '../../types';
import {
    CompanyProfileRequiredState,
    CompanyWorkspaceErrorState,
    CompanyWorkspaceLoader,
    EmptyState,
    SectionHeader,
    StatusBadge,
} from '../../components/features/companies';
import { useCompanyWorkspace } from '../../hooks/company/useCompanyWorkspace';
import { companyKeys } from '../../lib/queryKeys';
import { useTranslation } from 'react-i18next';

type JobFilter = 'All' | JobStatus;
const PAGE_SIZE = 10;

export function CompanyJobsPage() {
    const queryClient = useQueryClient();
    const { t } = useTranslation('company');
    const [activeFilter, setActiveFilter] = useState<JobFilter>('All');
    const [currentPage, setCurrentPage] = useState(1);
    const [processingJobId, setProcessingJobId] = useState<string | null>(null);
    const { companyId, isLoading: isWorkspaceLoading, isMissingProfile, hasFatalError } = useCompanyWorkspace();

    // Paginated query for the current page
    const jobsQuery = useQuery({
        queryKey: companyKeys.workspaceJobs(companyId!, currentPage),
        queryFn: () => jobsApi.getByCompanyPaged(companyId!, currentPage, PAGE_SIZE),
        enabled: !!companyId,
        placeholderData: (prev) => prev,
    });

    const statusMutation = useMutation({
        mutationFn: async ({ jobId, nextStatus }: { jobId: string; nextStatus: 'Open' | 'Closed' }) => {
            if (nextStatus === 'Closed') {
                await jobsApi.close(jobId);
                return;
            }

            await jobsApi.reopen(jobId);
        },
        onSuccess: async (_, variables) => {
            // Invalidate all workspace job pages
            await queryClient.invalidateQueries({
                queryKey: [...companyKeys.all, 'workspace', 'jobs', companyId!],
            });
            toast.success(
                variables.nextStatus === 'Closed'
                    ? t('jobs.toastClosed')
                    : t('jobs.toastReopened')
            );
        },
        onSettled: () => {
            setProcessingJobId(null);
        },
        onError: (error) => {
            const message = isAxiosError(error)
                ? error.response?.data?.message || t('jobs.toastStatusFailed')
                : t('jobs.toastStatusFailed');
            toast.error(message);
        },
    });

    const pagedData = jobsQuery.data;
    const jobs = useMemo(() => pagedData?.jobs || [], [pagedData]);
    const totalItems = pagedData?.total ?? 0;
    const totalPages = pagedData?.totalPages ?? 0;

    const filteredJobs = useMemo(() => {
        if (activeFilter === 'All') return jobs;
        return jobs.filter((job: Job) => job.status === activeFilter);
    }, [activeFilter, jobs]);

    // Stats based on the current page (approximate, since we only have paged data)
    // For accurate stats we count the paged items only — total is from server
    const statistics = useMemo(() => {
        return {
            total: totalItems,
            open: jobs.filter((job: Job) => job.status === 'Open').length,
            closed: jobs.filter((job: Job) => job.status === 'Closed').length,
            draft: jobs.filter((job: Job) => job.status === 'Draft').length,
        };
    }, [totalItems, jobs]);

    // Reset to page 1 when filter changes
    const handleFilterChange = (filter: JobFilter) => {
        setActiveFilter(filter);
        // Note: server-side filtering is not implemented — filtering is client-side on the current page
    };

    if (isWorkspaceLoading || jobsQuery.isLoading) {
        return <CompanyWorkspaceLoader />;
    }

    if (isMissingProfile) {
        return (
            <CompanyProfileRequiredState
                title={t('jobs.profileRequired')}
                description={t('jobs.profileRequiredDesc')}
                icon={<BriefcaseBusiness className="h-6 w-6" />}
                action={
                    <Link to="/company/profile">
                        <Button>{t('jobs.btnGoProfile')}</Button>
                    </Link>
                }
            />
        );
    }

    if (hasFatalError || jobsQuery.error) {
        return (
            <CompanyWorkspaceErrorState
                title={t('jobs.errorTitle')}
                description={t('jobs.errorDesc')}
                icon={<BriefcaseBusiness className="h-6 w-6" />}
            />
        );
    }

    return (
        <div className="space-y-6">
            <SectionHeader
                title={t('jobs.title')}
                description={t('jobs.description')}
                actions={
                    <Link to="/company/jobs/new">
                        <Button>
                            <Plus className="h-4 w-4" />
                            {t('jobs.btnCreate')}
                        </Button>
                    </Link>
                }
            />

            <section className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
                <Card className="p-5">
                    <p className="text-sm text-gray-500">{t('jobs.statTotal')}</p>
                    <p className="mt-2 text-3xl font-bold text-gray-900">{formatNumber(statistics.total)}</p>
                </Card>
                <Card className="p-5">
                    <p className="text-sm text-gray-500">{t('jobs.statOpen')}</p>
                    <p className="mt-2 text-3xl font-bold text-gray-900">{formatNumber(statistics.open)}</p>
                </Card>
                <Card className="p-5">
                    <p className="text-sm text-gray-500">{t('jobs.statClosed')}</p>
                    <p className="mt-2 text-3xl font-bold text-gray-900">{formatNumber(statistics.closed)}</p>
                </Card>
                <Card className="p-5">
                    <p className="text-sm text-gray-500">{t('jobs.statDraft')}</p>
                    <p className="mt-2 text-3xl font-bold text-gray-900">{formatNumber(statistics.draft)}</p>
                </Card>
            </section>

            <Card className="p-6">
                <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
                    <div className="flex flex-wrap gap-2">
                        {(['All', 'Open', 'Closed', 'Draft'] as JobFilter[]).map((filter) => (
                            <Button
                                key={filter}
                                variant={activeFilter === filter ? 'default' : 'outline'}
                                size="sm"
                                onClick={() => handleFilterChange(filter)}
                            >
                                {filter === 'All' ? t('jobs.filterAll') : null}
                                {filter === 'Open' ? t('jobs.filterOpen') : null}
                                {filter === 'Closed' ? t('jobs.filterClosed') : null}
                                {filter === 'Draft' ? t('jobs.filterDraft') : null}
                            </Button>
                        ))}
                    </div>

                    <div className="rounded-lg bg-gray-50 px-3 py-2 text-sm text-gray-500">
                        {t('jobs.actionHint')}
                    </div>
                </div>

                {filteredJobs.length === 0 ? (
                    <div className="mt-6">
                        <EmptyState
                            title={t('jobs.emptyTitle')}
                            description={t('jobs.emptyDesc')}
                            icon={<BriefcaseBusiness className="h-6 w-6" />}
                        />
                    </div>
                ) : (
                    <div className="mt-6 space-y-4">
                        {filteredJobs.map((job: Job) => (
                            <div
                                key={job.id}
                                className="rounded-xl border border-gray-100 p-5 transition hover:border-[#00b14f]/30 hover:shadow-sm"
                            >
                                <div className="flex flex-col gap-4 xl:flex-row xl:items-center xl:justify-between">
                                    <div className="space-y-2">
                                        <div className="flex flex-wrap items-center gap-2">
                                            <h3 className="text-lg font-semibold text-gray-900">{job.title}</h3>
                                            <StatusBadge status={job.status} kind="job" />
                                        </div>
                                        <p className="text-sm text-gray-500">
                                            {job.location || t('jobs.noLocation')}
                                        </p>
                                        <div className="flex flex-wrap gap-4 text-sm text-gray-500">
                                            <span>
                                                {t('jobs.views')}: {formatNumber(job.viewCount)}
                                            </span>
                                            <span>
                                                {t('jobs.candidates')}: {formatNumber(job.applicationCount)}
                                            </span>
                                            <span>
                                                {t('jobs.createdAt')}: {formatDate(job.createdAt)}
                                            </span>
                                        </div>
                                    </div>

                                    <div className="flex flex-wrap gap-2">
                                        <Link to={`/jobs/${job.id}`}>
                                            <Button variant="outline" size="sm">
                                                <Eye className="h-4 w-4" />
                                                {t('jobs.btnViewPublic')}
                                            </Button>
                                        </Link>
                                        <Link to={`/company/jobs/${job.id}/edit`}>
                                            <Button variant="outline" size="sm">
                                                <FilePenLine className="h-4 w-4" />
                                                {t('jobs.btnEdit')}
                                            </Button>
                                        </Link>
                                        {job.status === 'Closed' ? (
                                            <Button
                                                size="sm"
                                                onClick={() => {
                                                    setProcessingJobId(job.id);
                                                    statusMutation.mutate({ jobId: job.id, nextStatus: 'Open' });
                                                }}
                                                isLoading={statusMutation.isPending && processingJobId === job.id}
                                            >
                                                <RefreshCcw className="h-4 w-4" />
                                                {t('jobs.btnReopen')}
                                            </Button>
                                        ) : (
                                            <Button
                                                size="sm"
                                                variant="outline"
                                                onClick={() => {
                                                    setProcessingJobId(job.id);
                                                    statusMutation.mutate({ jobId: job.id, nextStatus: 'Closed' });
                                                }}
                                                isLoading={statusMutation.isPending && processingJobId === job.id}
                                            >
                                                {t('jobs.btnClose')}
                                            </Button>
                                        )}
                                    </div>
                                </div>
                            </div>
                        ))}
                    </div>
                )}

                {/* Pagination */}
                {totalPages > 1 && (
                    <div className="mt-6 flex items-center justify-between border-t border-gray-100 pt-4">
                        <p className="text-sm text-gray-500">
                            {t('jobs.paginationInfo', {
                                from: (currentPage - 1) * PAGE_SIZE + 1,
                                to: Math.min(currentPage * PAGE_SIZE, totalItems),
                                total: totalItems,
                                defaultValue: `Hiển thị ${(currentPage - 1) * PAGE_SIZE + 1}–${Math.min(currentPage * PAGE_SIZE, totalItems)} / ${totalItems} job`,
                            })}
                        </p>
                        <div className="flex items-center gap-1">
                            <Button
                                variant="outline"
                                size="sm"
                                disabled={currentPage <= 1}
                                onClick={() => setCurrentPage((p) => Math.max(1, p - 1))}
                            >
                                <ChevronLeft className="h-4 w-4" />
                            </Button>

                            {Array.from({ length: totalPages }, (_, i) => i + 1)
                                .filter((p) => {
                                    // Show first, last, and pages near current
                                    if (p === 1 || p === totalPages) return true;
                                    return Math.abs(p - currentPage) <= 1;
                                })
                                .reduce<(number | 'ellipsis')[]>((acc, p, idx, arr) => {
                                    if (idx > 0 && p - (arr[idx - 1] as number) > 1) {
                                        acc.push('ellipsis');
                                    }
                                    acc.push(p);
                                    return acc;
                                }, [])
                                .map((item, idx) =>
                                    item === 'ellipsis' ? (
                                        <span key={`e-${idx}`} className="px-2 text-sm text-gray-400">…</span>
                                    ) : (
                                        <Button
                                            key={item}
                                            variant={item === currentPage ? 'default' : 'outline'}
                                            size="sm"
                                            onClick={() => setCurrentPage(item)}
                                            className="min-w-[36px]"
                                        >
                                            {item}
                                        </Button>
                                    )
                                )}

                            <Button
                                variant="outline"
                                size="sm"
                                disabled={currentPage >= totalPages}
                                onClick={() => setCurrentPage((p) => Math.min(totalPages, p + 1))}
                            >
                                <ChevronRight className="h-4 w-4" />
                            </Button>
                        </div>
                    </div>
                )}
            </Card>
        </div>
    );
}
