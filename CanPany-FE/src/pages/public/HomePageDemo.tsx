import { Link } from 'react-router-dom';
import React from 'react';
import { Search, MapPin, ArrowRight, Briefcase, Building2, Star, Heart, Clock } from 'lucide-react';
import { Button, Badge, Carousel } from '../../components/ui';

// Job listing data structure inspired by TopCV density
const jobListings = [
    {
        id: 1,
        title: 'Senior Frontend Developer',
        company: 'FPT Software',
        logo: '💻',
        location: 'Hà Nội',
        salary: '20 - 30 triệu',
        tags: ['React', 'TypeScript', 'Remote'],
        featured: true,
        applicants: '45 ứng viên',
    },
    {
        id: 2,
        title: 'Product Manager',
        company: 'VNG Corporation',
        logo: '🎮',
        location: 'TP. Hồ Chí Minh',
        salary: '18 - 28 triệu',
        tags: ['Product', 'Data', 'Agile'],
        featured: false,
        applicants: '32 ứng viên',
    },
    {
        id: 3,
        title: 'UX/UI Designer',
        company: 'Viettel',
        logo: '📱',
        location: 'Hà Nội',
        salary: '15 - 22 triệu',
        tags: ['Figma', 'Prototyping', 'User Research'],
        featured: true,
        applicants: '28 ứng viên',
    },
    {
        id: 4,
        title: 'DevOps Engineer',
        company: 'VNPAY',
        logo: '💳',
        location: 'TP. Hồ Chí Minh',
        salary: '22 - 32 triệu',
        tags: ['Kubernetes', 'AWS', 'CI/CD'],
        featured: false,
        applicants: '19 ứng viên',
    },
    {
        id: 5,
        title: 'Business Analyst',
        company: 'Vingroup',
        logo: '🏗️',
        location: 'Hà Nội',
        salary: '16 - 24 triệu',
        tags: ['SQL', 'Analytics', 'Excel'],
        featured: false,
        applicants: '51 ứng viên',
    },
    {
        id: 6,
        title: 'Marketing Manager',
        company: 'Momo',
        logo: '💜',
        location: 'TP. Hồ Chí Minh',
        salary: '17 - 26 triệu',
        tags: ['Digital Marketing', 'Campaign', 'Analytics'],
        featured: true,
        applicants: '38 ứng viên',
    },
];

const categories = [
    { name: 'Công nghệ thông tin', count: 5420, icon: '💻' },
    { name: 'Kinh doanh - Bán hàng', count: 3200, icon: '💼' },
    { name: 'Marketing - PR', count: 2350, icon: '📈' },
    { name: 'Tài chính - Ngân hàng', count: 1890, icon: '💰' },
    { name: 'Thiết kế - Đồ họa', count: 1560, icon: '🎨' },
    { name: 'Nhân sự - Hành chính', count: 980, icon: '👥' },
];

const filterLocations = ['Hà Nội', 'TP. Hồ Chí Minh', 'Đà Nẵng', 'Cần Thơ', 'Huế', 'Đông Nam Bộ', 'Miền Bắc', 'Miền Nam'];

const stats = [
    { value: '50,000+', label: 'Việc làm đang tuyển' },
    { value: '10,000+', label: 'Doanh nghiệp' },
    { value: '100,000+', label: 'Ứng viên' },
    { value: '95%', label: 'Hài lòng' },
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

export function HomePageDemo() {
    const [activeLocation, setActiveLocation] = React.useState('Hà Nội');

    return (
        <div className="bg-background">
            {/* ===== HERO SECTION WITH DEPTH ===== */}
            <section className="relative overflow-hidden">
                {/* Multi-layer background */}
                <div className="absolute inset-0 bg-gradient-to-br from-[#00b14f] via-[#009844] to-[#007a35]" />

                {/* Wave pattern overlay */}
                <div className="absolute inset-0 opacity-10">
                    <div className="absolute inset-0" style={{ backgroundImage: 'url("data:image/svg+xml,%3Csvg width=\'60\' height=\'60\' viewBox=\'0 0 60 60\' xmlns=\'http://www.w3.org/2000/svg\'%3E%3Cpath d=\'M0,30 Q15,10 30,30 T60,30 V60 H0 Z\' fill=\'%23ffffff\' fill-opacity=\'0.4\'/%3E%3C/svg%3E")' }} />
                </div>

                {/* Gradient shapes for depth */}
                <div className="absolute -right-32 -top-32 h-96 w-96 rounded-full bg-white/5 blur-3xl" />
                <div className="absolute -left-40 bottom-0 h-80 w-80 rounded-full bg-white/10 blur-2xl" />

                {/* Content */}
                <div className="relative mx-auto max-w-7xl px-4 py-20 sm:px-6 lg:px-8">
                    <div className="grid gap-12 lg:grid-cols-2 lg:items-start">
                        {/* Left: Search & Content */}
                        <div className="space-y-8">
                            <div className="space-y-6">
                                <div className="space-y-3">
                                    <span className="inline-block rounded-full bg-white/20 px-4 py-1.5 text-sm font-medium text-white backdrop-blur-sm">
                                        🚀 Nền tảng tuyển dụng hàng đầu
                                    </span>
                                    <h1 className="text-5xl font-bold text-white leading-tight">
                                        Tìm việc làm
                                        <br />
                                        <span className="text-amber-200">nhanh chóng</span>
                                        <br />
                                        và dễ dàng
                                    </h1>
                                    <p className="text-lg text-white/90 leading-relaxed">
                                        Tiếp cận <span className="font-semibold">50,000+</span> tin tuyển dụng từ hàng nghìn doanh nghiệp uy tín tại Việt Nam. Tìm việc phù hợp với kỹ năng của bạn chỉ trong vài phút.
                                    </p>
                                </div>
                            </div>

                            {/* Search Box with shadow depth */}
                            <div className="space-y-4">
                                <div className="flex flex-col gap-3 rounded-2xl bg-white p-3 shadow-2xl sm:flex-row">
                                    <div className="relative flex-1">
                                        <Search className="absolute left-4 top-1/2 h-5 w-5 -translate-y-1/2 text-gray-400" />
                                        <input
                                            type="text"
                                            placeholder="Vị trí tuyển dụng, tên công ty..."
                                            className="w-full rounded-lg border-0 bg-muted py-3.5 pl-12 pr-4 text-foreground placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-[#00b14f]/30 transition"
                                        />
                                    </div>
                                    <div className="relative flex-1">
                                        <MapPin className="absolute left-4 top-1/2 h-5 w-5 -translate-y-1/2 text-gray-400" />
                                        <select className="w-full appearance-none rounded-lg border-0 bg-muted py-3.5 pl-12 pr-4 text-foreground focus:outline-none focus:ring-2 focus:ring-[#00b14f]/30 transition cursor-pointer">
                                            <option>Tất cả địa điểm</option>
                                            <option>Hà Nội</option>
                                            <option>TP. Hồ Chí Minh</option>
                                            <option>Đà Nẵng</option>
                                        </select>
                                    </div>
                                    <Link to="/jobs">
                                        <Button className="h-auto rounded-lg bg-[#00b14f] px-6 py-3.5 font-semibold text-white shadow-lg hover:bg-[#009844] transition whitespace-nowrap">
                                            <Search className="h-4 w-4" />
                                            Tìm kiếm
                                        </Button>
                                    </Link>
                                </div>

                                {/* Popular searches */}
                                <div className="flex flex-wrap items-center gap-2">
                                    <span className="text-sm text-white/80 font-medium">Phổ biến:</span>
                                    {['Frontend', 'Backend', 'UI/UX', 'Data'].map((term) => (
                                        <Link
                                            key={term}
                                            to={`/jobs?keyword=${encodeURIComponent(term)}`}
                                            className="rounded-full bg-white/15 px-3.5 py-1.5 text-sm font-medium text-white backdrop-blur-sm transition hover:bg-white/25"
                                        >
                                            {term}
                                        </Link>
                                    ))}
                                </div>
                            </div>
                        </div>

                        {/* Right: Carousel */}
                        <div className="hidden lg:block">
                            <Carousel items={jobBanners} />
                        </div>
                    </div>

                    {/* Stats below */}
                    <div className="mt-12">
                        <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
                            {stats.map((stat) => (
                                <div
                                    key={stat.label}
                                    className="rounded-xl bg-white/10 backdrop-blur-sm border border-white/10 p-6 hover:bg-white/15 transition"
                                >
                                    <div className="text-3xl font-bold text-white">{stat.value}</div>
                                    <div className="mt-2 text-sm text-white/80 font-medium">{stat.label}</div>
                                </div>
                            ))}
                        </div>
                    </div>
                </div>
            </section>

            {/* ===== PROMOTIONAL BANNER ===== */}
            <section className="relative bg-muted border-b border-border">
                <div className="absolute inset-0 opacity-40">
                    <div
                        className="h-full w-full opacity-5"
                        style={{
                            backgroundImage: `url("data:image/svg+xml,%3Csvg width='60' height='60' viewBox='0 0 60 60' xmlns='http://www.w3.org/2000/svg'%3E%3Cg fill='none' fillRule='evenodd'%3E%3Cg fill='%23000000'%3E%3Cpath d='M36 34v-4h-2v4h-4v2h4v4h2v-4h4v-2h-4zm0-30V0h-2v4h-4v2h4v4h2V6h4V4h-4zM6 34v-4H4v4H0v2h4v4h2v-4h4v-2H6zM6 4V0H4v4H0v2h4v4h2V6h4V4H6z'/%3E%3C/g%3E%3C/g%3E%3C/svg%3E")`,
                        }}
                    />
                </div>

                <div className="relative mx-auto max-w-7xl px-4 py-12 sm:px-6 lg:px-8">
                    <div className="grid gap-8 lg:grid-cols-3 lg:items-center">
                        <div className="lg:col-span-2">
                            <h2 className="text-2xl font-bold text-foreground">Trở thành nhà tuyển dụng?</h2>
                            <p className="mt-2 text-muted-foreground leading-relaxed">
                                Đăng tin tuyển dụng và tiếp cận ngay 100,000+ ứng viên chất lượng trên nền tảng của chúng tôi.
                            </p>
                        </div>
                        <div className="flex flex-col gap-3 sm:flex-row lg:justify-end">
                            <Button variant="outline" className="border-border text-foreground hover:bg-muted bg-transparent">
                                Tìm hiểu thêm
                            </Button>
                            <Button className="bg-[#00b14f] text-white hover:bg-[#009844]">
                                Đăng tin tuyển
                            </Button>
                        </div>
                    </div>
                </div>
            </section>

            {/* ===== BEST JOBS TODAY SECTION ===== */}
            <section className="relative bg-background py-16">
                <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
                    {/* Section header */}
                    <div className="flex items-end justify-between border-b border-border pb-6 mb-8">
                        <div>
                            <h2 className="text-3xl font-bold text-foreground">Việc làm nổi bật hôm nay</h2>
                            <p className="mt-1 text-muted-foreground text-sm">27/01/2026</p>
                        </div>
                        <button className="hidden sm:flex items-center gap-2 text-[#00b14f] font-semibold hover:underline">
                            Xem tất cả <ArrowRight className="h-4 w-4" />
                        </button>
                    </div>

                    {/* Location filter pills */}
                    <div className="mb-8 overflow-x-auto pb-2">
                        <div className="flex gap-2">
                            {filterLocations.map((location) => (
                                <button
                                    key={location}
                                    onClick={() => setActiveLocation(location)}
                                    className={`px-4 py-2 rounded-full font-medium text-sm whitespace-nowrap transition ${
                                        activeLocation === location
                                            ? 'bg-[#00b14f] text-white'
                                            : 'bg-muted text-muted-foreground hover:bg-muted/80'
                                    }`}
                                >
                                    {location}
                                </button>
                            ))}
                        </div>
                    </div>

                    {/* Job cards grid */}
                    <div className="space-y-3">
                        {jobListings.map((job) => (
                            <div
                                key={job.id}
                                className={`group relative overflow-hidden rounded-xl border transition-all hover:shadow-lg ${
                                    job.featured
                                        ? 'border-[#00b14f]/30 bg-background'
                                        : 'border-border bg-background hover:border-border/80'
                                }`}
                            >
                                <div className="flex items-start gap-4 p-4 sm:p-5">
                                    {/* Company logo */}
                                    <div className="flex-shrink-0">
                                        <div className="flex h-12 w-12 items-center justify-center rounded-lg bg-muted text-xl font-semibold group-hover:bg-[#00b14f] group-hover:text-white transition">
                                            {job.logo}
                                        </div>
                                    </div>

                                    {/* Job info */}
                                    <div className="flex-1 min-w-0">
                                        <div className="flex items-start justify-between gap-4 mb-2">
                                            <div>
                                                <h3 className="font-semibold text-foreground text-sm sm:text-base group-hover:text-[#00b14f] transition">
                                                    {job.title}
                                                </h3>
                                                <p className="text-xs sm:text-sm text-muted-foreground mt-0.5">{job.company}</p>
                                            </div>
                                            {job.featured && (
                                                <Badge className="flex-shrink-0 bg-[#00b14f] text-white text-xs">
                                                    Nổi bật
                                                </Badge>
                                            )}
                                        </div>

                                        {/* Tags */}
                                        <div className="flex flex-wrap gap-2 mb-3">
                                            {job.tags.map((tag) => (
                                                <span
                                                    key={tag}
                                                    className="inline-block px-2.5 py-0.5 rounded-md bg-muted text-xs text-muted-foreground"
                                                >
                                                    {tag}
                                                </span>
                                            ))}
                                        </div>

                                        {/* Job meta */}
                                        <div className="flex flex-wrap items-center gap-4 text-xs sm:text-sm text-muted-foreground">
                                            <div className="flex items-center gap-1">
                                                <MapPin className="h-4 w-4 flex-shrink-0" />
                                                {job.location}
                                            </div>
                                            <div className="font-semibold text-[#00b14f]">{job.salary}</div>
                                            <div className="flex items-center gap-1">
                                                <Clock className="h-4 w-4 flex-shrink-0" />
                                                {job.applicants}
                                            </div>
                                        </div>
                                    </div>

                                    {/* Action buttons */}
                                    <div className="flex-shrink-0 flex gap-2">
                                        <button className="p-2 hover:bg-muted rounded-lg transition">
                                            <Heart className="h-5 w-5 text-muted-foreground hover:text-red-500 transition" />
                                        </button>
                                        <Link to={`/jobs/${job.id}`}>
                                            <Button className="hidden sm:block px-3 py-2 bg-[#00b14f] text-white text-xs sm:text-sm font-semibold rounded-lg hover:bg-[#009844] transition">
                                                Ứng tuyển
                                            </Button>
                                        </Link>
                                    </div>
                                </div>
                            </div>
                        ))}
                    </div>

                    <div className="mt-8 text-center sm:hidden">
                        <Button className="bg-[#00b14f] text-white hover:bg-[#009844]">
                            Xem tất cả việc làm
                        </Button>
                    </div>
                </div>
            </section>

            {/* ===== JOB CATEGORIES SECTION ===== */}
            <section className="bg-muted border-t border-border py-16">
                <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
                    <div className="mb-8">
                        <h2 className="text-3xl font-bold text-foreground">Ngành nghề nổi bật</h2>
                        <p className="mt-2 text-muted-foreground">Khám phá hàng nghìn cơ hội việc làm trong các ngành khác nhau</p>
                    </div>

                    <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
                        {categories.map((cat) => (
                            <button
                                key={cat.name}
                                className="group text-left rounded-lg border border-border bg-background p-5 transition-all hover:border-[#00b14f] hover:shadow-md"
                            >
                                <div className="flex items-start justify-between">
                                    <div className="flex-1">
                                        <div className="flex items-center gap-3 mb-2">
                                            <span className="text-2xl">{cat.icon}</span>
                                            <h3 className="font-semibold text-foreground group-hover:text-[#00b14f] transition">
                                                {cat.name}
                                            </h3>
                                        </div>
                                        <p className="text-sm text-muted-foreground font-medium">
                                            {cat.count.toLocaleString()} việc làm
                                        </p>
                                    </div>
                                    <ArrowRight className="h-5 w-5 text-muted-foreground group-hover:text-[#00b14f] group-hover:translate-x-1 transition" />
                                </div>
                            </button>
                        ))}
                    </div>
                </div>
            </section>

            {/* ===== CTA SECTION ===== */}
            <section className="relative overflow-hidden bg-gradient-to-br from-[#00b14f] to-[#007a35] py-16">
                <div className="absolute inset-0 opacity-10">
                    <div
                        className="h-full w-full"
                        style={{
                            backgroundImage: `url("data:image/svg+xml,%3Csvg width='60' height='60' viewBox='0 0 60 60' xmlns='http://www.w3.org/2000/svg'%3E%3Cg fill='none' fillRule='evenodd'%3E%3Cg fill='%23ffffff'%3E%3Cpath d='M36 34v-4h-2v4h-4v2h4v4h2v-4h4v-2h-4zm0-30V0h-2v4h-4v2h4v4h2V6h4V4h-4zM6 34v-4H4v4H0v2h4v4h2v-4h4v-2H6zM6 4V0H4v4H0v2h4v4h2V6h4V4H6z'/%3E%3C/g%3E%3C/g%3E%3C/svg%3E")`,
                        }}
                    />
                </div>

                <div className="relative mx-auto max-w-3xl px-4 text-center sm:px-6 lg:px-8">
                    <h2 className="text-4xl font-bold text-white leading-tight">
                        Bắt đầu tìm việc ngay hôm nay
                    </h2>
                    <p className="mt-4 text-lg text-white/90">
                        Hàng nghìn doanh nghiệp đang chờ tìm người tài như bạn. Tạo profile trong vài phút và nhận ngay những gợi ý việc làm phù hợp.
                    </p>
                    <div className="mt-8 flex flex-col gap-3 sm:flex-row sm:justify-center">
                        <Button className="bg-white text-[#00b14f] font-semibold hover:bg-gray-100 shadow-lg">
                            <Briefcase className="h-5 w-5" />
                            Đăng ký tìm việc
                        </Button>
                        <Button variant="outline" className="border-white text-white hover:bg-white/10 font-semibold bg-transparent">
                            <Building2 className="h-5 w-5" />
                            Đây là nhà tuyển dụng
                        </Button>
                    </div>
                </div>
            </section>
        </div>
    );
}