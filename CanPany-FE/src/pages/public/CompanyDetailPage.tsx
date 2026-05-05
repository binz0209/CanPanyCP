import { useState } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import { MapPin, Phone, Globe, Building2, CheckCircle, ArrowLeft, Briefcase, Flag } from 'lucide-react';
import { Button, Badge, Card } from '@/components/ui';
import { JobCard } from '@/components/features/jobs';
import { companiesApi, jobsApi, reportsApi } from '@/api';
import { companiesKeys } from '@/lib/queryKeys';
import { useMutation } from '@tanstack/react-query';
import { useAuthStore } from '@/stores/auth.store';
import toast from 'react-hot-toast';

export function CompanyDetailPage() {
    const { t } = useTranslation('public');
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const { isAuthenticated } = useAuthStore();
    const [showReportModal, setShowReportModal] = useState(false);
    const [reportReason, setReportReason] = useState('');
    const [reportDescription, setReportDescription] = useState('');

    // Track click interaction
    const trackClickMutation = useMutation({
        mutationFn: (jobId: string) => jobsApi.trackInteraction(jobId, 2), // Type 2 = Click
    });

    const handleJobClick = (jobId: string) => {
        if (isAuthenticated) {
            trackClickMutation.mutate(jobId);
        }
    };

    const createReportMutation = useMutation({
        mutationFn: () => reportsApi.createReport({
            reportedCompanyId: id,
            reason: reportReason.trim(),
            description: reportDescription.trim(),
        }),
        onSuccess: () => {
            toast.success(t('companyDetail.report.success'));
            setShowReportModal(false);
            setReportReason('');
            setReportDescription('');
        },
        onError: () => toast.error(t('companyDetail.report.error')),
    });

    const { data: company, isLoading } = useQuery({
        queryKey: companiesKeys.detail(id!),
        queryFn: () => companiesApi.getById(id!),
        enabled: !!id,
    });

    const { data: jobs = [] } = useQuery({
        queryKey: companiesKeys.publicJobs(id!, 'Open'),
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
                <h2 className="text-xl font-semibold text-gray-900">{t('companyDetail.notFound')}</h2>
                <Link to="/companies">
                    <Button variant="outline" className="mt-4">
                        <ArrowLeft className="h-4 w-4" />
                        {t('companyDetail.backToList')}
                    </Button>
                </Link>
            </div>
        );
    }

    return (
        <>
        <div className="min-h-screen bg-gray-50">
            {/* Header */}
            <div className="border-b border-gray-200 bg-white">
                <div className="mx-auto max-w-4xl px-4 py-6 sm:px-6 lg:px-8">
                    <Link to="/companies" className="mb-4 inline-flex items-center gap-1 text-sm text-gray-500 hover:text-gray-700">
                        <ArrowLeft className="h-4 w-4" />
                        {t('companyDetail.back')}
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
                                    <Badge variant="success">{t('companyDetail.verifiedBadge')}</Badge>
                                )}
                                <Badge variant="outline">{t('companyDetail.jobsCount', { count: jobs.length })}</Badge>
                                <Button
                                    size="sm"
                                    variant="outline"
                                    onClick={() => isAuthenticated ? setShowReportModal(true) : navigate('/auth/login')}
                                >
                                    <Flag className="h-4 w-4 mr-1" />
                                    {t('companyDetail.report.button')}
                                </Button>
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
                                <h2 className="text-lg font-semibold text-gray-900">{t('companyDetail.aboutTitle')}</h2>
                                <p className="mt-4 whitespace-pre-wrap text-gray-600">{company.description}</p>
                            </Card>
                        )}

                        {/* Open Jobs */}
                        <div className="mt-8">
                            <div className="flex items-center justify-between">
                                <h2 className="text-lg font-semibold text-gray-900">
                                    {t('companyDetail.openJobsTitle', { count: jobs.length })}
                                </h2>
                            </div>

                            {jobs.length === 0 ? (
                                <Card className="mt-4 p-8 text-center">
                                    <Briefcase className="mx-auto h-12 w-12 text-gray-400" />
                                    <p className="mt-2 text-gray-500">{t('companyDetail.noJobs')}</p>
                                </Card>
                            ) : (
                                <div className="mt-4 space-y-4">
                                    {jobs.map((job) => (
                                        <div key={job.id} onClick={() => handleJobClick(job.id)}>
                                            <JobCard job={job} />
                                        </div>
                                    ))}
                                </div>
                            )}
                        </div>
                    </div>

                    {/* Sidebar */}
                    <div className="space-y-6">
                        <Card className="p-6">
                            <h2 className="text-lg font-semibold text-gray-900">{t('companyDetail.contactTitle')}</h2>
                            <dl className="mt-4 space-y-4">
                                {company.address && (
                                    <div>
                                        <dt className="text-sm text-gray-500">{t('companyDetail.address')}</dt>
                                        <dd className="mt-1 font-medium text-gray-900">{company.address}</dd>
                                    </div>
                                )}
                                {company.phone && (
                                    <div>
                                        <dt className="text-sm text-gray-500">{t('companyDetail.phone')}</dt>
                                        <dd className="mt-1 font-medium text-gray-900">{company.phone}</dd>
                                    </div>
                                )}
                                {company.website && (
                                    <div>
                                        <dt className="text-sm text-gray-500">{t('companyDetail.website')}</dt>
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
        {showReportModal && (
            <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
                <div className="w-full max-w-lg rounded-2xl bg-white p-6 shadow-xl">
                    <h3 className="text-lg font-semibold text-gray-900">{t('companyDetail.report.title')}</h3>
                    <div className="mt-4 space-y-3">
                        <div>
                            <label className="mb-1 block text-sm font-medium text-gray-700">
                                {t('companyDetail.report.reason')}
                            </label>
                            <input
                                value={reportReason}
                                onChange={(e) => setReportReason(e.target.value)}
                                placeholder={t('companyDetail.report.reasonPlaceholder')}
                                className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-slate-900"
                            />
                        </div>
                        <div>
                            <label className="mb-1 block text-sm font-medium text-gray-700">
                                {t('companyDetail.report.description')}
                            </label>
                            <textarea
                                value={reportDescription}
                                onChange={(e) => setReportDescription(e.target.value)}
                                rows={4}
                                placeholder={t('companyDetail.report.descriptionPlaceholder')}
                                className="w-full resize-none rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-slate-900"
                            />
                        </div>
                    </div>
                    <div className="mt-5 flex justify-end gap-2">
                        <Button variant="outline" onClick={() => setShowReportModal(false)}>
                            {t('applyModal.cancelButton')}
                        </Button>
                        <Button
                            disabled={!reportReason.trim() || !reportDescription.trim() || createReportMutation.isPending}
                            onClick={() => createReportMutation.mutate()}
                        >
                            {createReportMutation.isPending ? t('companyDetail.report.submitting') : t('companyDetail.report.submit')}
                        </Button>
                    </div>
                </div>
            </div>
        )}
        </>
    );
}
