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
