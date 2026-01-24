import { Link } from 'react-router-dom';
import { MapPin, Clock, DollarSign, Bookmark, Building2, Users } from 'lucide-react';
import { Card, Badge, Button } from '@/components/ui';
import type { Job } from '@/types';
import { formatRelativeTime, formatCurrency } from '@/utils';
import { cn } from '@/utils';

interface JobCardProps {
    job: Job;
    onBookmark?: (id: string) => void;
    isBookmarked?: boolean;
}

export function JobCard({ job, onBookmark, isBookmarked }: JobCardProps) {
    const levelColors = {
        Junior: 'bg-green-50 text-green-700 border-green-200',
        Mid: 'bg-blue-50 text-blue-700 border-blue-200',
        Senior: 'bg-purple-50 text-purple-700 border-purple-200',
        Expert: 'bg-orange-50 text-orange-700 border-orange-200',
    };

    return (
        <Card className="group overflow-hidden border border-gray-100 bg-white p-0 transition-all hover:border-[#00b14f]/30 hover:shadow-lg">
            <div className="p-5">
                <div className="flex items-start gap-4">
                    {/* Company Logo */}
                    <div className="flex h-16 w-16 shrink-0 items-center justify-center rounded-xl border border-gray-100 bg-gray-50 transition group-hover:border-[#00b14f]/20 group-hover:bg-[#00b14f]/5">
                        <Building2 className="h-8 w-8 text-gray-400 group-hover:text-[#00b14f]" />
                    </div>

                    <div className="flex-1 min-w-0">
                        <Link to={`/jobs/${job.id}`}>
                            <h3 className="font-semibold text-gray-900 transition line-clamp-1 group-hover:text-[#00b14f]">
                                {job.title}
                            </h3>
                        </Link>
                        <p className="mt-1 text-sm font-medium text-gray-600">Tên công ty</p>

                        <div className="mt-3 flex flex-wrap items-center gap-x-4 gap-y-2 text-sm text-gray-500">
                            {job.budgetAmount && (
                                <span className="flex items-center gap-1.5 font-semibold text-[#00b14f]">
                                    <DollarSign className="h-4 w-4" />
                                    {formatCurrency(job.budgetAmount)}
                                    {job.budgetType === 'Hourly' && '/giờ'}
                                </span>
                            )}
                            {job.location && (
                                <span className="flex items-center gap-1.5">
                                    <MapPin className="h-4 w-4" />
                                    {job.location}
                                </span>
                            )}
                            <span className="flex items-center gap-1.5">
                                <Clock className="h-4 w-4" />
                                {formatRelativeTime(job.createdAt)}
                            </span>
                        </div>
                    </div>

                    {onBookmark && (
                        <button
                            onClick={(e) => {
                                e.preventDefault();
                                onBookmark(job.id);
                            }}
                            className={cn(
                                'rounded-full p-2.5 transition-all',
                                isBookmarked
                                    ? 'bg-[#00b14f]/10 text-[#00b14f]'
                                    : 'text-gray-400 hover:bg-gray-100 hover:text-[#00b14f]'
                            )}
                        >
                            <Bookmark className={cn('h-5 w-5', isBookmarked && 'fill-current')} />
                        </button>
                    )}
                </div>

                {/* Tags */}
                <div className="mt-4 flex flex-wrap gap-2">
                    {job.level && (
                        <Badge className={levelColors[job.level]}>{job.level}</Badge>
                    )}
                    {job.isRemote && (
                        <Badge variant="secondary" className="bg-blue-50 text-blue-700 border-blue-200">
                            Remote
                        </Badge>
                    )}
                    {job.status === 'Closed' && <Badge variant="destructive">Đã đóng</Badge>}
                </div>
            </div>

            {/* Footer */}
            <div className="flex items-center justify-between border-t border-gray-100 bg-gray-50/50 px-5 py-3">
                <div className="flex items-center gap-4 text-sm text-gray-500">
                    <span className="flex items-center gap-1">
                        <Users className="h-4 w-4" />
                        {job.applicationCount} ứng viên
                    </span>
                </div>
                <Link to={`/jobs/${job.id}`}>
                    <Button variant="ghost" size="sm" className="text-[#00b14f] hover:bg-[#00b14f]/10">
                        Xem chi tiết
                    </Button>
                </Link>
            </div>
        </Card>
    );
}
