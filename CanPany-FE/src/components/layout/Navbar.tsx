import { Link, useNavigate } from 'react-router-dom';
import { Bell, Menu, X, User, LogOut, Briefcase, Settings, ChevronDown, Sun, Moon } from 'lucide-react';
import { useState } from 'react';
import { Button, Card, Badge } from '@/components/ui';
import { useAuthStore } from '@/stores/auth.store';
import { useThemeStore } from '@/stores/theme.store';
import { useNotifications } from '@/hooks/useNotifications';
import { formatRelativeTime } from '@/utils';

export function Navbar() {
    const [isMenuOpen, setIsMenuOpen] = useState(false);
    const [isProfileOpen, setIsProfileOpen] = useState(false);
    const [isNotificationOpen, setIsNotificationOpen] = useState(false);
    const { user, isAuthenticated, logout } = useAuthStore();
    const { theme, toggleTheme } = useThemeStore();
    const navigate = useNavigate();

    const displayName = user?.fullName?.trim() || 'Người dùng';
    const displayInitial = displayName.charAt(0).toUpperCase();
    const displayFirstName = displayName.split(' ')[0];

    const {
        notifications,
        unreadCount,
        isFetching: isFetchingNotifications,
        markAsRead,
        markAllAsRead,
        isMarkingAllAsRead,
    } = useNotifications({ enabled: isAuthenticated });

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
                            <Link
                                to="/jobs"
                                className="text-sm font-medium text-gray-700 transition-colors hover:text-[#00b14f] dark:text-gray-300 dark:hover:text-[#00b14f]"
                            >
                                Tìm việc làm
                            </Link>
                            <Link
                                to="/companies"
                                className="text-sm font-medium text-gray-700 transition-colors hover:text-[#00b14f] dark:text-gray-300 dark:hover:text-[#00b14f]"
                            >
                                Danh sách công ty
                            </Link>
                            <Link
                                to="/cv"
                                className="text-sm font-medium text-gray-700 transition-colors hover:text-[#00b14f] dark:text-gray-300 dark:hover:text-[#00b14f]"
                            >
                                Hồ sơ & CV
                            </Link>
                        </div>
                    </div>

                    {/* Desktop Actions */}
                    <div className="hidden items-center gap-3 lg:flex">
                        {isAuthenticated && user ? (
                            <>
                                <div className="relative">
                                    <button
                                        className="relative rounded-full p-2 text-gray-500 transition-colors hover:bg-gray-100 hover:text-[#00b14f] dark:text-gray-400 dark:hover:bg-gray-800"
                                        onClick={() => setIsNotificationOpen((open) => !open)}
                                        aria-label="Xem thông báo"
                                    >
                                        <Bell className="h-5 w-5" />
                                        {unreadCount > 0 && (
                                            <span className="absolute -right-1 -top-1 inline-flex min-h-[18px] min-w-[18px] items-center justify-center rounded-full bg-red-500 px-1 text-[10px] font-semibold text-white ring-2 ring-white dark:ring-gray-900">
                                                {unreadCount > 9 ? '9+' : unreadCount}
                                            </span>
                                        )}
                                    </button>

                                    {isNotificationOpen && (
                                        <div className="absolute right-0 mt-2 w-80">
                                            <Card className="max-h-[420px] overflow-hidden border border-gray-100 p-0 shadow-xl dark:border-gray-700">
                                                <div className="flex items-center justify-between border-b border-gray-100 px-4 py-3 dark:border-gray-700">
                                                    <p className="text-sm font-semibold text-gray-900 dark:text-gray-100">
                                                        Thông báo
                                                    </p>
                                                    <div className="flex items-center gap-2">
                                                        {unreadCount > 0 && (
                                                            <button
                                                                type="button"
                                                                onClick={() => markAllAsRead()}
                                                                disabled={isMarkingAllAsRead}
                                                                className="text-xs font-medium text-[#00b14f] hover:underline disabled:cursor-not-allowed disabled:text-gray-400"
                                                            >
                                                                Đánh dấu đã đọc hết
                                                            </button>
                                                        )}
                                                        <Badge variant="secondary">
                                                            {isFetchingNotifications ? 'Đang cập nhật…' : 'Mới nhất'}
                                                        </Badge>
                                                    </div>
                                                </div>

                                                {notifications.length === 0 ? (
                                                    <div className="px-4 py-6 text-sm text-gray-500 dark:text-gray-300">
                                                        Hiện chưa có thông báo mới.
                                                    </div>
                                                ) : (
                                                    <div className="max-h-[340px] overflow-y-auto">
                                                        <ul className="divide-y divide-gray-100 dark:divide-gray-800">
                                                            {notifications.map((notification) => (
                                                                <li
                                                                    key={notification.id}
                                                                    className={`cursor-pointer px-4 py-3 text-sm ${
                                                                        notification.isRead
                                                                            ? 'bg-white dark:bg-gray-900'
                                                                            : 'bg-gray-50 dark:bg-gray-800/80'
                                                                    } hover:bg-gray-100 dark:hover:bg-gray-800`}
                                                                    onClick={() => {
                                                                        if (!notification.isRead) {
                                                                            markAsRead(notification.id);
                                                                        }
                                                                    }}
                                                                >
                                                                    <p className="font-semibold text-gray-900 dark:text-gray-100">
                                                                        {notification.title}
                                                                    </p>
                                                                    <p className="mt-1 text-xs text-gray-600 dark:text-gray-300">
                                                                        {notification.content}
                                                                    </p>
                                                                    <p className="mt-1 text-[11px] text-gray-400 dark:text-gray-500">
                                                                        {formatRelativeTime(notification.timestamp)}
                                                                    </p>
                                                                </li>
                                                            ))}
                                                        </ul>
                                                    </div>
                                                )}
                                            </Card>
                                        </div>
                                    )}
                                </div>
                                <div className="relative">
                                    <button
                                        onClick={() => setIsProfileOpen(!isProfileOpen)}
                                        className="flex items-center gap-2 rounded-lg border border-gray-200 bg-white px-3 py-2 transition-colors hover:border-[#00b14f]/30 hover:bg-gray-50 dark:border-gray-700 dark:bg-gray-800 dark:hover:bg-gray-700"
                                    >
                                        <div className="flex h-7 w-7 items-center justify-center rounded-full bg-[#00b14f] text-xs font-semibold text-white">
                                            {displayInitial}
                                        </div>
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
                                                <User className="h-4 w-4" />
                                                Dashboard
                                            </Link>
                                            <Link
                                                to={`/${user.role.toLowerCase()}/settings`}
                                                className="flex items-center gap-3 px-4 py-2.5 text-sm text-gray-700 transition-colors hover:bg-gray-50 hover:text-[#00b14f] dark:text-gray-300 dark:hover:bg-gray-700"
                                                onClick={() => setIsProfileOpen(false)}
                                            >
                                                <Settings className="h-4 w-4" />
                                                Cài đặt
                                            </Link>
                                            <div className="my-1 border-t border-gray-100 dark:border-gray-700" />
                                            <button
                                                onClick={handleLogout}
                                                className="flex w-full items-center gap-3 px-4 py-2.5 text-sm text-red-600 transition-colors hover:bg-red-50 dark:hover:bg-red-900/20"
                                            >
                                                <LogOut className="h-4 w-4" />
                                                Đăng xuất
                                            </button>
                                        </div>
                                    )}
                                </div>
                            </>
                        ) : (
                            <>
                                <Link to="/auth/login">
                                    <Button variant="outline" className="border-gray-300 text-gray-700 hover:border-[#00b14f] hover:text-[#00b14f] dark:border-gray-600 dark:text-gray-300">
                                        Đăng nhập
                                    </Button>
                                </Link>
                                <Link to="/auth/register">
                                    <Button>Đăng ký</Button>
                                </Link>
                                <div className="h-6 w-px bg-gray-200 dark:bg-gray-700" />
                                <Link to="/auth/register?role=company" className="flex items-center gap-2 text-sm font-medium text-gray-700 transition-colors hover:text-[#00b14f] dark:text-gray-300">
                                    Nhà tuyển dụng
                                    <Settings className="h-4 w-4" />
                                </Link>
                            </>
                        )}

                        {/* Theme Toggle Button */}
                        <button
                            onClick={toggleTheme}
                            className="rounded-lg p-2 text-gray-500 transition-colors hover:bg-gray-100 hover:text-[#00b14f] dark:text-gray-400 dark:hover:bg-gray-800 dark:hover:text-[#00b14f]"
                            title={theme === 'light' ? 'Chế độ tối' : 'Chế độ sáng'}
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
                            Tìm việc làm
                        </Link>
                        <Link
                            to="/companies"
                            className="block rounded-lg px-4 py-3 text-base font-medium text-gray-700 hover:bg-gray-50 hover:text-[#00b14f] dark:text-gray-300 dark:hover:bg-gray-800"
                            onClick={() => setIsMenuOpen(false)}
                        >
                            Danh sách công ty
                        </Link>
                        <Link
                            to="/cv"
                            className="block rounded-lg px-4 py-3 text-base font-medium text-gray-700 hover:bg-gray-50 hover:text-[#00b14f] dark:text-gray-300 dark:hover:bg-gray-800"
                            onClick={() => setIsMenuOpen(false)}
                        >
                            Hồ sơ & CV
                        </Link>
                    </div>
                    {!isAuthenticated && (
                        <div className="border-t border-gray-100 px-4 py-4 dark:border-gray-700">
                            <div className="flex flex-col gap-2">
                                <Link to="/auth/login" onClick={() => setIsMenuOpen(false)}>
                                    <Button variant="outline" className="w-full">
                                        Đăng nhập
                                    </Button>
                                </Link>
                                <Link to="/auth/register" onClick={() => setIsMenuOpen(false)}>
                                    <Button className="w-full">Đăng ký</Button>
                                </Link>
                                <Link
                                    to="/auth/register?role=company"
                                    onClick={() => setIsMenuOpen(false)}
                                    className="mt-2 flex items-center justify-center gap-2 text-sm font-medium text-gray-700 dark:text-gray-300"
                                >
                                    <Settings className="h-4 w-4" />
                                    Nhà tuyển dụng
                                </Link>
                            </div>
                        </div>
                    )}
                </div>
            )}
        </nav>
    );
}
