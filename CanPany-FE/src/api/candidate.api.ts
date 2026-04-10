import apiClient from './axios.config';
import type { ApiResponse, User, UserProfile } from '../types';
import type { CV } from '../types/cv.types';

export interface GitHubRepo {
    name: string;
    fullName: string;
    description?: string;
    language?: string;
    stars: number;
    forks: number;
    htmlUrl: string;
    isFork: boolean;
    updatedAt: string;
}

export interface GitHubReposData {
    gitHubUsername: string;
    totalCount: number;
    repositories: GitHubRepo[];
}

export interface GitHubJobStatus {
    jobId: string;
    status: 'Pending' | 'Running' | 'Completed' | 'Failed' | 'Retrying';
    percentComplete: number;
    currentStep?: string;
    startedAt?: string;
    completedAt?: string;
}

export interface GitHubSyncResult {
    jobId: string;
    gitHubUsername: string;
    selectedRepos: string[];
    message: string;
}

export interface RecommendationSyncResult {
    jobId: string;
    limit: number;
    message: string;
}

export interface AvatarUploadResult {
    url: string;
    publicId: string;
}

export type JobStatus = 'Pending' | 'Running' | 'Completed' | 'Failed' | 'Cancelled' | 'Retrying';

export interface JobProgressRecord {
    jobId: string;
    userId?: string;
    jobType?: string;
    jobTitle?: string;
    status: JobStatus;
    percentComplete: number;
    currentStep?: string;
    totalSteps: number;
    completedSteps: number;
    details?: Record<string, any>;
    result?: Record<string, any>;
    errorMessage?: string;
    startedAt?: string;
    completedAt?: string;
    durationMs?: number;
    updatedAt: string;
}

export interface MyJobsResponse {
    total: number;
    skip: number;
    take: number;
    jobs: JobProgressRecord[];
}

export interface CandidatePublicInfo {
    id: string;
    fullName: string;
    avatarUrl?: string;
    profile?: {
        title?: string;
        bio?: string;
        location?: string;
        hourlyRate?: number;
        skills: string[];
    };
}

export interface CandidateFullProfile {
    user: {
        id: string;
        fullName: string;
        email: string;
        avatarUrl?: string;
        role: string;
    };
    profile: UserProfile | null;
    cVs: {
        id: string;
        fileName: string;
        isDefault: boolean;
        extractedSkills: string[];
    }[];
}

export interface CandidateStatistics {
    totalApplications: number;
    pendingApplications: number;
    acceptedApplications: number;
    rejectedApplications: number;
    totalCVs: number;
    defaultCV?: CV | null;
    profileCompleteness: number;
    skillsCount: number;
}

export interface CandidateSearchResult {
    profile: UserProfile;
    matchScore: number;
    userInfo?: {
        id: string;
        fullName: string;
        email?: string;
        avatarUrl?: string;
        role?: string;
    };
}

export interface CandidateCVSummary {
    id: string;
    fileName: string;
    isDefault: boolean;
    latestAnalysisId?: string;
    extractedSkills?: string[];
}

export interface SemanticCandidateSearchRequest {
    jobDescription: string;
    location?: string;
    experienceLevel?: string;
    limit?: number;
}

export interface UnlockedCandidate {
    user: User;
    profile: UserProfile;
}

export const candidateApi = {
    // Get candidate public profile
    getCandidate: async (id: string): Promise<CandidatePublicInfo> => {
        const response = await apiClient.get<ApiResponse<CandidatePublicInfo>>(`/candidates/${id}`);
        return response.data.data!;
    },

    // Get candidate full profile (authenticated)
    getCandidateProfile: async (id: string): Promise<CandidateFullProfile> => {
        const response = await apiClient.get<ApiResponse<CandidateFullProfile>>(`/candidates/${id}/profile`);
        return response.data.data!;
    },

    // Get candidate statistics
    getCandidateStatistics: async (id: string): Promise<CandidateStatistics> => {
        const response = await apiClient.get<ApiResponse<CandidateStatistics>>(`/candidates/${id}/statistics`);
        return response.data.data!;
    },

    // Get candidate applications
    getCandidateApplications: async (id: string, status?: string): Promise<any[]> => {
        const response = await apiClient.get<ApiResponse<any[]>>(`/candidates/${id}/applications`, {
            params: status ? { status } : undefined,
        });
        return response.data.data || [];
    },

    // Unlock candidate contact info
    unlockCandidate: async (candidateId: string): Promise<void> => {
        await apiClient.post(`/candidates/${candidateId}/unlock`);
    },

    // Get unlocked candidates
    getUnlockedCandidates: async (page = 1, pageSize = 20): Promise<UnlockedCandidate[]> => {
        const response = await apiClient.get<ApiResponse<UnlockedCandidate[]>>(`/candidates/unlocked`, {
            params: { page, pageSize },
        });
        return response.data.data || [];
    },

    // Search candidates by job
    searchCandidates: async (jobId: string, limit = 20): Promise<CandidateSearchResult[]> => {
        const response = await apiClient.get<ApiResponse<CandidateSearchResult[]>>(`/candidates/search`, {
            params: { jobId, limit },
        });
        return response.data.data || [];
    },

    // Search candidates with filters
    searchCandidatesWithFilters: async (params: {
        keyword?: string;
        skillIds?: string[];
        location?: string;
        experience?: string;
        minHourlyRate?: number;
        maxHourlyRate?: number;
        page?: number;
        pageSize?: number;
    }): Promise<CandidateSearchResult[]> => {
        const response = await apiClient.get<ApiResponse<CandidateSearchResult[]>>(`/candidates/search/filters`, {
            params,
        });
        return response.data.data || [];
    },

    semanticSearchCandidates: async (payload: SemanticCandidateSearchRequest): Promise<CandidateSearchResult[]> => {
        const response = await apiClient.post<ApiResponse<CandidateSearchResult[]>>('/candidates/semantic-search', payload);
        return response.data.data || [];
    },

    // Get candidate CVs
    getCandidateCVs: async (id: string): Promise<CandidateCVSummary[]> => {
        const response = await apiClient.get<ApiResponse<CandidateCVSummary[]>>(`/candidates/${id}/cvs`);
        return response.data.data || [];
    },

    // Update candidate profile
    updateProfile: async (data: Partial<UserProfile>): Promise<void> => {
        await apiClient.put('/userprofiles/me', data);
    },

    // Sync profile from LinkedIn (paste-data approach)
    syncLinkedInProfile: async (linkedInData: string): Promise<void> => {
        await apiClient.post('/userprofiles/sync/linkedin', { linkedInData });
    },

    // Upload candidate avatar
    uploadAvatar: async (file: File): Promise<AvatarUploadResult> => {
        const formData = new FormData();
        formData.append('file', file);

        const response = await apiClient.post<ApiResponse<{ Url?: string; PublicId?: string; url?: string; publicId?: string }>>(
            '/userprofiles/avatar',
            formData,
            {
                headers: { 'Content-Type': 'multipart/form-data' },
            }
        );

        const data = response.data.data;
        return {
            url: data?.url ?? data?.Url ?? '',
            publicId: data?.publicId ?? data?.PublicId ?? '',
        };
    },

    // Get list of repos from the GitHub account linked in the user profile
    getGitHubRepos: async (includeForked = false): Promise<GitHubReposData> => {
        const response = await apiClient.get<ApiResponse<GitHubReposData>>('/github/repos', {
            params: { includeForked },
        });
        return response.data.data!;
    },

    // Start Gemini skill-extraction job on selected repos
    syncSkillsFromRepos: async (repositoryNames: string[]): Promise<GitHubSyncResult> => {
        const response = await apiClient.post<ApiResponse<GitHubSyncResult>>('/github/sync-skills', {
            RepositoryNames: repositoryNames,
        });
        return response.data.data!;
    },

    // Trigger background sync for recommendation skills (Gemini + profile/CV/GitHub aggregation)
    syncRecommendationSkills: async (limit = 20): Promise<RecommendationSyncResult> => {
        const response = await apiClient.post<ApiResponse<RecommendationSyncResult>>('/jobs/recommended/sync-skills', null, {
            params: { limit },
        });
        return response.data.data!;
    },

    // Poll background job status
    getGitHubJobStatus: async (jobId: string): Promise<GitHubJobStatus> => {
        const response = await apiClient.get<GitHubJobStatus>(`/github/status/${jobId}`);
        return response.data;
    },

    // Fetch latest analysis result
    getLatestGitHubAnalysis: async (): Promise<any> => {
        const response = await apiClient.get('/github/analysis/latest');
        return response.data;
    },

    // Get list of all background jobs for the current user (newest first)
    getMyJobs: async (skip = 0, take = 20): Promise<MyJobsResponse> => {
        const response = await apiClient.get<MyJobsResponse>('/background-jobs/my-jobs', {
            params: { skip, take },
        });
        return response.data;
    },

    // Get detail of a specific job
    getMyJobDetail: async (jobId: string): Promise<JobProgressRecord> => {
        const response = await apiClient.get<JobProgressRecord>(`/background-jobs/my-jobs/${jobId}`);
        return response.data;
    },
};