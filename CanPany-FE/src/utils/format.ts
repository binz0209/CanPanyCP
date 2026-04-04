import { format, formatDistanceToNow, parseISO } from 'date-fns';
import { vi, enUS } from 'date-fns/locale';
import i18n from '../i18n';

function getLocale() {
    return i18n.language === 'en' ? enUS : vi;
}

export function formatDate(date: Date | string, pattern = 'dd/MM/yyyy') {
    const d = typeof date === 'string' ? parseISO(date) : date;
    return format(d, pattern, { locale: getLocale() });
}

export function formatDateTime(date: Date | string) {
    return formatDate(date, 'dd/MM/yyyy HH:mm');
}

export function formatRelativeTime(date: Date | string) {
    const d = typeof date === 'string' ? parseISO(date) : date;
    return formatDistanceToNow(d, { addSuffix: true, locale: getLocale() });
}

export function formatCurrency(amount: number, currency = 'VND') {
    const localeCode = i18n.language === 'en' ? 'en-US' : 'vi-VN';
    return new Intl.NumberFormat(localeCode, {
        style: 'currency',
        currency,
        maximumFractionDigits: 0,
    }).format(amount);
}

export function formatNumber(num: number) {
    const localeCode = i18n.language === 'en' ? 'en-US' : 'vi-VN';
    return new Intl.NumberFormat(localeCode).format(num);
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
