import { useParams, Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { MapPin, Phone, Globe, Building2, CheckCircle, ArrowLeft, Briefcase } from 'lucide-react';
import { Button, Badge, Card } from '@/components/ui';
import { JobCard } from '@/components/features/jobs';
import { companiesApi } from '@/api';

export function CompanyDetailPage() {
    const { id } = useParams<{ id: string }>();

    const { data: company, isLoading } = useQuery({
        queryKey: ['company', id],
        queryFn: () => companiesApi.getById(id!),
        enabled: !!id,
    });

    const { data: jobs = [] } = useQuery({
        queryKey: ['company-jobs', id],
        queryFn: () => companiesApi.getJobs(id!, 'Open'),
        enabled: !!id,
    });

    if (isLoading) {
        return (
            <div className="min-h-screen bg-gray-50">
                <div className="mx-auto max-w-4xl px-4 py-8">
                    <div className="h-64 animate-pulse rounded-xl bg-gray-200" />
                </div>
            </div>
        );
    }

    if (!company) {
        return (
            <div className="min-h-screen bg-gray-50 py-20 text-center">
                <h2 className="text-xl font-semibold text-gray-900">Không tìm thấy công ty</h2>
                <Link to="/companies">
                    <Button variant="outline" className="mt-4">
                        <ArrowLeft className="h-4 w-4" />
                        Quay lại danh sách
                    </Button>
                </Link>
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-gray-50">
            {/* Header */}
            <div className="border-b border-gray-200 bg-white">
                <div className="mx-auto max-w-4xl px-4 py-6 sm:px-6 lg:px-8">
                    <Link to="/companies" className="mb-4 inline-flex items-center gap-1 text-sm text-gray-500 hover:text-gray-700">
                        <ArrowLeft className="h-4 w-4" />
                        Quay lại
                    </Link>

                    <div className="flex flex-col gap-6 sm:flex-row sm:items-start">
                        {company.logoUrl ? (
                            <img
                                src={company.logoUrl}
                                alt={company.name}
                                className="h-24 w-24 rounded-xl object-cover"
                            />
                        ) : (
                            <div className="flex h-24 w-24 shrink-0 items-center justify-center rounded-xl bg-gradient-to-br from-blue-100 to-purple-100">
                                <Building2 className="h-12 w-12 text-blue-600" />
                            </div>
                        )}

                        <div className="flex-1">
                            <div className="flex items-center gap-2">
                                <h1 className="text-2xl font-bold text-gray-900 sm:text-3xl">{company.name}</h1>
                                {company.isVerified && (
                                    <CheckCircle className="h-6 w-6 text-blue-600" />
                                )}
                            </div>

                            <div className="mt-3 flex flex-wrap items-center gap-4 text-sm text-gray-500">
                                {company.address && (
                                    <span className="flex items-center gap-1">
                                        <MapPin className="h-4 w-4" />
                                        {company.address}
                                    </span>
                                )}
                                {company.phone && (
                                    <span className="flex items-center gap-1">
                                        <Phone className="h-4 w-4" />
                                        {company.phone}
                                    </span>
                                )}
                                {company.website && (
                                    <a
                                        href={company.website}
                                        target="_blank"
                                        rel="noopener noreferrer"
                                        className="flex items-center gap-1 text-blue-600 hover:underline"
                                    >
                                        <Globe className="h-4 w-4" />
                                        Website
                                    </a>
                                )}
                            </div>

                            <div className="mt-3 flex gap-2">
                                {company.isVerified && (
                                    <Badge variant="success">Đã xác thực</Badge>
                                )}
                                <Badge variant="outline">{jobs.length} việc làm</Badge>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            {/* Content */}
            <div className="mx-auto max-w-4xl px-4 py-8 sm:px-6 lg:px-8">
                <div className="grid gap-8 lg:grid-cols-3">
                    {/* Main Content */}
                    <div className="lg:col-span-2">
                        {company.description && (
                            <Card className="p-6">
                                <h2 className="text-lg font-semibold text-gray-900">Giới thiệu</h2>
                                <p className="mt-4 whitespace-pre-wrap text-gray-600">{company.description}</p>
                            </Card>
                        )}

                        {/* Open Jobs */}
                        <div className="mt-8">
                            <div className="flex items-center justify-between">
                                <h2 className="text-lg font-semibold text-gray-900">
                                    Việc làm đang tuyển ({jobs.length})
                                </h2>
                            </div>

                            {jobs.length === 0 ? (
                                <Card className="mt-4 p-8 text-center">
                                    <Briefcase className="mx-auto h-12 w-12 text-gray-400" />
                                    <p className="mt-2 text-gray-500">Chưa có việc làm nào</p>
                                </Card>
                            ) : (
                                <div className="mt-4 space-y-4">
                                    {jobs.map((job) => (
                                        <JobCard key={job.id} job={job} />
                                    ))}
                                </div>
                            )}
                        </div>
                    </div>

                    {/* Sidebar */}
                    <div className="space-y-6">
                        <Card className="p-6">
                            <h2 className="text-lg font-semibold text-gray-900">Thông tin liên hệ</h2>
                            <dl className="mt-4 space-y-4">
                                {company.address && (
                                    <div>
                                        <dt className="text-sm text-gray-500">Địa chỉ</dt>
                                        <dd className="mt-1 font-medium text-gray-900">{company.address}</dd>
                                    </div>
                                )}
                                {company.phone && (
                                    <div>
                                        <dt className="text-sm text-gray-500">Điện thoại</dt>
                                        <dd className="mt-1 font-medium text-gray-900">{company.phone}</dd>
                                    </div>
                                )}
                                {company.website && (
                                    <div>
                                        <dt className="text-sm text-gray-500">Website</dt>
                                        <dd className="mt-1">
                                            <a
                                                href={company.website}
                                                target="_blank"
                                                rel="noopener noreferrer"
                                                className="font-medium text-blue-600 hover:underline"
                                            >
                                                {company.website.replace(/^https?:\/\//, '')}
                                            </a>
                                        </dd>
                                    </div>
                                )}
                            </dl>
                        </Card>
                    </div>
                </div>
            </div>
        </div>
    );
}
