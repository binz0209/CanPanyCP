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
                    fullName: candidateProfile?.user.fullName || 'Đang tải thông tin ứng viên',
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
            toast.success('Đã mở khóa liên hệ ứng viên');
        },
        onError: (error) => {
            const message = isAxiosError(error)
                ? error.response?.data?.message || 'Không thể mở khóa liên hệ ứng viên'
                : 'Không thể mở khóa liên hệ ứng viên';
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
                toast.error('Vui lòng chọn job để tìm ứng viên');
                return;
            }

            setSubmittedSearch({
                mode: 'job',
                selectedJobId,
                summary: selectedJobTitle
                    ? `Kết quả matching theo job: ${selectedJobTitle}`
                    : 'Kết quả matching theo job đang chọn',
            });
            return;
        }

        if (searchMode === 'filters') {
            setSubmittedSearch({
                mode: 'filters',
                summary: 'Kết quả tìm kiếm theo bộ lọc',
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
            toast.error('Vui lòng nhập mô tả công việc để semantic search');
            return;
        }

        setSubmittedSearch({
            mode: 'semantic',
            summary: 'Kết quả semantic search theo mô tả công việc',
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
                title="Bạn chưa có hồ sơ công ty"
                description="Hãy hoàn thiện hồ sơ công ty trước khi bắt đầu tìm kiếm ứng viên."
                icon={<Users className="h-6 w-6" />}
            />
        );
    }

    if (hasFatalError || jobsQuery.error) {
        return (
            <CompanyWorkspaceErrorState
                title="Không thể tải dữ liệu tìm kiếm ứng viên"
                description="Vui lòng kiểm tra lại API backend hoặc tài khoản company của bạn."
                icon={<Users className="h-6 w-6" />}
            />
        );
    }

    if (searchQuery.error) {
        const message = isAxiosError(searchQuery.error)
            ? searchQuery.error.response?.data?.message || 'Không thể tìm ứng viên'
            : 'Không thể tìm ứng viên';

        return (
            <div className="space-y-6">
                <SectionHeader
                    title="Tìm kiếm ứng viên"
                    description="Tìm ứng viên theo job đang tuyển, theo bộ lọc hoặc bằng semantic search dựa trên mô tả công việc. Kết quả hiển thị match score để bạn shortlist nhanh hơn."
                />
                <CompanyWorkspaceErrorState
                    title="Tìm kiếm ứng viên thất bại"
                    description={message}
                    icon={<Users className="h-6 w-6" />}
                />
            </div>
        );
    }

    return (
        <div className="space-y-6">
            <SectionHeader
                title="Tìm kiếm ứng viên"
                description="Khai thác nhiều cách tìm kiếm (theo job, theo bộ lọc, semantic search) để nhanh chóng tìm được hồ sơ phù hợp nhất với nhu cầu tuyển dụng."
            />

            <div className="grid gap-6 xl:grid-cols-[0.95fr_1.05fr]">
                <Card className="p-6">
                    <h2 className="text-lg font-semibold text-gray-900">Bộ điều khiển tìm kiếm</h2>
                    <div className="mt-4 flex flex-wrap gap-2">
                        {([
                            { key: 'job', label: 'Theo job', icon: <BriefcaseBusiness className="h-4 w-4" /> },
                            { key: 'filters', label: 'Theo bộ lọc', icon: <Search className="h-4 w-4" /> },
                            { key: 'semantic', label: 'Semantic', icon: <Sparkles className="h-4 w-4" /> },
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
                                <label className="mb-2 block text-sm font-medium text-gray-700">Chọn job để matching</label>
                                <select
                                    value={selectedJobId}
                                    onChange={(event) => setSelectedJobId(event.target.value)}
                                    className="h-11 w-full rounded-lg border border-gray-300 bg-white px-4 text-sm text-gray-900 outline-none transition focus:border-[#00b14f] focus:ring-2 focus:ring-[#00b14f]/20"
                                >
                                    <option value="">Chọn tin tuyển dụng</option>
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
                                    label="Keyword"
                                    placeholder="React, .NET, Designer..."
                                    value={keyword}
                                    onChange={(event) => setKeyword(event.target.value)}
                                />
                                <Input
                                    label="Skills"
                                    placeholder="React, TypeScript, Tailwind"
                                    value={skillIdsText}
                                    onChange={(event) => setSkillIdsText(event.target.value)}
                                />
                            </>
                        )}

                        {searchMode === 'semantic' && (
                            <div>
                                <label className="mb-2 block text-sm font-medium text-gray-700">Mô tả công việc</label>
                                <textarea
                                    rows={8}
                                    value={jobDescription}
                                    onChange={(event) => setJobDescription(event.target.value)}
                                    placeholder="Mô tả chân dung ứng viên bạn đang tìm, kỹ năng chính, level, domain..."
                                    className="w-full rounded-lg border border-gray-300 px-4 py-3 text-sm text-gray-900 outline-none transition focus:border-[#00b14f] focus:ring-2 focus:ring-[#00b14f]/20"
                                />
                            </div>
                        )}

                        <Input
                            label="Location"
                            placeholder="Ho Chi Minh City"
                            value={location}
                            onChange={(event) => setLocation(event.target.value)}
                        />

                        <Input
                            label="Experience / Level"
                            placeholder="Junior, Mid, Senior..."
                            value={experience}
                            onChange={(event) => setExperience(event.target.value)}
                        />

                        {searchMode === 'filters' && (
                            <div className="grid gap-4 md:grid-cols-2">
                                <Input
                                    label="Min hourly rate"
                                    type="number"
                                    placeholder="100000"
                                    value={minHourlyRate}
                                    onChange={(event) => setMinHourlyRate(event.target.value)}
                                />
                                <Input
                                    label="Max hourly rate"
                                    type="number"
                                    placeholder="500000"
                                    value={maxHourlyRate}
                                    onChange={(event) => setMaxHourlyRate(event.target.value)}
                                />
                            </div>
                        )}

                        <div className="flex flex-wrap gap-3 border-t border-gray-100 pt-4">
                            <Button onClick={handleSearch} isLoading={searchQuery.isFetching}>
                                Tìm ứng viên
                            </Button>
                            <Button variant="outline" onClick={handleReset}>
                                Làm mới
                            </Button>
                        </div>
                    </div>
                </Card>

                <Card className="p-6">
                    <div className="flex items-center justify-between gap-3">
                        <div>
                            <h2 className="text-lg font-semibold text-gray-900">Kết quả tìm kiếm</h2>
                            <p className="mt-1 text-sm text-gray-500">
                                {submittedSearch?.summary || selectedJobTitle || 'Chưa thực hiện tìm kiếm'}
                            </p>
                        </div>
                        <span className="rounded-full bg-gray-100 px-3 py-1 text-xs font-semibold text-gray-700">
                            {results.length} ứng viên
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
                                title="Chưa có kết quả"
                                description="Thực hiện một truy vấn tìm kiếm để xem danh sách ứng viên phù hợp."
                                icon={<Users className="h-6 w-6" />}
                            />
                        </div>
                    ) : results.length === 0 ? (
                        <div className="mt-6">
                            <EmptyState
                                title="Không có ứng viên phù hợp"
                                description="Hãy thử nới lỏng bộ lọc hoặc đổi mô tả tìm kiếm để nhận thêm kết quả."
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
