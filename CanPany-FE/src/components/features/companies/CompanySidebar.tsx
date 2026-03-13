import { useState } from 'react';
import type { ReactNode } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { BriefcaseBusiness, Building2, ChevronDown, LayoutDashboard, Search } from 'lucide-react';
import { Button } from '../../ui';
import { cn } from '../../../utils';
import { companyNavigationItems } from '../../../lib/companyNavigation';

interface NavItem {
    label: string;
    icon: ReactNode;
    path?: string;
    items?: { label: string; path: string }[];
}

const navIcons: Record<string, ReactNode> = {
    Dashboard: <LayoutDashboard className="h-5 w-5" />,
    'Company Profile': <Building2 className="h-5 w-5" />,
    'Job Management': <BriefcaseBusiness className="h-5 w-5" />,
    'Candidate Search': <Search className="h-5 w-5" />,
};

const navItems: NavItem[] = companyNavigationItems.map((item) => ({
    ...item,
    icon: navIcons[item.label],
}));

interface CompanySidebarProps {
    isOpen: boolean;
    onClose: () => void;
}

export function CompanySidebar({ isOpen, onClose }: CompanySidebarProps) {
    const location = useLocation();
    const [expandedItems, setExpandedItems] = useState<Set<string>>(
        new Set(['Company Profile', 'Job Management'])
    );

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

    const toggleExpand = (label: string) => {
        setExpandedItems((previous) => {
            const next = new Set(previous);
            if (next.has(label)) {
                next.delete(label);
            } else {
                next.add(label);
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
                        <div key={item.label}>
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
                                    </Button>
                                </Link>
                            ) : (
                                <Button
                                    variant="ghost"
                                    className={cn(
                                        'w-full justify-start gap-3 rounded-lg px-4 py-2 text-sm font-medium text-gray-700 transition-colors hover:bg-[#00b14f]/10 hover:text-[#00b14f]',
                                        isItemActive(item) && 'bg-[#00b14f]/10 text-[#00b14f]'
                                    )}
                                    onClick={() => toggleExpand(item.label)}
                                >
                                    <div className={cn('text-gray-500', isItemActive(item) && 'text-[#00b14f]')}>
                                        {item.icon}
                                    </div>
                                    <span className="flex-1 text-left">{item.label}</span>
                                    <ChevronDown
                                        className={cn(
                                            'h-4 w-4 transition-transform duration-200',
                                            expandedItems.has(item.label) && 'rotate-180'
                                        )}
                                    />
                                </Button>
                            )}

                            {item.items && expandedItems.has(item.label) && (
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
