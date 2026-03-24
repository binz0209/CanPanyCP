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
} from 'lucide-react';
import { Button } from '../../ui/Button';
import { cn } from '../../../utils';

interface NavItem {
  label: string;
  icon: React.ReactNode;
  path?: string;
  items?: { label: string; path: string; icon: React.ReactNode }[];
}

const navItems: NavItem[] = [
  {
    label: 'Tổng quan',
    icon: <LayoutDashboard className="h-5 w-5" />,
    path: '/candidate/dashboard',
  },
  {
    label: 'Hồ sơ cá nhân',
    icon: <UserIcon className="h-5 w-5" />,
    path: '/candidate/profile',
  },
  {
    label: 'Quản lý CV',
    icon: <FileText className="h-5 w-5" />,
    items: [
      { label: 'CV của tôi', path: '/candidate/cv/list', icon: <FileText className="h-4 w-4" /> },
      { label: 'Trợ lý tạo CV AI', path: '/candidate/cv/ai', icon: <Wand2 className="h-4 w-4" /> },
    ],
  },
  {
    label: 'Việc làm',
    icon: <Briefcase className="h-5 w-5" />,
    items: [
      { label: 'Tìm kiếm việc làm', path: '/jobs', icon: <Briefcase className="h-4 w-4" /> },
      { label: 'Việc làm đã lưu', path: '/candidate/jobs/bookmarks', icon: <Bookmark className="h-4 w-4" /> },
      { label: 'Gợi ý từ AI', path: '/candidate/jobs/recommended', icon: <Wand2 className="h-4 w-4" /> },
    ],
  },
  {
    label: 'Đơn ứng tuyển',
    icon: <Send className="h-5 w-5" />,
    path: '/candidate/applications/history',
  },
  {
    label: 'Nhắn tin',
    icon: <MessageSquare className="h-5 w-5" />,
    path: '/candidate/messages',
  },
  {
    label: 'Ví của tôi',
    icon: <WalletIcon className="h-5 w-5" />,
    path: '/candidate/wallet',
  },
  {
    label: 'Job Alerts',
    icon: <BellRing className="h-5 w-5" />,
    path: '/candidate/job-alerts',
  },
  {
    label: 'Tiến trình',
    icon: <Activity className="h-5 w-5" />,
    path: '/candidate/background-jobs',
  },
  {
    label: 'AI Career',
    icon: <Wand2 className="h-5 w-5" />,
    items: [
      { label: 'Tư vấn nghề nghiệp AI', path: '/candidate/ai/chat', icon: <Wand2 className="h-4 w-4" /> },
      { label: 'Phân tích kỹ năng', path: '/candidate/ai/skills', icon: <FileText className="h-4 w-4" /> },
      { label: 'Định hướng nghề nghiệp', path: '/candidate/ai/guidance', icon: <Wand2 className="h-4 w-4" /> },
    ],
  },
  {
    label: 'Gói Premium',
    icon: <Crown className="h-5 w-5" />,
    path: '/candidate/premium',
  },
  {
    label: 'Cài đặt',
    icon: <Settings className="h-5 w-5" />,
    items: [
      { label: 'Tài khoản', path: '/candidate/settings/account', icon: <UserIcon className="h-4 w-4" /> },
      { label: 'Thông báo', path: '/candidate/notifications', icon: <FileText className="h-4 w-4" /> },
      { label: 'Quyền riêng tư', path: '/candidate/settings/privacy', icon: <Settings className="h-4 w-4" /> },
    ],
  },
];

interface CandidateSidebarProps {
  isOpen: boolean;
  onClose: () => void;
}

export function CandidateSidebar({ isOpen, onClose }: CandidateSidebarProps) {
  const [expandedItems, setExpandedItems] = useState<Set<string>>(new Set());
  const location = useLocation();

  const toggleExpand = (label: string) => {
    setExpandedItems((prev) => {
      const newSet = new Set(prev);
      if (newSet.has(label)) newSet.delete(label);
      else newSet.add(label);
      return newSet;
    });
  };

  const isActive = (path?: string) => {
    if (!path) return false;
    return location.pathname === path;
  };

  const isItemActive = (item: NavItem) => {
    if (item.path && isActive(item.path)) return true;
    if (item.items) return item.items.some((subItem) => isActive(subItem.path));
    return false;
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
        <div className="px-4 pb-2 pt-4">
          <Link to="/candidate/notifications" onClick={onClose}>
            <Button
              variant="ghost"
              className={cn(
                'w-full justify-start gap-3 rounded-lg px-4 py-2 text-sm font-medium text-gray-700 transition-colors hover:bg-[#00b14f]/10 hover:text-[#00b14f]',
                isActive('/candidate/notifications') && 'bg-[#00b14f]/10 text-[#00b14f]'
              )}
            >
              <div className={cn('text-gray-500', isActive('/candidate/notifications') && 'text-[#00b14f]')}>
                <Bell className="h-5 w-5" />
              </div>
              <span className="flex-1 text-left">Thông báo</span>
            </Button>
          </Link>
        </div>

        <div className="space-y-2 px-4 pb-4">
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
                    <div className={cn('text-gray-500', isItemActive(item) && 'text-[#00b14f]')}>{item.icon}</div>
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
                  <div className={cn('text-gray-500', isItemActive(item) && 'text-[#00b14f]')}>{item.icon}</div>
                  <span className="flex-1 text-left">{item.label}</span>
                  {item.items && (
                    <ChevronDown
                      className={cn(
                        'h-4 w-4 transition-transform duration-200',
                        expandedItems.has(item.label) ? 'rotate-180' : ''
                      )}
                    />
                  )}
                </Button>
              )}

              {item.items && expandedItems.has(item.label) && (
                <div className="ml-4 space-y-1 border-l border-gray-200 py-2 pl-3">
                  {item.items.map((subItem) => (
                    <Link key={subItem.label} to={subItem.path} onClick={onClose}>
                      <Button
                        variant="ghost"
                        className={cn(
                          'w-full justify-start gap-2 rounded-md px-3 py-1.5 text-xs font-medium text-gray-600 transition-colors hover:bg-[#00b14f]/10 hover:text-[#00b14f]',
                          isActive(subItem.path) && 'bg-[#00b14f]/10 text-[#00b14f]'
                        )}
                      >
                        <div className={cn('text-gray-400', isActive(subItem.path) && 'text-[#00b14f]')}>
                          {subItem.icon}
                        </div>
                        <span>{subItem.label}</span>
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
