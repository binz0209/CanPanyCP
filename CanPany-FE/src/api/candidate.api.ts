import apiClient from './axios.config';
import type { ApiResponse, User, UserProfile } from '../types';

interface CandidatePublicInfo {
    Id: string;
    FullName: string;
    AvatarUrl?: string;
    Profile?: {
        Title?: string;
        Bio?: string;
        Location?: string;
        HourlyRate?: number;
        Skills: string[];
    };
}

interface CandidateFullProfile {
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

interface CandidateStatistics {
    TotalApplications: number;
    PendingApplications: number;
    AcceptedApplications: number;
    RejectedApplications: number;
    TotalCVs: number;
    DefaultCV?: any;
    ProfileCompleteness: number;
    SkillsCount: number;
}

interface CandidateSearchResult {
    Profile: UserProfile;
    MatchScore: number;
}

interface UnlockedCandidate {
    User: User;
    Profile: UserProfile;
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

    // Get candidate CVs
    getCandidateCVs: async (id: string): Promise<any[]> => {
        const response = await apiClient.get<ApiResponse<any[]>>(`/candidates/${id}/cvs`);
        return response.data.data || [];
    },

    // Update candidate profile
    updateProfile: async (data: Partial<UserProfile>): Promise<void> => {
        await apiClient.put('/userprofiles/me', data);
    },
};