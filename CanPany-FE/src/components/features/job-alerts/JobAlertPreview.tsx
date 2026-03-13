import { X, Loader2, Briefcase, MapPin, TrendingUp, ExternalLink } from 'lucide-react';
import { Link } from 'react-router-dom';
import { useJobAlertPreview } from '../../../hooks/useJobAlerts';
import type { JobAlertResponse } from '../../../api/jobAlerts.api';
import { Button } from '../../ui/Button';
import { cn } from '../../../utils';

interface JobAlertPreviewProps {
    alert: JobAlertResponse;
    onClose: () => void;
}

function MatchScoreBadge({ score }: { score: number }) {
    const color =
        score >= 80 ? 'bg-green-100 text-green-700' :
        score >= 60 ? 'bg-blue-100 text-blue-700' :
        'bg-amber-100 text-amber-700';
    return (
        <span className={cn('inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-xs font-semibold', color)}>
            <TrendingUp className="h-3 w-3" />
            {score}%
        </span>
    );
}

export function JobAlertPreview({ alert, onClose }: JobAlertPreviewProps) {
    const { data: matches, isLoading } = useJobAlertPreview(alert.id, true);

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
            <div className="absolute inset-0 bg-black/50" onClick={onClose} />
            <div className="relative w-full max-w-lg rounded-xl bg-white shadow-xl max-h-[85vh] flex flex-col">
                {/* Header */}
                <div className="flex items-center justify-between border-b border-gray-100 px-6 py-4">
                    <div>
                        <h2 className="text-lg font-semibold text-gray-900">Xem trước kết quả</h2>
                        <p className="text-sm text-gray-500 mt-0.5">
                            Alert: <span className="font-medium text-gray-700">{alert.title || 'Job Alert'}</span>
                        </p>
                    </div>
                    <button onClick={onClose} className="rounded-lg p-1 text-gray-400 hover:bg-gray-100 hover:text-gray-600">
                        <X className="h-5 w-5" />
                    </button>
                </div>

                {/* Content */}
                <div className="flex-1 overflow-y-auto p-6">
                    {isLoading ? (
                        <div className="flex flex-col items-center justify-center py-12 text-gray-400">
                            <Loader2 className="h-8 w-8 animate-spin text-[#00b14f]" />
                            <p className="mt-3 text-sm">Đang tìm jobs phù hợp...</p>
                        </div>
                    ) : !matches || matches.length === 0 ? (
                        <div className="flex flex-col items-center justify-center py-12 text-center">
                            <div className="mb-3 flex h-14 w-14 items-center justify-center rounded-full bg-gray-100">
                                <Briefcase className="h-7 w-7 text-gray-400" />
                            </div>
                            <p className="font-medium text-gray-700">Chưa có kết quả phù hợp</p>
                            <p className="mt-1 text-sm text-gray-500">
                                Hãy thử nới lỏng tiêu chí lọc để nhận được nhiều gợi ý hơn.
                            </p>
                        </div>
                    ) : (
                        <div className="space-y-3">
                            <p className="text-sm text-gray-500 mb-4">
                                Tìm thấy <span className="font-semibold text-gray-800">{matches.length}</span> công việc phù hợp
                            </p>
                            {matches.map((job) => (
                                <div
                                    key={job.jobId}
                                    className="flex items-start justify-between gap-3 rounded-lg border border-gray-200 p-4 hover:border-[#00b14f]/30 hover:bg-gray-50 transition-colors"
                                >
                                    <div className="min-w-0 flex-1">
                                        <div className="flex items-center gap-2 flex-wrap">
                                            <span className="font-medium text-gray-900 truncate">
                                                {job.jobTitle}
                                            </span>
                                            <MatchScoreBadge score={job.matchScore} />
                                        </div>
                                        <div className="mt-1 flex items-center gap-3 text-xs text-gray-500">
                                            <span>{job.companyName}</span>
                                            {job.location && (
                                                <span className="flex items-center gap-0.5">
                                                    <MapPin className="h-3 w-3" />
                                                    {job.location}
                                                </span>
                                            )}
                                        </div>
                                        {job.budget && job.budget !== 'Negotiable' && (
                                            <div className="mt-1 text-xs font-medium text-[#00b14f]">
                                                {job.budget}
                                            </div>
                                        )}
                                    </div>
                                    <Link
                                        to={`/jobs/${job.jobId}`}
                                        onClick={onClose}
                                        className="shrink-0 rounded-md p-1.5 text-gray-400 hover:bg-gray-100 hover:text-gray-600"
                                    >
                                        <ExternalLink className="h-4 w-4" />
                                    </Link>
                                </div>
                            ))}
                        </div>
                    )}
                </div>

                {/* Footer */}
                <div className="border-t border-gray-100 px-6 py-4">
                    <Button variant="outline" onClick={onClose} className="w-full">
                        Đóng
                    </Button>
                </div>
            </div>
        </div>
    );
}
