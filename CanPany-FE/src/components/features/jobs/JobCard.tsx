import { Link } from 'react-router-dom';
import { MapPin, Clock, DollarSign, Bookmark, Building2, Users, Eye } from 'lucide-react';
import { useTranslation } from 'react-i18next';
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
    const { t } = useTranslation('public');

    const levelColors: Record<string, string> = {
        Junior: 'bg-green-50 text-green-700 border-green-200',
        Mid: 'bg-blue-50 text-blue-700 border-blue-200',
        Senior: 'bg-purple-50 text-purple-700 border-purple-200',
        Expert: 'bg-orange-50 text-orange-700 border-orange-200',
    };

    const getLevelColor = (level?: string): string =>
        level ? levelColors[level] || '' : '';

    return (
        <Card className="group overflow-hidden border border-gray-100 bg-white p-0 transition-all hover:border-[#00b14f]/30 hover:shadow-lg">
            <div className="p-5">
                <div className="flex items-start gap-4">
                    {/* Company Logo */}
                    <div className="flex h-16 w-16 shrink-0 items-center justify-center rounded-xl border border-gray-100 bg-gray-50 transition group-hover:border-[#00b14f]/20 group-hover:bg-[#00b14f]/5">
                        {job.company?.logoUrl ? (
                            <img
                                src={job.company.logoUrl}
                                alt={`${job.company.name} logo`}
                                className="h-10 w-10 object-contain"
                            />
                        ) : (
                            <Building2 className="h-8 w-8 text-gray-400 group-hover:text-[#00b14f]" />
                        )}
                    </div>

                    <div className="flex-1 min-w-0">
                        <Link to={`/jobs/${job.id}`}>
                            <h3 className="font-semibold text-gray-900 transition line-clamp-1 group-hover:text-[#00b14f]">
                                {job.title}
                            </h3>
                        </Link>
                        <p className="mt-1 text-sm font-medium text-gray-600">
                            {job.company?.name || t('jobCard.companyFallback')}
                        </p>

                        <div className="mt-3 flex flex-wrap items-center gap-x-4 gap-y-2 text-sm text-gray-500">
                            {job.budgetAmount && (
                                <span className="flex items-center gap-1.5 font-semibold text-[#00b14f]">
                                    <DollarSign className="h-4 w-4" aria-hidden="true" />
                                    {formatCurrency(job.budgetAmount)}
                                    {job.budgetType === 'Hourly' && t('jobCard.perHour')}
                                </span>
                            )}
                            {job.location && (
                                <span className="flex items-center gap-1.5">
                                    <MapPin className="h-4 w-4" aria-hidden="true" />
                                    {job.location}
                                </span>
                            )}
                            <span className="flex items-center gap-1.5">
                                <Clock className="h-4 w-4" aria-hidden="true" />
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
                            aria-label={isBookmarked ? t('jobCard.ariaUnbookmark') : t('jobCard.ariaSave')}
                        >
                            <Bookmark className={cn('h-5 w-5', isBookmarked && 'fill-current')} aria-hidden="true" />
                        </button>
                    )}
                </div>

                {/* Tags */}
                <div className="mt-4 flex flex-wrap gap-2">
                    {job.level && (
                        <Badge className={getLevelColor(job.level)}>{job.level}</Badge>
                    )}
                    {job.isRemote && (
                        <Badge variant="secondary" className="bg-blue-50 text-blue-700 border-blue-200">
                            Remote
                        </Badge>
                    )}
                    {job.status === 'Closed' && (
                        <Badge variant="destructive">{t('jobCard.closedBadge')}</Badge>
                    )}
                    {job.deadline && (
                        <Badge variant="outline" className="text-gray-500 border-gray-200">
                            {t('jobCard.deadlinePrefix')} {new Date(job.deadline).toLocaleDateString()}
                        </Badge>
                    )}
                </div>
            </div>

            {/* Footer */}
            <div className="flex items-center justify-between border-t border-gray-100 bg-gray-50/50 px-5 py-3">
                <div className="flex items-center gap-4 text-sm text-gray-500">
                    <span
                        className="flex items-center gap-1"
                        aria-label={t('jobCard.applicants', { count: job.applicationCount })}
                    >
                        <Users className="h-4 w-4" aria-hidden="true" />
                        {t('jobCard.applicants', { count: job.applicationCount })}
                    </span>
                    <span
                        className="flex items-center gap-1"
                        aria-label={t('jobCard.views', { count: job.viewCount })}
                    >
                        <Eye className="h-4 w-4" aria-hidden="true" />
                        {t('jobCard.views', { count: job.viewCount })}
                    </span>
                </div>
                <Link to={`/jobs/${job.id}`}>
                    <Button variant="ghost" size="sm" className="text-[#00b14f] hover:bg-[#00b14f]/10">
                        {t('jobCard.viewDetail')}
                    </Button>
                </Link>
            </div>
        </Card>
    );
}
