import { Link, useLocation } from 'react-router-dom';
import {
    LayoutDashboard,
    Users,
    Building2,
    Briefcase,
    Tags,
    CreditCard,
    ScrollText,
    Flag,
    Radio,
} from 'lucide-react';
import type { ReactNode } from 'react';
import { useTranslation } from 'react-i18next';
import { cn } from '../../../utils';
import { adminNavigationItems } from '../../../lib/adminNavigation';

const navIcons: Record<string, ReactNode> = {
    dashboard:    <LayoutDashboard className="h-5 w-5" />,
    users:        <Users className="h-5 w-5" />,
    verification: <Building2 className="h-5 w-5" />,
    companies:    <Building2 className="h-5 w-5" />,
    jobs:         <Briefcase className="h-5 w-5" />,
    catalog:      <Tags className="h-5 w-5" />,
    payments:     <CreditCard className="h-5 w-5" />,
    auditLogs:    <ScrollText className="h-5 w-5" />,
    reports:      <Flag className="h-5 w-5" />,
    broadcast:    <Radio className="h-5 w-5" />,
};

interface AdminSidebarProps {
    isOpen: boolean;
    onClose: () => void;
}

export function AdminSidebar({ isOpen, onClose }: AdminSidebarProps) {
    const { t } = useTranslation('admin');
    const location = useLocation();

    return (
        <>
            {isOpen && (
                <div
                    className="fixed inset-0 z-30 bg-black/50 backdrop-blur-sm md:hidden"
                    onClick={onClose}
                    aria-hidden
                />
            )}

            <aside
                className={cn(
                    'fixed left-0 top-16 z-40 h-[calc(100vh-64px)] w-64 overflow-y-auto border-r border-gray-200 bg-white transition-transform duration-300',
                    'md:translate-x-0',
                    isOpen ? 'translate-x-0' : '-translate-x-full md:translate-x-0'
                )}
            >
                <nav className="space-y-1 p-4">
                    {adminNavigationItems.map((item) => {
                        const active = location.pathname === item.path;
                        return (
                            <Link
                                key={item.id}
                                to={item.path}
                                onClick={onClose}
                                className={cn(
                                    'flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition-colors',
                                    active
                                        ? 'bg-slate-900 text-white'
                                        : 'text-gray-700 hover:bg-gray-100 hover:text-slate-900'
                                )}
                            >
                                <span className={cn(active ? 'text-white' : 'text-gray-500')}>
                                    {navIcons[item.id] ?? <LayoutDashboard className="h-5 w-5" />}
                                </span>
                                {t(item.labelKey)}
                            </Link>
                        );
                    })}
                </nav>
            </aside>
        </>
    );
}
