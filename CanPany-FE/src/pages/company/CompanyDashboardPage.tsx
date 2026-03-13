import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { ArrowRight, BriefcaseBusiness, Building2, ShieldCheck, Users } from 'lucide-react';
import { Button, Card } from '../../components/ui';
import { companiesApi, jobsApi } from '../../api';
import { formatNumber } from '../../utils';
import type { Job } from '../../types';
import {
    CompanyWorkspaceErrorState,
    CompanyWorkspaceLoader,
    EmptyState,
    SectionHeader,
    StatusBadge,
} from '../../components/features/companies';
import { useCompanyWorkspace } from '../../hooks/company/useCompanyWorkspace';
import { companyKeys } from '../../lib/queryKeys';

export function CompanyDashboardPage() {
    const { company, companyId, isLoading: isWorkspaceLoading, hasFatalError } = useCompanyWorkspace();

    const statisticsQuery = useQuery({
        queryKey: companyKeys.statistics(companyId!),
        queryFn: () => companiesApi.getStatistics(companyId!),
        enabled: !!companyId,
    });

    const jobsQuery = useQuery({
        queryKey: companyKeys.workspaceJobs(companyId!),
        queryFn: () => jobsApi.getByCompany(companyId!),
        enabled: !!companyId,
    });

    if (isWorkspaceLoading) {
        return <CompanyWorkspaceLoader />;
    }

    if (hasFatalError || statisticsQuery.error || jobsQuery.error) {
        return (
            <CompanyWorkspaceErrorState
                title="Không thể tải dashboard công ty"
                description="Đã xảy ra lỗi khi tải dữ liệu. Vui lòng thử tải lại trang hoặc liên hệ quản trị viên nếu vấn đề tiếp diễn."
                icon={<BriefcaseBusiness className="h-6 w-6" />}
            />
        );
    }

    const statistics = statisticsQuery.data;
    const recentJobs = (jobsQuery.data || []).slice(0, 5);

    return (
        <div className="space-y-6">
            <SectionHeader
                tone="hero"
                eyebrow="Không gian tuyển dụng"
                title={company?.name || 'Hoàn thiện hồ sơ công ty để bắt đầu'}
                description="Xem nhanh hồ sơ công ty, hiệu quả tin tuyển dụng và các chỉ số ứng tuyển để điều chỉnh chiến lược tuyển dụng kịp thời."
                actions={
                    <>
                        <Link to="/company/profile">
                            <Button className="bg-white text-[#00b14f] hover:bg-white/90">
                                <Building2 className="h-4 w-4" />
                                Hồ sơ công ty
                            </Button>
                        </Link>
                        <Link to="/company/jobs/new">
                            <Button className="border border-white/30 bg-white/10 text-white hover:bg-white/20">
                                <BriefcaseBusiness className="h-4 w-4" />
                                Tạo tin tuyển dụng
                            </Button>
                        </Link>
                    </>
                }
            />

            <section className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
                <Card className="p-5">
                    <p className="text-sm text-gray-500">Tổng số job</p>
                    <p className="mt-2 text-3xl font-bold text-gray-900">
                        {formatNumber(statistics?.totalJobs || 0)}
                    </p>
                </Card>
                <Card className="p-5">
                    <p className="text-sm text-gray-500">Job đang mở</p>
                    <p className="mt-2 text-3xl font-bold text-gray-900">
                        {formatNumber(statistics?.activeJobs || 0)}
                    </p>
                </Card>
                <Card className="p-5">
                    <p className="text-sm text-gray-500">Tổng ứng tuyển</p>
                    <p className="mt-2 text-3xl font-bold text-gray-900">
                        {formatNumber(statistics?.totalApplications || 0)}
                    </p>
                </Card>
                <Card className="p-5">
                    <p className="text-sm text-gray-500">Lượt xem job</p>
                    <p className="mt-2 text-3xl font-bold text-gray-900">
                        {formatNumber(statistics?.totalViews || 0)}
                    </p>
                </Card>
            </section>

            <section className="grid gap-6 xl:grid-cols-[1.2fr_0.8fr]">
                <Card className="p-6">
                    <div className="flex items-center justify-between">
                        <div>
                            <h2 className="text-lg font-semibold text-gray-900">Tin tuyển dụng gần đây</h2>
                            <p className="mt-1 text-sm text-gray-500">
                                Từ đây bạn có thể đi vào UC-31 và UC-32 nhanh nhất.
                            </p>
                        </div>
                        <Link to="/company/jobs">
                            <Button variant="outline">
                                Xem tất cả
                                <ArrowRight className="h-4 w-4" />
                            </Button>
                        </Link>
                    </div>

                    {jobsQuery.isLoading ? (
                        <div className="mt-6 space-y-3">
                            {[1, 2, 3].map((item) => (
                                <div key={item} className="h-20 animate-pulse rounded-xl bg-gray-100" />
                            ))}
                        </div>
                    ) : recentJobs.length === 0 ? (
                        <div className="mt-6">
                            <EmptyState
                                title="Chưa có tin tuyển dụng"
                                description="Bạn chưa có tin tuyển dụng nào. Hãy tạo job đầu tiên để bắt đầu luồng tuyển dụng."
                                icon={<BriefcaseBusiness className="h-6 w-6" />}
                            />
                        </div>
                    ) : (
                        <div className="mt-6 space-y-3">
                            {recentJobs.map((job: Job) => (
                                <div
                                    key={job.id}
                                    className="rounded-xl border border-gray-100 p-4 transition hover:border-[#00b14f]/30 hover:shadow-sm"
                                >
                                    <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                                        <div>
                                            <p className="font-semibold text-gray-900">{job.title}</p>
                                            <p className="mt-1 text-sm text-gray-500">
                                                {job.location || 'Chưa cập nhật địa điểm'}
                                            </p>
                                        </div>
                                        <div className="flex flex-wrap items-center gap-2">
                                            <StatusBadge status={job.status} kind="job" />
                                            <Link to={`/company/jobs/${job.id}/edit`}>
                                                <Button variant="outline" size="sm">Chỉnh sửa</Button>
                                            </Link>
                                        </div>
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}
                </Card>

                <Card className="p-6">
                    <h2 className="text-lg font-semibold text-gray-900">Trạng thái công ty</h2>
                    <div className="mt-6 space-y-4">
                        <div className="rounded-xl bg-gray-50 p-4">
                            <div className="flex items-center gap-3">
                                <div className="rounded-lg bg-[#00b14f]/10 p-2 text-[#00b14f]">
                                    <ShieldCheck className="h-5 w-5" />
                                </div>
                                <div>
                                    <p className="text-sm text-gray-500">Xác minh công ty</p>
                                    <div className="mt-1">
                                        <StatusBadge
                                            status={statistics?.verificationStatus || company?.verificationStatus || 'Pending'}
                                            kind="verification"
                                        />
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div className="rounded-xl bg-gray-50 p-4">
                            <div className="flex items-center gap-3">
                                <div className="rounded-lg bg-[#00b14f]/10 p-2 text-[#00b14f]">
                                    <Users className="h-5 w-5" />
                                </div>
                                <div>
                                    <p className="text-sm text-gray-500">Tỷ lệ xử lý ứng tuyển</p>
                                    <p className="mt-1 text-sm font-semibold text-gray-900">
                                        Pending: {formatNumber(statistics?.pendingApplications || 0)} / Accepted: {formatNumber(statistics?.acceptedApplications || 0)}
                                    </p>
                                </div>
                            </div>
                        </div>

                        <div className="rounded-xl border border-dashed border-gray-300 p-4 text-sm text-gray-600">
                            Hãy đảm bảo hồ sơ công ty đầy đủ và tin tuyển dụng luôn được cập nhật để tăng tỷ lệ ứng tuyển phù hợp.
                        </div>
                    </div>
                </Card>
            </section>
        </div>
    );
}
