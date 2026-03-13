import type { ReactNode } from 'react';
import { EmptyState } from './EmptyState';

export function CompanyWorkspaceLoader() {
    return (
        <div className="flex min-h-[60vh] items-center justify-center">
            <div className="h-10 w-10 animate-spin rounded-full border-b-2 border-[#00b14f]" />
        </div>
    );
}

interface CompanyWorkspaceEmptyStateProps {
    title: string;
    description: string;
    icon?: ReactNode;
    action?: ReactNode;
}

export function CompanyWorkspaceErrorState(props: CompanyWorkspaceEmptyStateProps) {
    return <EmptyState {...props} />;
}

export function CompanyProfileRequiredState(props: CompanyWorkspaceEmptyStateProps) {
    return <EmptyState {...props} />;
}
