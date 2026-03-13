export type NotificationType = 'JobMatch' | 'ApplicationUpdate' | 'NewMessage' | 'PaymentConfirmation';

export interface NotificationItem {
  id: string;
  userId: string;
  type: NotificationType;
  title: string;
  content: string;
  timestamp: string;
  isRead: boolean;
  relatedId?: string;
  relatedType?: string;
}

export interface PaginatedNotifications {
  items: NotificationItem[];
  total: number;
  page: number;
  pageSize: number;
}
