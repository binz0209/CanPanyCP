import { Link, useLocation, useNavigate } from 'react-router-dom';
import { Bell, Menu, X, User, LogOut, Briefcase, Settings, ChevronDown, Sun, Moon, LayoutDashboard, BellOff, CheckCheck, Check } from 'lucide-react';
import { useState, useRef, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Button } from '@/components/ui';
import { LanguageSwitcher } from './LanguageSwitcher';
import { useAuthStore } from '@/stores/auth.store';
import { useThemeStore } from '@/stores/theme.store';
import { useNotifications } from '@/hooks/useNotifications';

export function Navbar() {
    const { t } = useTranslation('common');
    const { t: tC } = useTranslation('candidate');
    const [isMenuOpen, setIsMenuOpen] = useState(false);
    const [isProfileOpen, setIsProfileOpen] = useState(false);
    const [isNotifOpen, setIsNotifOpen] = useState(false);
    const { user, isAuthenticated, logout } = useAuthStore();
    const { theme, toggleTheme } = useThemeStore();
    const navigate = useNavigate();
    const location = useLocation();

    const profileRef = useRef<HTMLDivElement>(null);
    const notifRef = useRef<HTMLDivElement>(null);

    const { notifications, unreadCount, markAsRead, markAllAsRead, isMarkingAllAsRead } = useNotifications({
        enabled: isAuthenticated && !!user,
    });

    const displayName = user?.fullName?.trim() || t('nav.login');
    const displayInitial = displayName.charAt(0).toUpperCase();
    const displayFirstName = displayName.split(' ')[0];

    // Close profile dropdown when clicking outside
    useEffect(() => {
        function handleClickOutside(event: MouseEvent) {
            if (profileRef.current && !profileRef.current.contains(event.target as Node)) {
                setIsProfileOpen(false);
            }
        }
        if (isProfileOpen) document.addEventListener('mousedown', handleClickOutside);
        return () => document.removeEventListener('mousedown', handleClickOutside);
    }, [isProfileOpen]);

    // Close notification dropdown when clicking outside
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

    const getDashboardLink = () => {
        if (!user) return '/';
        switch (user.role) {
            case 'Candidate':
                return '/candidate/dashboard';
            case 'Company':
                return '/company/dashboard';
            case 'Admin':
                return '/admin/dashboard';
            default:
                return '/';
        }
    };

    const timeAgo = (date: string | Date): string => {
        const diff = (Date.now() - new Date(date).getTime()) / 1000;
        if (diff < 60) return tC('notificationsPanel.timeAgoJustNow');
        if (diff < 3600) return tC('notificationsPanel.timeAgoMinutes', { count: Math.floor(diff / 60) });
        if (diff < 86400) return tC('notificationsPanel.timeAgoHours', { count: Math.floor(diff / 3600) });
        return tC('notificationsPanel.timeAgoDays', { count: Math.floor(diff / 86400) });
    };

    return (
        <nav className="sticky top-0 z-50 border-b border-gray-200 bg-white dark:border-gray-700 dark:bg-gray-900">
            <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
                <div className="flex h-16 items-center justify-between">
                    {/* Logo */}
                    <div className="flex items-center gap-8">
                        <Link to="/" className="flex items-center gap-2">
                            <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-[#00b14f]">
                                <Briefcase className="h-5 w-5 text-white" />
                            </div>
                            <span className="text-xl font-bold">
                                <span className="text-gray-900 dark:text-white">Can</span>
                                <span className="text-[#00b14f]">Pany</span>
                            </span>
                        </Link>

                        {/* Desktop Navigation */}
                        <div className="hidden items-center gap-6 lg:flex">
                            {isAuthenticated && user && (
                                <Link
                                    to={getDashboardLink()}
                                    className="text-sm font-medium text-gray-700 transition-colors hover:text-[#00b14f] dark:text-gray-300 dark:hover:text-[#00b14f]"
                                >
                                    {t('nav.candidateDashboard')}
                                </Link>
                            )}
                            <Link
                                to="/jobs"
                                className="text-sm font-medium text-gray-700 transition-colors hover:text-[#00b14f] dark:text-gray-300 dark:hover:text-[#00b14f]"
                            >
                                {t('nav.jobs')}
                            </Link>
                            <Link
                                to="/companies"
                                className="text-sm font-medium text-gray-700 transition-colors hover:text-[#00b14f] dark:text-gray-300 dark:hover:text-[#00b14f]"
                            >
                                {t('nav.companies')}
                            </Link>
                            <Link
                                to="/candidate/cv/list"
                                className="text-sm font-medium text-gray-700 transition-colors hover:text-[#00b14f] dark:text-gray-300 dark:hover:text-[#00b14f]"
                            >
                                {t('nav.cvProfile')}
                            </Link>
                        </div>
                    </div>

                    {/* Desktop Actions */}
                    <div className="hidden items-center gap-3 lg:flex">
                        <LanguageSwitcher />
                        {isAuthenticated && user ? (
                            <>
                                {/* Notification Bell with Dropdown */}
                                <div className="relative" ref={notifRef}>
                                    <button
                                        className="relative rounded-full p-2 transition-colors text-gray-500 hover:bg-gray-100 hover:text-[#00b14f] dark:text-gray-400 dark:hover:bg-gray-800"
                                        onClick={() => setIsNotifOpen((prev) => !prev)}
                                    >
                                        <Bell className="h-5 w-5" />
                                        {unreadCount > 0 && (
                                            <span className="absolute right-1 top-1 flex h-4 w-4 items-center justify-center rounded-full bg-[#00b14f] text-[10px] font-bold text-white ring-2 ring-white dark:ring-gray-900">
                                                {unreadCount > 9 ? '9+' : unreadCount}
                                            </span>
                                        )}
                                    </button>

                                    {isNotifOpen && (
                                        <div className="absolute right-0 top-full mt-2 w-80 rounded-xl border border-gray-100 bg-white shadow-xl z-50 dark:border-gray-700 dark:bg-gray-800">
                                            <div className="flex items-center justify-between border-b border-gray-100 px-4 py-3 dark:border-gray-700">
                                                <div className="flex items-center gap-2">
                                                    <span className="font-semibold text-gray-900 text-sm dark:text-white">{tC('notificationsPanel.title')}</span>
                                                    {unreadCount > 0 && (
                                                        <span className="rounded-full bg-[#00b14f] px-1.5 py-0.5 text-xs font-semibold text-white leading-none">
                                                            {unreadCount}
                                                        </span>
                                                    )}
                                                </div>
                                                {unreadCount > 0 && (
                                                    <button
                                                        onClick={markAllAsRead}
                                                        disabled={isMarkingAllAsRead}
                                                        className="text-xs text-[#00b14f] hover:underline flex items-center gap-1"
                                                    >
                                                        <CheckCheck className="h-3 w-3" />
                                                        {tC('notificationsPanel.markAllRead')}
                                                    </button>
                                                )}
                                            </div>

                                            <div className="max-h-80 overflow-y-auto">
                                                {notifications.length === 0 ? (
                                                    <div className="flex flex-col items-center justify-center py-10 text-center">
                                                        <BellOff className="h-8 w-8 text-gray-300 mb-2" />
                                                        <p className="text-sm text-gray-500">{tC('notificationsPanel.empty')}</p>
                                                    </div>
                                                ) : (
                                                    notifications.slice(0, 8).map((n) => (
                                                        <div
                                                            key={n.id}
                                                            className={`flex items-start gap-3 px-4 py-3 border-b border-gray-50 last:border-0 dark:border-gray-700 ${!n.isRead ? 'bg-[#00b14f]/[0.03]' : ''}`}
                                                        >
                                                            <div className="mt-0.5 flex-1 min-w-0">
                                                                <p className={`text-xs ${!n.isRead ? 'font-semibold text-gray-900 dark:text-white' : 'text-gray-700 dark:text-gray-300'}`}>
                                                                    {n.title}
                                                                </p>
                                                                <p className="mt-0.5 text-xs text-gray-500 line-clamp-2">{n.content}</p>
                                                                <p className="mt-1 text-xs text-gray-400">{timeAgo(n.timestamp)}</p>
                                                            </div>
                                                            {!n.isRead && (
                                                                <button
                                                                    onClick={() => markAsRead(n.id)}
                                                                    className="mt-1 shrink-0 rounded-full p-1 text-gray-300 hover:bg-gray-100 hover:text-[#00b14f]"
                                                                    title={tC('notificationsPanel.markAsRead')}
                                                                >
                                                                    <Check className="h-3 w-3" />
                                                                </button>
                                                            )}
                                                        </div>
                                                    ))
                                                )}
                                            </div>

                                            <div className="border-t border-gray-100 p-2 dark:border-gray-700">
                                                <Link
                                                    to="/candidate/notifications"
                                                    onClick={() => setIsNotifOpen(false)}
                                                    className="flex w-full items-center justify-center rounded-lg py-2 text-sm font-medium text-[#00b14f] transition-colors hover:bg-[#00b14f]/5"
                                                >
                                                    {tC('notificationsPanel.viewAll')}
                                                </Link>
                                            </div>
                                        </div>
                                    )}
                                </div>

                                {/* Profile Dropdown */}
                                <div className="relative" ref={profileRef}>
                                    <button
                                        onClick={() => setIsProfileOpen(!isProfileOpen)}
                                        className="flex items-center gap-2 rounded-lg border border-gray-200 bg-white px-3 py-2 transition-colors hover:border-[#00b14f]/30 hover:bg-gray-50 dark:border-gray-700 dark:bg-gray-800 dark:hover:bg-gray-700"
                                    >
                                        {user?.avatarUrl ? (
                                            <img src={user.avatarUrl} alt={displayName} className="h-7 w-7 rounded-full object-cover" />
                                        ) : (
                                            <div className="flex h-7 w-7 items-center justify-center rounded-full bg-[#00b14f] text-xs font-semibold text-white">
                                                {displayInitial}
                                            </div>
                                        )}
                                        <span className="text-sm font-medium text-gray-700 dark:text-gray-300">{displayFirstName}</span>
                                        <ChevronDown className="h-4 w-4 text-gray-400" />
                                    </button>
                                    {isProfileOpen && (
                                        <div className="absolute right-0 mt-2 w-56 rounded-xl border border-gray-100 bg-white py-2 shadow-xl dark:border-gray-700 dark:bg-gray-800">
                                            <div className="border-b border-gray-100 px-4 pb-3 pt-1 dark:border-gray-700">
                                                <p className="font-semibold text-gray-900 dark:text-white">{displayName}</p>
                                                <p className="text-sm text-gray-500 dark:text-gray-400">{user.email}</p>
                                            </div>
                                            <Link
                                                to={getDashboardLink()}
                                                className="flex items-center gap-3 px-4 py-2.5 text-sm text-gray-700 transition-colors hover:bg-gray-50 hover:text-[#00b14f] dark:text-gray-300 dark:hover:bg-gray-700"
                                                onClick={() => setIsProfileOpen(false)}
                                            >
                                                <LayoutDashboard className="h-4 w-4" />
                                                {t('nav.candidateDashboard')}
                                            </Link>
                                            <Link
                                                to={user.role === 'Company' ? '/company/profile' : '/candidate/profile'}
                                                className="flex items-center gap-3 px-4 py-2.5 text-sm text-gray-700 transition-colors hover:bg-gray-50 hover:text-[#00b14f] dark:text-gray-300 dark:hover:bg-gray-700"
                                                onClick={() => setIsProfileOpen(false)}
                                            >
                                                <User className="h-4 w-4" />
                                                {t('userMenu.profile')}
                                            </Link>
                                            <Link
                                                to={user.role === 'Company' ? '/company/settings/account' : '/candidate/settings/account'}
                                                className="flex items-center gap-3 px-4 py-2.5 text-sm text-gray-700 transition-colors hover:bg-gray-50 hover:text-[#00b14f] dark:text-gray-300 dark:hover:bg-gray-700"
                                                onClick={() => setIsProfileOpen(false)}
                                            >
                                                <Settings className="h-4 w-4" />
                                                {t('userMenu.accountSettings', 'Cài đặt tài khoản')}
                                            </Link>
                                            <Link
                                                to="/candidate/cv/list"
                                                className="flex items-center gap-3 px-4 py-2.5 text-sm text-gray-700 transition-colors hover:bg-gray-50 hover:text-[#00b14f] dark:text-gray-300 dark:hover:bg-gray-700"
                                                onClick={() => setIsProfileOpen(false)}
                                            >
                                                <Briefcase className="h-4 w-4" />
                                                {t('nav.cvProfile')}
                                            </Link>
                                            <Link
                                                to="/candidate/premium"
                                                className="flex items-center gap-3 px-4 py-2.5 text-sm text-gray-700 transition-colors hover:bg-gray-50 hover:text-[#00b14f] dark:text-gray-300 dark:hover:bg-gray-700"
                                                onClick={() => setIsProfileOpen(false)}
                                            >
                                                <Settings className="h-4 w-4" />
                                                Premium
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
                            </>
                        ) : (
                            <>
                                <Link to="/auth/login">
                                    <Button variant="outline" className="border-gray-300 text-gray-700 hover:border-[#00b14f] hover:text-[#00b14f] dark:border-gray-600 dark:text-gray-300">
                                        {t('nav.login')}
                                    </Button>
                                </Link>
                                <Link to="/auth/register">
                                    <Button>{t('nav.register')}</Button>
                                </Link>
                                <div className="h-6 w-px bg-gray-200 dark:bg-gray-700" />
                                <Link to="/auth/register?role=company" className="flex items-center gap-2 text-sm font-medium text-gray-700 transition-colors hover:text-[#00b14f] dark:text-gray-300">
                                    {t('nav.companyWorkspace')}
                                    <Settings className="h-4 w-4" />
                                </Link>
                            </>
                        )}

                        {/* Theme Toggle Button */}
                        <button
                            onClick={toggleTheme}
                            className="rounded-lg p-2 text-gray-500 transition-colors hover:bg-gray-100 hover:text-[#00b14f] dark:text-gray-400 dark:hover:bg-gray-800 dark:hover:text-[#00b14f]"
                            title={theme === 'light' ? t('nav.darkMode') : t('nav.lightMode')}
                        >
                            {theme === 'light' ? (
                                <Moon className="h-5 w-5" />
                            ) : (
                                <Sun className="h-5 w-5" />
                            )}
                        </button>
                    </div>

                    {/* Mobile menu button */}
                    <div className="flex items-center gap-2 lg:hidden">
                        <button
                            onClick={toggleTheme}
                            className="rounded-lg p-2 text-gray-500 transition-colors hover:bg-gray-100 dark:text-gray-400 dark:hover:bg-gray-800"
                        >
                            {theme === 'light' ? <Moon className="h-5 w-5" /> : <Sun className="h-5 w-5" />}
                        </button>
                        <button
                            className="rounded-lg p-2 text-gray-600 hover:bg-gray-100 dark:text-gray-400 dark:hover:bg-gray-800"
                            onClick={() => setIsMenuOpen(!isMenuOpen)}
                        >
                            {isMenuOpen ? <X className="h-6 w-6" /> : <Menu className="h-6 w-6" />}
                        </button>
                    </div>
                </div>
            </div>

            {/* Mobile menu */}
            {isMenuOpen && (
                <div className="border-t border-gray-100 bg-white dark:border-gray-700 dark:bg-gray-900 lg:hidden">
                    <div className="space-y-1 px-4 py-4">
                        <Link
                            to="/jobs"
                            className="block rounded-lg px-4 py-3 text-base font-medium text-gray-700 hover:bg-gray-50 hover:text-[#00b14f] dark:text-gray-300 dark:hover:bg-gray-800"
                            onClick={() => setIsMenuOpen(false)}
                        >
                            {t('nav.jobs')}
                        </Link>
                        <Link
                            to="/companies"
                            className="block rounded-lg px-4 py-3 text-base font-medium text-gray-700 hover:bg-gray-50 hover:text-[#00b14f] dark:text-gray-300 dark:hover:bg-gray-800"
                            onClick={() => setIsMenuOpen(false)}
                        >
                            {t('nav.companies')}
                        </Link>
                        <Link
                            to="/candidate/cv/list"
                            className="block rounded-lg px-4 py-3 text-base font-medium text-gray-700 hover:bg-gray-50 hover:text-[#00b14f] dark:text-gray-300 dark:hover:bg-gray-800"
                            onClick={() => setIsMenuOpen(false)}
                        >
                            {t('nav.cvProfile')}
                        </Link>
                        {isAuthenticated && user && (
                            <Link
                                to={getDashboardLink()}
                                className="block rounded-lg px-4 py-3 text-base font-medium text-gray-700 hover:bg-gray-50 hover:text-[#00b14f] dark:text-gray-300 dark:hover:bg-gray-800"
                                onClick={() => setIsMenuOpen(false)}
                            >
                                {t('nav.candidateDashboard')}
                            </Link>
                        )}
                    </div>
                    {!isAuthenticated && (
                        <div className="border-t border-gray-100 px-4 py-4 dark:border-gray-700">
                            <div className="flex flex-col gap-2">
                                <Link to="/auth/login" onClick={() => setIsMenuOpen(false)}>
                                    <Button variant="outline" className="w-full">
                                        {t('nav.login')}
                                    </Button>
                                </Link>
                                <Link to="/auth/register" onClick={() => setIsMenuOpen(false)}>
                                    <Button className="w-full">{t('nav.register')}</Button>
                                </Link>
                                <Link
                                    to="/auth/register?role=company"
                                    onClick={() => setIsMenuOpen(false)}
                                    className="mt-2 flex items-center justify-center gap-2 text-sm font-medium text-gray-700 dark:text-gray-300"
                                >
                                    <Settings className="h-4 w-4" />
                                    {t('nav.companyWorkspace')}
                                </Link>
                            </div>
                        </div>
                    )}
                </div>
            )}
        </nav>
    );
}
