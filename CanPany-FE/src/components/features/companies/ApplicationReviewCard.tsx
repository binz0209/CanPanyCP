import { Link } from 'react-router-dom';
import { UserRound } from 'lucide-react';
import type { Application, ApplicationStatus } from '../../../types';
import type { CandidateFullProfile } from '../../../api/candidate.api';
import { formatDateTime } from '../../../utils';
import { Button } from '../../ui';
import { StatusBadge } from './StatusBadge';

export interface ApplicationReviewCardData extends Application {
    candidateUser: CandidateFullProfile['user'] | null;
    candidateProfile: CandidateFullProfile['profile'] | null;
}

interface ApplicationReviewCardProps {
    application: ApplicationReviewCardData;
    applicationDetailPath: string;
    onAccept?: () => void;
    isAccepting?: boolean;
    onPrefetch?: () => void;
}

export function ApplicationReviewCard({
    application,
    applicationDetailPath,
    onAccept,
    isAccepting = false,
    onPrefetch,
}: ApplicationReviewCardProps) {
    return (
        <div className="rounded-xl border border-gray-100 p-5 transition hover:border-[#00b14f]/30 hover:shadow-sm">
            <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
                <div className="min-w-0 flex-1">
                    <div className="flex flex-wrap items-center gap-3">
                        <div className="flex h-10 w-10 items-center justify-center rounded-full bg-[#00b14f]/10 text-[#00b14f]">
                            <UserRound className="h-5 w-5" />
                        </div>
                        <div>
                            <p className="font-semibold text-gray-900">
                                {application.candidateUser?.fullName || application.candidateId}
                            </p>
                            <p className="text-sm text-gray-500">
                                {application.candidateProfile?.title || 'Đang tải hồ sơ ứng viên...'}
                            </p>
                        </div>
                        <StatusBadge status={application.status as ApplicationStatus} kind="application" />
                    </div>

                    <div className="mt-4 flex flex-wrap gap-4 text-sm text-gray-500">
                        <span>Ứng tuyển lúc: {formatDateTime(application.createdAt)}</span>
                        <span>Mức phù hợp: {Math.round(Number(application.matchScore || 0))}%</span>
                    </div>

                    <p className="mt-3 line-clamp-3 text-sm leading-6 text-gray-600">
                        {application.coverLetter || 'Ứng viên chưa nhập cover letter.'}
                    </p>
                </div>

                <div className="flex flex-wrap gap-2">
                    {application.status === 'Pending' && onAccept && (
                        <Button size="sm" onClick={onAccept} isLoading={isAccepting}>
                            Chấp nhận
                        </Button>
                    )}
                    <Link
                        to={applicationDetailPath}
                        onMouseEnter={onPrefetch}
                        onFocus={onPrefetch}
                    >
                        <Button size="sm" variant="outline">
                            Review chi tiết
                        </Button>
                    </Link>
                </div>
            </div>
        </div>
    );
}
