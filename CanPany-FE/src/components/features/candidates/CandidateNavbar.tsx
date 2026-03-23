import { Bell, Search, Bot, LogOut, Settings, User, Menu, X, Sun, Moon, ChevronDown, Briefcase, BellOff, CheckCheck, Check } from 'lucide-react';
import { Link, useNavigate } from 'react-router-dom';
import { useState, useRef, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Button } from '../../ui/Button';
import { useAuthStore } from '../../../stores/auth.store';
import { useThemeStore } from '../../../stores/theme.store';
import { useNotifications } from '../../../hooks/useNotifications';
import type { NotificationItem } from '../../../types/notification.types';
import { LanguageSwitcher } from '../../layout/LanguageSwitcher';
import { cn } from '../../../utils';

interface CandidateNavbarProps {
  onMenuClick: () => void;
  isMenuOpen: boolean;
}

function NotificationPanel({
  notifications,
  unreadCount,
  onMarkAsRead,
  onMarkAllAsRead,
  isMarkingAllAsRead,
  onClose,
  t,
  timeAgo,
}: {
  notifications: NotificationItem[];
  unreadCount: number;
  onMarkAsRead: (id: string) => void;
  onMarkAllAsRead: () => void;
  isMarkingAllAsRead: boolean;
  onClose: () => void;
  t: (key: string, opts?: { count?: number }) => string;
  timeAgo: (date: string | Date) => string;
}) {
  return (
    <div className="absolute right-0 top-full mt-2 w-80 rounded-xl border border-gray-100 bg-white shadow-xl z-50">
      <div className="flex items-center justify-between border-b border-gray-100 px-4 py-3">
        <div className="flex items-center gap-2">
          <span className="font-semibold text-gray-900 text-sm">{t('notificationsPanel.title')}</span>
          {unreadCount > 0 && (
            <span className="rounded-full bg-[#00b14f] px-1.5 py-0.5 text-xs font-semibold text-white leading-none">
              {unreadCount}
            </span>
          )}
        </div>
        {unreadCount > 0 && (
          <button
            onClick={onMarkAllAsRead}
            disabled={isMarkingAllAsRead}
            className="text-xs text-[#00b14f] hover:underline flex items-center gap-1"
          >
            <CheckCheck className="h-3 w-3" />
            {t('notificationsPanel.markAllRead')}
          </button>
        )}
      </div>

      <div className="max-h-80 overflow-y-auto">
        {notifications.length === 0 ? (
          <div className="flex flex-col items-center justify-center py-10 text-center">
            <BellOff className="h-8 w-8 text-gray-300 mb-2" />
            <p className="text-sm text-gray-500">{t('notificationsPanel.empty')}</p>
          </div>
        ) : (
          notifications.slice(0, 8).map((n) => (
            <div
              key={n.id}
              className={cn(
                'flex items-start gap-3 px-4 py-3 border-b border-gray-50 last:border-0',
                !n.isRead ? 'bg-[#00b14f]/[0.03]' : ''
              )}
            >
              <div className="mt-0.5 flex-1 min-w-0">
                <p className={cn('text-xs', !n.isRead ? 'font-semibold text-gray-900' : 'text-gray-700')}>
                  {n.title}
                </p>
                <p className="mt-0.5 text-xs text-gray-500 line-clamp-2">{n.content}</p>
                <p className="mt-1 text-xs text-gray-400">{timeAgo(n.timestamp)}</p>
              </div>
              {!n.isRead && (
                <button
                  onClick={() => onMarkAsRead(n.id)}
                  className="mt-1 shrink-0 rounded-full p-1 text-gray-300 hover:bg-gray-100 hover:text-[#00b14f]"
                  title={t('notificationsPanel.markAsRead')}
                >
                  <Check className="h-3 w-3" />
                </button>
              )}
            </div>
          ))
        )}
      </div>

      <div className="border-t border-gray-100 p-2">
        <Link
          to="/candidate/notifications"
          onClick={onClose}
          className="flex w-full items-center justify-center rounded-lg py-2 text-sm font-medium text-[#00b14f] transition-colors hover:bg-[#00b14f]/5"
        >
          {t('notificationsPanel.viewAll')}
        </Link>
      </div>
    </div>
  );
}

export function CandidateNavbar({ onMenuClick, isMenuOpen }: CandidateNavbarProps) {
  const { t } = useTranslation('candidate');
  const [isProfileOpen, setIsProfileOpen] = useState(false);
  const [isNotifOpen, setIsNotifOpen] = useState(false);
  const { user, logout } = useAuthStore();
  const { theme, toggleTheme } = useThemeStore();
  const navigate = useNavigate();
  const notifRef = useRef<HTMLDivElement>(null);

  const { notifications, unreadCount, markAsRead, markAllAsRead, isMarkingAllAsRead } = useNotifications({
    enabled: true,
  });

  const timeAgo = (date: string | Date): string => {
    const diff = (Date.now() - new Date(date).getTime()) / 1000;
    if (diff < 60) return t('notificationsPanel.timeAgoJustNow');
    if (diff < 3600) return t('notificationsPanel.timeAgoMinutes', { count: Math.floor(diff / 60) });
    if (diff < 86400) return t('notificationsPanel.timeAgoHours', { count: Math.floor(diff / 3600) });
    return t('notificationsPanel.timeAgoDays', { count: Math.floor(diff / 86400) });
  };

  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (notifRef.current && !notifRef.current.contains(event.target as Node)) {
        setIsNotifOpen(false);
      }
    }
    if (isNotifOpen) document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, [isNotifOpen]);

  const handleLogout = () => {
    logout();
    navigate('/');
    setIsProfileOpen(false);
  };

  return (
    <nav className="fixed top-0 left-0 right-0 h-16 border-b border-gray-200 bg-white/95 backdrop-blur supports-[backdrop-filter]:bg-white/60 dark:border-gray-700 dark:bg-gray-900/95 z-40">
      <div className="flex h-full items-center justify-between px-6">
        {/* Left: Logo and Mobile Menu */}
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" className="md:hidden" onClick={onMenuClick}>
            {isMenuOpen ? <X className="h-5 w-5" /> : <Menu className="h-5 w-5" />}
          </Button>
          <Link to="/" className="flex items-center gap-2">
            <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-[#00b14f]">
              <Briefcase className="h-5 w-5 text-white" />
            </div>
            <span className="text-xl font-bold">
              <span className="text-gray-900 dark:text-white">Can</span>
              <span className="text-[#00b14f]">Pany</span>
            </span>
          </Link>
        </div>

        {/* Center: Search */}
        <div className="hidden md:flex flex-1 max-w-sm mx-8">
          <div className="relative w-full">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
            <input
              type="text"
              placeholder={t('nav.searchPlaceholder')}
              className="w-full pl-10 pr-4 py-2 rounded-lg border border-gray-300 bg-white text-sm placeholder:text-gray-500 focus:outline-none focus:ring-2 focus:ring-[#00b14f]/20 focus:border-[#00b14f] dark:border-gray-600 dark:bg-gray-800 dark:text-white dark:placeholder:text-gray-400"
            />
          </div>
        </div>

        {/* Right: Actions */}
        <div className="flex items-center gap-3">
          <LanguageSwitcher />
          <Button
            variant="outline"
            size="sm"
            className="hidden sm:flex gap-2 bg-transparent border-gray-300 hover:bg-[#00b14f] hover:text-white"
          >
            <Bot className="h-4 w-4" />
            <span>{t('nav.aiAdvisor')}</span>
          </Button>

          <button
            onClick={toggleTheme}
            className="rounded-lg p-2 text-gray-500 transition-colors hover:bg-gray-100 hover:text-[#00b14f] dark:text-gray-400 dark:hover:bg-gray-800 dark:hover:text-[#00b14f]"
            title={theme === 'light' ? t('nav.darkMode') : t('nav.lightMode')}
          >
            {theme === 'light' ? <Moon className="h-5 w-5" /> : <Sun className="h-5 w-5" />}
          </button>

          {/* Notifications */}
          <div className="relative" ref={notifRef}>
            <Button
              variant="ghost"
              size="icon"
              className="relative"
              onClick={() => setIsNotifOpen((prev) => !prev)}
            >
              <Bell className="h-5 w-5" />
              {unreadCount > 0 && (
                <span className="absolute -right-0.5 -top-0.5 flex h-4 w-4 items-center justify-center rounded-full bg-[#00b14f] text-[10px] font-bold text-white">
                  {unreadCount > 9 ? '9+' : unreadCount}
                </span>
              )}
            </Button>

            {isNotifOpen && (
              <NotificationPanel
                notifications={notifications}
                unreadCount={unreadCount}
                onMarkAsRead={markAsRead}
                onMarkAllAsRead={markAllAsRead}
                isMarkingAllAsRead={isMarkingAllAsRead}
                onClose={() => setIsNotifOpen(false)}
                t={t as (key: string, opts?: { count?: number }) => string}
                timeAgo={timeAgo}
              />
            )}
          </div>

          <div className="relative">
            <button
              onClick={() => setIsProfileOpen(!isProfileOpen)}
              className="flex items-center gap-2 rounded-lg border border-gray-200 bg-white px-3 py-2 transition-colors hover:border-[#00b14f]/30 hover:bg-gray-50 dark:border-gray-700 dark:bg-gray-800 dark:hover:bg-gray-700"
            >
              {user?.avatarUrl ? (
                <img src={user.avatarUrl} alt={user.fullName} className="w-7 h-7 rounded-full" />
              ) : (
                <div className="flex h-7 w-7 items-center justify-center rounded-full bg-[#00b14f] text-xs font-semibold text-white">
                  {user?.fullName.charAt(0).toUpperCase()}
                </div>
              )}
              <span className="text-sm font-medium text-gray-700 dark:text-gray-300">{user?.fullName.split(' ')[0]}</span>
              <ChevronDown className="h-4 w-4 text-gray-400" />
            </button>
            {isProfileOpen && (
              <div className="absolute right-0 mt-2 w-56 rounded-xl border border-gray-100 bg-white py-2 shadow-xl dark:border-gray-700 dark:bg-gray-800">
                <div className="border-b border-gray-100 px-4 pb-3 pt-1 dark:border-gray-700">
                  <p className="font-semibold text-gray-900 dark:text-white">{user?.fullName}</p>
                  <p className="text-sm text-gray-500 dark:text-gray-400">{user?.email}</p>
                </div>
                <Link
                  to="/candidate/dashboard"
                  className="flex items-center gap-3 px-4 py-2.5 text-sm text-gray-700 transition-colors hover:bg-gray-50 hover:text-[#00b14f] dark:text-gray-300 dark:hover:bg-gray-700"
                  onClick={() => setIsProfileOpen(false)}
                >
                  <User className="h-4 w-4" />
                  {t('nav.dashboard')}
                </Link>
                <Link
                  to="/candidate/settings"
                  className="flex items-center gap-3 px-4 py-2.5 text-sm text-gray-700 transition-colors hover:bg-gray-50 hover:text-[#00b14f] dark:text-gray-300 dark:hover:bg-gray-700"
                  onClick={() => setIsProfileOpen(false)}
                >
                  <Settings className="h-4 w-4" />
                  {t('nav.settings')}
                </Link>
                <div className="my-1 border-t border-gray-100 dark:border-gray-700" />
                <button
                  onClick={handleLogout}
                  className="flex w-full items-center gap-3 px-4 py-2.5 text-sm text-red-600 transition-colors hover:bg-red-50 dark:hover:bg-red-900/20"
                >
                  <LogOut className="h-4 w-4" />
                  {t('nav.logout')}
                </button>
              </div>
            )}
          </div>
        </div>
      </div>
    </nav>
  );
}
