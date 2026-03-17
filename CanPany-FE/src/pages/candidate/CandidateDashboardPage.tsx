import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../../components/ui/Card';
import { Button } from '../../components/ui/Button';
import { ChevronRight, ArrowRight, Briefcase, Bookmark, FileText, Clock, CheckCircle, XCircle, Loader2, Star } from 'lucide-react';
import { useEffect, useState } from 'react';
import { candidateApi } from '../../api/candidate.api';
import type { CandidateStatistics } from '../../api/candidate.api';
import { applicationsApi } from '../../api/applications.api';
import { jobsApi } from '../../api/jobs.api';
import { useAuthStore } from '../../stores/auth.store';
import type { Application } from '../../types';
import type { RecommendedJob, Job } from '../../types/job.types';

type ApplicationStatus = 'Pending' | 'Shortlisted' | 'Accepted' | 'Rejected' | 'Withdrawn';

const STATUS_CONFIG: Record<ApplicationStatus, { label: string; className: string }> = {
  Pending:     { label: 'Đang chờ',      className: 'bg-blue-100 text-blue-700' },
  Shortlisted: { label: 'Được xem xét',  className: 'bg-yellow-100 text-yellow-700' },
  Accepted:    { label: 'Đã nhận',       className: 'bg-green-100 text-green-700' },
  Rejected:    { label: 'Từ chối',       className: 'bg-red-100 text-red-700' },
  Withdrawn:   { label: 'Đã rút',        className: 'bg-gray-100 text-gray-700' },
};

function formatRelativeDate(date: string | Date): string {
  const diffDays = Math.floor((Date.now() - new Date(date).getTime()) / 86_400_000);
  if (diffDays === 0) return 'Hôm nay';
  if (diffDays === 1) return '1 ngày trước';
  if (diffDays < 7) return `${diffDays} ngày trước`;
  if (diffDays < 14) return '1 tuần trước';
  if (diffDays < 30) return `${Math.floor(diffDays / 7)} tuần trước`;
  return `${Math.floor(diffDays / 30)} tháng trước`;
}

function formatBudget(job: Job): string {
  if (!job.budgetAmount) return 'Thương lượng';
  const amount = job.budgetAmount.toLocaleString('en-US');
  return job.budgetType === 'Hourly' ? `$${amount}/giờ` : `$${amount}`;
}

export function CandidateDashboardPage() {
  const { user } = useAuthStore();
  const [statistics, setStatistics] = useState<CandidateStatistics | null>(null);
  const [recentApplications, setRecentApplications] = useState<Application[]>([]);
  const [recommendedJobs, setRecommendedJobs] = useState<RecommendedJob[]>([]);
  const [bookmarkCount, setBookmarkCount] = useState(0);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchData = async () => {
      if (!user?.id) {
        setLoading(false);
        return;
      }

      setLoading(true);
      const [statsResult, applicationsResult, recommendedResult, bookmarkedResult] = await Promise.allSettled([
        candidateApi.getCandidateStatistics(user.id),
        applicationsApi.getMyApplications(),
        jobsApi.getRecommended(3),
        jobsApi.getBookmarked(),
      ]);

      if (statsResult.status === 'fulfilled') {
        setStatistics(statsResult.value);
      } else {
        console.error('Failed to fetch statistics:', statsResult.reason);
        setStatistics({
          totalApplications: 0, pendingApplications: 0,
          acceptedApplications: 0, rejectedApplications: 0,
          totalCVs: 0, profileCompleteness: 0, skillsCount: 0,
        });
      }

      if (applicationsResult.status === 'fulfilled') {
        const sorted = [...applicationsResult.value].sort(
          (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
        );
        setRecentApplications(sorted.slice(0, 3));
      } else {
        console.error('Failed to fetch applications:', applicationsResult.reason);
      }

      if (recommendedResult.status === 'fulfilled') {
        setRecommendedJobs(recommendedResult.value);
      } else {
        console.error('Failed to fetch recommended jobs:', recommendedResult.reason);
      }

      if (bookmarkedResult.status === 'fulfilled') {
        setBookmarkCount(bookmarkedResult.value.length);
      } else {
        console.error('Failed to fetch bookmarks:', bookmarkedResult.reason);
      }

      setLoading(false);
    };

    fetchData();
  }, [user?.id]);

  const cardAnimation = { animation: 'fadeSlideUp 0.5s ease-out forwards', opacity: 0 };

  return (
    <div className="min-h-screen">
      {/* Breadcrumb */}
      <div className="mb-8 flex items-center gap-2 text-sm text-gray-600">
        <span>Dashboard</span>
        <ChevronRight className="h-4 w-4" />
        <span className="text-gray-900 font-medium">Tổng quan</span>
      </div>

      {/* Page Header */}
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">Chào mừng trở lại!</h1>
        <p className="text-gray-600">Đây là những gì đang diễn ra với quá trình tìm việc của bạn hôm nay</p>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-8">
        {/* Total Applications */}
        <Card className="border border-gray-200 bg-white hover:shadow-lg transition-shadow duration-300" style={{ ...cardAnimation, animationDelay: '0.1s' }}>
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-gray-600 flex items-center gap-2">
              <Briefcase className="h-4 w-4 text-[#00b14f]" />
              Tổng ứng tuyển
            </CardTitle>
          </CardHeader>
          <CardContent>
            {loading ? (
              <div className="flex items-center justify-center h-12">
                <Loader2 className="h-6 w-6 animate-spin text-[#00b14f]" />
              </div>
            ) : (
              <>
                <div className="text-3xl font-bold text-gray-900">{statistics?.totalApplications ?? '-'}</div>
                <p className="text-xs text-gray-600 mt-1">
                  {statistics?.pendingApplications ?? 0} đang chờ · {statistics?.acceptedApplications ?? 0} đã nhận
                </p>
              </>
            )}
          </CardContent>
        </Card>

        {/* Bookmarked Jobs */}
        <Card className="border border-gray-200 bg-white hover:shadow-lg transition-shadow duration-300" style={{ ...cardAnimation, animationDelay: '0.2s' }}>
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-gray-600 flex items-center gap-2">
              <Bookmark className="h-4 w-4 text-[#00b14f]" />
              Việc đã lưu
            </CardTitle>
          </CardHeader>
          <CardContent>
            {loading ? (
              <div className="flex items-center justify-center h-12">
                <Loader2 className="h-6 w-6 animate-spin text-[#00b14f]" />
              </div>
            ) : (
              <>
                <div className="text-3xl font-bold text-gray-900">{bookmarkCount}</div>
                <p className="text-xs text-gray-600 mt-1">Việc làm đang theo dõi</p>
              </>
            )}
          </CardContent>
        </Card>

        {/* Skills Count */}
        <Card className="border border-gray-200 bg-white hover:shadow-lg transition-shadow duration-300" style={{ ...cardAnimation, animationDelay: '0.3s' }}>
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-gray-600 flex items-center gap-2">
              <Star className="h-4 w-4 text-[#00b14f]" />
              Kỹ năng
            </CardTitle>
          </CardHeader>
          <CardContent>
            {loading ? (
              <div className="flex items-center justify-center h-12">
                <Loader2 className="h-6 w-6 animate-spin text-[#00b14f]" />
              </div>
            ) : (
              <>
                <div className="text-3xl font-bold text-gray-900">{statistics?.skillsCount ?? 0}</div>
                <p className="text-xs text-gray-600 mt-1">Kỹ năng trong hồ sơ</p>
              </>
            )}
          </CardContent>
        </Card>

        {/* Total CVs */}
        <Card className="border border-gray-200 bg-white hover:shadow-lg transition-shadow duration-300" style={{ ...cardAnimation, animationDelay: '0.4s' }}>
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-gray-600 flex items-center gap-2">
              <FileText className="h-4 w-4 text-[#00b14f]" />
              CV đã tạo
            </CardTitle>
          </CardHeader>
          <CardContent>
            {loading ? (
              <div className="flex items-center justify-center h-12">
                <Loader2 className="h-6 w-6 animate-spin text-[#00b14f]" />
              </div>
            ) : (
              <>
                <div className="text-3xl font-bold text-gray-900">{statistics?.totalCVs ?? '-'}</div>
                <p className="text-xs text-gray-600 mt-1">
                  {statistics?.defaultCV?.fileName ? `Mặc định: ${statistics.defaultCV.fileName}` : 'Chưa có CV mặc định'}
                </p>
              </>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Application Status Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-8">
        <Card className="border border-gray-200 bg-white hover:shadow-lg transition-shadow duration-300" style={{ ...cardAnimation, animationDelay: '0.5s' }}>
          <CardContent className="pt-6">
            <div className="flex items-center gap-4">
              <div className="p-3 bg-blue-100 rounded-full">
                <Clock className="h-6 w-6 text-blue-600" />
              </div>
              <div>
                <p className="text-sm text-gray-600">Đang chờ</p>
                <p className="text-2xl font-bold text-gray-900">{loading ? '-' : statistics?.pendingApplications ?? 0}</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card className="border border-gray-200 bg-white hover:shadow-lg transition-shadow duration-300" style={{ ...cardAnimation, animationDelay: '0.6s' }}>
          <CardContent className="pt-6">
            <div className="flex items-center gap-4">
              <div className="p-3 bg-green-100 rounded-full">
                <CheckCircle className="h-6 w-6 text-green-600" />
              </div>
              <div>
                <p className="text-sm text-gray-600">Đã nhận</p>
                <p className="text-2xl font-bold text-gray-900">{loading ? '-' : statistics?.acceptedApplications ?? 0}</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card className="border border-gray-200 bg-white hover:shadow-lg transition-shadow duration-300" style={{ ...cardAnimation, animationDelay: '0.7s' }}>
          <CardContent className="pt-6">
            <div className="flex items-center gap-4">
              <div className="p-3 bg-red-100 rounded-full">
                <XCircle className="h-6 w-6 text-red-600" />
              </div>
              <div>
                <p className="text-sm text-gray-600">Đã từ chối</p>
                <p className="text-2xl font-bold text-gray-900">{loading ? '-' : statistics?.rejectedApplications ?? 0}</p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Profile Completeness */}
      <Card className="mb-8 border border-gray-200 bg-white hover:shadow-lg transition-shadow duration-300" style={{ ...cardAnimation, animationDelay: '0.8s' }}>
        <CardHeader>
          <CardTitle className="text-lg">Hoàn thiện hồ sơ</CardTitle>
          <CardDescription>Cập nhật thông tin để tăng cơ hội được nhà tuyển dụng chú ý</CardDescription>
        </CardHeader>
        <CardContent>
          {loading ? (
            <div className="flex items-center justify-center h-8">
              <Loader2 className="h-6 w-6 animate-spin text-[#00b14f]" />
            </div>
          ) : (
            <div className="space-y-4">
              <div className="flex items-center justify-between">
                <span className="text-sm font-medium text-gray-700">
                  Mức độ hoàn thiện: {statistics?.profileCompleteness ?? 0}%
                </span>
                <span className="text-sm text-gray-500">{statistics?.skillsCount ?? 0} kỹ năng</span>
              </div>
              <div className="w-full h-4 bg-gray-200 rounded-full overflow-hidden">
                <div
                  className="h-full bg-linear-to-r from-[#00b14f] to-[#00d463] rounded-full transition-all duration-1000 ease-out"
                  style={{ width: `${statistics?.profileCompleteness ?? 0}%` }}
                />
              </div>
              {(statistics?.profileCompleteness ?? 0) < 100 && (
                <p className="text-xs text-gray-500">Hoàn thiện thêm thông tin để đạt 100%</p>
              )}
            </div>
          )}
        </CardContent>
      </Card>

      {/* Main Content Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Recent Applications */}
        <Card className="lg:col-span-2 border border-gray-200 bg-white hover:shadow-lg transition-shadow duration-300" style={{ ...cardAnimation, animationDelay: '0.9s' }}>
          <CardHeader>
            <CardTitle>Ứng tuyển gần đây</CardTitle>
            <CardDescription>Các ứng tuyển việc làm gần đây của bạn</CardDescription>
          </CardHeader>
          <CardContent>
            {loading ? (
              <div className="flex items-center justify-center h-24">
                <Loader2 className="h-6 w-6 animate-spin text-[#00b14f]" />
              </div>
            ) : recentApplications.length === 0 ? (
              <p className="text-sm text-gray-500 text-center py-6">Bạn chưa có ứng tuyển nào.</p>
            ) : (
              <div className="space-y-4">
                {recentApplications.map((app) => {
                  const statusCfg = STATUS_CONFIG[app.status as ApplicationStatus] ?? { label: app.status, className: 'bg-gray-100 text-gray-700' };
                  return (
                    <div key={app.id} className="flex items-center justify-between p-4 rounded-lg border border-gray-200 hover:bg-gray-50 hover:shadow-md transition-all duration-300">
                      <div className="flex-1 min-w-0">
                        <p className="font-medium text-gray-900 text-sm truncate">
                          {app.job?.title ?? 'Vị trí không xác định'}
                        </p>
                        <p className="text-xs text-gray-600">
                          {app.job?.company?.name ?? 'Công ty không xác định'} · {formatRelativeDate(app.createdAt)}
                        </p>
                      </div>
                      <span className={`ml-3 shrink-0 text-xs font-semibold px-2 py-1 rounded-full ${statusCfg.className}`}>
                        {statusCfg.label}
                      </span>
                    </div>
                  );
                })}
              </div>
            )}
          </CardContent>
        </Card>

        {/* Quick Actions */}
        <Card className="border border-gray-200 bg-white hover:shadow-lg transition-shadow duration-300" style={{ ...cardAnimation, animationDelay: '1.0s' }}>
          <CardHeader>
            <CardTitle>Hành động nhanh</CardTitle>
            <CardDescription>Bắt đầu ngay lập tức</CardDescription>
          </CardHeader>
          <CardContent className="space-y-3">
            <Button className="w-full justify-between bg-[#00b14f] hover:bg-[#00a045] text-white">
              <span>Tìm việc làm</span>
              <ArrowRight className="h-4 w-4" />
            </Button>
            <Button variant="outline" className="w-full justify-between border-gray-300 bg-transparent hover:bg-[#00b14f] hover:text-white">
              <span>Tải lên CV</span>
              <ArrowRight className="h-4 w-4" />
            </Button>
            <Button variant="outline" className="w-full justify-between border-gray-300 bg-transparent hover:bg-[#00b14f] hover:text-white">
              <span>Hỏi AI Advisor</span>
              <ArrowRight className="h-4 w-4" />
            </Button>
            <Button variant="outline" className="w-full justify-between border-gray-300 bg-transparent hover:bg-[#00b14f] hover:text-white">
              <span>Xem đề xuất</span>
              <ArrowRight className="h-4 w-4" />
            </Button>
          </CardContent>
        </Card>
      </div>

      {/* Recommended Jobs */}
      <Card className="mt-6 border border-gray-200 bg-white hover:shadow-lg transition-shadow duration-300" style={{ ...cardAnimation, animationDelay: '1.1s' }}>
        <CardHeader>
          <CardTitle>Việc làm được đề xuất cho bạn</CardTitle>
          <CardDescription>Dựa trên hồ sơ và sở thích của bạn</CardDescription>
        </CardHeader>
        <CardContent>
          {loading ? (
            <div className="flex items-center justify-center h-24">
              <Loader2 className="h-6 w-6 animate-spin text-[#00b14f]" />
            </div>
          ) : recommendedJobs.length === 0 ? (
            <p className="text-sm text-gray-500 text-center py-6">
              Chưa có đề xuất. Hãy cập nhật hồ sơ để nhận gợi ý phù hợp.
            </p>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
              {recommendedJobs.map(({ job }) => (
                <div
                  key={job.id}
                  className="flex flex-col gap-3 p-4 rounded-lg border border-gray-200 hover:border-[#00b14f] hover:bg-gray-50 hover:shadow-lg transition-all duration-300 group cursor-pointer"
                >
                  <div>
                    <p className="font-semibold text-gray-900 text-sm group-hover:text-[#00b14f] transition-colors truncate">
                      {job.title}
                    </p>
                    <p className="text-xs text-gray-600 truncate">{job.company?.name ?? 'Công ty không xác định'}</p>
                  </div>
                  <div className="space-y-1 text-xs text-gray-600">
                    {(job.location || job.isRemote) && (
                      <p>📍 {job.isRemote ? 'Remote' : job.location}</p>
                    )}
                    <p>💰 {formatBudget(job)}</p>
                  </div>
                  <Button size="sm" variant="outline" className="w-full text-xs border-gray-300 hover:border-[#00b14f] bg-transparent">
                    Xem chi tiết
                  </Button>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
