import { useQuery } from '@tanstack/react-query';
import {
    Briefcase,
    Building2,
    Clock3,
    Cpu,
    FileText,
    Landmark,
    RefreshCw,
    ShieldQuestion,
    Users,
} from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { adminApi } from '../../api';
import { Button, Card } from '../../components/ui';
import { adminKeys } from '../../lib/queryKeys';
import { formatNumber } from '../../utils';

export function AdminDashboardPage() {
    const { t } = useTranslation('admin');
    const dashboardQuery = useQuery({
        queryKey: adminKeys.dashboard(),
        queryFn: () => adminApi.getDashboard(),
    });

    const stats = dashboardQuery.data;

    if (dashboardQuery.isError) {
        return (
            <div className="rounded-xl border border-red-100 bg-red-50 p-6 text-sm text-red-800">
                {t('dashboard.loadError')}
            </div>
        );
    }

    return (
        <div className="space-y-6">
            <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
                <div>
                    <h1 className="text-2xl font-bold text-gray-900">{t('dashboard.title')}</h1>
                    <p className="mt-1 text-sm text-gray-600">{t('dashboard.subtitle')}</p>
                </div>
                <Button
                    variant="outline"
                    className="gap-2"
                    disabled={dashboardQuery.isFetching}
                    onClick={() => void dashboardQuery.refetch()}
                >
                    <RefreshCw className={`h-4 w-4 ${dashboardQuery.isFetching ? 'animate-spin' : ''}`} />
                    {t('dashboard.refresh')}
                </Button>
            </div>

            <section className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
                <Card className="p-5">
                    <div className="flex items-center gap-3">
                        <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-slate-100 text-slate-700">
                            <Users className="h-5 w-5" />
                        </div>
                        <div>
                            <p className="text-sm text-gray-500">{t('dashboard.cards.users')}</p>
                            <p className="text-2xl font-bold text-gray-900">
                                {dashboardQuery.isLoading ? '—' : formatNumber(stats?.totalUsers ?? 0)}
                            </p>
                        </div>
                    </div>
                </Card>
                <Card className="p-5">
                    <div className="flex items-center gap-3">
                        <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-emerald-50 text-[#00b14f]">
                            <Briefcase className="h-5 w-5" />
                        </div>
                        <div>
                            <p className="text-sm text-gray-500">{t('dashboard.cards.jobs')}</p>
                            <p className="text-2xl font-bold text-gray-900">
                                {dashboardQuery.isLoading ? '—' : formatNumber(stats?.totalJobs ?? 0)}
                            </p>
                        </div>
                    </div>
                </Card>
                <Card className="p-5">
                    <div className="flex items-center gap-3">
                        <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-amber-50 text-amber-700">
                            <Landmark className="h-5 w-5" />
                        </div>
                        <div>
                            <p className="text-sm text-gray-500">{t('dashboard.cards.revenue')}</p>
                            <p className="text-2xl font-bold text-gray-900">
                                {dashboardQuery.isLoading ? '—' : formatNumber(stats?.totalRevenue ?? 0)}
                            </p>
                        </div>
                    </div>
                </Card>
                <Card className="p-5 ring-1 ring-dashed ring-gray-200">
                    <div className="flex items-center gap-3">
                        <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-violet-50 text-violet-700">
                            <Cpu className="h-5 w-5" />
                        </div>
                        <div>
                            <p className="text-sm text-gray-500">{t('dashboard.cards.aiUsage')}</p>
                            <p className="text-lg font-semibold text-gray-500">{t('dashboard.aiUsageValue')}</p>
                            <p className="mt-1 text-xs text-gray-500">{t('dashboard.aiUsageHint')}</p>
                        </div>
                    </div>
                </Card>
            </section>

            <section className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
                <Card className="p-5">
                    <div className="flex items-center gap-3">
                        <Building2 className="h-5 w-5 text-gray-400" />
                        <div>
                            <p className="text-sm text-gray-500">{t('dashboard.cards.companies')}</p>
                            <p className="text-xl font-bold text-gray-900">
                                {dashboardQuery.isLoading ? '—' : formatNumber(stats?.totalCompanies ?? 0)}
                            </p>
                        </div>
                    </div>
                </Card>
                <Card className="p-5">
                    <div className="flex items-center gap-3">
                        <FileText className="h-5 w-5 text-gray-400" />
                        <div>
                            <p className="text-sm text-gray-500">{t('dashboard.cards.applications')}</p>
                            <p className="text-xl font-bold text-gray-900">
                                {dashboardQuery.isLoading ? '—' : formatNumber(stats?.totalApplications ?? 0)}
                            </p>
                        </div>
                    </div>
                </Card>
                <Card className="p-5">
                    <div className="flex items-center gap-3">
                        <ShieldQuestion className="h-5 w-5 text-gray-400" />
                        <div>
                            <p className="text-sm text-gray-500">{t('dashboard.cards.pendingVerifications')}</p>
                            <p className="text-xl font-bold text-gray-900">
                                {dashboardQuery.isLoading ? '—' : formatNumber(stats?.pendingVerifications ?? 0)}
                            </p>
                        </div>
                    </div>
                </Card>
                <Card className="p-5">
                    <div className="flex items-center gap-3">
                        <Clock3 className="h-5 w-5 text-gray-400" />
                        <div>
                            <p className="text-sm text-gray-500">{t('dashboard.cards.pendingPayments')}</p>
                            <p className="text-xl font-bold text-gray-900">
                                {dashboardQuery.isLoading ? '—' : formatNumber(stats?.pendingPayments ?? 0)}
                            </p>
                        </div>
                    </div>
                </Card>
            </section>
        </div>
    );
}
