import { Link } from 'react-router-dom';
import { Search, Building2, Users, Zap, Shield, TrendingUp, ArrowRight, MapPin } from 'lucide-react';
import { Button, Carousel } from '../../components/ui';

const features = [
    {
        icon: Zap,
        title: 'AI Matching thông minh',
        description: 'Công nghệ AI phân tích CV và đề xuất công việc phù hợp nhất với bạn',
    },
    {
        icon: Shield,
        title: 'Bảo mật tuyệt đối',
        description: 'Thông tin được mã hóa AES-256, đảm bảo an toàn cho mọi dữ liệu',
    },
    {
        icon: TrendingUp,
        title: 'Tối ưu hóa CV',
        description: 'Phân tích ATS score và đề xuất cải thiện CV để tăng cơ hội',
    },
];

const stats = [
    { value: '50,000+', label: 'Việc làm đang tuyển' },
    { value: '10,000+', label: 'Doanh nghiệp tin dùng' },
    { value: '100,000+', label: 'Ứng viên đăng ký' },
    { value: '95%', label: 'Hài lòng dịch vụ' },
];

const categories = [
    { name: 'Công nghệ thông tin', count: 5420, icon: '💻', color: 'bg-blue-50 text-blue-600' },
    { name: 'Marketing', count: 2350, icon: '📈', color: 'bg-purple-50 text-purple-600' },
    { name: 'Tài chính - Ngân hàng', count: 1890, icon: '💰', color: 'bg-amber-50 text-amber-600' },
    { name: 'Thiết kế - Đồ họa', count: 1560, icon: '🎨', color: 'bg-pink-50 text-pink-600' },
    { name: 'Kinh doanh - Bán hàng', count: 3200, icon: '💼', color: 'bg-green-50 text-green-600' },
    { name: 'Nhân sự - Hành chính', count: 980, icon: '👥', color: 'bg-cyan-50 text-cyan-600' },
];

const topCompanies = [
    { name: 'FPT Software', logo: '🏢' },
    { name: 'VNG Corporation', logo: '🎮' },
    { name: 'Viettel', logo: '📱' },
    { name: 'Vingroup', logo: '🏗️' },
    { name: 'VNPAY', logo: '💳' },
    { name: 'Momo', logo: '💜' },
];

const jobBanners = [
    {
        id: 1,
        title: 'Senior Frontend Developer',
        company: 'FPT Software',
        location: 'Hà Nội',
        salary: '20-30 triệu',
        image: 'https://images.unsplash.com/photo-1559136555-9303baea8ebd?w=800&h=400&fit=crop',
    },
    {
        id: 2,
        title: 'UI/UX Designer',
        company: 'VNG Corporation',
        location: 'TP. Hồ Chí Minh',
        salary: '15-25 triệu',
        image: 'https://images.unsplash.com/photo-1581291518857-4e27b48ff24e?w=800&h=400&fit=crop',
    },
    {
        id: 3,
        title: 'Data Analyst',
        company: 'Viettel',
        location: 'Đà Nẵng',
        salary: '18-28 triệu',
        image: 'https://images.unsplash.com/photo-1551288049-bebda4e38f71?w=800&h=400&fit=crop',
    },
];

export function HomePage() {

    return (
        <div>
            {/* Hero Section - TopCV Style */}
            <section className="relative overflow-hidden bg-gradient-to-br from-[#00b14f] via-[#00a045] to-[#008f3c]">
                {/* Background pattern */}
                <div className="absolute inset-0 opacity-10">
                    <div className="absolute inset-0" style={{ backgroundImage: 'url("data:image/svg+xml,%3Csvg width=\'60\' height=\'60\' viewBox=\'0 0 60 60\' xmlns=\'http://www.w3.org/2000/svg\'%3E%3Cpath d=\'M0,30 Q15,10 30,30 T60,30 V60 H0 Z\' fill=\'%23ffffff\' fill-opacity=\'0.4\'/%3E%3C/svg%3E")' }} />
                </div>

                <div className="relative mx-auto max-w-7xl px-4 py-16 sm:px-6 lg:px-8 lg:py-24">
                    {/* Job Banners Carousel */}
                    <Carousel items={jobBanners} />

                    <div className="grid items-center gap-12 lg:grid-cols-2">
                        <div>
                            <h1 className="text-4xl font-bold tracking-tight text-white sm:text-5xl lg:text-6xl">
                                Tìm việc làm
                                <br />
                                <span className="text-yellow-300">nhanh chóng</span>
                            </h1>
                            <p className="mt-6 max-w-xl text-lg text-white/90">
                                Tiếp cận <span className="font-semibold">50,000+</span> tin tuyển dụng việc làm mỗi ngày từ hàng nghìn doanh nghiệp uy tín tại Việt Nam
                            </p>

                            {/* Search Box */}
                            <div className="mt-8">
                                <div className="flex flex-col gap-3 rounded-xl bg-white p-2 shadow-2xl sm:flex-row">
                                    <div className="relative flex-1">
                                        <Search className="absolute left-4 top-1/2 h-5 w-5 -translate-y-1/2 text-gray-400" />
                                        <input
                                            type="text"
                                            placeholder="Vị trí tuyển dụng, tên công ty..."
                                            className="w-full rounded-lg border-0 bg-gray-50 py-3.5 pl-12 pr-4 text-gray-900 placeholder:text-gray-500 focus:outline-none focus:ring-2 focus:ring-[#00b14f]/20"
                                        />
                                    </div>
                                    <div className="relative flex-1">
                                        <MapPin className="absolute left-4 top-1/2 h-5 w-5 -translate-y-1/2 text-gray-400" />
                                        <select className="w-full appearance-none rounded-lg border-0 bg-gray-50 py-3.5 pl-12 pr-4 text-gray-900 focus:outline-none focus:ring-2 focus:ring-[#00b14f]/20">
                                            <option>Tất cả địa điểm</option>
                                            <option>Hà Nội</option>
                                            <option>TP. Hồ Chí Minh</option>
                                            <option>Đà Nẵng</option>
                                        </select>
                                    </div>
                                    <Link to="/jobs">
                                        <Button size="lg" className="w-full sm:w-auto sm:px-8">
                                            <Search className="h-4 w-4" />
                                            Tìm kiếm
                                        </Button>
                                    </Link>
                                </div>
                            </div>

                            {/* Popular searches */}
                            <div className="mt-6 flex flex-wrap items-center gap-2">
                                <span className="text-sm text-white/80">Phổ biến:</span>
                                {['Frontend Developer', 'UI/UX Designer', 'Data Analyst', 'Marketing'].map((term) => (
                                    <Link
                                        key={term}
                                        to={`/jobs?keyword=${encodeURIComponent(term)}`}
                                        className="rounded-full bg-white/15 px-3 py-1.5 text-sm font-medium text-white backdrop-blur-sm transition hover:bg-white/25"
                                    >
                                        {term}
                                    </Link>
                                ))}
                            </div>
                        </div>

                        {/* Hero Image/Stats */}
                        <div className="hidden lg:block">
                            <div className="relative">
                                <div className="grid grid-cols-2 gap-4">
                                    {stats.map((stat) => (
                                        <div
                                            key={stat.label}
                                            className="rounded-2xl bg-white/10 p-6 backdrop-blur-sm"
                                        >
                                            <div className="text-3xl font-bold text-white">{stat.value}</div>
                                            <div className="mt-1 text-sm text-white/80">{stat.label}</div>
                                        </div>
                                    ))}
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </section>

            {/* Mobile Stats */}
            <section className="border-b border-gray-100 bg-white py-8 lg:hidden">
                <div className="mx-auto max-w-7xl px-4">
                    <div className="grid grid-cols-2 gap-6 sm:grid-cols-4">
                        {stats.map((stat) => (
                            <div key={stat.label} className="text-center">
                                <div className="text-2xl font-bold text-[#00b14f]">{stat.value}</div>
                                <div className="mt-1 text-xs text-gray-600">{stat.label}</div>
                            </div>
                        ))}
                    </div>
                </div>
            </section>

            {/* Top Companies */}
            <section className="border-b border-gray-100 bg-gray-50 py-8">
                <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
                    <div className="flex items-center justify-center gap-3 text-sm text-gray-500">
                        <span>Được tin dùng bởi:</span>
                        <div className="flex gap-6">
                            {topCompanies.map((company) => (
                                <div key={company.name} className="flex items-center gap-2">
                                    <span className="text-2xl">{company.logo}</span>
                                    <span className="hidden font-medium text-gray-700 sm:inline">{company.name}</span>
                                </div>
                            ))}
                        </div>
                    </div>
                </div>
            </section>

            {/* Job Categories */}
            <section className="bg-white py-16">
                <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
                    <div className="flex items-center justify-between">
                        <div>
                            <h2 className="text-2xl font-bold text-gray-900 sm:text-3xl">Top ngành nghề nổi bật</h2>
                            <p className="mt-2 text-gray-600">Khám phá hàng nghìn cơ hội việc làm hấp dẫn</p>
                        </div>
                        <Link to="/jobs" className="hidden sm:block">
                            <Button variant="outline">
                                Xem tất cả
                                <ArrowRight className="h-4 w-4" />
                            </Button>
                        </Link>
                    </div>

                    <div className="mt-8 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
                        {categories.map((cat) => (
                            <Link
                                key={cat.name}
                                to={`/jobs?category=${encodeURIComponent(cat.name)}`}
                                className="group flex items-center gap-4 rounded-xl border border-gray-100 bg-white p-5 transition-all hover:border-[#00b14f]/30 hover:shadow-lg"
                            >
                                <div className={`flex h-14 w-14 items-center justify-center rounded-xl ${cat.color}`}>
                                    <span className="text-2xl">{cat.icon}</span>
                                </div>
                                <div className="flex-1">
                                    <h3 className="font-semibold text-gray-900 group-hover:text-[#00b14f]">{cat.name}</h3>
                                    <p className="text-sm text-gray-500">{cat.count.toLocaleString()} việc làm</p>
                                </div>
                                <ArrowRight className="h-5 w-5 text-gray-300 transition group-hover:translate-x-1 group-hover:text-[#00b14f]" />
                            </Link>
                        ))}
                    </div>

                    <div className="mt-6 text-center sm:hidden">
                        <Link to="/jobs">
                            <Button variant="outline">
                                Xem tất cả ngành nghề
                                <ArrowRight className="h-4 w-4" />
                            </Button>
                        </Link>
                    </div>
                </div>
            </section>

            {/* Features Section */}
            <section className="bg-gray-50 py-16">
                <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
                    <div className="text-center">
                        <h2 className="text-2xl font-bold text-gray-900 sm:text-3xl">Tại sao chọn CanPany?</h2>
                        <p className="mx-auto mt-3 max-w-2xl text-gray-600">
                            Nền tảng tuyển dụng thông minh với công nghệ AI tiên tiến nhất
                        </p>
                    </div>

                    <div className="mt-12 grid gap-8 md:grid-cols-3">
                        {features.map((feature) => (
                            <div
                                key={feature.title}
                                className="group rounded-2xl border border-gray-100 bg-white p-8 text-center transition hover:border-[#00b14f]/30 hover:shadow-xl"
                            >
                                <div className="mx-auto flex h-16 w-16 items-center justify-center rounded-2xl bg-gradient-to-br from-[#00b14f] to-[#008f3c] transition group-hover:scale-110">
                                    <feature.icon className="h-8 w-8 text-white" />
                                </div>
                                <h3 className="mt-6 text-lg font-semibold text-gray-900">{feature.title}</h3>
                                <p className="mt-3 text-gray-600">{feature.description}</p>
                            </div>
                        ))}
                    </div>
                </div>
            </section>

            {/* CTA Section */}
            <section className="bg-gradient-to-br from-[#00b14f] to-[#008f3c] py-16">
                <div className="mx-auto max-w-7xl px-4 text-center sm:px-6 lg:px-8">
                    <h2 className="text-3xl font-bold text-white sm:text-4xl">
                        Sẵn sàng cho cơ hội mới?
                    </h2>
                    <p className="mx-auto mt-4 max-w-2xl text-lg text-white/90">
                        Đăng ký ngay để nhận tin việc làm phù hợp và kết nối với nhà tuyển dụng hàng đầu
                    </p>
                    <div className="mt-8 flex flex-col items-center justify-center gap-4 sm:flex-row">
                        <Link to="/auth/register">
                            <Button size="lg" className="bg-white text-[#00b14f] hover:bg-gray-100">
                                <Users className="h-5 w-5" />
                                Tìm việc ngay
                            </Button>
                        </Link>
                        <Link to="/auth/register">
                            <Button size="lg" variant="outline" className="border-white text-white hover:bg-white/10">
                                <Building2 className="h-5 w-5" />
                                Đăng tin tuyển dụng
                            </Button>
                        </Link>
                    </div>
                </div>
            </section>
        </div>
    );
}
