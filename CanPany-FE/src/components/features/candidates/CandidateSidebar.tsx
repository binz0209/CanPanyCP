import React, { useState } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
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
  Wallet as WalletIcon,
} from 'lucide-react';
import { Button } from '../../ui/Button';
import { cn } from '../../../utils';

interface NavItem {
  id: string;
  labelKey?: string;
  label: string; // fallback label (always present)
  icon: React.ReactNode;
  path?: string;
  items?: { id: string; labelKey?: string; label: string; path: string; icon: React.ReactNode }[];
}

const navItems: NavItem[] = [
  {
    id: 'overview',
    labelKey: 'sidebar.overview',
    label: 'Tổng quan',
    icon: <LayoutDashboard className="h-5 w-5" />,
    path: '/candidate/dashboard',
  },
  {
    id: 'profile',
    labelKey: 'sidebar.profile',
    label: 'Hồ sơ cá nhân',
    icon: <UserIcon className="h-5 w-5" />,
    path: '/candidate/profile',
  },
  {
    id: 'CvManagement',
    labelKey: 'sidebar.CvManagement',
    label: 'Quản lý CV',
    icon: <FileText className="h-5 w-5" />,
    items: [
      {
        id: 'myCVs',
        labelKey: 'sidebar.myCVs',
        label: 'CV của tôi',
        path: '/candidate/cv/list',
        icon: <FileText className="h-4 w-4" />,
      },
      {
        id: 'aiCVAssistant',
        labelKey: 'sidebar.aiCVAssistant',
        label: 'Trợ lý tạo CV AI',
        path: '/candidate/cv/ai',
        icon: <Wand2 className="h-4 w-4" />,
      },
    ],
  },
  {
    id: 'jobs',
    labelKey: 'sidebar.jobs',
    label: 'Việc làm',
    icon: <Briefcase className="h-5 w-5" />,
    items: [
      {
        id: 'searchJobs',
        labelKey: 'sidebar.searchJobs',
        label: 'Tìm kiếm việc làm',
        path: '/jobs',
        icon: <Briefcase className="h-4 w-4" />,
      },
      {
        id: 'savedJobs',
        labelKey: 'sidebar.savedJobs',
        label: 'Việc làm đã lưu',
        path: '/candidate/jobs/bookmarks',
        icon: <Bookmark className="h-4 w-4" />,
      },
      {
        id: 'aiRecommendations',
        labelKey: 'sidebar.aiRecommendations',
        label: 'Gợi ý từ AI',
        path: '/candidate/jobs/recommended',
        icon: <Wand2 className="h-4 w-4" />,
      },
    ],
  },
  {
    id: 'application',
    labelKey: 'sidebar.application',
    label: 'Đơn ứng tuyển',
    icon: <Send className="h-5 w-5" />,
    path: '/candidate/applications/history',
  },
  {
    id: 'wallet',
    labelKey: 'sidebar.wallet',
    label: 'Ví của tôi',
    icon: <WalletIcon className="h-5 w-5" />,
    path: '/candidate/wallet',
  },
  {
    id: 'AICareer',
    labelKey: 'sidebar.AICareer',
    label: 'AI Career',
    icon: <Wand2 className="h-5 w-5" />,
    items: [
      {
        id: 'aiChat',
        labelKey: 'sidebar.aiChat',
        label: 'Tư vấn nghề nghiệp AI',
        path: '/candidate/ai/chat',
        icon: <Wand2 className="h-4 w-4" />,
      },
      {
        id: 'aiSkills',
        labelKey: 'sidebar.aiSkills',
        label: 'Phân tích kỹ năng',
        path: '/candidate/ai/skills',
        icon: <FileText className="h-4 w-4" />,
      },
      {
        id: 'aiGuidance',
        labelKey: 'sidebar.aiGuidance',
        label: 'Định hướng nghề nghiệp',
        path: '/candidate/ai/guidance',
        icon: <Wand2 className="h-4 w-4" />,
      },
    ],
  },
  {
    id: 'premium',
    labelKey: 'sidebar.premium',
    label: 'Gói Premium',
    icon: <Crown className="h-5 w-5" />,
    path: '/candidate/premium',
  },
  {
    id: 'settingsGroup',
    labelKey: 'sidebar.settingsGroup',
    label: 'Cài đặt',
    icon: <Settings className="h-5 w-5" />,
    items: [
      {
        id: 'account',
        labelKey: 'sidebar.account',
        label: 'Tài khoản',
        path: '/candidate/settings/account',
        icon: <UserIcon className="h-4 w-4" />,
      },
      {
        id: 'notifications',
        labelKey: 'sidebar.notifications',
        label: 'Thông báo',
        path: '/candidate/settings/notifications',
        icon: <FileText className="h-4 w-4" />,
      },
      {
        id: 'privacy',
        labelKey: 'sidebar.privacy',
        label: 'Quyền riêng tư',
        path: '/candidate/settings/privacy',
        icon: <Settings className="h-4 w-4" />,
      },
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
    // Mặc định mở 1 mục con nếu cần; nếu không thì để rỗng.
    new Set()
  );
  const location = useLocation();

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

  const getLabel = (item: { labelKey?: string; label: string }) =>
    item.labelKey
      ? t(item.labelKey, { defaultValue: item.label })
      : item.label;

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
        <div className="space-y-2 p-4">
          {navItems.map((item) => (
            <div key={item.id}>
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
                    <div className={cn(
                      "text-gray-500",
                      isItemActive(item) && "text-[#00b14f]"
                    )}>
                      {item.icon}
                    </div>
                    <span className="flex-1 text-left">{getLabel(item)}</span>
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
                  <div className={cn(
                    "text-gray-500",
                    isItemActive(item) && "text-[#00b14f]"
                  )}>
                    {item.icon}
                  </div>
                  <span className="flex-1 text-left">{getLabel(item)}</span>
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
                        <div className={cn(
                          "text-gray-400",
                          isActive(subItem.path) && "text-[#00b14f]"
                        )}>
                          {subItem.icon}
                        </div>
                        <span>{getLabel(subItem)}</span>
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