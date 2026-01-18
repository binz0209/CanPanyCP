import { useParams, Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { MapPin, Clock, DollarSign, Bookmark, Share2, Building2, ExternalLink, Users, Eye, ArrowLeft, CheckCircle } from 'lucide-react';
import { Button, Badge, Card } from '@/components/ui';
import { jobsApi } from '@/api';
import { formatRelativeTime, formatCurrency, formatDate } from '@/utils';
import { cn } from '@/utils';

export function JobDetailPage() {
    const { id } = useParams<{ id: string }>();

    const { data, isLoading, error } = useQuery({
        queryKey: ['job', id],
        queryFn: () => jobsApi.getById(id!),
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

    if (error || !data) {
        return (
            <div className="min-h-screen bg-gray-50 py-20 text-center">
                <h2 className="text-xl font-semibold text-gray-900">Không tìm thấy việc làm</h2>
                <Link to="/jobs">
                    <Button variant="outline" className="mt-4">
                        <ArrowLeft className="h-4 w-4" />
                        Quay lại danh sách
                    </Button>
                </Link>
            </div>
        );
    }

    const { job, isBookmarked } = data;

    const levelColors = {
        Junior: 'bg-green-100 text-green-800',
        Mid: 'bg-blue-100 text-blue-800',
        Senior: 'bg-purple-100 text-purple-800',
        Expert: 'bg-red-100 text-red-800',
    };

    return (
        <div className="min-h-screen bg-gray-50">
            {/* Header */}
            <div className="border-b border-gray-200 bg-white">
                <div className="mx-auto max-w-4xl px-4 py-6 sm:px-6 lg:px-8">
                    <Link to="/jobs" className="mb-4 inline-flex items-center gap-1 text-sm text-gray-500 hover:text-gray-700">
                        <ArrowLeft className="h-4 w-4" />
                        Quay lại
                    </Link>

                    <div className="flex flex-col gap-6 lg:flex-row lg:items-start lg:justify-between">
                        <div className="flex gap-4">
                            <div className="flex h-16 w-16 shrink-0 items-center justify-center rounded-xl bg-gradient-to-br from-blue-100 to-purple-100 lg:h-20 lg:w-20">
                                <Building2 className="h-8 w-8 text-blue-600 lg:h-10 lg:w-10" />
                            </div>
                            <div>
                                <h1 className="text-2xl font-bold text-gray-900 lg:text-3xl">{job.title}</h1>
                                <p className="mt-1 text-lg text-gray-600">Company Name</p>
                                <div className="mt-3 flex flex-wrap items-center gap-4 text-sm text-gray-500">
                                    {job.location && (
                                        <span className="flex items-center gap-1">
                                            <MapPin className="h-4 w-4" />
                                            {job.location}
                                        </span>
                                    )}
                                    <span className="flex items-center gap-1">
                                        <Clock className="h-4 w-4" />
                                        {formatRelativeTime(job.createdAt)}
                                    </span>
                                </div>
                            </div>
                        </div>

                        <div className="flex gap-3">
                            <Button variant="outline" size="lg">
                                <Bookmark className={cn('h-4 w-4', isBookmarked && 'fill-current text-blue-600')} />
                                Lưu
                            </Button>
                            <Button size="lg">
                                Ứng tuyển ngay
                            </Button>
                        </div>
                    </div>
                </div>
            </div>

            {/* Content */}
            <div className="mx-auto max-w-4xl px-4 py-8 sm:px-6 lg:px-8">
                <div className="grid gap-8 lg:grid-cols-3">
                    {/* Main Content */}
                    <div className="lg:col-span-2">
                        <Card className="p-6">
                            <h2 className="text-lg font-semibold text-gray-900">Mô tả công việc</h2>
                            <div className="prose prose-gray mt-4 max-w-none">
                                <p className="whitespace-pre-wrap text-gray-600">{job.description}</p>
                            </div>
                        </Card>

                        {job.skillIds && job.skillIds.length > 0 && (
                            <Card className="mt-6 p-6">
                                <h2 className="text-lg font-semibold text-gray-900">Kỹ năng yêu cầu</h2>
                                <div className="mt-4 flex flex-wrap gap-2">
                                    {job.skillIds.map((skill) => (
                                        <Badge key={skill} variant="secondary">{skill}</Badge>
                                    ))}
                                </div>
                            </Card>
                        )}
                    </div>

                    {/* Sidebar */}
                    <div className="space-y-6">
                        <Card className="p-6">
                            <h2 className="text-lg font-semibold text-gray-900">Thông tin chung</h2>
                            <dl className="mt-4 space-y-4">
                                {job.budgetAmount && (
                                    <div>
                                        <dt className="text-sm text-gray-500">Mức lương</dt>
                                        <dd className="mt-1 flex items-center gap-1 font-medium text-gray-900">
                                            <DollarSign className="h-4 w-4 text-green-600" />
                                            {formatCurrency(job.budgetAmount)}
                                            {job.budgetType === 'Hourly' && <span className="text-gray-500">/giờ</span>}
                                        </dd>
                                    </div>
                                )}
                                {job.level && (
                                    <div>
                                        <dt className="text-sm text-gray-500">Cấp độ</dt>
                                        <dd className="mt-1">
                                            <Badge className={levelColors[job.level]}>{job.level}</Badge>
                                        </dd>
                                    </div>
                                )}
                                <div>
                                    <dt className="text-sm text-gray-500">Hình thức</dt>
                                    <dd className="mt-1 font-medium text-gray-900">
                                        {job.isRemote ? 'Remote' : 'Onsite'}
                                    </dd>
                                </div>
                                {job.deadline && (
                                    <div>
                                        <dt className="text-sm text-gray-500">Hạn nộp</dt>
                                        <dd className="mt-1 font-medium text-gray-900">{formatDate(job.deadline)}</dd>
                                    </div>
                                )}
                            </dl>
                        </Card>

                        <Card className="p-6">
                            <h2 className="text-lg font-semibold text-gray-900">Thống kê</h2>
                            <dl className="mt-4 grid grid-cols-2 gap-4">
                                <div className="rounded-lg bg-gray-50 p-3 text-center">
                                    <dt className="text-2xl font-bold text-blue-600">{job.viewCount}</dt>
                                    <dd className="text-sm text-gray-500">Lượt xem</dd>
                                </div>
                                <div className="rounded-lg bg-gray-50 p-3 text-center">
                                    <dt className="text-2xl font-bold text-green-600">{job.applicationCount}</dt>
                                    <dd className="text-sm text-gray-500">Ứng viên</dd>
                                </div>
                            </dl>
                        </Card>
                    </div>
                </div>
            </div>
        </div>
    );
}
