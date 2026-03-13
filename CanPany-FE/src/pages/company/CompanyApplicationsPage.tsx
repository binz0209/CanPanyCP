import { useEffect, useMemo, useState } from 'react';
import { isAxiosError } from 'axios';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { BriefcaseBusiness, FileSearch } from 'lucide-react';
import toast from 'react-hot-toast';
import { Button, Card } from '../../components/ui';
import { applicationsApi, candidateApi, jobsApi } from '../../api';
import type { Application, ApplicationStatus } from '../../types';
import {
    ApplicationReviewCard,
    CompanyProfileRequiredState,
    CompanyWorkspaceErrorState,
    CompanyWorkspaceLoader,
    EmptyState,
    SectionHeader,
} from '../../components/features/companies';
import { useCandidateProfilesMap } from '../../hooks/company/useCandidateProfilesMap';
import { useCompanyWorkspace } from '../../hooks/company/useCompanyWorkspace';
import { applicationKeys, candidateKeys, companyKeys } from '../../lib/queryKeys';

type StatusFilter = 'All' | ApplicationStatus;

export function CompanyApplicationsPage() {
    const queryClient = useQueryClient();
    const [selectedJobId, setSelectedJobId] = useState('');
    const [statusFilter, setStatusFilter] = useState<StatusFilter>('All');
    const [processingApplicationId, setProcessingApplicationId] = useState<string | null>(null);

    const { companyId, isLoading: isWorkspaceLoading, isMissingProfile, hasFatalError } = useCompanyWorkspace();

    const jobsQuery = useQuery({
        queryKey: companyKeys.workspaceJobs(companyId!),
        queryFn: () => jobsApi.getByCompany(companyId!),
        enabled: !!companyId,
    });

    useEffect(() => {
        if (!selectedJobId && jobsQuery.data?.length) {
            setSelectedJobId(jobsQuery.data[0].id);
        }
    }, [jobsQuery.data, selectedJobId]);

    const applicationsQuery = useQuery({
        queryKey: applicationKeys.byJob(selectedJobId),
        queryFn: () => applicationsApi.getJobApplications(selectedJobId),
        enabled: !!selectedJobId,
    });

    const applications = applicationsQuery.data || [];
    const { candidateProfilesMap } = useCandidateProfilesMap(applications.map((application) => application.candidateId));

    const acceptMutation = useMutation({
        mutationFn: async (applicationId: string) => {
            await applicationsApi.accept(applicationId);
            return applicationId;
        },
        onSuccess: async (applicationId) => {
            queryClient.setQueryData<Application[]>(applicationKeys.byJob(selectedJobId), (currentApplications = []) =>
                currentApplications.map((application) =>
                    application.id === applicationId
                        ? { ...application, status: 'Accepted' }
                        : application
                )
            );
            queryClient.setQueryData<Application>(applicationKeys.detail(applicationId), (currentApplication) =>
                currentApplication ? { ...currentApplication, status: 'Accepted' } : currentApplication
            );
            await queryClient.invalidateQueries({ queryKey: applicationKeys.byJob(selectedJobId), exact: true });
            toast.success('Đã chấp nhận application');
        },
        onSettled: () => {
            setProcessingApplicationId(null);
        },
        onError: (error) => {
            const message = isAxiosError(error)
                ? error.response?.data?.message || 'Không thể chấp nhận application'
                : 'Không thể chấp nhận application';
            toast.error(message);
        },
    });

    const enrichedApplications = useMemo(
        () =>
            applications.map((application) => {
                const candidateProfile = candidateProfilesMap[application.candidateId];
                return {
                    ...application,
                    candidateUser: candidateProfile?.user || null,
                    candidateProfile: candidateProfile?.profile || null,
                };
            }),
        [applications, candidateProfilesMap]
    );

    const filteredApplications = useMemo(() => {
        if (statusFilter === 'All') return enrichedApplications;
        return enrichedApplications.filter((application) => application.status === statusFilter);
    }, [enrichedApplications, statusFilter]);

    const selectedJob = useMemo(
        () => jobsQuery.data?.find((job) => job.id === selectedJobId),
        [jobsQuery.data, selectedJobId]
    );

    const prefetchApplicationContext = (application: Application) => {
        void queryClient.prefetchQuery({
            queryKey: applicationKeys.detail(application.id),
            queryFn: () => applicationsApi.getDetails(application.id),
        });
        void queryClient.prefetchQuery({
            queryKey: candidateKeys.profile(application.candidateId),
            queryFn: () => candidateApi.getCandidateProfile(application.candidateId),
        });
        void queryClient.prefetchQuery({
            queryKey: companyKeys.workspaceJobDetail(application.jobId),
            queryFn: () => jobsApi.getById(application.jobId),
        });
    };

    if (isWorkspaceLoading || jobsQuery.isLoading) {
        return <CompanyWorkspaceLoader />;
    }

    if (isMissingProfile) {
        return (
            <CompanyProfileRequiredState
                title="Bạn chưa có hồ sơ công ty"
                description="Hãy hoàn thiện hồ sơ công ty trước khi review applications."
                icon={<FileSearch className="h-6 w-6" />}
            />
        );
    }

    if (hasFatalError || jobsQuery.error || applicationsQuery.error) {
        return (
            <CompanyWorkspaceErrorState
                title="Không thể tải danh sách ứng tuyển"
                description="Đã xảy ra lỗi khi tải dữ liệu ứng tuyển. Vui lòng thử lại sau hoặc liên hệ quản trị viên để được hỗ trợ."
                icon={<FileSearch className="h-6 w-6" />}
            />
        );
    }

    return (
        <div className="space-y-6">
            <SectionHeader
                title="Review ứng tuyển"
                description="Chọn một job để xem danh sách hồ sơ ứng tuyển, lọc theo trạng thái và cập nhật quyết định Accepted/Rejected."
            />

            <div className="grid gap-6 xl:grid-cols-[0.9fr_1.1fr]">
                <Card className="p-6">
                    <h2 className="text-lg font-semibold text-gray-900">Bộ lọc review</h2>
                    <div className="mt-5 space-y-5">
                        <div>
                            <label className="mb-2 block text-sm font-medium text-gray-700">Chọn job</label>
                            <select
                                value={selectedJobId}
                                onChange={(event) => setSelectedJobId(event.target.value)}
                                className="h-11 w-full rounded-lg border border-gray-300 bg-white px-4 text-sm text-gray-900 outline-none transition focus:border-[#00b14f] focus:ring-2 focus:ring-[#00b14f]/20"
                            >
                                <option value="">Chọn tin tuyển dụng</option>
                                {(jobsQuery.data || []).map((job) => (
                                    <option key={job.id} value={job.id}>
                                        {job.title}
                                    </option>
                                ))}
                            </select>
                        </div>

                        <div>
                            <label className="mb-2 block text-sm font-medium text-gray-700">Lọc trạng thái</label>
                            <div className="flex flex-wrap gap-2">
                                {(['All', 'Pending', 'Accepted', 'Rejected', 'Withdrawn'] as StatusFilter[]).map((filter) => (
                                    <Button
                                        key={filter}
                                        size="sm"
                                        variant={statusFilter === filter ? 'default' : 'outline'}
                                        onClick={() => setStatusFilter(filter)}
                                    >
                                        {filter === 'All' ? 'Tất cả' : filter}
                                    </Button>
                                ))}
                            </div>
                        </div>

                        <div className="rounded-xl bg-gray-50 p-4 text-sm text-gray-600">
                            {selectedJob
                                ? `Đang review job: ${selectedJob.title}`
                                : 'Chọn một job để tải danh sách applications.'}
                        </div>
                    </div>
                </Card>

                <Card className="p-6">
                    <div className="flex items-center justify-between">
                        <div>
                            <h2 className="text-lg font-semibold text-gray-900">Danh sách applications</h2>
                            <p className="mt-1 text-sm text-gray-500">
                                {selectedJob ? selectedJob.title : 'Chưa chọn job'}
                            </p>
                        </div>
                        <span className="rounded-full bg-gray-100 px-3 py-1 text-xs font-semibold text-gray-700">
                            {filteredApplications.length} hồ sơ
                        </span>
                    </div>

                    {applicationsQuery.isLoading ? (
                        <div className="mt-6 space-y-3">
                            {[1, 2, 3].map((item) => (
                                <div key={item} className="h-28 animate-pulse rounded-xl bg-gray-100" />
                            ))}
                        </div>
                    ) : !selectedJobId ? (
                        <div className="mt-6">
                            <EmptyState
                                title="Chưa chọn job"
                                description="Hãy chọn một tin tuyển dụng ở panel bên trái để xem hồ sơ ứng tuyển."
                                icon={<BriefcaseBusiness className="h-6 w-6" />}
                            />
                        </div>
                    ) : filteredApplications.length === 0 ? (
                        <div className="mt-6">
                            <EmptyState
                                title="Chưa có application phù hợp"
                                description="Chưa có ứng viên nào apply hoặc bộ lọc hiện tại không khớp."
                                icon={<FileSearch className="h-6 w-6" />}
                            />
                        </div>
                    ) : (
                        <div className="mt-6 space-y-4">
                            {filteredApplications.map((application) => (
                                <ApplicationReviewCard
                                    key={application.id}
                                    application={application}
                                    applicationDetailPath={`/company/applications/${application.id}`}
                                    onPrefetch={() => prefetchApplicationContext(application)}
                                    onAccept={
                                        application.status === 'Pending'
                                            ? () => {
                                                setProcessingApplicationId(application.id);
                                                acceptMutation.mutate(application.id);
                                            }
                                            : undefined
                                    }
                                    isAccepting={acceptMutation.isPending && processingApplicationId === application.id}
                                />
                            ))}
                        </div>
                    )}
                </Card>
            </div>
        </div>
    );
}
