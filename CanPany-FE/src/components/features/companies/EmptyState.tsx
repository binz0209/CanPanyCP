import type { ReactNode } from 'react';
import { Card } from '../../ui';

interface EmptyStateProps {
    title: string;
    description: string;
    icon?: ReactNode;
    action?: ReactNode;
}

export function EmptyState({ title, description, icon, action }: EmptyStateProps) {
    return (
        <Card className="p-6">
            <div className="text-center sm:text-left">
                {icon && (
                    <div className="mb-4 inline-flex h-12 w-12 items-center justify-center rounded-full bg-[#00b14f]/10 text-[#00b14f]">
                        {icon}
                    </div>
                )}
                <h1 className="text-xl font-semibold text-gray-900">{title}</h1>
                <p className="mt-2 text-sm leading-6 text-gray-600">{description}</p>
                {action && <div className="mt-4">{action}</div>}
            </div>
        </Card>
    );
}
