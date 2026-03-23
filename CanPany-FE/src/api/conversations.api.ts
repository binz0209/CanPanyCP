import apiClient from './axios.config';
import type { ApiResponse } from '../types';

export interface Conversation {
    id: string;
    participantIds: string[];
    otherUserName: string;
    otherUserAvatar?: string;
    jobId?: string;
    lastMessageAt?: string;
    lastMessagePreview?: string;
    unreadCount: number;
    createdAt: string;
}

export const conversationsApi = {
    /** List all conversations for the current user (paginated). */
    getConversations: async (page = 1, pageSize = 20): Promise<Conversation[]> => {
        const response = await apiClient.get<ApiResponse<Conversation[]>>('/conversations', {
            params: { page, pageSize },
        });
        return response.data.data || [];
    },

    /** Get or create a conversation with another user. */
    getOrCreateConversation: async (
        otherUserId: string,
        jobId?: string
    ): Promise<Conversation> => {
        const response = await apiClient.post<ApiResponse<Conversation>>('/conversations', {
            otherUserId,
            jobId,
        });
        return response.data.data!;
    },

    /** Get a single conversation by ID. */
    getConversation: async (id: string): Promise<Conversation> => {
        const response = await apiClient.get<ApiResponse<Conversation>>(`/conversations/${id}`);
        return response.data.data!;
    },

    /** Get the total unread message count across all conversations. */
    getUnreadCount: async (): Promise<number> => {
        const response = await apiClient.get<ApiResponse<number>>('/conversations/unread-count');
        return response.data.data ?? 0;
    },
};
