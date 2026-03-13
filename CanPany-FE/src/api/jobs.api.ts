import apiClient from './axios.config';
import type {
    ApiResponse,
    CreateJobRequest,
    Job,
    JobDetailsResponse,
    JobListResponse,
    JobSearchParams,
    UpdateJobRequest,
} from '../types';

/**
 * Job Search API for Candidates
 * Based on JobsController endpoints:
 * - UC-CAN-13: Search Jobs
 * - UC-CAN-14: View Job Details
 * - UC-CAN-15: View AI-Recommended Jobs
 * - UC-CAN-16: Bookmark Job
 * - UC-CAN-17: Remove Bookmarked Job
 */
export const jobsApi = {
    /**
     * UC-CAN-13: Search Jobs with filters
     * GET /api/jobs
     * 
     * @param params - Search parameters including keyword, category, skills, budget, location, etc.
     * @returns Array of jobs matching the search criteria
     */
    search: async (params?: JobSearchParams): Promise<Job[]> => {
        const response = await apiClient.get<ApiResponse<Job[]>>('/jobs', { params });
        return response.data.data || [];
    },

    /**
     * UC-CAN-13: Search Jobs with pagination
     * GET /api/jobs with page parameters
     * 
     * @param params - Search parameters including pagination
     * @returns Paginated job list with total count
     */
    searchWithPagination: async (params?: JobSearchParams): Promise<JobListResponse> => {
        const response = await apiClient.get<ApiResponse<JobListResponse>>('/jobs', { params });
        return response.data.data || { jobs: [], total: 0, page: 1, pageSize: 20, totalPages: 0 };
    },

    /**
     * UC-CAN-14: View Job Details
     * GET /api/jobs/{id}
     * 
     * @param id - Job ID
     * @returns Job details with bookmark status
     */
    getById: async (id: string): Promise<JobDetailsResponse> => {
        const response = await apiClient.get<ApiResponse<JobDetailsResponse>>(`/jobs/${id}`);
        return response.data.data!;
    },

    /**
     * UC-CAN-15: View AI-Recommended Jobs
     * GET /api/jobs/recommended
     * 
     * @param limit - Maximum number of recommendations (default: 10)
     * @returns Array of AI-recommended jobs
     */
    getRecommended: async (limit: number = 10): Promise<Job[]> => {
        const response = await apiClient.get<ApiResponse<Job[]>>('/jobs/recommended', { 
            params: { limit } 
        });
        return response.data.data || [];
    },

    /**
     * UC-CAN-16: Bookmark Job
     * POST /api/jobs/{id}/bookmark
     * 
     * @param id - Job ID to bookmark
     */
    bookmark: async (id: string): Promise<void> => {
        await apiClient.post(`/jobs/${id}/bookmark`);
    },

    /**
     * UC-CAN-17: Remove Bookmarked Job
     * DELETE /api/jobs/{id}/bookmark
     * 
     * @param id - Job ID to remove from bookmarks
     */
    removeBookmark: async (id: string): Promise<void> => {
        await apiClient.delete(`/jobs/${id}/bookmark`);
    },

    /**
     * Get all bookmarked jobs
     * GET /api/jobs/bookmarked
     * 
     * @returns Array of bookmarked jobs
     */
    getBookmarked: async (): Promise<Job[]> => {
        const response = await apiClient.get<ApiResponse<Job[]>>('/jobs/bookmarked');
        return response.data.data || [];
    },

    /**
     * Get jobs by company
     * GET /api/jobs/company/{companyId}
     * 
     * @param companyId - Company ID
     * @returns Array of jobs posted by the company
     */
    getByCompany: async (companyId: string): Promise<Job[]> => {
        const response = await apiClient.get<ApiResponse<Job[]>>(`/jobs/company/${companyId}`);
        return response.data.data || [];
    },

    create: async (payload: CreateJobRequest): Promise<Job> => {
        const response = await apiClient.post<ApiResponse<Job>>('/jobs', payload);
        return response.data.data!;
    },

    update: async (jobId: string, payload: UpdateJobRequest): Promise<void> => {
        await apiClient.put(`/jobs/${jobId}`, payload);
    },

    close: async (jobId: string): Promise<void> => {
        await apiClient.put(`/jobs/${jobId}/close`);
    },

    reopen: async (jobId: string): Promise<void> => {
        await apiClient.put(`/jobs/${jobId}/reopen`);
    },

    /**
     * Get latest jobs (most recent)
     * GET /api/jobs with sortBy=createdAt&sortOrder=desc
     * 
     * @param limit - Maximum number of jobs to return
     * @returns Array of latest jobs
     */
    getLatest: async (limit: number = 10): Promise<Job[]> => {
        const response = await apiClient.get<ApiResponse<Job[]>>('/jobs', {
            params: {
                sortBy: 'createdAt',
                sortOrder: 'desc',
                pageSize: limit
            }
        });
        return response.data.data || [];
    },

    /**
     * Get jobs by location
     * GET /api/jobs with location filter
     * 
     * @param location - Location to filter by
     * @param params - Additional search parameters
     * @returns Array of jobs in the specified location
     */
    getByLocation: async (location: string, params?: Partial<JobSearchParams>): Promise<Job[]> => {
        const response = await apiClient.get<ApiResponse<Job[]>>('/jobs', {
            params: {
                location,
                ...params
            }
        });
        return response.data.data || [];
    },

    /**
     * Get jobs by salary range
     * GET /api/jobs with minBudget and maxBudget
     * 
     * @param minSalary - Minimum salary
     * @param maxSalary - Maximum salary
     * @param params - Additional search parameters
     * @returns Array of jobs within the salary range
     */
    getBySalaryRange: async (
        minSalary?: number, 
        maxSalary?: number, 
        params?: Partial<JobSearchParams>
    ): Promise<Job[]> => {
        const response = await apiClient.get<ApiResponse<Job[]>>('/jobs', {
            params: {
                minBudget: minSalary,
                maxBudget: maxSalary,
                ...params
            }
        });
        return response.data.data || [];
    },

    /**
     * Get remote jobs
     * GET /api/jobs with isRemote=true
     * 
     * @param params - Additional search parameters
     * @returns Array of remote jobs
     */
    getRemoteJobs: async (params?: Partial<JobSearchParams>): Promise<Job[]> => {
        const response = await apiClient.get<ApiResponse<Job[]>>('/jobs', {
            params: {
                isRemote: true,
                ...params
            }
        });
        return response.data.data || [];
    },

    /**
     * Get jobs by level
     * GET /api/jobs with level filter
     * 
     * @param level - Job level (Junior, Mid, Senior, Expert)
     * @param params - Additional search parameters
     * @returns Array of jobs at the specified level
     */
    getByLevel: async (
        level: string, 
        params?: Partial<JobSearchParams>
    ): Promise<Job[]> => {
        const response = await apiClient.get<ApiResponse<Job[]>>('/jobs', {
            params: {
                level,
                ...params
            }
        });
        return response.data.data || [];
    },

    /**
     * Get jobs by budget type
     * GET /api/jobs with budgetType filter
     * 
     * @param budgetType - Budget type (Fixed or Hourly)
     * @param params - Additional search parameters
     * @returns Array of jobs with the specified budget type
     */
    getByBudgetType: async (
        budgetType: 'Fixed' | 'Hourly', 
        params?: Partial<JobSearchParams>
    ): Promise<Job[]> => {
        const response = await apiClient.get<ApiResponse<Job[]>>('/jobs', {
            params: {
                budgetType,
                ...params
            }
        });
        return response.data.data || [];
    },
};
