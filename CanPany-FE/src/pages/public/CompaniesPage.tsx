import { useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Search, Building2, CheckCircle } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { Button } from '@/components/ui';
import { CompanyCard } from '@/components/features/companies';
import { companiesApi } from '@/api';
import type { Company } from '@/types';

export function CompaniesPage() {
    const { t } = useTranslation('public');
    const [searchParams, setSearchParams] = useSearchParams();
    const [keyword, setKeyword] = useState(searchParams.get('keyword') || '');

    const { data, isLoading } = useQuery({
        queryKey: ['companies', searchParams.toString()],
        queryFn: () => companiesApi.getAll({
            keyword: searchParams.get('keyword') || undefined,
            isVerified: searchParams.get('verified') === 'true' ? true : undefined,
        }),
    });

    const companies = data?.companies || [];

    const handleSearch = (e: React.FormEvent) => {
        e.preventDefault();
        if (keyword.trim()) {
            setSearchParams({ keyword: keyword.trim() });
        } else {
            setSearchParams({});
        }
    };

    return (
        <div className="min-h-screen bg-gray-50">
            {/* Search Header */}
            <div className="border-b border-gray-200 bg-white">
                <div className="mx-auto max-w-7xl px-4 py-6 sm:px-6 lg:px-8">
                    <h1 className="text-2xl font-bold text-gray-900">{t('companies.title')}</h1>

                    <form onSubmit={handleSearch} className="mt-4">
                        <div className="flex flex-col gap-3 sm:flex-row">
                            <div className="relative flex-1">
                                <Search className="absolute left-3 top-1/2 h-5 w-5 -translate-y-1/2 text-gray-400" />
                                <input
                                    type="text"
                                    value={keyword}
                                    onChange={(e) => setKeyword(e.target.value)}
                                    placeholder={t('companies.searchPlaceholder')}
                                    className="w-full rounded-lg border border-gray-300 py-3 pl-10 pr-4 focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20"
                                />
                            </div>
                            <Button type="submit" size="lg">
                                {t('companies.searchButton')}
                            </Button>
                        </div>
                    </form>

                    {/* Quick Filters */}
                    <div className="mt-4 flex gap-2">
                        <Button
                            variant={searchParams.get('verified') === 'true' ? 'default' : 'outline'}
                            size="sm"
                            onClick={() => {
                                if (searchParams.get('verified') === 'true') {
                                    searchParams.delete('verified');
                                } else {
                                    searchParams.set('verified', 'true');
                                }
                                setSearchParams(searchParams);
                            }}
                        >
                            <CheckCircle className="h-4 w-4" />
                            {t('companies.filterVerified')}
                        </Button>
                    </div>
                </div>
            </div>

            {/* Content */}
            <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
                <div className="mb-6 flex items-center justify-between">
                    <p className="text-sm text-gray-600">
                        {isLoading
                            ? t('companies.loading')
                            : t('companies.resultCount', { count: data?.total || 0 })}
                    </p>
                </div>

                {/* Company Grid */}
                {isLoading ? (
                    <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
                        {[1, 2, 3].map((i) => (
                            <div key={i} className="h-48 animate-pulse rounded-xl bg-gray-200" />
                        ))}
                    </div>
                ) : companies.length === 0 ? (
                    <div className="rounded-xl border border-gray-200 bg-white py-16 text-center">
                        <div className="mx-auto h-24 w-24 rounded-full bg-gray-100 p-6">
                            <Building2 className="h-12 w-12 text-gray-400" />
                        </div>
                        <h3 className="mt-4 text-lg font-medium text-gray-900">{t('companies.emptyTitle')}</h3>
                        <p className="mt-2 text-gray-500">{t('companies.emptyDesc')}</p>
                    </div>
                ) : (
                    <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
                        {companies.map((company: Company) => (
                            <CompanyCard key={company.id} company={company} />
                        ))}
                    </div>
                )}
            </div>
        </div>
    );
}
