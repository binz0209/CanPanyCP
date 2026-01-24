import { format, formatDistanceToNow, parseISO } from 'date-fns';
import { vi } from 'date-fns/locale';

export function formatDate(date: Date | string, pattern = 'dd/MM/yyyy') {
    const d = typeof date === 'string' ? parseISO(date) : date;
    return format(d, pattern, { locale: vi });
}

export function formatDateTime(date: Date | string) {
    return formatDate(date, 'dd/MM/yyyy HH:mm');
}

export function formatRelativeTime(date: Date | string) {
    const d = typeof date === 'string' ? parseISO(date) : date;
    return formatDistanceToNow(d, { addSuffix: true, locale: vi });
}

export function formatCurrency(amount: number, currency = 'VND') {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency,
        maximumFractionDigits: 0,
    }).format(amount);
}

export function formatNumber(num: number) {
    return new Intl.NumberFormat('vi-VN').format(num);
}

export function formatCompactNumber(num: number) {
    if (num >= 1000000) {
        return (num / 1000000).toFixed(1) + 'M';
    }
    if (num >= 1000) {
        return (num / 1000).toFixed(1) + 'K';
    }
    return num.toString();
}
