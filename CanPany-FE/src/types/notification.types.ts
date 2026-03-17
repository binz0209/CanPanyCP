export type NotificationType = 'JobMatch' | 'ApplicationUpdate' | 'NewMessage' | 'PaymentConfirmation' | 'ProposalAccepted' | 'JobAlert' | 'SystemNotification' | string;

export interface NotificationItem {
  id: string;
  type: string;
  title: string;
  content: string;
  timestamp: string;
  isRead: boolean;
}

export interface PaginatedNotifications {
  items: NotificationItem[];
  total: number;
  page: number;
  pageSize: number;
}
