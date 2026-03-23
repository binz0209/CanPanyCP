import { useEffect, useRef, useState, useCallback } from 'react';
import {
    HubConnectionBuilder,
    HubConnection,
    HubConnectionState,
    LogLevel,
} from '@microsoft/signalr';
import { useAuthStore } from '../stores/auth.store';

const HUB_URL = (import.meta.env.VITE_API_URL || 'http://localhost:5001/api')
    .replace(/\/api$/, '/hubs/chat');

export interface ReceivedMessage {
    id: string;
    conversationId: string;
    senderId: string;
    text: string;
    isRead: boolean;
    createdAt: string;
}

export interface ConversationUpdate {
    conversationId: string;
    lastMessagePreview: string;
    lastMessageAt: string;
    senderId: string;
}

export interface MessageReadEvent {
    conversationId: string;
    readByUserId: string;
    count: number;
}

export interface TypingEvent {
    conversationId: string;
    userId: string;
}

interface UseSignalROptions {
    onReceiveMessage?: (msg: ReceivedMessage) => void;
    onConversationUpdated?: (update: ConversationUpdate) => void;
    onMessageRead?: (event: MessageReadEvent) => void;
    onUserTyping?: (event: TypingEvent) => void;
}

export function useSignalR(options: UseSignalROptions = {}) {
    const { token } = useAuthStore();
    const connectionRef = useRef<HubConnection | null>(null);
    const [isConnected, setIsConnected] = useState(false);
    const optionsRef = useRef(options);
    optionsRef.current = options;

    // Build and manage connection lifecycle
    useEffect(() => {
        if (!token) return;

        const connection = new HubConnectionBuilder()
            .withUrl(HUB_URL, { accessTokenFactory: () => token })
            .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
            .configureLogging(LogLevel.Warning)
            .build();

        connectionRef.current = connection;

        // Register event handlers
        connection.on('ReceiveMessage', (msg: ReceivedMessage) => {
            optionsRef.current.onReceiveMessage?.(msg);
        });

        connection.on('ConversationUpdated', (update: ConversationUpdate) => {
            optionsRef.current.onConversationUpdated?.(update);
        });

        connection.on('MessageRead', (event: MessageReadEvent) => {
            optionsRef.current.onMessageRead?.(event);
        });

        connection.on('UserTyping', (event: TypingEvent) => {
            optionsRef.current.onUserTyping?.(event);
        });

        connection.onclose(() => setIsConnected(false));
        connection.onreconnected(() => setIsConnected(true));
        connection.onreconnecting(() => setIsConnected(false));

        connection
            .start()
            .then(() => setIsConnected(true))
            .catch((err) => console.error('SignalR connection failed:', err));

        return () => {
            connection.stop();
            connectionRef.current = null;
            setIsConnected(false);
        };
    }, [token]);

    // ─── Hub invocations ──────────────────────────────────────────────────────

    const joinConversation = useCallback(async (conversationId: string) => {
        const conn = connectionRef.current;
        if (conn?.state === HubConnectionState.Connected) {
            await conn.invoke('JoinConversation', conversationId);
        }
    }, []);

    const leaveConversation = useCallback(async (conversationId: string) => {
        const conn = connectionRef.current;
        if (conn?.state === HubConnectionState.Connected) {
            await conn.invoke('LeaveConversation', conversationId);
        }
    }, []);

    const sendMessage = useCallback(async (conversationId: string, text: string) => {
        const conn = connectionRef.current;
        if (conn?.state === HubConnectionState.Connected) {
            await conn.invoke('SendMessage', conversationId, text);
        }
    }, []);

    const markAsRead = useCallback(async (conversationId: string) => {
        const conn = connectionRef.current;
        if (conn?.state === HubConnectionState.Connected) {
            await conn.invoke('MarkAsRead', conversationId);
        }
    }, []);

    const sendTyping = useCallback(async (conversationId: string) => {
        const conn = connectionRef.current;
        if (conn?.state === HubConnectionState.Connected) {
            await conn.invoke('Typing', conversationId);
        }
    }, []);

    return {
        isConnected,
        joinConversation,
        leaveConversation,
        sendMessage,
        markAsRead,
        sendTyping,
    };
}
