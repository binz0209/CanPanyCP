import apiClient from './axios.config';
import type { ApiResponse, Application, CreateApplicationRequest } from '../types';

/**
 * Application API for Candidates
 * Based on ApplicationsController endpoints:
 * - UC-CAN-18: Submit Job Application (Proposal)
 * - UC-CAN-20: View Application History
 * - UC-CAN-21: View Application Status
 * - UC-CAN-22: Withdraw Application
 */
export const applicationsApi = {
    /**
     * UC-CAN-18: Submit Job Application (Proposal)
     * POST /api/applications
     * 
     * @param request - Application details including jobId, cvId, coverLetter, expectedSalary
     * @returns Created application object
     */
    create: async (request: CreateApplicationRequest): Promise<Application> => {
        const response = await apiClient.post<ApiResponse<Application>>('/applications', request);
        return response.data.data!;
    },

    /**
     * UC-CAN-18: Submit job application with full details
     * POST /api/applications
     * 
     * @param jobId - Job ID to apply for
     * @param cvId - CV ID to use (optional, uses default if not provided)
     * @param coverLetter - Cover letter content
     * @param expectedSalary - Expected salary
     * @returns Created application object
     */
    apply: async (
        jobId: string,
        cvId?: string,
        coverLetter?: string,
        expectedSalary?: number
    ): Promise<Application> => {
        const request: CreateApplicationRequest = {
            jobId,
            cvId,
            coverLetter,
            expectedSalary
        };
        return applicationsApi.create(request);
    },

    /**
     * UC-CAN-20: View Application History
     * GET /api/applications/my-applications
     * 
     * @returns Array of all applications submitted by the current user
     */
    getMyApplications: async (): Promise<Application[]> => {
        const response = await apiClient.get<ApiResponse<Application[]>>('/applications/my-applications');
        return response.data.data || [];
    },

    /**
     * UC-CAN-20: View Application History with status filter
     * GET /api/applications/my-applications?status={status}
     * 
     * @param status - Filter by application status (Pending, Accepted, Rejected, Withdrawn)
     * @returns Array of filtered applications
     */
    getMyApplicationsByStatus: async (status: string): Promise<Application[]> => {
        const response = await apiClient.get<ApiResponse<Application[]>>('/applications/my-applications', {
            params: { status }
        });
        return response.data.data || [];
    },

    /**
     * UC-CAN-21: View Application Status
     * GET /api/applications/{id}
     * 
     * @param id - Application ID
     * @returns Application details
     */
    getById: async (id: string): Promise<Application> => {
        const response = await apiClient.get<ApiResponse<Application>>(`/applications/${id}`);
        return response.data.data!;
    },

    /**
     * UC-CAN-22: Withdraw Application
     * PUT /api/applications/{id}/withdraw
     * 
     * @param id - Application ID to withdraw
     */
    withdraw: async (id: string): Promise<void> => {
        await apiClient.put(`/applications/${id}/withdraw`);
    },

    /**
     * Check if user has already applied to a job
     * This is an internal helper that checks the application list
     * 
     * @param jobId - Job ID to check
     * @returns Boolean indicating if user has applied
     */
    hasApplied: async (jobId: string): Promise<boolean> => {
        const applications = await applicationsApi.getMyApplications();
        return applications.some(app => app.jobId === jobId);
    },

    /**
     * Get application by job ID
     * Returns the application for a specific job if it exists
     * 
     * @param jobId - Job ID to find application for
     * @returns Application object or null if not found
     */
    getByJobId: async (jobId: string): Promise<Application | null> => {
        const applications = await applicationsApi.getMyApplications();
        return applications.find(app => app.jobId === jobId) || null;
    },

    /**
     * Get application statistics
     * Returns counts of applications by status
     * 
     * @returns Object with counts of applications by status
     */
    getStatistics: async (): Promise<{
        total: number;
        pending: number;
        accepted: number;
        rejected: number;
        withdrawn: number;
    }> => {
        const applications = await applicationsApi.getMyApplications();
        return {
            total: applications.length,
            pending: applications.filter(a => a.status === 'Pending').length,
            accepted: applications.filter(a => a.status === 'Accepted').length,
            rejected: applications.filter(a => a.status === 'Rejected').length,
            withdrawn: applications.filter(a => a.status === 'Withdrawn').length
        };
    },
};
