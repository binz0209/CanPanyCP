import React, { useState } from 'react';
import { Link, useLocation } from 'react-router-dom';
import {
  LayoutDashboard,
  User as UserIcon,
  FileText,
  Briefcase,
  Send,
  Wand2,
  Crown,
  Settings,
  ChevronDown,
  Bookmark,
  Bell,
  BellRing,
} from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { Button } from '../../ui/Button';
import { useNotifications } from '../../../hooks/useNotifications';
import { cn } from '../../../utils';

interface NavItem {
  labelKey: string;
  icon: React.ReactNode;
  path?: string;
  items?: { labelKey: string; path: string; icon: React.ReactNode }[];
}

// Keys are stable — labels come from i18n
const navItems: NavItem[] = [
  {
    labelKey: 'sidebar.overview',
    icon: <LayoutDashboard className="h-5 w-5" />,
    path: '/candidate/dashboard',
  },
  {
    labelKey: 'sidebar.profile',
    icon: <UserIcon className="h-5 w-5" />,
    path: '/candidate/profile',
  },
  {
    labelKey: 'sidebar.cvManagement',
    icon: <FileText className="h-5 w-5" />,
    items: [
      { labelKey: 'sidebar.myCVs', path: '/candidate/cv/list', icon: <FileText className="h-4 w-4" /> },
      { labelKey: 'sidebar.aiCVAssistant', path: '/candidate/cv/ai', icon: <Wand2 className="h-4 w-4" /> },
    ],
  },
  {
    labelKey: 'sidebar.jobs',
    icon: <Briefcase className="h-5 w-5" />,
    items: [
      { labelKey: 'sidebar.findJobs', path: '/jobs', icon: <Briefcase className="h-4 w-4" /> },
      { labelKey: 'sidebar.savedJobs', path: '/candidate/jobs/bookmarks', icon: <Bookmark className="h-4 w-4" /> },
      { labelKey: 'sidebar.aiSuggestions', path: '/candidate/jobs/recommended', icon: <Wand2 className="h-4 w-4" /> },
    ],
  },
  {
    labelKey: 'sidebar.applications',
    icon: <Send className="h-5 w-5" />,
    path: '/candidate/applications/history',
  },
  {
    labelKey: 'sidebar.jobAlerts',
    icon: <BellRing className="h-5 w-5" />,
    path: '/candidate/job-alerts',
  },
  {
    labelKey: 'sidebar.aiCareer',
    icon: <Wand2 className="h-5 w-5" />,
    items: [
      { labelKey: 'sidebar.aiAdvisor', path: '/candidate/ai/chat', icon: <Wand2 className="h-4 w-4" /> },
      { labelKey: 'sidebar.skillAnalysis', path: '/candidate/ai/skills', icon: <FileText className="h-4 w-4" /> },
      { labelKey: 'sidebar.careerPath', path: '/candidate/ai/guidance', icon: <Wand2 className="h-4 w-4" /> },
    ],
  },
  {
    labelKey: 'sidebar.premium',
    icon: <Crown className="h-5 w-5" />,
    path: '/candidate/premium',
  },
  {
    labelKey: 'sidebar.settingsGroup',
    icon: <Settings className="h-5 w-5" />,
    items: [
      { labelKey: 'sidebar.account', path: '/candidate/settings/account', icon: <UserIcon className="h-4 w-4" /> },
      { labelKey: 'sidebar.notifications', path: '/candidate/settings/notifications', icon: <FileText className="h-4 w-4" /> },
      { labelKey: 'sidebar.privacy', path: '/candidate/settings/privacy', icon: <Settings className="h-4 w-4" /> },
    ],
  },
];

interface CandidateSidebarProps {
  isOpen: boolean;
  onClose: () => void;
}

export function CandidateSidebar({ isOpen, onClose }: CandidateSidebarProps) {
  const { t } = useTranslation('candidate');
  const [expandedItems, setExpandedItems] = useState<Set<string>>(
    new Set(['sidebar.profile'])
  );
  const location = useLocation();
  const { unreadCount } = useNotifications({ enabled: true });

  const toggleExpand = (key: string) => {
    setExpandedItems((prev) => {
      const newSet = new Set(prev);
      if (newSet.has(key)) {
        newSet.delete(key);
      } else {
        newSet.add(key);
      }
      return newSet;
    });
  };

  const isActive = (path?: string) => {
    if (!path) return false;
    return location.pathname === path;
  };

  const isItemActive = (item: NavItem) => {
    if (item.path && isActive(item.path)) return true;
    if (item.items) {
      return item.items.some(subItem => isActive(subItem.path));
    }
    return false;
  };

  const getBadge = (labelKey: string) => {
    if (labelKey === 'sidebar.notifications' && unreadCount > 0) return unreadCount;
    return null;
  };

  return (
    <>
      {isOpen && (
        <div
          className="fixed inset-0 bg-black/50 backdrop-blur-sm z-30 md:hidden"
          onClick={onClose}
        />
      )}

      <aside
        className={cn(
          'fixed left-0 top-16 h-[calc(100vh-64px)] w-64 border-r border-gray-200 bg-white overflow-y-auto transition-transform duration-300 z-40',
          'md:translate-x-0',
          isOpen ? 'translate-x-0' : '-translate-x-full md:translate-x-0'
        )}
      >
        {/* Notification shortcut at top */}
        <div className="px-4 pt-4 pb-2">
          <Link to="/candidate/notifications" onClick={onClose}>
            <Button
              variant="ghost"
              className={cn(
                'w-full justify-start gap-3 text-gray-700 hover:bg-[#00b14f]/10 hover:text-[#00b14f]',
                'rounded-lg px-4 py-2 text-sm font-medium transition-colors',
                isActive('/candidate/notifications') && 'bg-[#00b14f]/10 text-[#00b14f]'
              )}
            >
              <div className={cn('text-gray-500', isActive('/candidate/notifications') && 'text-[#00b14f]')}>
                <Bell className="h-5 w-5" />
              </div>
              <span className="flex-1 text-left">{t('sidebar.notifications')}</span>
              {unreadCount > 0 && (
                <span className="flex h-5 min-w-5 items-center justify-center rounded-full bg-[#00b14f] px-1 text-[10px] font-bold text-white">
                  {unreadCount > 99 ? '99+' : unreadCount}
                </span>
              )}
            </Button>
          </Link>
        </div>

        <div className="space-y-2 px-4 pb-4">
          {navItems.map((item) => {
            const badge = getBadge(item.labelKey);
            return (
              <div key={item.labelKey}>
                {item.path ? (
                  <Link to={item.path} onClick={onClose}>
                    <Button
                      variant="ghost"
                      className={cn(
                        'w-full justify-start gap-3 text-gray-700 hover:bg-[#00b14f]/10 hover:text-[#00b14f]',
                        'rounded-lg px-4 py-2 text-sm font-medium transition-colors',
                        isItemActive(item) && 'bg-[#00b14f]/10 text-[#00b14f]'
                      )}
                    >
                      <div className={cn('text-gray-500', isItemActive(item) && 'text-[#00b14f]')}>
                        {item.icon}
                      </div>
                      <span className="flex-1 text-left">{t(item.labelKey as never)}</span>
                      {badge != null && (
                        <span className="flex h-5 min-w-5 items-center justify-center rounded-full bg-[#00b14f] px-1 text-[10px] font-bold text-white">
                          {badge > 99 ? '99+' : badge}
                        </span>
                      )}
                    </Button>
                  </Link>
                ) : (
                  <Button
                    variant="ghost"
                    className={cn(
                      'w-full justify-start gap-3 text-gray-700 hover:bg-[#00b14f]/10 hover:text-[#00b14f]',
                      'rounded-lg px-4 py-2 text-sm font-medium transition-colors',
                      isItemActive(item) && 'bg-[#00b14f]/10 text-[#00b14f]'
                    )}
                    onClick={() => toggleExpand(item.labelKey)}
                  >
                    <div className={cn('text-gray-500', isItemActive(item) && 'text-[#00b14f]')}>
                      {item.icon}
                    </div>
                    <span className="flex-1 text-left">{t(item.labelKey as never)}</span>
                    {item.items && (
                      <ChevronDown
                        className={cn(
                          'h-4 w-4 transition-transform duration-200',
                          expandedItems.has(item.labelKey) ? 'rotate-180' : ''
                        )}
                      />
                    )}
                  </Button>
                )}

                {item.items != null && expandedItems.has(item.labelKey) && (
                  <div className="ml-4 space-y-1 border-l border-gray-200 pl-3 py-2">
                    {item.items.map((subItem) => (
                      <Link key={subItem.labelKey} to={subItem.path} onClick={onClose}>
                        <Button
                          variant="ghost"
                          className={cn(
                            'w-full justify-start gap-2 text-gray-600 hover:bg-[#00b14f]/10 hover:text-[#00b14f]',
                            'rounded-md px-3 py-1.5 text-xs font-medium transition-colors',
                            isActive(subItem.path) && 'bg-[#00b14f]/10 text-[#00b14f]'
                          )}
                        >
                          <div className={cn('text-gray-400', isActive(subItem.path) && 'text-[#00b14f]')}>
                            {subItem.icon}
                          </div>
                          <span>{t(subItem.labelKey as never)}</span>
                        </Button>
                      </Link>
                    ))}
                  </div>
                )}
              </div>
            );
          })}
        </div>
      </aside>
    </>
  );
}
