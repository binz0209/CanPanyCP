import type { UserProfile } from '../../../types';
import { formatCurrency } from '../../../utils';
import { Button } from '../../ui';
import { useTranslation } from 'react-i18next';

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
    const { t } = useTranslation('company');

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
                                {candidate.profile.title || t('candidateSearch.positionPlaceholder')}
                            </p>
                        </div>
                    </div>

                    <div className="mt-4 flex flex-wrap gap-4 text-sm text-gray-500">
                        <span>{candidate.profile.location || t('candidateSearch.locationPlaceholder')}</span>
                        <span>
                            {candidate.profile.hourlyRate
                                ? formatCurrency(candidate.profile.hourlyRate)
                                : t('candidateSearch.salaryPlaceholder')}
                        </span>
                    </div>

                    <p className="mt-3 line-clamp-3 text-sm leading-6 text-gray-600">
                        {candidate.profile.bio || candidate.profile.experience || t('candidateSearch.bioPlaceholder')}
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
                        <p className="text-xs font-medium text-green-700">{t('candidateSearch.matchLabel')}</p>
                        <p className="text-2xl font-bold text-green-700">
                            {Math.round(candidate.matchScore)}%
                        </p>
                    </div>

                    <div className="text-xs text-gray-500">
                        <p className="mb-1">
                            {t('candidateSearch.emailLabel')}{' '}
                            {isUnlocked
                                ? candidate.email || t('candidateSearch.emailPlaceholder')
                                : t('candidateSearch.emailHidden')}
                        </p>

                        {onUnlock && !isUnlocked && (
                            <Button
                                size="sm"
                                variant="outline"
                                onClick={onUnlock}
                                isLoading={isUnlocking}
                            >
                                {t('candidateSearch.btnUnlockContact')}
                            </Button>
                        )}

                        {isUnlocked && (
                            <span className="mt-1 inline-flex rounded-full bg-emerald-50 px-2 py-0.5 text-[10px] font-semibold uppercase tracking-wide text-emerald-700">
                                {t('candidateSearch.unlockedBadge')}
                            </span>
                        )}
                    </div>
                </div>
            </div>
        </div>
    );
}
