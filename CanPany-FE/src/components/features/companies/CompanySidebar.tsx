import { useState } from 'react';
import type { ReactNode } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { Bell, BriefcaseBusiness, Building2, ChevronDown, Crown, LayoutDashboard, MessageSquare, Wallet } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { Button } from '../../ui';
import { cn } from '../../../utils';
import { companyNavigationItems } from '../../../lib/companyNavigation';
import { useNotifications } from '../../../hooks/useNotifications';
import { useQuery } from '@tanstack/react-query';
import { conversationKeys } from '../../../lib/queryKeys';
import { conversationsApi } from '../../../api/conversations.api';

interface NavItem {
    labelKey: string;
    label: string;
    icon: ReactNode;
    path?: string;
    items?: { labelKey: string; label: string; path: string }[];
}

const navIcons: Record<string, ReactNode> = {
    'sidebar.dashboard': <LayoutDashboard className="h-5 w-5" />,
    'sidebar.companyProfile': <Building2 className="h-5 w-5" />,
    'sidebar.jobManagement': <BriefcaseBusiness className="h-5 w-5" />,
    'sidebar.messages': <MessageSquare className="h-5 w-5" />,
    'sidebar.notifications': <Bell className="h-5 w-5" />,
    'sidebar.billing': <Wallet className="h-5 w-5" />,
    'sidebar.wallet': <Wallet className="h-5 w-5" />,
    'sidebar.premium': <Crown className="h-5 w-5" />,
};

interface CompanySidebarProps {
    isOpen: boolean;
    onClose: () => void;
}

export function CompanySidebar({ isOpen, onClose }: CompanySidebarProps) {
    const { t } = useTranslation('company');
    const location = useLocation();

    const navItems: NavItem[] = companyNavigationItems.map((item) => ({
        labelKey: item.labelKey,
        label: t(item.labelKey as any),
        icon: navIcons[item.labelKey],
        path: item.path,
        items: item.items?.map(subItem => ({
            labelKey: subItem.labelKey,
            label: t(subItem.labelKey as any),
            path: subItem.path
        }))
    }));

    const [expandedItems, setExpandedItems] = useState<Set<string>>(
        new Set(['sidebar.companyProfile', 'sidebar.jobManagement', 'sidebar.billing'])
    );

    const { unreadCount: unreadNotifications } = useNotifications({ enabled: true, refetchInterval: 60000 });
    
    const { data: unreadMessages = 0 } = useQuery({
        queryKey: conversationKeys.unreadCount(),
        queryFn: () => conversationsApi.getUnreadCount(),
        refetchInterval: 60000,
    });

    const getBadge = (labelKey: string) => {
        if (labelKey === 'sidebar.notifications' && unreadNotifications > 0) return unreadNotifications;
        if (labelKey === 'sidebar.messages' && unreadMessages > 0) return unreadMessages;
        return null;
    };

    const isActive = (path?: string) => {
        if (!path) return false;
        return location.pathname === path || location.pathname.startsWith(`${path}/`);
    };

    const isItemActive = (item: NavItem) => {
        if (item.path && isActive(item.path)) return true;
        if (item.items) {
            return item.items.some((subItem) => isActive(subItem.path));
        }
        return false;
    };

    const toggleExpand = (labelKey: string) => {
        setExpandedItems((previous) => {
            const next = new Set(previous);
            if (next.has(labelKey)) {
                next.delete(labelKey);
            } else {
                next.add(labelKey);
            }
            return next;
        });
    };

    return (
        <>
            {isOpen && (
                <div
                    className="fixed inset-0 z-30 bg-black/50 backdrop-blur-sm md:hidden"
                    onClick={onClose}
                />
            )}

            <aside
                className={cn(
                    'fixed left-0 top-16 z-40 h-[calc(100vh-64px)] w-64 overflow-y-auto border-r border-gray-200 bg-white transition-transform duration-300',
                    'md:translate-x-0',
                    isOpen ? 'translate-x-0' : '-translate-x-full md:translate-x-0'
                )}
            >
                <div className="space-y-2 p-4">
                    {navItems.map((item) => (
                        <div key={item.labelKey}>
                            {item.path ? (
                                <Link to={item.path} onClick={onClose}>
                                    <Button
                                        variant="ghost"
                                        className={cn(
                                            'w-full justify-start gap-3 rounded-lg px-4 py-2 text-sm font-medium text-gray-700 transition-colors hover:bg-[#00b14f]/10 hover:text-[#00b14f]',
                                            isItemActive(item) && 'bg-[#00b14f]/10 text-[#00b14f]'
                                        )}
                                    >
                                        <div className={cn('text-gray-500', isItemActive(item) && 'text-[#00b14f]')}>
                                            {item.icon}
                                        </div>
                                        <span className="flex-1 text-left">{item.label}</span>
                                        {getBadge(item.labelKey) !== null && (
                                            <span className="flex h-5 min-w-[20px] items-center justify-center rounded-full bg-red-500 px-1.5 text-[10px] font-bold text-white">
                                                {getBadge(item.labelKey)! > 99 ? '99+' : getBadge(item.labelKey)}
                                            </span>
                                        )}
                                    </Button>
                                </Link>
                            ) : (
                                <Button
                                    variant="ghost"
                                    className={cn(
                                        'w-full justify-start gap-3 rounded-lg px-4 py-2 text-sm font-medium text-gray-700 transition-colors hover:bg-[#00b14f]/10 hover:text-[#00b14f]',
                                        isItemActive(item) && 'bg-[#00b14f]/10 text-[#00b14f]'
                                    )}
                                    onClick={() => toggleExpand(item.labelKey)}
                                >
                                    <div className={cn('text-gray-500', isItemActive(item) && 'text-[#00b14f]')}>
                                        {item.icon}
                                    </div>
                                    <span className="flex-1 text-left">{item.label}</span>
                                    {getBadge(item.labelKey) !== null && (
                                        <span className="flex h-5 min-w-[20px] items-center justify-center rounded-full bg-red-500 px-1.5 text-[10px] font-bold text-white">
                                            {getBadge(item.labelKey)! > 99 ? '99+' : getBadge(item.labelKey)}
                                        </span>
                                    )}
                                    <ChevronDown
                                        className={cn(
                                            'h-4 w-4 transition-transform duration-200',
                                            expandedItems.has(item.labelKey) && 'rotate-180'
                                        )}
                                    />
                                </Button>
                            )}

                            {item.items && expandedItems.has(item.labelKey) && (
                                <div className="ml-4 space-y-1 border-l border-gray-200 py-2 pl-3">
                                    {item.items.map((subItem) => (
                                        <Link key={subItem.path} to={subItem.path} onClick={onClose}>
                                            <Button
                                                variant="ghost"
                                                className={cn(
                                                    'w-full justify-start rounded-md px-3 py-1.5 text-xs font-medium text-gray-600 transition-colors hover:bg-[#00b14f]/10 hover:text-[#00b14f]',
                                                    isActive(subItem.path) && 'bg-[#00b14f]/10 text-[#00b14f]'
                                                )}
                                            >
                                                {subItem.label}
                                            </Button>
                                        </Link>
                                    ))}
                                </div>
                            )}
                        </div>
                    ))}
                </div>
            </aside>
        </>
    );
}
