import { useEffect, useRef } from 'react';
import { HubConnection, HubConnectionBuilder, LogLevel, HttpTransportType } from '@microsoft/signalr';
import { useAuthStore } from '../stores/auth.store';
import { useQueryClient } from '@tanstack/react-query';
import { notificationKeys } from '../lib/queryKeys';
import toast from 'react-hot-toast';
import { useTranslation } from 'react-i18next';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5001/api';
const HUB_URL = API_BASE_URL.replace('/api', '/hubs/notifications');

export function useNotificationsSignalR() {
    const { token, isAuthenticated } = useAuthStore();
    const queryClient = useQueryClient();
    const { t } = useTranslation('candidate');
    const connectionRef = useRef<HubConnection | null>(null);

    useEffect(() => {
        if (!isAuthenticated || !token) {
            if (connectionRef.current) {
                connectionRef.current.stop();
                connectionRef.current = null;
            }
            return;
        }

        // Prevent multiple connections
        if (connectionRef.current) return;

        const connection = new HubConnectionBuilder()
            .withUrl(HUB_URL, {
                accessTokenFactory: () => token,
                skipNegotiation: false,
                transport: HttpTransportType.WebSockets
            })
            .withAutomaticReconnect()
            .configureLogging(LogLevel.Information)
            .build();

        connection.on('ReceiveNotification', (notification: any) => {
            console.log('🔔 SignalR: New notification received:', notification);
            
            // Invalidate notification queries to refresh lists and badges
            queryClient.invalidateQueries({ queryKey: notificationKeys.all });

            // Show a plain string toast to avoid JSX in .ts file
            const title = notification.title || t('notificationCenter.newNotification');
            const content = notification.content || '';
            const message = content ? `${title}: ${content}` : title;
            
            toast.success(message, {
                icon: '🔔',
                duration: 6000,
            });
        });

        connection.start()
            .then(() => {
                console.log('✅ SignalR: Connected to NotificationHub');
                connectionRef.current = connection;
            })
            .catch(err => {
                console.error('❌ SignalR: Error connecting to NotificationHub:', err);
                connectionRef.current = null;
            });

        return () => {
            if (connectionRef.current) {
                connectionRef.current.stop();
                connectionRef.current = null;
            }
        };
    }, [isAuthenticated, token, queryClient, t]);
}
