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
  Activity,
  MessageSquare,
  Wallet as WalletIcon,
  FileSignature,
  Shield,
} from 'lucide-react';
import { Button } from '../../ui/Button';
import { useNotifications } from '../../../hooks/useNotifications';
import { cn } from '../../../utils';
import { useTranslation } from 'react-i18next';

interface NavItem {
  id: string;
  labelKey: string;
  icon: React.ReactNode;
  path?: string;
  items?: { id: string; labelKey: string; path: string; icon: React.ReactNode }[];
}

interface CandidateSidebarProps {
  isOpen: boolean;
  onClose: () => void;
}

export function CandidateSidebar({ isOpen, onClose }: CandidateSidebarProps) {
  const { t } = useTranslation('candidate');

  const navItems: NavItem[] = [
    {
      id: 'premium',
      labelKey: 'sidebar.premium',
      icon: <Crown className="h-5 w-5" />,
      path: '/candidate/premium',
    },
    {
      id: 'overview',
      labelKey: 'sidebar.overview',
      icon: <LayoutDashboard className="h-5 w-5" />,
      path: '/candidate/dashboard',
    },
    {
      id: 'profile',
      labelKey: 'sidebar.profile',
      icon: <UserIcon className="h-5 w-5" />,
      path: '/candidate/profile',
    },
    {
      id: 'cv',
      labelKey: 'sidebar.cv',
      icon: <FileText className="h-5 w-5" />,
      items: [
        { id: 'cv.list', labelKey: 'sidebar.cvList', path: '/candidate/cv/list', icon: <FileText className="h-4 w-4" /> },
        { id: 'cv.ai', labelKey: 'sidebar.cvAi', path: '/candidate/cv/ai', icon: <Wand2 className="h-4 w-4" /> },
      ],
    },
    {
      id: 'jobs',
      labelKey: 'sidebar.jobs',
      icon: <Briefcase className="h-5 w-5" />,
      items: [
        { id: 'jobs.applications', labelKey: 'sidebar.applications', path: '/candidate/applications/history', icon: <Send className="h-4 w-4" /> },
        { id: 'jobs.search', labelKey: 'sidebar.jobsSearch', path: '/jobs', icon: <Briefcase className="h-4 w-4" /> },
        { id: 'jobs.saved', labelKey: 'sidebar.jobsSaved', path: '/candidate/jobs/bookmarks', icon: <Bookmark className="h-4 w-4" /> },
        { id: 'jobs.recommended', labelKey: 'sidebar.jobsRecommended', path: '/candidate/jobs/recommended', icon: <Wand2 className="h-4 w-4" /> },
        { id: 'jobs.contracts', labelKey: 'sidebar.contracts', path: '/candidate/contracts', icon: <FileSignature className="h-4 w-4" /> },
      ],
    },
    {
      id: 'updates',
      labelKey: 'sidebar.updates',
      icon: <Bell className="h-5 w-5" />,
      items: [
        { id: 'notifications', labelKey: 'sidebar.notifications', path: '/candidate/notifications', icon: <Bell className="h-4 w-4" /> },
        { id: 'messages', labelKey: 'sidebar.messages', path: '/candidate/messages', icon: <MessageSquare className="h-4 w-4" /> },
        { id: 'jobAlerts', labelKey: 'sidebar.jobAlerts', path: '/candidate/job-alerts', icon: <BellRing className="h-4 w-4" /> },
      ],
    },
    {
      id: 'wallet',
      labelKey: 'sidebar.wallet',
      icon: <WalletIcon className="h-5 w-5" />,
      path: '/candidate/wallet',
    },
    {
      id: 'backgroundJobs',
      labelKey: 'sidebar.backgroundJobs',
      icon: <Activity className="h-5 w-5" />,
      path: '/candidate/background-jobs',
    },
    {
      id: 'aiCareer',
      labelKey: 'sidebar.aiCareer',
      icon: <Wand2 className="h-5 w-5" />,
      items: [
        { id: 'ai.chat', labelKey: 'sidebar.aiChat', path: '/candidate/ai/chat', icon: <Wand2 className="h-4 w-4" /> },
        { id: 'ai.skills', labelKey: 'sidebar.aiSkills', path: '/candidate/ai/skills', icon: <FileText className="h-4 w-4" /> },
        { id: 'ai.guidance', labelKey: 'sidebar.aiGuidance', path: '/candidate/ai/guidance', icon: <Wand2 className="h-4 w-4" /> },
      ],
    },
    {
      id: 'settings',
      labelKey: 'sidebar.settings',
      icon: <Settings className="h-5 w-5" />,
      items: [
        { id: 'settings.account', labelKey: 'sidebar.settingsAccount', path: '/candidate/settings/account', icon: <UserIcon className="h-4 w-4" /> },
        { id: 'settings.notifications', labelKey: 'sidebar.settingsNotifications', path: '/candidate/settings/notifications', icon: <FileText className="h-4 w-4" /> },
        { id: 'settings.privacy', labelKey: 'sidebar.settingsPrivacy', path: '/candidate/settings/privacy', icon: <Shield className="h-4 w-4" /> },
      ],
    },
  ];

  const [expandedItems, setExpandedItems] = useState<Set<string>>(
    new Set(['profile'])
  );
  const location = useLocation();
  const { unreadCount } = useNotifications({ enabled: true });

  const toggleExpand = (id: string) => {
    setExpandedItems((prev) => {
      const newSet = new Set(prev);
      if (newSet.has(id)) {
        newSet.delete(id);
      } else {
        newSet.add(id);
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

  const getBadge = (id: string) => {
    if ((id === 'notifications' || id === 'updates') && unreadCount > 0) return unreadCount;
    return null;
  };

  return (
    <>
      {/* Mobile backdrop */}
      {isOpen && (
        <div
          className="fixed inset-0 bg-black/50 backdrop-blur-sm z-30 md:hidden"
          onClick={onClose}
        />
      )}

      {/* Sidebar */}
      <aside
        className={cn(
          'fixed left-0 top-16 h-[calc(100vh-64px)] w-64 border-r border-gray-200 bg-white overflow-y-auto transition-transform duration-300 z-40',
          'md:translate-x-0',
          isOpen ? 'translate-x-0' : '-translate-x-full md:translate-x-0'
        )}
      >
        <div className="space-y-2 px-4 py-4">
          {navItems.map((item) => {
            const badge = getBadge(item.id);
            const isPremium = item.id === 'premium';
            return (
              <div key={item.id}>
                {item.path ? (
                  <Link to={item.path} onClick={onClose}>
                    <Button
                      variant="ghost"
                      className={cn(
                        'w-full justify-start gap-3 rounded-lg px-4 py-2 text-sm font-medium transition-all duration-300',
                        isPremium
                          ? 'bg-linear-to-r from-amber-400 via-orange-400 to-rose-500 text-white shadow-md shadow-orange-200 hover:scale-[1.02] hover:shadow-lg hover:shadow-orange-300'
                          : 'text-gray-700 hover:bg-[#00b14f]/10 hover:text-[#00b14f]',
                        isItemActive(item) && (isPremium ? 'ring-2 ring-orange-200' : 'bg-[#00b14f]/10 text-[#00b14f]')
                      )}
                    >
                      <div className={cn(isPremium ? 'text-white' : 'text-gray-500', isItemActive(item) && !isPremium && 'text-[#00b14f]')}>
                        {item.icon}
                      </div>
                      <span className="flex-1 text-left">{t(item.labelKey as any)}</span>
                      {badge && (
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
                    onClick={() => toggleExpand(item.id)}
                  >
                    <div className={cn('text-gray-500', isItemActive(item) && 'text-[#00b14f]')}>
                      {item.icon}
                    </div>
                    <span className="flex-1 text-left">{t(item.labelKey as any)}</span>
                    {badge && (
                      <span className="flex h-5 min-w-5 items-center justify-center rounded-full bg-[#00b14f] px-1 text-[10px] font-bold text-white">
                        {badge > 99 ? '99+' : badge}
                      </span>
                    )}
                    {item.items && (
                      <ChevronDown
                        className={cn(
                          'h-4 w-4 transition-transform duration-200',
                          expandedItems.has(item.id) ? 'rotate-180' : ''
                        )}
                      />
                    )}
                  </Button>
                )}

                {/* Sub-items */}
                {item.items && expandedItems.has(item.id) && (
                  <div className="ml-4 space-y-1 border-l border-gray-200 pl-3 py-2">
                    {item.items.map((subItem) => (
                      <Link key={subItem.id} to={subItem.path} onClick={onClose}>
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
                          <span className="flex-1 text-left">{t(subItem.labelKey as any)}</span>
                          {subItem.id === 'notifications' && unreadCount > 0 && (
                            <span className="flex h-4 min-w-4 items-center justify-center rounded-full bg-[#00b14f] px-1 text-[9px] font-bold text-white">
                              {unreadCount > 99 ? '99+' : unreadCount}
                            </span>
                          )}
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