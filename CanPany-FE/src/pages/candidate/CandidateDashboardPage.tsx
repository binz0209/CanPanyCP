import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../../components/ui/Card';
import { Button } from '../../components/ui/Button';
import { ChevronRight, ArrowRight, Briefcase, Eye, FileText, TrendingUp } from 'lucide-react';

export function CandidateDashboardPage() {
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
        <Card className="border border-gray-200 bg-white">
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-gray-600">
              Ứng tuyển đang hoạt động
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-3xl font-bold text-gray-900">12</div>
            <p className="text-xs text-gray-600 mt-1">
              +2 tuần này
            </p>
          </CardContent>
        </Card>

        <Card className="border border-gray-200 bg-white">
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-gray-600">
              Việc làm đã lưu
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-3xl font-bold text-gray-900">28</div>
            <p className="text-xs text-gray-600 mt-1">
              +5 tuần này
            </p>
          </CardContent>
        </Card>

        <Card className="border border-gray-200 bg-white">
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-gray-600">
              Lượt xem hồ sơ
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-3xl font-bold text-gray-900">156</div>
            <p className="text-xs text-gray-600 mt-1">
              +23 tuần này
            </p>
          </CardContent>
        </Card>

        <Card className="border border-gray-200 bg-white">
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-gray-600">
              CV đã tạo
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-3xl font-bold text-gray-900">3</div>
            <p className="text-xs text-gray-600 mt-1">
              Tất cả đã được đồng bộ
            </p>
          </CardContent>
        </Card>
      </div>

      {/* Main Content Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Recent Applications */}
        <Card className="lg:col-span-2 border border-gray-200 bg-white">
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
                  className="flex items-center justify-between p-4 rounded-lg border border-gray-200 hover:bg-gray-50 transition-colors"
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
        <Card className="border border-gray-200 bg-white">
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
      <Card className="mt-6 border border-gray-200 bg-white">
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
                className="flex flex-col gap-3 p-4 rounded-lg border border-gray-200 hover:border-[#00b14f] hover:bg-gray-50 transition-all group cursor-pointer"
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
      <Card className="mt-6 border border-gray-200 bg-white">
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
                    className="h-full bg-[#00b14f] rounded-full transition-all"
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