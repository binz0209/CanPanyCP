import { useState, useEffect } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { MapPin, Clock, DollarSign, Bookmark, Building2, ArrowLeft, Sparkles, CheckCircle, XCircle, RefreshCw, FileText, ExternalLink } from 'lucide-react';
import toast from 'react-hot-toast';
import { Button, Badge, Card } from '../../components/ui';
import { ApplyModal } from '../../components/features/jobs';
import { jobsApi } from '../../api';
import { cvApi } from '../../api/cv.api';
import { candidateApi } from '../../api/candidate.api';
import { formatRelativeTime, formatCurrency, formatDate } from '../../utils';
import { cn } from '../../utils';
import { useAuthStore } from '@/stores/auth.store';
import { useBookmarks } from '@/hooks/candidate/useBookmarks';

export function JobDetailPage() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const queryClient = useQueryClient();
    const { isAuthenticated } = useAuthStore();
    const [showApplyModal, setShowApplyModal] = useState(false);

    // AI CV gen state
    const [showCVModal, setShowCVModal] = useState(false);
    const [cvJobId, setCvJobId] = useState<string | null>(null);
    const [cvUrl, setCvUrl] = useState<string | null>(null);

    const { data, isLoading, error } = useQuery({
        queryKey: ['job', id],
        queryFn: () => jobsApi.getById(id!),
        enabled: !!id,
    });

    // Track View interaction when page loads
    useEffect(() => {
        if (id && isAuthenticated) {
            jobsApi.trackInteraction(id, 1).catch(console.error);
        }
    }, [id, isAuthenticated]);

    const { isBookmarked, toggle, isToggling } = useBookmarks();

    // Generate CV mutation
    const generateCVMutation = useMutation({
        mutationFn: () => cvApi.generateCV(id),
        onSuccess: (d) => {
            setCvJobId(d.jobId);
            setCvUrl(null);
            toast.success('🚀 Đang tạo CV phù hợp với vị trí này...');
        },
        onError: () => toast.error('Không thể bắt đầu tạo CV. Vui lòng thử lại.'),
    });

    // Poll CV generation progress
    const { data: cvJobStatus } = useQuery({
        queryKey: ['cv-gen-job', cvJobId],
        queryFn: () => candidateApi.getMyJobDetail(cvJobId!),
        enabled: !!cvJobId,
        refetchInterval: (q) => {
            const s = q.state.data?.status;
            if (s === 'Completed' || s === 'Failed') return false;
            return 2000;
        },
    });

    useEffect(() => {
        if (cvJobStatus?.status === 'Completed' && cvJobId) {
            const url = cvJobStatus.result?.FileUrl || cvJobStatus.result?.fileUrl;
            if (url) setCvUrl(url as string);
            setCvJobId(null);
            queryClient.invalidateQueries({ queryKey: ['candidate-cvs'] });
            toast.success('🎉 CV đã được tạo thành công!');
        }
        if (cvJobStatus?.status === 'Failed' && cvJobId) {
            setCvJobId(null);
            toast.error('Tạo CV thất bại. Vui lòng thử lại.');
        }
    }, [cvJobStatus?.status]); // eslint-disable-line react-hooks/exhaustive-deps

    const cvIsRunning = !!cvJobId && cvJobStatus?.status !== 'Completed' && cvJobStatus?.status !== 'Failed';

    if (isLoading) {
        return (
            <div className="min-h-screen bg-gray-50 dark:bg-slate-950">
                <div className="mx-auto max-w-4xl px-4 py-8">
                    <div className="h-64 animate-pulse rounded-xl bg-gray-200 dark:bg-slate-800" />
                </div>
            </div>
        );
    }

    if (error || !data) {
        return (
            <div className="min-h-screen bg-gray-50 py-20 text-center dark:bg-slate-950">
                <h2 className="text-xl font-semibold text-gray-900 dark:text-slate-100">Không tìm thấy việc làm</h2>
                <Link to="/jobs">
                    <Button variant="outline" className="mt-4">
                        <ArrowLeft className="h-4 w-4" />
                        Quay lại danh sách
                    </Button>
                </Link>
            </div>
        );
    }

    const { job } = data;
    const bookmarked = isBookmarked(job.id) || data.isBookmarked;

    const levelColors = {
        Junior: 'bg-green-100 text-green-800 dark:bg-green-900/40 dark:text-green-200',
        Mid: 'bg-blue-100 text-blue-800 dark:bg-blue-900/40 dark:text-blue-200',
        Senior: 'bg-purple-100 text-purple-800 dark:bg-purple-900/40 dark:text-purple-200',
        Expert: 'bg-red-100 text-red-800 dark:bg-red-900/40 dark:text-red-200',
    };

    return (
        <>
        <div className="min-h-screen bg-gray-50 dark:bg-slate-950">
            {/* Header */}
            <div className="border-b border-gray-200 bg-white dark:border-slate-800 dark:bg-slate-900">
                <div className="mx-auto max-w-4xl px-4 py-6 sm:px-6 lg:px-8">
                    <Link to="/jobs" className="mb-4 inline-flex items-center gap-1 text-sm text-gray-500 hover:text-gray-700 dark:text-slate-400 dark:hover:text-slate-200">
                        <ArrowLeft className="h-4 w-4" />
                        Quay lại
                    </Link>

                    <div className="flex flex-col gap-6 lg:flex-row lg:items-start lg:justify-between">
                        <div className="flex gap-4">
                            <div className="flex h-16 w-16 shrink-0 items-center justify-center rounded-xl bg-gradient-to-br from-blue-100 to-purple-100 dark:from-slate-800 dark:to-slate-700 lg:h-20 lg:w-20">
                                <Building2 className="h-8 w-8 text-blue-600 dark:text-blue-300 lg:h-10 lg:w-10" />
                            </div>
                            <div>
                                <h1 className="text-2xl font-bold text-gray-900 dark:text-slate-100 lg:text-3xl">{job.title}</h1>
                                <p className="mt-1 text-lg text-gray-600 dark:text-slate-300">Company Name</p>
                                <div className="mt-3 flex flex-wrap items-center gap-4 text-sm text-gray-500 dark:text-slate-400">
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

                        <div className="flex flex-wrap gap-3">
                            <Button
                                variant="outline"
                                size="lg"
                                onClick={() => toggle(job)}
                                isLoading={isToggling(job.id)}
                                aria-label={bookmarked ? 'Bỏ lưu việc làm' : 'Lưu việc làm'}
                            >
                                <Bookmark
                                    className={cn(
                                        'h-4 w-4',
                                        bookmarked && 'fill-current text-[#00b14f]'
                                    )}
                                />
                                {bookmarked ? 'Đã lưu' : 'Lưu'}
                            </Button>

                            {/* AI CV button – only for logged-in candidates */}
                            {isAuthenticated && (
                                <Button
                                    size="lg"
                                    variant="outline"
                                    onClick={() => setShowCVModal(true)}
                                    className="gap-2 border-[#00b14f] text-[#00b14f] hover:bg-[#00b14f]/5"
                                >
                                    <Sparkles className="h-4 w-4" />
                                    Tạo CV phù hợp
                                </Button>
                            )}

                            <Button
                                size="lg"
                                onClick={() =>
                                    isAuthenticated
                                        ? setShowApplyModal(true)
                                        : navigate('/auth/login')
                                }
                            >
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
                        <Card className="p-6 dark:border-slate-800 dark:bg-slate-900">
                            <h2 className="text-lg font-semibold text-gray-900 dark:text-slate-100">Mô tả công việc</h2>
                            <div className="prose prose-gray mt-4 max-w-none dark:prose-invert">
                                <p className="whitespace-pre-wrap text-gray-600 dark:text-slate-300">{job.description}</p>
                            </div>
                        </Card>

                        {job.skillIds && job.skillIds.length > 0 && (
                            <Card className="mt-6 p-6">
                                <h2 className="text-lg font-semibold text-gray-900 dark:text-slate-100">Kỹ năng yêu cầu</h2>
                                <div className="mt-4 flex flex-wrap gap-2">
                                    {job.skillIds.map((skill) => (
                                        <Badge key={skill} variant="secondary">{skill}</Badge>
                                    ))}
                                </div>
                            </Card>
                        )}

                        {/* AI CV banner */}
                        {isAuthenticated && (
                            <div className="mt-6 rounded-xl border border-[#00b14f]/20 bg-gradient-to-br from-[#00b14f]/5 to-transparent p-5 flex items-center justify-between gap-4">
                                <div>
                                    <p className="text-sm font-semibold text-gray-900 flex items-center gap-1.5">
                                        <Sparkles className="h-4 w-4 text-[#00b14f]" />
                                        Tạo CV được tối ưu cho vị trí này
                                    </p>
                                    <p className="text-xs text-gray-500 mt-0.5">
                                        Gemini AI sẽ viết lại CV của bạn để highlight đúng kỹ năng nhà tuyển dụng cần.
                                    </p>
                                </div>
                                <Button
                                    size="sm"
                                    onClick={() => setShowCVModal(true)}
                                    className="bg-[#00b14f] hover:bg-[#00a047] text-white shrink-0"
                                >
                                    Tạo ngay
                                </Button>
                            </div>
                        )}
                    </div>

                    {/* Sidebar */}
                    <div className="space-y-6">
                        <Card className="p-6 dark:border-slate-800 dark:bg-slate-900">
                            <h2 className="text-lg font-semibold text-gray-900 dark:text-slate-100">Thông tin chung</h2>
                            <dl className="mt-4 space-y-4">
                                {job.budgetAmount && (
                                    <div>
                                        <dt className="text-sm text-gray-500 dark:text-slate-400">Mức lương</dt>
                                        <dd className="mt-1 flex items-center gap-1 font-medium text-gray-900 dark:text-slate-100">
                                            <DollarSign className="h-4 w-4 text-green-600" />
                                            {formatCurrency(job.budgetAmount)}
                                            {job.budgetType === 'Hourly' && <span className="text-gray-500 dark:text-slate-400">/giờ</span>}
                                        </dd>
                                    </div>
                                )}
                                {job.level && (
                                    <div>
                                        <dt className="text-sm text-gray-500 dark:text-slate-400">Cấp độ</dt>
                                        <dd className="mt-1">
                                            <Badge className={levelColors[job.level]}>{job.level}</Badge>
                                        </dd>
                                    </div>
                                )}
                                <div>
                                    <dt className="text-sm text-gray-500 dark:text-slate-400">Hình thức</dt>
                                    <dd className="mt-1 font-medium text-gray-900 dark:text-slate-100">
                                        {job.isRemote ? 'Remote' : 'Onsite'}
                                    </dd>
                                </div>
                                {job.deadline && (
                                    <div>
                                        <dt className="text-sm text-gray-500 dark:text-slate-400">Hạn nộp</dt>
                                        <dd className="mt-1 font-medium text-gray-900 dark:text-slate-100">{formatDate(job.deadline)}</dd>
                                    </div>
                                )}
                            </dl>
                        </Card>

                        <Card className="p-6 dark:border-slate-800 dark:bg-slate-900">
                            <h2 className="text-lg font-semibold text-gray-900 dark:text-slate-100">Thống kê</h2>
                            <dl className="mt-4 grid grid-cols-2 gap-4">
                                <div className="rounded-lg bg-gray-50 p-3 text-center dark:bg-slate-800">
                                    <dt className="text-2xl font-bold text-blue-600 dark:text-blue-300">{job.viewCount}</dt>
                                    <dd className="text-sm text-gray-500 dark:text-slate-400">Lượt xem</dd>
                                </div>
                                <div className="rounded-lg bg-gray-50 p-3 text-center dark:bg-slate-800">
                                    <dt className="text-2xl font-bold text-green-600 dark:text-green-300">{job.applicationCount}</dt>
                                    <dd className="text-sm text-gray-500 dark:text-slate-400">Ứng viên</dd>
                                </div>
                            </dl>
                        </Card>
                    </div>
                </div>
            </div>
        </div>

        <ApplyModal
            jobId={job.id}
            jobTitle={job.title}
            isOpen={showApplyModal}
            onClose={() => setShowApplyModal(false)}
            onSuccess={() => {
                setShowApplyModal(false);
                queryClient.invalidateQueries({ queryKey: ['job', id] });
                toast.success('Ứng tuyển thành công!', {
                    duration: 2000,
                    position: 'top-right',
                });
            }}
        />

        {/* AI CV Generation Modal */}
        {showCVModal && (
            <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4 backdrop-blur-sm">
                <div className="w-full max-w-md rounded-2xl bg-white shadow-2xl">
                    {/* Modal header */}
                    <div className="rounded-t-2xl bg-gradient-to-br from-[#00b14f] to-[#009940] px-6 py-5 text-white">
                        <div className="flex items-center gap-2">
                            <Sparkles className="h-5 w-5" />
                            <h3 className="text-lg font-semibold">Tạo CV bằng AI</h3>
                        </div>
                        <p className="mt-1 text-sm text-emerald-100">
                            CV sẽ được tối ưu cho vị trí <strong>{job.title}</strong>
                        </p>
                    </div>

                    <div className="p-6">
                        {/* Idle / start */}
                        {!cvJobId && !cvJobStatus && !cvUrl && (
                            <>
                                <p className="text-sm text-gray-600 mb-4">
                                    Gemini AI sẽ đọc thông tin hồ sơ + mô tả công việc để tạo CV phù hợp nhất.
                                </p>
                                <ul className="space-y-1.5 text-xs text-gray-500 mb-6">
                                    <li className="flex items-center gap-1.5"><CheckCircle className="h-3.5 w-3.5 text-[#00b14f]" /> Highlight đúng kỹ năng nhà tuyển dụng cần</li>
                                    <li className="flex items-center gap-1.5"><CheckCircle className="h-3.5 w-3.5 text-[#00b14f]" /> Viết lại Summary phù hợp vị trí</li>
                                    <li className="flex items-center gap-1.5"><CheckCircle className="h-3.5 w-3.5 text-[#00b14f]" /> Tự động lưu vào danh sách CV của bạn</li>
                                </ul>
                                <div className="flex gap-3">
                                    <Button
                                        className="flex-1 bg-[#00b14f] hover:bg-[#00a047] text-white"
                                        onClick={() => generateCVMutation.mutate()}
                                        isLoading={generateCVMutation.isPending}
                                    >
                                        <Sparkles className="h-4 w-4 mr-2" />
                                        Tạo CV ngay
                                    </Button>
                                    <Button variant="outline" onClick={() => setShowCVModal(false)}>
                                        Hủy
                                    </Button>
                                </div>
                            </>
                        )}

                        {/* Running */}
                        {cvIsRunning && cvJobStatus && (
                            <div className="space-y-4">
                                <div className="flex items-center gap-3">
                                    <RefreshCw className="h-5 w-5 text-[#00b14f] animate-spin" />
                                    <span className="text-sm font-medium text-gray-700">Đang tạo CV...</span>
                                </div>
                                <div>
                                    <div className="flex justify-between text-xs text-gray-500 mb-1">
                                        <span>{cvJobStatus.currentStep ?? 'Đang xử lý...'}</span>
                                        <span>{cvJobStatus.percentComplete ?? 0}%</span>
                                    </div>
                                    <div className="w-full bg-gray-100 rounded-full h-2 overflow-hidden">
                                        <div
                                            className="h-full bg-gradient-to-r from-[#00b14f] to-emerald-400 transition-all duration-500"
                                            style={{ width: `${cvJobStatus.percentComplete ?? 0}%` }}
                                        />
                                    </div>
                                </div>
                                <p className="text-xs text-gray-400 text-center">Quá trình thường mất 30-60 giây</p>
                            </div>
                        )}

                        {/* Success */}
                        {cvUrl && (
                            <div className="space-y-4">
                                <div className="flex items-center gap-3 text-emerald-600">
                                    <CheckCircle className="h-6 w-6" />
                                    <span className="font-semibold">CV đã được tạo thành công!</span>
                                </div>
                                <div className="flex gap-3">
                                    <a
                                        href={cvUrl}
                                        target="_blank"
                                        rel="noopener noreferrer"
                                        className="flex-1 flex items-center justify-center gap-2 rounded-xl bg-[#00b14f] text-white py-2.5 text-sm font-medium hover:bg-[#00a047] transition-colors"
                                    >
                                        <ExternalLink className="h-4 w-4" />
                                        Xem CV
                                    </a>
                                    <Link
                                        to="/candidate/cv/list"
                                        className="flex-1 flex items-center justify-center gap-2 rounded-xl border border-gray-200 text-gray-700 py-2.5 text-sm font-medium hover:bg-gray-50 transition-colors"
                                        onClick={() => setShowCVModal(false)}
                                    >
                                        <FileText className="h-4 w-4" />
                                        Danh sách CV
                                    </Link>
                                </div>
                                <button
                                    onClick={() => { setCvUrl(null); }}
                                    className="w-full text-xs text-gray-400 hover:text-gray-600 transition-colors"
                                >
                                    Tạo lại
                                </button>
                            </div>
                        )}

                        {/* Failed */}
                        {cvJobStatus?.status === 'Failed' && !cvUrl && (
                            <div className="space-y-4">
                                <div className="flex items-center gap-3 text-red-500">
                                    <XCircle className="h-6 w-6" />
                                    <span className="font-semibold">Tạo CV thất bại</span>
                                </div>
                                <p className="text-sm text-gray-500">{cvJobStatus.errorMessage ?? 'Đã có lỗi xảy ra.'}</p>
                                <Button className="w-full bg-[#00b14f] hover:bg-[#00a047] text-white" onClick={() => generateCVMutation.mutate()}>
                                    Thử lại
                                </Button>
                            </div>
                        )}
                    </div>
                </div>
            </div>
        )}
        </>
    );
}
