import { Badge } from '../../ui';
import { useTranslation } from 'react-i18next';
import type { ApplicationStatus, JobStatus, VerificationStatus } from '../../../types';

type CompanyStatus = JobStatus | VerificationStatus | ApplicationStatus | string;
type StatusKind = 'job' | 'verification' | 'application';
type BadgeVariant = 'success' | 'warning' | 'destructive' | 'secondary';

interface StatusBadgeProps {
    status: CompanyStatus;
    kind: StatusKind;
    className?: string;
}

const statusConfig = {
    verification: {
        Approved: { label: 'Đã duyệt', variant: 'success' as const },
        Pending: { label: 'Chờ duyệt', variant: 'warning' as const },
        Rejected: { label: 'Bị từ chối', variant: 'destructive' as const },
    },
    job: {
        Open: { label: 'Đang tuyển', variant: 'success' as const },
        Closed: { label: 'Đã đóng', variant: 'destructive' as const },
        Draft: { label: 'Bản nháp', variant: 'warning' as const },
    },
    application: {
        Pending: { label: 'Chờ xét duyệt', variant: 'warning' as const },
        Accepted: { label: 'Đã chấp nhận', variant: 'success' as const },
        Rejected: { label: 'Bị từ chối', variant: 'destructive' as const },
        Withdrawn: { label: 'Đã rút đơn', variant: 'secondary' as const },
    },
};

export function StatusBadge({ status, kind, className }: StatusBadgeProps) {
    const { t } = useTranslation('company');

    const kindConfig = statusConfig[kind] as Record<string, { label: string; variant: BadgeVariant }>;
    const config = kindConfig?.[status];

    const label = (() => {
        if (!config) return status;

        if (kind === 'job') {
            if (status === 'Open') return t('jobs.filterOpen');
            if (status === 'Closed') return t('jobs.filterClosed');
            if (status === 'Draft') return t('jobs.filterDraft');
        }

        if (kind === 'application') {
            if (status === 'Pending') return t('applications.filterPending');
            if (status === 'Accepted') return t('applications.filterAccepted');
            if (status === 'Rejected') return t('applications.filterRejected');
            if (status === 'Withdrawn') return t('applications.filterWithdrawn');
        }

        if (kind === 'verification') {
            if (status === 'Approved') return t('verification.statusApproved');
            if (status === 'Pending') return t('verification.statusPending');
            if (status === 'Rejected') return t('verification.statusRejected');
        }

        return config.label;
    })();

    return (
        <Badge
            variant={config?.variant || 'secondary'}
            className={className}
        >
            {label}
        </Badge>
    );
}
