import apiClient from './axios.config';
import type { ApiResponse, CV, UploadCVRequest, UpdateCVRequest } from '../types';

// ─── Structured CV types (editable editor) ────────────────────────────────────
export interface CVExperienceEntry {
    company: string;
    role: string;
    period: string;
    bullets: string[];
}

export interface CVEducationEntry {
    institution: string;
    degree: string;
    period: string;
    notes?: string;
}

export interface CVStructuredData {
    fullName: string;
    title: string;
    email: string;
    phone: string;
    location: string;
    linkedIn: string;
    gitHub: string;
    portfolio: string;
    summary: string;
    experience: CVExperienceEntry[];
    education: CVEducationEntry[];
    skills: string[];
    languages: string[];
    certifications: string[];
    targetJobTitle?: string;
}

export const cvApi = {
    /** GET /api/cvs */
    getCVs: async (): Promise<CV[]> => {
        const response = await apiClient.get<ApiResponse<CV[]>>('/cvs');
        return response.data.data || [];
    },

    /** GET /api/cvs/{id} */
    getCV: async (id: string): Promise<CV> => {
        const response = await apiClient.get<ApiResponse<CV>>(`/cvs/${id}`);
        return response.data.data!;
    },

    /** POST /api/cvs/upload  (upload file) */
    uploadCV: async (request: UploadCVRequest): Promise<CV> => {
        const formData = new FormData();
        formData.append('file', request.file);
        if (request.isDefault !== undefined) formData.append('isDefault', request.isDefault.toString());
        const response = await apiClient.post<ApiResponse<CV>>('/cvs/upload', formData, {
            headers: { 'Content-Type': 'multipart/form-data' },
        });
        return response.data.data!;
    },

    /** PUT /api/cvs/{id}  (rename) */
    updateCV: async (id: string, request: UpdateCVRequest): Promise<void> => {
        await apiClient.put(`/cvs/${id}`, request);
    },

    /** DELETE /api/cvs/{id} */
    deleteCV: async (id: string): Promise<void> => {
        await apiClient.delete(`/cvs/${id}`);
    },

    /** PUT /api/cvs/{id}/set-default */
    setDefaultCV: async (id: string): Promise<void> => {
        await apiClient.put(`/cvs/${id}/set-default`);
    },

    /** POST /api/cvs/{id}/analyze */
    analyzeCV: async (id: string): Promise<{ jobId: string }> => {
        const response = await apiClient.post<ApiResponse<{ jobId: string }>>(`/cvs/${id}/analyze`);
        return response.data.data!;
    },

    /**
     * POST /api/cvs/generate?targetJobId=...
     * Returns jobId to poll progress; CVId is in result when done
     */
    generateCV: async (targetJobId?: string): Promise<{ jobId: string }> => {
        const params = targetJobId ? { targetJobId } : undefined;
        const response = await apiClient.post<ApiResponse<{ jobId: string; JobId: string }>>('/cvs/generate', null, { params });
        const data = response.data.data!;
        return { jobId: data.jobId || data.JobId };
    },

    /** GET /api/cvs/{id}/data — load structured CV for editor */
    getCVData: async (id: string): Promise<CVStructuredData> => {
        const response = await apiClient.get<ApiResponse<CVStructuredData>>(`/cvs/${id}/data`);
        return response.data.data!;
    },

    /** PUT /api/cvs/{id}/data — save edited CV data */
    updateCVData: async (id: string, data: CVStructuredData): Promise<void> => {
        await apiClient.put(`/cvs/${id}/data`, data);
    },

    /** GET /api/cvs/{id}/versions — get version history for a CV */
    getCVVersions: async (id: string): Promise<CV[]> => {
        const response = await apiClient.get<ApiResponse<CV[]>>(`/cvs/${id}/versions`);
        return response.data.data || [];
    },

    /** POST /api/cvs/{id}/save-version — snapshot current CV as a new version */
    saveCVVersion: async (id: string, versionNote?: string): Promise<CV> => {
        const response = await apiClient.post<ApiResponse<CV>>(`/cvs/${id}/save-version`, { versionNote });
        return response.data.data!;
    },
};
