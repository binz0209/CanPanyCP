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

const statusVariants: Record<StatusKind, Record<string, BadgeVariant>> = {
    verification: {
        Approved: 'success',
        Pending: 'warning',
        Rejected: 'destructive',
    },
    job: {
        Open: 'success',
        Closed: 'destructive',
        Draft: 'warning',
    },
    application: {
        Pending: 'warning',
        Accepted: 'success',
        Rejected: 'destructive',
        Withdrawn: 'secondary',
    },
};

export function StatusBadge({ status, kind, className }: StatusBadgeProps) {
    const { t } = useTranslation('company');

    const variant = statusVariants[kind]?.[status] ?? 'secondary';

    const label = (() => {
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

        return String(status);
    })();

    return (
        <Badge variant={variant} className={className}>
            {label}
        </Badge>
    );
}
