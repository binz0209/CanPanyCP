import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../../components/ui/Card';
import { Button } from '../../components/ui/Button';
import { ChevronRight, ArrowRight, Briefcase, Eye, FileText, TrendingUp, Clock, CheckCircle, XCircle, Loader2 } from 'lucide-react';
import { useEffect, useState } from 'react';
import { candidateApi } from '../../api/candidate.api';
import type { CandidateStatistics } from '../../api/candidate.api';
import { useAuthStore } from '../../stores/auth.store';

export function CandidateDashboardPage() {
  const { user } = useAuthStore();
  const [statistics, setStatistics] = useState<CandidateStatistics | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Mock data for stats not available in API
  const savedJobs = 28;
  const profileViews = 156;

  useEffect(() => {
    const fetchStatistics = async () => {
      if (!user?.id) {
        setLoading(false);
        return;
      }

      try {
        setLoading(true);
        const stats = await candidateApi.getCandidateStatistics(user.id);
        setStatistics(stats);
        setError(null);
      } catch (err) {
        console.error('Failed to fetch candidate statistics:', err);
        setError('Không thể tải dữ liệu thống kê');
        // Set default values on error
        setStatistics({
          totalApplications: 0,
          pendingApplications: 0,
          acceptedApplications: 0,
          rejectedApplications: 0,
          totalCVs: 0,
          profileCompleteness: 0,
          skillsCount: 0,
        });
      } finally {
        setLoading(false);
      }
    };

    fetchStatistics();
  }, [user?.id]);

  // Animation keyframes
  const cardAnimation = {
    animation: 'fadeSlideUp 0.5s ease-out forwards',
    opacity: 0,
  };

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
        <h1 className="text-3xl font-bold text-gray-900 mb-2">
          Chào mừng trở lại!
        </h1>
        <p className="text-gray-600">
          Đây là những gì đang diễn ra với quá trình tìm việc của bạn hôm nay
        </p>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-8">
        {/* Total Applications - from API */}
        <Card
          className="border border-gray-200 bg-white hover:shadow-lg transition-shadow duration-300"
          style={{ ...cardAnimation, animationDelay: '0.1s' }}
        >
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
                <div className="text-3xl font-bold text-gray-900">
                  {statistics?.totalApplications ?? '-'}
                </div>
                <p className="text-xs text-gray-600 mt-1">
                  {statistics?.pendingApplications ?? 0} đang chờ · {statistics?.acceptedApplications ?? 0} đã nhận
                </p>
              </>
            )}
          </CardContent>
        </Card>

        {/* Saved Jobs - keep existing */}
        <Card
          className="border border-gray-200 bg-white hover:shadow-lg transition-shadow duration-300"
          style={{ ...cardAnimation, animationDelay: '0.2s' }}
        >
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-gray-600 flex items-center gap-2">
              <Eye className="h-4 w-4 text-[#00b14f]" />
              Việc làm đã lưu
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-3xl font-bold text-gray-900">{savedJobs}</div>
            <p className="text-xs text-gray-600 mt-1">
              +5 tuần này
            </p>
          </CardContent>
        </Card>

        {/* Profile Views - keep existing */}
        <Card
          className="border border-gray-200 bg-white hover:shadow-lg transition-shadow duration-300"
          style={{ ...cardAnimation, animationDelay: '0.3s' }}
        >
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-gray-600 flex items-center gap-2">
              <TrendingUp className="h-4 w-4 text-[#00b14f]" />
              Lượt xem hồ sơ
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-3xl font-bold text-gray-900">{profileViews}</div>
            <p className="text-xs text-gray-600 mt-1">
              +23 tuần này
            </p>
          </CardContent>
        </Card>

        {/* Total CVs - from API */}
        <Card
          className="border border-gray-200 bg-white hover:shadow-lg transition-shadow duration-300"
          style={{ ...cardAnimation, animationDelay: '0.4s' }}
        >
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
            ) : statistics?.totalCVs !== undefined ? (
              <>
                <div className="text-3xl font-bold text-gray-900">
                  {statistics?.totalCVs}
                </div>
                <p className="text-xs text-gray-600 mt-1">
                  {statistics?.defaultCV?.fileName ? `Mặc định: ${statistics.defaultCV?.fileName}` : 'Chưa có CV mặc định'}
                </p>
              </>
            ) : (
              <div className="text-3xl font-bold text-gray-900">-</div>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Application Status Cards - New Section from API */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-8">
        {/* Pending Applications */}
        <Card
          className="border border-gray-200 bg-white hover:shadow-lg transition-shadow duration-300"
          style={{ ...cardAnimation, animationDelay: '0.5s' }}
        >
          <CardContent className="pt-6">
            <div className="flex items-center gap-4">
              <div className="p-3 bg-blue-100 rounded-full">
                <Clock className="h-6 w-6 text-blue-600" />
              </div>
              <div>
                <p className="text-sm text-gray-600">Đang chờ</p>
                <p className="text-2xl font-bold text-gray-900">
                  {loading ? '-' : statistics?.pendingApplications ?? 0}
                </p>
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Accepted Applications */}
        <Card
          className="border border-gray-200 bg-white hover:shadow-lg transition-shadow duration-300"
          style={{ ...cardAnimation, animationDelay: '0.6s' }}
        >
          <CardContent className="pt-6">
            <div className="flex items-center gap-4">
              <div className="p-3 bg-green-100 rounded-full">
                <CheckCircle className="h-6 w-6 text-green-600" />
              </div>
              <div>
                <p className="text-sm text-gray-600">Đã nhận</p>
                <p className="text-2xl font-bold text-gray-900">
                  {loading ? '-' : statistics?.acceptedApplications ?? 0}
                </p>
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Rejected Applications */}
        <Card
          className="border border-gray-200 bg-white hover:shadow-lg transition-shadow duration-300"
          style={{ ...cardAnimation, animationDelay: '0.7s' }}
        >
          <CardContent className="pt-6">
            <div className="flex items-center gap-4">
              <div className="p-3 bg-red-100 rounded-full">
                <XCircle className="h-6 w-6 text-red-600" />
              </div>
              <div>
                <p className="text-sm text-gray-600">Đã từ chối</p>
                <p className="text-2xl font-bold text-gray-900">
                  {loading ? '-' : statistics?.rejectedApplications ?? 0}
                </p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Profile Completeness - New Section from API */}
      <Card
        className="mb-8 border border-gray-200 bg-white hover:shadow-lg transition-shadow duration-300"
        style={{ ...cardAnimation, animationDelay: '0.8s' }}
      >
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
                <span className="text-sm text-gray-500">
                  {statistics?.skillsCount ?? 0} kỹ năng
                </span>
              </div>
              <div className="w-full h-4 bg-gray-200 rounded-full overflow-hidden">
                <div
                  className="h-full bg-gradient-to-r from-[#00b14f] to-[#00d463] rounded-full transition-all duration-1000 ease-out"
                  style={{ width: `${statistics?.profileCompleteness ?? 0}%` }}
                />
              </div>
              {(statistics?.profileCompleteness ?? 0) < 100 && (
                <p className="text-xs text-gray-500">
                  Hoàn thiện thêm thông tin để đạt 100%
                </p>
              )}
            </div>
          )}
        </CardContent>
      </Card>

      {/* Main Content Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Recent Applications */}
        <Card
          className="lg:col-span-2 border border-gray-200 bg-white hover:shadow-lg transition-shadow duration-300"
          style={{ ...cardAnimation, animationDelay: '0.9s' }}
        >
          <CardHeader>
            <CardTitle>Ứng tuyển gần đây</CardTitle>
            <CardDescription>
              Các ứng tuyển việc làm gần đây của bạn
            </CardDescription>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {[
                { title: 'Senior React Developer', company: 'TechCorp Inc', status: 'Đang xem xét', date: '2 ngày trước' },
                { title: 'Full Stack Engineer', company: 'StartupXYZ', status: 'Đã ứng tuyển', date: '1 tuần trước' },
                { title: 'Frontend Lead', company: 'CloudSystems', status: 'Từ chối', date: '2 tuần trước' },
              ].map((app, i) => (
                <div
                  key={i}
                  className="flex items-center justify-between p-4 rounded-lg border border-gray-200 hover:bg-gray-50 hover:shadow-md transition-all duration-300"
                >
                  <div className="flex-1">
                    <p className="font-medium text-gray-900 text-sm">
                      {app.title}
                    </p>
                    <p className="text-xs text-gray-600">
                      {app.company} · {app.date}
                    </p>
                  </div>
                  <span
                    className={`text-xs font-semibold px-2 py-1 rounded-full ${
                      app.status === 'Đang xem xét'
                        ? 'bg-blue-100 text-blue-700'
                        : app.status === 'Đã ứng tuyển'
                          ? 'bg-green-100 text-green-700'
                          : 'bg-red-100 text-red-700'
                    }`}
                  >
                    {app.status}
                  </span>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>

        {/* Quick Actions */}
        <Card
          className="border border-gray-200 bg-white hover:shadow-lg transition-shadow duration-300"
          style={{ ...cardAnimation, animationDelay: '1.0s' }}
        >
          <CardHeader>
            <CardTitle>Hành động nhanh</CardTitle>
            <CardDescription>
              Bắt đầu ngay lập tức
            </CardDescription>
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

      {/* Recommended Jobs Section */}
      <Card
        className="mt-6 border border-gray-200 bg-white hover:shadow-lg transition-shadow duration-300"
        style={{ ...cardAnimation, animationDelay: '1.1s' }}
      >
        <CardHeader>
          <CardTitle>Việc làm được đề xuất cho bạn</CardTitle>
          <CardDescription>
            Dựa trên hồ sơ và sở thích của bạn
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {[
              { title: 'Senior React Developer', company: 'MetaTech', location: 'San Francisco, CA', salary: '$150K - $200K' },
              { title: 'Full Stack Engineer', company: 'WebScale', location: 'New York, NY', salary: '$140K - $180K' },
              { title: 'Lead Frontend Engineer', company: 'CloudFirst', location: 'Austin, TX', salary: '$160K - $210K' },
            ].map((job, i) => (
              <div
                key={i}
                className="flex flex-col gap-3 p-4 rounded-lg border border-gray-200 hover:border-[#00b14f] hover:bg-gray-50 hover:shadow-lg transition-all duration-300 group cursor-pointer"
              >
                <div>
                  <p className="font-semibold text-gray-900 text-sm group-hover:text-[#00b14f] transition-colors">
                    {job.title}
                  </p>
                  <p className="text-xs text-gray-600">
                    {job.company}
                  </p>
                </div>
                <div className="space-y-1 text-xs text-gray-600">
                  <p>📍 {job.location}</p>
                  <p>💰 {job.salary}</p>
                </div>
                <Button
                  size="sm"
                  variant="outline"
                  className="w-full text-xs border-gray-300 hover:border-[#00b14f] bg-transparent"
                >
                  Xem chi tiết
                </Button>
              </div>
            ))}
          </div>
        </CardContent>
      </Card>

      {/* Skill Gap Analysis Widget */}
      <Card
        className="mt-6 border border-gray-200 bg-white hover:shadow-lg transition-shadow duration-300"
        style={{ ...cardAnimation, animationDelay: '1.2s' }}
      >
        <CardHeader>
          <CardTitle>Phân tích khoảng trống kỹ năng</CardTitle>
          <CardDescription>
            Các kỹ năng bạn cần cải thiện cho vị trí mục tiêu
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            {[
              { skill: 'TypeScript', level: 75, gap: 25 },
              { skill: 'System Design', level: 60, gap: 40 },
              { skill: 'AWS', level: 45, gap: 55 },
            ].map((item, i) => (
              <div key={i}>
                <div className="flex items-center justify-between mb-2">
                  <p className="text-sm font-medium text-gray-900">
                    {item.skill}
                  </p>
                  <p className="text-xs text-gray-600">
                    {item.level}% proficiency
                  </p>
                </div>
                <div className="w-full h-2 bg-gray-200 rounded-full overflow-hidden">
                  <div
                    className="h-full bg-[#00b14f] rounded-full transition-all duration-1000 ease-out"
                    style={{ width: `${item.level}%` }}
                  />
                </div>
              </div>
            ))}
          </div>
        </CardContent>
      </Card>
    </div>
  );
}