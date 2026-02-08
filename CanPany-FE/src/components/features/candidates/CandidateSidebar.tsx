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
    label: 'Dashboard',
    icon: <LayoutDashboard className="h-5 w-5" />,
    path: '/candidate/dashboard',
  },
  {
    label: 'Profile',
    icon: <UserIcon className="h-5 w-5" />,
    items: [
      { label: 'Personal Profile', path: '/candidate/profile', icon: <UserIcon className="h-4 w-4" /> },
      { label: 'Skills Management', path: '/candidate/skills', icon: <Briefcase className="h-4 w-4" /> },
      { label: 'LinkedIn & GitHub Sync', path: '/candidate/sync', icon: <FileText className="h-4 w-4" /> },
    ],
  },
  {
    label: 'CV Management',
    icon: <FileText className="h-5 w-5" />,
    items: [
      { label: 'Upload CV', path: '/candidate/cv/upload', icon: <FileText className="h-4 w-4" /> },
      { label: 'CV List', path: '/candidate/cv/list', icon: <FileText className="h-4 w-4" /> },
      { label: 'CV Details', path: '/candidate/cv/details', icon: <FileText className="h-4 w-4" /> },
      { label: 'AI CV Analysis', path: '/candidate/cv/analysis', icon: <Wand2 className="h-4 w-4" /> },
      { label: 'AI-Generated CVs', path: '/candidate/cv/generated', icon: <Wand2 className="h-4 w-4" /> },
    ],
  },
  {
    label: 'Jobs',
    icon: <Briefcase className="h-5 w-5" />,
    items: [
      { label: 'Job Search', path: '/candidate/jobs/search', icon: <Briefcase className="h-4 w-4" /> },
      { label: 'Job Details', path: '/candidate/jobs/details', icon: <Briefcase className="h-4 w-4" /> },
      { label: 'Bookmarked Jobs', path: '/candidate/jobs/bookmarks', icon: <FileText className="h-4 w-4" /> },
      { label: 'AI-Recommended Jobs', path: '/candidate/jobs/recommended', icon: <Wand2 className="h-4 w-4" /> },
    ],
  },
  {
    label: 'Applications',
    icon: <Send className="h-5 w-5" />,
    items: [
      { label: 'Submit Applications', path: '/candidate/applications/submit', icon: <Send className="h-4 w-4" /> },
      { label: 'Application History', path: '/candidate/applications/history', icon: <FileText className="h-4 w-4" /> },
      { label: 'Application Status', path: '/candidate/applications/status', icon: <Send className="h-4 w-4" /> },
      { label: 'Withdraw Application', path: '/candidate/applications/withdraw', icon: <Send className="h-4 w-4" /> },
    ],
  },
  {
    label: 'AI Career',
    icon: <Wand2 className="h-5 w-5" />,
    items: [
      { label: 'Chat with AI Advisor', path: '/candidate/ai/chat', icon: <Wand2 className="h-4 w-4" /> },
      { label: 'Skill Gap Analysis', path: '/candidate/ai/skills', icon: <FileText className="h-4 w-4" /> },
      { label: 'Career Guidance', path: '/candidate/ai/guidance', icon: <Wand2 className="h-4 w-4" /> },
    ],
  },
  {
    label: 'Premium',
    icon: <Crown className="h-5 w-5" />,
    path: '/candidate/premium',
  },
  {
    label: 'Settings',
    icon: <Settings className="h-5 w-5" />,
    items: [
      { label: 'Account', path: '/candidate/settings/account', icon: <UserIcon className="h-4 w-4" /> },
      { label: 'Notifications', path: '/candidate/settings/notifications', icon: <FileText className="h-4 w-4" /> },
      { label: 'Privacy', path: '/candidate/settings/privacy', icon: <Settings className="h-4 w-4" /> },
    ],
  },
];

interface CandidateSidebarProps {
  isOpen: boolean;
  onClose: () => void;
}

export function CandidateSidebar({ isOpen, onClose }: CandidateSidebarProps) {
  const [expandedItems, setExpandedItems] = useState<Set<string>>(
    new Set(['Profile'])
  );
  const location = useLocation();

  const toggleExpand = (label: string) => {
    setExpandedItems((prev) => {
      const newSet = new Set(prev);
      if (newSet.has(label)) {
        newSet.delete(label);
      } else {
        newSet.add(label);
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
            <div key={item.label}>
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
                    <span className="flex-1 text-left">{item.label}</span>
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
                  onClick={() => toggleExpand(item.label)}
                >
                  <div className={cn(
                    "text-gray-500",
                    isItemActive(item) && "text-[#00b14f]"
                  )}>
                    {item.icon}
                  </div>
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

              {/* Sub-items */}
              {item.items && expandedItems.has(item.label) && (
                <div className="ml-4 space-y-1 border-l border-gray-200 pl-3 py-2">
                  {item.items.map((subItem) => (
                    <Link key={subItem.label} to={subItem.path} onClick={onClose}>
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