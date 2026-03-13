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

type JobFilter = 'All' | JobStatus;

export function CompanyJobsPage() {
    const queryClient = useQueryClient();
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
                    ? 'Đã đóng tin tuyển dụng'
                    : 'Đã mở lại tin tuyển dụng'
            );
        },
        onSettled: () => {
            setProcessingJobId(null);
        },
        onError: (error) => {
            const message = isAxiosError(error)
                ? error.response?.data?.message || 'Không thể cập nhật trạng thái job'
                : 'Không thể cập nhật trạng thái job';
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
                title="Bạn chưa có hồ sơ công ty"
                description="Hãy hoàn thiện hồ sơ công ty trước khi tạo và quản lý tin tuyển dụng."
                icon={<BriefcaseBusiness className="h-6 w-6" />}
                action={
                    <Link to="/company/profile">
                        <Button>Đi tới hồ sơ công ty</Button>
                    </Link>
                }
            />
        );
    }

    if (hasFatalError || jobsQuery.error) {
        return (
            <CompanyWorkspaceErrorState
                title="Không thể tải danh sách job"
                description="Đã xảy ra lỗi khi tải danh sách tin tuyển dụng. Vui lòng thử lại sau hoặc liên hệ quản trị viên nếu cần thêm hỗ trợ."
                icon={<BriefcaseBusiness className="h-6 w-6" />}
            />
        );
    }

    return (
        <div className="space-y-6">
            <SectionHeader
                title="Quản lý tin tuyển dụng"
                description="Xem, lọc và quản lý danh sách tin tuyển dụng của công ty; đóng/mở lại job tuỳ theo nhu cầu tuyển dụng thực tế."
                actions={
                    <Link to="/company/jobs/new">
                        <Button>
                            <Plus className="h-4 w-4" />
                            Tạo tin tuyển dụng
                        </Button>
                    </Link>
                }
            />

            <section className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
                <Card className="p-5">
                    <p className="text-sm text-gray-500">Tổng số job</p>
                    <p className="mt-2 text-3xl font-bold text-gray-900">{formatNumber(statistics.total)}</p>
                </Card>
                <Card className="p-5">
                    <p className="text-sm text-gray-500">Đang tuyển</p>
                    <p className="mt-2 text-3xl font-bold text-gray-900">{formatNumber(statistics.open)}</p>
                </Card>
                <Card className="p-5">
                    <p className="text-sm text-gray-500">Đã đóng</p>
                    <p className="mt-2 text-3xl font-bold text-gray-900">{formatNumber(statistics.closed)}</p>
                </Card>
                <Card className="p-5">
                    <p className="text-sm text-gray-500">Bản nháp</p>
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
                                {({'All':'Tất cả','Open':'Đang tuyển','Closed':'Đã đóng','Draft':'Bản nháp'} as Record<string,string>)[filter]}
                            </Button>
                        ))}
                    </div>

                    <div className="rounded-lg bg-gray-50 px-3 py-2 text-sm text-gray-500">
                        Bạn có thể tạo mới, chỉnh sửa và đóng/mở lại tin tuyển dụng trực tiếp tại đây.
                    </div>
                </div>

                {filteredJobs.length === 0 ? (
                    <div className="mt-6">
                        <EmptyState
                            title="Chưa có tin tuyển dụng phù hợp"
                            description="Hãy tạo tin tuyển dụng mới hoặc đổi bộ lọc để xem dữ liệu khác."
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
                                            {job.location || 'Chưa cập nhật địa điểm'}
                                        </p>
                                        <div className="flex flex-wrap gap-4 text-sm text-gray-500">
                                            <span>Lượt xem: {formatNumber(job.viewCount)}</span>
                                            <span>Ứng viên: {formatNumber(job.applicationCount)}</span>
                                            <span>Tạo ngày: {formatDate(job.createdAt)}</span>
                                        </div>
                                    </div>

                                    <div className="flex flex-wrap gap-2">
                                        <Link to={`/jobs/${job.id}`}>
                                            <Button variant="outline" size="sm">
                                                <Eye className="h-4 w-4" />
                                                Xem trang công khai
                                            </Button>
                                        </Link>
                                        <Link to={`/company/jobs/${job.id}/edit`}>
                                            <Button variant="outline" size="sm">
                                                <FilePenLine className="h-4 w-4" />
                                                Chỉnh sửa
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
                                                Mở lại
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
                                                Đóng tin
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
