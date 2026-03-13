import { Badge } from '../../ui';
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
        Approved: { label: 'Approved', variant: 'success' as const },
        Pending: { label: 'Pending', variant: 'warning' as const },
        Rejected: { label: 'Rejected', variant: 'destructive' as const },
    },
    job: {
        Open: { label: 'Open', variant: 'success' as const },
        Closed: { label: 'Closed', variant: 'destructive' as const },
        Draft: { label: 'Draft', variant: 'warning' as const },
    },
    application: {
        Pending: { label: 'Pending', variant: 'warning' as const },
        Accepted: { label: 'Accepted', variant: 'success' as const },
        Rejected: { label: 'Rejected', variant: 'destructive' as const },
        Withdrawn: { label: 'Withdrawn', variant: 'secondary' as const },
    },
};

export function StatusBadge({ status, kind, className }: StatusBadgeProps) {
    const kindConfig = statusConfig[kind] as Record<string, { label: string; variant: BadgeVariant }>;
    const config = kindConfig[status];

    return (
        <Badge
            variant={config?.variant || 'secondary'}
            className={className}
        >
            {config?.label || status}
        </Badge>
    );
}
