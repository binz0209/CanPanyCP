import apiClient from './axios.config';
import type { ApiResponse, CV, UploadCVRequest, UpdateCVRequest } from '../types';

export const cvApi = {
    /**
     * UC-CAN-07: View CV List
     * GET /api/cvs
     */
    getCVs: async (): Promise<CV[]> => {
        const response = await apiClient.get<ApiResponse<CV[]>>('/cvs');
        return response.data.data || [];
    },

    /**
     * UC-CAN-08: View CV Details
     * GET /api/cvs/{id}
     */
    getCV: async (id: string): Promise<CV> => {
        const response = await apiClient.get<ApiResponse<CV>>(`/cvs/${id}`);
        return response.data.data!;
    },

    /**
     * UC-CAN-06: Upload CV
     * POST /api/cvs
     */
    uploadCV: async (request: UploadCVRequest): Promise<CV> => {
        const formData = new FormData();
        formData.append('File', request.file);
        if (request.isDefault !== undefined) {
            formData.append('IsDefault', request.isDefault.toString());
        }

        const response = await apiClient.post<ApiResponse<CV>>('/cvs', formData, {
            headers: {
                'Content-Type': 'multipart/form-data',
            },
        });
        return response.data.data!;
    },

    /**
     * UC-CAN-09: Update CV Name
     * PUT /api/cvs/{id}
     */
    updateCV: async (id: string, request: UpdateCVRequest): Promise<void> => {
        await apiClient.put(`/cvs/${id}`, request);
    },

    /**
     * UC-CAN-10: Delete CV
     * DELETE /api/cvs/{id}
     */
    deleteCV: async (id: string): Promise<void> => {
        await apiClient.delete(`/cvs/${id}`);
    },

    /**
     * UC-CAN-11: Set Default CV
     * PUT /api/cvs/{id}/set-default
     */
    setDefaultCV: async (id: string): Promise<void> => {
        await apiClient.put(`/cvs/${id}/set-default`);
    },

    /**
     * UC-CAN-12: Analyze CV via AI
     * POST /api/cvs/{id}/analyze
     */
    analyzeCV: async (id: string): Promise<{ jobId: string }> => {
        // The backend returns { JobId: "..." } inside data.data
        const response = await apiClient.post<ApiResponse<{ jobId: string }>>(`/cvs/${id}/analyze`);
        return response.data.data!;
    },
};
