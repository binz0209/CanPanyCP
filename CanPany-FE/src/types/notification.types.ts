export type NotificationType =
    | 'ApplicationUpdate'
    | 'NewMessage'
    | 'JobMatch'
    | 'PaymentConfirmation'
    | string;

export interface NotificationItem {
    id: string;
    type: NotificationType;
    title: string;
    content: string;
    timestamp: string | Date;
    isRead: boolean;
}

