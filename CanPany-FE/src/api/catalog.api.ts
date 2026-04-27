import apiClient from './axios.config';
import type { ApiResponse } from '../types';

export interface Skill {
    id: string;
    name: string;
}

export interface Category {
    id: string;
    name: string;
    description?: string;
}

export const catalogApi = {
    getSkills: async (): Promise<Skill[]> => {
        const response = await apiClient.get<ApiResponse<Skill[]>>('/catalog/skills');
        return response.data.data || [];
    },

    getCategories: async (): Promise<Category[]> => {
        const response = await apiClient.get<ApiResponse<Category[]>>('/catalog/categories');
        return response.data.data || [];
    },

    getLocations: async (): Promise<string[]> => {
        const response = await apiClient.get<ApiResponse<string[]>>('/catalog/locations');
        return response.data.data || [];
    },

    getExperienceLevels: async (): Promise<string[]> => {
        const response = await apiClient.get<ApiResponse<string[]>>('/catalog/experience-levels');
        return response.data.data || [];
    },
};
