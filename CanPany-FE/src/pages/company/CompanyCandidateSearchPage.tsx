import { useEffect, useMemo, useState } from 'react';
import { isAxiosError } from 'axios';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { BriefcaseBusiness, Search, Sparkles, Users } from 'lucide-react';
import toast from 'react-hot-toast';
import { Button, Card, Input } from '../../components/ui';
import { candidateApi, jobsApi } from '../../api';
import type { CandidateSearchResult, SemanticCandidateSearchRequest } from '../../api/candidate.api';
import {
    CandidateSearchResultCard,
    type CandidateSearchResultCardData,
    CompanyProfileRequiredState,
    CompanyWorkspaceErrorState,
    CompanyWorkspaceLoader,
    EmptyState,
    SectionHeader,
} from '../../components/features/companies';
import { useCandidateProfilesMap } from '../../hooks/company/useCandidateProfilesMap';
import { useCompanyWorkspace } from '../../hooks/company/useCompanyWorkspace';
import { candidateKeys, companyKeys } from '../../lib/queryKeys';
import { useTranslation } from 'react-i18next';

type SearchMode = 'job' | 'filters' | 'semantic';

type SubmittedCandidateSearch =
    | {
        mode: 'job';
        summary: string;
        selectedJobId: string;
    }
    | {
        mode: 'filters';
        summary: string;
        params: {
            keyword?: string;
            skillIds?: string[];
            location?: string;
            experience?: string;
            minHourlyRate?: number;
            maxHourlyRate?: number;
            page: number;
            pageSize: number;
        };
    }
    | {
        mode: 'semantic';
        summary: string;
        payload: SemanticCandidateSearchRequest;
    };

export function CompanyCandidateSearchPage() {
    const [searchMode, setSearchMode] = useState<SearchMode>('job');
    const [selectedJobId, setSelectedJobId] = useState('');
    const [keyword, setKeyword] = useState('');
    const [skillIdsText, setSkillIdsText] = useState('');
    const [location, setLocation] = useState('');
    const [experience, setExperience] = useState('');
    const [minHourlyRate, setMinHourlyRate] = useState('');
    const [maxHourlyRate, setMaxHourlyRate] = useState('');
    const [jobDescription, setJobDescription] = useState('');
    const [submittedSearch, setSubmittedSearch] = useState<SubmittedCandidateSearch | null>(null);
    const [unlockingCandidateId, setUnlockingCandidateId] = useState<string | null>(null);

    const queryClient = useQueryClient();
    const { t } = useTranslation('company');
    const { companyId, isLoading: isWorkspaceLoading, isMissingProfile, hasFatalError } = useCompanyWorkspace();

    const jobsQuery = useQuery({
        queryKey: companyKeys.workspaceJobs(companyId!),
        queryFn: () => jobsApi.getByCompany(companyId!),
        enabled: !!companyId,
    });

    const unlockedCandidatesQuery = useQuery({
        queryKey: candidateKeys.unlocked(),
        queryFn: () => candidateApi.getUnlockedCandidates(1, 200),
        enabled: !!companyId,
    });

    const unlockedIds = useMemo(
        () => new Set((unlockedCandidatesQuery.data || []).map((item) => item.user.id)),
        [unlockedCandidatesQuery.data]
    );

    useEffect(() => {
        if (!selectedJobId && jobsQuery.data?.length) {
            setSelectedJobId(jobsQuery.data[0].id);
        }
    }, [jobsQuery.data, selectedJobId]);

    const searchQuery = useQuery({
        queryKey: candidateKeys.search(submittedSearch?.mode || 'idle', submittedSearch || {}),
        queryFn: async (): Promise<CandidateSearchResult[]> => {
            if (!submittedSearch) return [];

            if (submittedSearch.mode === 'job') {
                return candidateApi.searchCandidates(submittedSearch.selectedJobId, 20);
            }

            if (submittedSearch.mode === 'filters') {
                return candidateApi.searchCandidatesWithFilters(submittedSearch.params);
            }

            return candidateApi.semanticSearchCandidates(submittedSearch.payload);
        },
        enabled: !!submittedSearch,
        placeholderData: (previousData) => previousData,
    });

    const rawResults = searchQuery.data || [];
    const missingCandidateIds = useMemo(
        () =>
            Array.from(
                new Set(
                    rawResults
                        .filter((result) => !result.userInfo)
                        .map((result) => result.profile.userId)
                        .filter(Boolean)
                )
            ),
        [rawResults]
    );

    const { candidateProfilesMap } = useCandidateProfilesMap(missingCandidateIds);

    const results = useMemo<CandidateSearchResultCardData[]>(
        () =>
            rawResults.map((result) => {
                if (result.userInfo) {
                    return {
                        userId: result.userInfo.id,
                        fullName: result.userInfo.fullName,
                        email: result.userInfo.email,
                        avatarUrl: result.userInfo.avatarUrl,
                        profile: result.profile,
                        matchScore: result.matchScore,
                    };
                }

                const candidateProfile = candidateProfilesMap[result.profile.userId];
                return {
                    userId: result.profile.userId,
                    fullName: candidateProfile?.user.fullName || t('candidateSearch.loadingCandidate'),
                    email: candidateProfile?.user.email,
                    avatarUrl: candidateProfile?.user.avatarUrl,
                    profile: candidateProfile?.profile || result.profile,
                    matchScore: result.matchScore,
                };
            }),
        [candidateProfilesMap, rawResults]
    );

    const unlockMutation = useMutation({
        mutationFn: async (candidateId: string) => {
            await candidateApi.unlockCandidate(candidateId);
        },
        onSuccess: async () => {
            await queryClient.invalidateQueries({ queryKey: candidateKeys.unlocked(), exact: true });
            toast.success(t('candidateSearch.toastUnlocked'));
        },
        onError: (error) => {
            const message = isAxiosError(error)
                ? error.response?.data?.message || t('candidateSearch.toastUnlockFailed')
                : t('candidateSearch.toastUnlockFailed');
            toast.error(message);
        },
        onSettled: () => {
            setUnlockingCandidateId(null);
        },
    });

    const selectedJobTitle = useMemo(
        () => jobsQuery.data?.find((job) => job.id === selectedJobId)?.title,
        [jobsQuery.data, selectedJobId]
    );

    const handleSearch = () => {
        if (searchMode === 'job') {
            if (!selectedJobId) {
                toast.error(t('candidateSearch.toastSelectJob'));
                return;
            }

            setSubmittedSearch({
                mode: 'job',
                selectedJobId,
                summary: selectedJobTitle
                    ? t('candidateSearch.resultsByJob', { title: selectedJobTitle })
                    : t('candidateSearch.resultsByCurrentJob'),
            });
            return;
        }

        if (searchMode === 'filters') {
            setSubmittedSearch({
                mode: 'filters',
                summary: t('candidateSearch.resultsByFilter'),
                params: {
                    keyword: keyword.trim() || undefined,
                    skillIds: skillIdsText
                        .split(',')
                        .map((item) => item.trim())
                        .filter(Boolean),
                    location: location.trim() || undefined,
                    experience: experience.trim() || undefined,
                    minHourlyRate: minHourlyRate ? Number(minHourlyRate) : undefined,
                    maxHourlyRate: maxHourlyRate ? Number(maxHourlyRate) : undefined,
                    page: 1,
                    pageSize: 20,
                },
            });
            return;
        }

        const payload: SemanticCandidateSearchRequest = {
            jobDescription: jobDescription.trim(),
            location: location.trim() || undefined,
            experienceLevel: experience.trim() || undefined,
            limit: 20,
        };

        if (!payload.jobDescription) {
            toast.error(t('candidateSearch.toastEnterDesc'));
            return;
        }

        setSubmittedSearch({
            mode: 'semantic',
            summary: t('candidateSearch.resultsBySemantic'),
            payload,
        });
    };

    const handleReset = () => {
        setKeyword('');
        setSkillIdsText('');
        setLocation('');
        setExperience('');
        setMinHourlyRate('');
        setMaxHourlyRate('');
        setJobDescription('');
        setSubmittedSearch(null);
    };

    if (isWorkspaceLoading || jobsQuery.isLoading) {
        return <CompanyWorkspaceLoader />;
    }

    if (isMissingProfile) {
        return (
            <CompanyProfileRequiredState
                title={t('candidateSearch.profileRequired')}
                description={t('candidateSearch.profileRequiredDesc')}
                icon={<Users className="h-6 w-6" />}
            />
        );
    }

    if (hasFatalError || jobsQuery.error) {
        return (
            <CompanyWorkspaceErrorState
                title={t('candidateSearch.errorTitle')}
                description={t('candidateSearch.errorDesc')}
                icon={<Users className="h-6 w-6" />}
            />
        );
    }

    if (searchQuery.error) {
        const message = isAxiosError(searchQuery.error)
            ? searchQuery.error.response?.data?.message || t('candidateSearch.searchFailed')
            : t('candidateSearch.searchFailed');

        return (
            <div className="space-y-6">
                <SectionHeader
                    title={t('candidateSearch.title')}
                    description={t('candidateSearch.description')}
                />
                <CompanyWorkspaceErrorState
                    title={t('candidateSearch.errorTitle')}
                    description={message}
                    icon={<Users className="h-6 w-6" />}
                />
            </div>
        );
    }

    return (
        <div className="space-y-6">
            <SectionHeader
                title={t('candidateSearch.title')}
                description={t('candidateSearch.description')}
            />

            <div className="grid gap-6 xl:grid-cols-[0.95fr_1.05fr]">
                <Card className="p-6">
                    <h2 className="text-lg font-semibold text-gray-900">{t('candidateSearch.filterTitle')}</h2>
                    <div className="mt-4 flex flex-wrap gap-2">
                        {([
                            { key: 'job', label: t('candidateSearch.modeByJob'), icon: <BriefcaseBusiness className="h-4 w-4" /> },
                            { key: 'filters', label: t('candidateSearch.modeByFilter'), icon: <Search className="h-4 w-4" /> },
                            { key: 'semantic', label: t('candidateSearch.modeSemantic'), icon: <Sparkles className="h-4 w-4" /> },
                        ] as const).map((mode) => (
                            <Button
                                key={mode.key}
                                variant={searchMode === mode.key ? 'default' : 'outline'}
                                size="sm"
                                onClick={() => setSearchMode(mode.key)}
                            >
                                {mode.icon}
                                {mode.label}
                            </Button>
                        ))}
                    </div>

                    <div className="mt-6 space-y-5">
                        {searchMode === 'job' && (
                            <div>
                                <label className="mb-2 block text-sm font-medium text-gray-700">{t('candidateSearch.selectJobLabel')}</label>
                                <select
                                    value={selectedJobId}
                                    onChange={(event) => setSelectedJobId(event.target.value)}
                                    className="h-11 w-full rounded-lg border border-gray-300 bg-white px-4 text-sm text-gray-900 outline-none transition focus:border-[#00b14f] focus:ring-2 focus:ring-[#00b14f]/20"
                                >
                                    <option value="">{t('candidateSearch.selectJobPlaceholder')}</option>
                                    {(jobsQuery.data || []).map((job) => (
                                        <option key={job.id} value={job.id}>
                                            {job.title}
                                        </option>
                                    ))}
                                </select>
                            </div>
                        )}

                        {searchMode === 'filters' && (
                            <>
                                <Input
                                    label={t('candidateSearch.keywordLabel')}
                                    placeholder={t('candidateSearch.keywordsPlaceholder')}
                                    value={keyword}
                                    onChange={(event) => setKeyword(event.target.value)}
                                />
                                <Input
                                    label={t('candidateSearch.skillsLabel')}
                                    placeholder={t('candidateSearch.skillsPlaceholder')}
                                    value={skillIdsText}
                                    onChange={(event) => setSkillIdsText(event.target.value)}
                                />
                            </>
                        )}

                        {searchMode === 'semantic' && (
                            <div>
                                <label className="mb-2 block text-sm font-medium text-gray-700">{t('candidateSearch.descriptionLabel')}</label>
                                <textarea
                                    rows={8}
                                    value={jobDescription}
                                    onChange={(event) => setJobDescription(event.target.value)}
                                    placeholder={t('candidateSearch.descriptionPlaceholder')}
                                    className="w-full rounded-lg border border-gray-300 px-4 py-3 text-sm text-gray-900 outline-none transition focus:border-[#00b14f] focus:ring-2 focus:ring-[#00b14f]/20"
                                />
                            </div>
                        )}

                        <Input
                            label={t('candidateSearch.locationLabel')}
                            placeholder={t('candidateSearch.locationPlaceholder')}
                            value={location}
                            onChange={(event) => setLocation(event.target.value)}
                        />

                        <Input
                            label={t('candidateSearch.levelLabel')}
                            placeholder={t('candidateSearch.levelPlaceholder')}
                            value={experience}
                            onChange={(event) => setExperience(event.target.value)}
                        />

                        {searchMode === 'filters' && (
                            <div className="grid gap-4 md:grid-cols-2">
                                <Input
                                    label={t('candidateSearch.minSalaryLabel')}
                                    type="number"
                                    placeholder="100000"
                                    value={minHourlyRate}
                                    onChange={(event) => setMinHourlyRate(event.target.value)}
                                />
                                <Input
                                    label={t('candidateSearch.maxSalaryLabel')}
                                    type="number"
                                    placeholder="500000"
                                    value={maxHourlyRate}
                                    onChange={(event) => setMaxHourlyRate(event.target.value)}
                                />
                            </div>
                        )}

                        <div className="flex flex-wrap gap-3 border-t border-gray-100 pt-4">
                            <Button onClick={handleSearch} isLoading={searchQuery.isFetching}>
                                {t('candidateSearch.btnSearch')}
                            </Button>
                            <Button variant="outline" onClick={handleReset}>
                                {t('candidateSearch.btnReset')}
                            </Button>
                        </div>
                    </div>
                </Card>

                <Card className="p-6">
                    <div className="flex items-center justify-between gap-3">
                        <div>
                            <h2 className="text-lg font-semibold text-gray-900">{t('candidateSearch.resultsTitle')}</h2>
                            <p className="mt-1 text-sm text-gray-500">
                                {submittedSearch?.summary || t('candidateSearch.notSearched')}
                            </p>
                        </div>
                        <span className="rounded-full bg-gray-100 px-3 py-1 text-xs font-semibold text-gray-700">
                            {t('candidateSearch.resultCount', { count: results.length })}
                        </span>
                    </div>

                    {searchQuery.isLoading && !searchQuery.data ? (
                        <div className="mt-6 space-y-3">
                            {[1, 2, 3].map((item) => (
                                <div key={item} className="h-28 animate-pulse rounded-xl bg-gray-100" />
                            ))}
                        </div>
                    ) : !submittedSearch ? (
                        <div className="mt-6">
                            <EmptyState
                                title={t('candidateSearch.emptyTitle')}
                                description={t('candidateSearch.emptyDesc')}
                                icon={<Users className="h-6 w-6" />}
                            />
                        </div>
                    ) : results.length === 0 ? (
                        <div className="mt-6">
                            <EmptyState
                                title={t('candidateSearch.emptyNoMatch')}
                                description={t('candidateSearch.emptyNoMatchDesc')}
                                icon={<Users className="h-6 w-6" />}
                            />
                        </div>
                    ) : (
                                <div className="mt-6 space-y-4">
                            {results.map((candidate) => (
                                <CandidateSearchResultCard
                                    key={candidate.userId}
                                    candidate={candidate}
                                    isUnlocked={unlockedIds.has(candidate.userId)}
                                    isUnlocking={unlockingCandidateId === candidate.userId && unlockMutation.isPending}
                                    onUnlock={
                                        unlockedIds.has(candidate.userId)
                                            ? undefined
                                            : () => {
                                                  setUnlockingCandidateId(candidate.userId);
                                                  unlockMutation.mutate(candidate.userId);
                                              }
                                    }
                                />
                            ))}
                        </div>
                    )}
                </Card>
            </div>
        </div>
    );
}
