import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Briefcase, ChevronDown, LogOut, Menu, Moon, Shield, Sun, X } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { Button } from '../../ui';
import { useAuthStore } from '../../../stores/auth.store';
import { useThemeStore } from '../../../stores/theme.store';
import { LanguageSwitcher } from '../../layout/LanguageSwitcher';
import { adminPaths } from '../../../lib/adminNavigation';

interface AdminNavbarProps {
    onMenuClick: () => void;
    isMenuOpen: boolean;
}

export function AdminNavbar({ onMenuClick, isMenuOpen }: AdminNavbarProps) {
    const [isProfileOpen, setIsProfileOpen] = useState(false);
    const { t } = useTranslation('admin');
    const { t: tCommon } = useTranslation('common');
    const { user, logout } = useAuthStore();
    const { theme, toggleTheme } = useThemeStore();
    const navigate = useNavigate();

    const displayName = user?.fullName?.trim() || t('nav.fallbackUser');
    const displayInitial = displayName.charAt(0).toUpperCase();
    const displayFirstName = displayName.split(' ')[0];

    const handleLogout = () => {
        logout();
        navigate('/');
        setIsProfileOpen(false);
    };

    return (
        <nav className="fixed left-0 right-0 top-0 z-40 h-16 border-b border-gray-200 bg-white/95 backdrop-blur supports-[backdrop-filter]:bg-white/60">
            <div className="flex h-full items-center justify-between px-6">
                <div className="flex items-center gap-4">
                    <Button variant="ghost" size="icon" className="md:hidden" onClick={onMenuClick}>
                        {isMenuOpen ? <X className="h-5 w-5" /> : <Menu className="h-5 w-5" />}
                    </Button>

                    <Link to={adminPaths.dashboard} className="flex items-center gap-2">
                        <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-slate-800">
                            <Shield className="h-5 w-5 text-white" />
                        </div>
                        <span className="text-xl font-bold">
                            <span className="text-gray-900">Can</span>
                            <span className="text-[#00b14f]">Pany</span>
                        </span>
                    </Link>

                    <div className="hidden items-center gap-1.5 text-sm text-gray-500 lg:flex">
                        <Briefcase className="h-4 w-4" />
                        {t('nav.workspaceBadge')}
                    </div>
                </div>

                <div className="flex items-center gap-3">
                    <LanguageSwitcher />
                    <button
                        type="button"
                        onClick={toggleTheme}
                        className="rounded-lg p-2 text-gray-500 transition-colors hover:bg-gray-100 hover:text-[#00b14f]"
                        aria-label={theme === 'light' ? t('nav.darkMode') : t('nav.lightMode')}
                    >
                        {theme === 'light' ? <Moon className="h-5 w-5" /> : <Sun className="h-5 w-5" />}
                    </button>

                    <div className="relative">
                        <button
                            type="button"
                            onClick={() => setIsProfileOpen((current) => !current)}
                            className="flex items-center gap-2 rounded-lg border border-gray-200 bg-white px-3 py-2 transition-colors hover:border-[#00b14f]/30 hover:bg-gray-50"
                        >
                            <div className="flex h-7 w-7 items-center justify-center rounded-full bg-slate-800 text-xs font-semibold text-white">
                                {displayInitial}
                            </div>
                            <span className="hidden text-sm font-medium text-gray-700 sm:block">{displayFirstName}</span>
                            <ChevronDown className="h-4 w-4 text-gray-400" />
                        </button>

                        {isProfileOpen && (
                            <>
                                <button
                                    type="button"
                                    className="fixed inset-0 z-10"
                                    aria-label={tCommon('buttons.close')}
                                    onClick={() => setIsProfileOpen(false)}
                                />
                                <div className="absolute right-0 z-20 mt-2 w-56 rounded-xl border border-gray-100 bg-white py-2 shadow-lg">
                                    <div className="border-b border-gray-100 px-4 py-3">
                                        <p className="text-sm font-semibold text-gray-900">{displayName}</p>
                                        <p className="text-xs text-gray-500">{user?.email}</p>
                                    </div>
                                    <button
                                        type="button"
                                        onClick={handleLogout}
                                        className="flex w-full items-center gap-2 px-4 py-2 text-sm text-red-600 hover:bg-red-50"
                                    >
                                        <LogOut className="h-4 w-4" />
                                        {t('nav.logout')}
                                    </button>
                                </div>
                            </>
                        )}
                    </div>
                </div>
            </div>
        </nav>
    );
}
