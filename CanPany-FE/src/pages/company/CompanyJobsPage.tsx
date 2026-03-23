import { useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { isAxiosError } from 'axios';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { BriefcaseBusiness, Eye, FilePenLine, Plus, RefreshCcw } from 'lucide-react';
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

export function CompanyJobsPage() {
    const queryClient = useQueryClient();
    const { t } = useTranslation('company');
    const [activeFilter, setActiveFilter] = useState<JobFilter>('All');
    const [processingJobId, setProcessingJobId] = useState<string | null>(null);
    const { companyId, isLoading: isWorkspaceLoading, isMissingProfile, hasFatalError } = useCompanyWorkspace();

    const jobsQuery = useQuery({
        queryKey: companyKeys.workspaceJobs(companyId!),
        queryFn: () => jobsApi.getByCompany(companyId!),
        enabled: !!companyId,
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
            queryClient.setQueryData<Job[]>(companyKeys.workspaceJobs(companyId!), (currentJobs = []) =>
                currentJobs.map((job) =>
                    job.id === variables.jobId
                        ? { ...job, status: variables.nextStatus }
                        : job
                )
            );
            await queryClient.invalidateQueries({ queryKey: companyKeys.workspaceJobs(companyId!), exact: true });
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

    const jobs = useMemo(() => jobsQuery.data || [], [jobsQuery.data]);
    const filteredJobs = useMemo(() => {
        if (activeFilter === 'All') return jobs;
        return jobs.filter((job: Job) => job.status === activeFilter);
    }, [activeFilter, jobs]);

    const statistics = useMemo(() => {
        return {
            total: jobs.length,
            open: jobs.filter((job: Job) => job.status === 'Open').length,
            closed: jobs.filter((job: Job) => job.status === 'Closed').length,
            draft: jobs.filter((job: Job) => job.status === 'Draft').length,
        };
    }, [jobs]);

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
                                onClick={() => setActiveFilter(filter)}
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
            </Card>
        </div>
    );
}
