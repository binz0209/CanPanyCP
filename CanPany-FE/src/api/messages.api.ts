import apiClient from './axios.config';
import type { ApiResponse } from '../types';

export interface Message {
    id: string;
    conversationId: string;
    senderId: string;
    text: string;
    isRead: boolean;
    readAt?: string;
    createdAt: string;
}

export const messagesApi = {
    getMessages: async (conversationId: string, page = 1, pageSize = 50): Promise<Message[]> => {
        const response = await apiClient.get<ApiResponse<Message[]>>('/messages', {
            params: { conversationId, page, pageSize },
        });
        const msgs = response.data.data || [];
        // The backend returns messages in descending order (newest first) for pagination.
        // We reverse them here so the chat UI naturally displays from oldest (top) to newest (bottom).
        return msgs.reverse();
    },

    sendMessage: async (conversationId: string, text: string): Promise<Message> => {
        const response = await apiClient.post<ApiResponse<Message>>('/messages', {
            conversationId,
            text,
        });
        return response.data.data!;
    },

    // Marks all messages in the conversation (not sent by current user) as read.
    markConversationAsRead: async (conversationId: string): Promise<void> => {
        await apiClient.put(`/messages/conversations/${conversationId}/read`);
    },
};
