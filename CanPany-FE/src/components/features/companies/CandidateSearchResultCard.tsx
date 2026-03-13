import type { UserProfile } from '../../../types';
import { formatCurrency } from '../../../utils';
import { Button } from '../../ui';

export interface CandidateSearchResultCardData {
    userId: string;
    fullName: string;
    email?: string;
    avatarUrl?: string;
    profile: UserProfile;
    matchScore: number;
}

interface CandidateSearchResultCardProps {
    candidate: CandidateSearchResultCardData;
    isUnlocked?: boolean;
    isUnlocking?: boolean;
    onUnlock?: () => void;
}

export function CandidateSearchResultCard({
    candidate,
    isUnlocked,
    isUnlocking,
    onUnlock,
}: CandidateSearchResultCardProps) {
    return (
        <div className="rounded-xl border border-gray-100 p-5 transition hover:border-[#00b14f]/30 hover:shadow-sm">
            <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
                <div className="min-w-0 flex-1">
                    <div className="flex flex-wrap items-center gap-3">
                        <div className="flex h-10 w-10 items-center justify-center rounded-full bg-[#00b14f] text-sm font-semibold text-white">
                            {candidate.fullName.charAt(0).toUpperCase()}
                        </div>
                        <div className="min-w-0">
                            <p className="truncate font-semibold text-gray-900">{candidate.fullName}</p>
                            <p className="text-sm text-gray-500">
                                {candidate.profile.title || 'Chưa cập nhật vị trí'}
                            </p>
                        </div>
                    </div>

                    <div className="mt-4 flex flex-wrap gap-4 text-sm text-gray-500">
                        <span>{candidate.profile.location || 'Chưa cập nhật địa điểm'}</span>
                        <span>
                            {candidate.profile.hourlyRate
                                ? formatCurrency(candidate.profile.hourlyRate)
                                : 'Chưa cập nhật mức lương'}
                        </span>
                    </div>

                    <p className="mt-3 line-clamp-3 text-sm leading-6 text-gray-600">
                        {candidate.profile.bio || candidate.profile.experience || 'Chưa có mô tả hồ sơ'}
                    </p>

                    <div className="mt-4 flex flex-wrap gap-2">
                        {(candidate.profile.skillIds || []).slice(0, 6).map((skillId) => (
                            <span
                                key={skillId}
                                className="rounded-full bg-[#00b14f]/10 px-3 py-1 text-xs font-semibold text-[#00b14f]"
                            >
                                {skillId}
                            </span>
                        ))}
                    </div>
                </div>

                <div className="flex flex-col items-start gap-3 sm:items-end">
                    <div className="rounded-xl bg-green-50 px-4 py-2 text-right">
                        <p className="text-xs font-medium text-green-700">Mức phù hợp</p>
                        <p className="text-2xl font-bold text-green-700">
                            {Math.round(candidate.matchScore)}%
                        </p>
                    </div>

                    <div className="text-xs text-gray-500">
                        <p className="mb-1">
                            Email:{' '}
                            {isUnlocked
                                ? candidate.email || 'Chưa cập nhật email'
                                : 'Đã ẩn – cần mở khóa để xem'}
                        </p>

                        {onUnlock && !isUnlocked && (
                            <Button
                                size="sm"
                                variant="outline"
                                onClick={onUnlock}
                                isLoading={isUnlocking}
                            >
                                Mở khóa liên hệ
                            </Button>
                        )}

                        {isUnlocked && (
                            <span className="mt-1 inline-flex rounded-full bg-emerald-50 px-2 py-0.5 text-[10px] font-semibold uppercase tracking-wide text-emerald-700">
                                Đã mở khóa
                            </span>
                        )}
                    </div>
                </div>
            </div>
        </div>
    );
}
