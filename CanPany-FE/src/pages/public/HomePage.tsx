import { Link } from 'react-router-dom';
import { Search, Building2, Users, Zap, Shield, TrendingUp, ArrowRight, MapPin } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { Button, Carousel } from '../../components/ui';

export function HomePage() {
    const { t } = useTranslation('public');

    const jobBanners = [
        {
            id: 1,
            title: 'Senior Frontend Developer',
            company: 'FPT Software',
            location: t('home.bannerLocation1'),
            salary: t('home.bannerSalary1'),
            image: 'https://images.unsplash.com/photo-1559136555-9303baea8ebd?w=800&h=400&fit=crop',
        },
        {
            id: 2,
            title: 'UI/UX Designer',
            company: 'VNG Corporation',
            location: t('home.bannerLocation2'),
            salary: t('home.bannerSalary2'),
            image: 'https://images.unsplash.com/photo-1581291518857-4e27b48ff24e?w=800&h=400&fit=crop',
        },
        {
            id: 3,
            title: 'Data Analyst',
            company: 'Viettel',
            location: t('home.bannerLocation3'),
            salary: t('home.bannerSalary3'),
            image: 'https://images.unsplash.com/photo-1551288049-bebda4e38f71?w=800&h=400&fit=crop',
        },
    ];

    const features = [
        {
            icon: Zap,
            title: t('home.features.aiTitle'),
            description: t('home.features.aiDescription'),
        },
        {
            icon: Shield,
            title: t('home.features.securityTitle'),
            description: t('home.features.securityDescription'),
        },
        {
            icon: TrendingUp,
            title: t('home.features.cvTitle'),
            description: t('home.features.cvDescription'),
        },
    ];

    const stats = [
        { value: '50,000+', label: t('home.statsJobs') },
        { value: '10,000+', label: t('home.statsCompanies') },
        { value: '100,000+', label: t('home.statsCandidates') },
        { value: '95%', label: t('home.statsSatisfaction') },
    ];

    const categories = [
        { name: t('home.categories.it'), count: 5420, icon: '💻', color: 'bg-blue-50 text-blue-600' },
        { name: t('home.categories.marketing'), count: 2350, icon: '📈', color: 'bg-purple-50 text-purple-600' },
        { name: t('home.categories.finance'), count: 1890, icon: '💰', color: 'bg-amber-50 text-amber-600' },
        { name: t('home.categories.design'), count: 1560, icon: '🎨', color: 'bg-pink-50 text-pink-600' },
        { name: t('home.categories.sales'), count: 3200, icon: '💼', color: 'bg-green-50 text-green-600' },
        { name: t('home.categories.hr'), count: 980, icon: '👥', color: 'bg-cyan-50 text-cyan-600' },
    ];

    const topCompanies = [
        { name: 'FPT Software', logo: '🏢' },
        { name: 'VNG Corporation', logo: '🎮' },
        { name: 'Viettel', logo: '📱' },
        { name: 'Vingroup', logo: '🏗️' },
        { name: 'VNPAY', logo: '💳' },
        { name: 'Momo', logo: '💜' },
    ];

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
                                {t('home.heroTitleLine1')}
                                <br />
                                <span className="text-yellow-300">{t('home.heroTitleHighlight')}</span>
                            </h1>
                            <p className="mt-6 max-w-xl text-lg text-white/90">
                                {t('home.heroSubtitlePrefix')}{' '}
                                <span className="font-semibold">50,000+</span>{' '}
                                {t('home.heroSubtitleSuffix')}
                            </p>

                            {/* Search Box */}
                            <div className="mt-8">
                                <div className="flex flex-col gap-3 rounded-xl bg-white p-2 shadow-2xl sm:flex-row">
                                    <div className="relative flex-1">
                                        <Search className="absolute left-4 top-1/2 h-5 w-5 -translate-y-1/2 text-gray-400" />
                                        <input
                                            type="text"
                                            placeholder={t('home.searchPlaceholder')}
                                            className="w-full rounded-lg border-0 bg-gray-50 py-3.5 pl-12 pr-4 text-gray-900 placeholder:text-gray-500 focus:outline-none focus:ring-2 focus:ring-[#00b14f]/20"
                                        />
                                    </div>
                                    <div className="relative flex-1">
                                        <MapPin className="absolute left-4 top-1/2 h-5 w-5 -translate-y-1/2 text-gray-400" />
                                        <select className="w-full appearance-none rounded-lg border-0 bg-gray-50 py-3.5 pl-12 pr-4 text-gray-900 focus:outline-none focus:ring-2 focus:ring-[#00b14f]/20">
                                            <option>{t('home.allLocations')}</option>
                                            <option>{t('home.locations.hanoi')}</option>
                                            <option>{t('home.locations.hcm')}</option>
                                            <option>{t('home.locations.danang')}</option>
                                        </select>
                                    </div>
                                    <Link to="/jobs">
                                        <Button size="lg" className="w-full sm:w-auto sm:px-8">
                                            <Search className="h-4 w-4" />
                                            {t('home.searchButton')}
                                        </Button>
                                    </Link>
                                </div>
                            </div>

                            {/* Popular searches */}
                            <div className="mt-6 flex flex-wrap items-center gap-2">
                                <span className="text-sm text-white/80">{t('home.popularLabel')}</span>
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
                        <span>{t('home.trustedBy')}</span>
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
                            <h2 className="text-2xl font-bold text-gray-900 sm:text-3xl">{t('home.categoriesTitle')}</h2>
                            <p className="mt-2 text-gray-600">{t('home.categoriesSubtitle')}</p>
                        </div>
                        <Link to="/jobs" className="hidden sm:block">
                            <Button variant="outline">
                                {t('home.categoriesViewAll')}
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
                                    <p className="text-sm text-gray-500">{cat.count.toLocaleString()} {t('home.jobsUnit')}</p>
                                </div>
                                <ArrowRight className="h-5 w-5 text-gray-300 transition group-hover:translate-x-1 group-hover:text-[#00b14f]" />
                            </Link>
                        ))}
                    </div>

                    <div className="mt-6 text-center sm:hidden">
                        <Link to="/jobs">
                            <Button variant="outline">
                                {t('home.categoriesViewAllMobile')}
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
                        <h2 className="text-2xl font-bold text-gray-900 sm:text-3xl">{t('home.featuresTitle')}</h2>
                        <p className="mx-auto mt-3 max-w-2xl text-gray-600">
                            {t('home.featuresSubtitle')}
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
                        {t('home.ctaTitle')}
                    </h2>
                    <p className="mx-auto mt-4 max-w-2xl text-lg text-white/90">
                        {t('home.ctaSubtitle')}
                    </p>
                    <div className="mt-8 flex flex-col items-center justify-center gap-4 sm:flex-row">
                        <Link to="/auth/register">
                            <Button size="lg" className="bg-white text-[#00b14f] hover:bg-gray-100">
                                <Users className="h-5 w-5" />
                                {t('home.ctaFindJob')}
                            </Button>
                        </Link>
                        <Link to="/auth/register">
                            <Button size="lg" variant="outline" className="border-white text-white hover:bg-white/10">
                                <Building2 className="h-5 w-5" />
                                {t('home.ctaPostJob')}
                            </Button>
                        </Link>
                    </div>
                </div>
            </section>
        </div>
    );
}
