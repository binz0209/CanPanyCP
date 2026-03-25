import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Briefcase, ChevronDown, LogOut, Menu, Moon, Sun, User, X } from 'lucide-react';
import { Button } from '../../ui';
import { useAuthStore } from '../../../stores/auth.store';
import { useThemeStore } from '../../../stores/theme.store';
import { LanguageSwitcher } from '../../layout/LanguageSwitcher';
import { useTranslation } from 'react-i18next';

interface CompanyNavbarProps {
    onMenuClick: () => void;
    isMenuOpen: boolean;
}

export function CompanyNavbar({ onMenuClick, isMenuOpen }: CompanyNavbarProps) {
    const [isProfileOpen, setIsProfileOpen] = useState(false);
    const { user, logout } = useAuthStore();
    const { theme, toggleTheme } = useThemeStore();
    const navigate = useNavigate();
    const { t } = useTranslation('company');
    const { t: tCommon } = useTranslation('common');

    const displayName = user?.fullName?.trim() || t('navbar.fallbackUser');
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
                    <Button
                        variant="ghost"
                        size="icon"
                        className="md:hidden"
                        onClick={onMenuClick}
                    >
                        {isMenuOpen ? <X className="h-5 w-5" /> : <Menu className="h-5 w-5" />}
                    </Button>

                    <Link to="/" className="flex items-center gap-2">
                        <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-[#00b14f]">
                            <Briefcase className="h-5 w-5 text-white" />
                        </div>
                        <span className="text-xl font-bold">
                            <span className="text-gray-900">Can</span>
                            <span className="text-[#00b14f]">Pany</span>
                        </span>
                    </Link>

                    <div className="hidden text-sm text-gray-500 lg:block">{t('navbar.workspaceBadge')}</div>
                </div>

                <div className="flex items-center gap-3">
                    <LanguageSwitcher />
                    <button
                        onClick={toggleTheme}
                        className="rounded-lg p-2 text-gray-500 transition-colors hover:bg-gray-100 hover:text-[#00b14f]"
                        title={theme === 'light' ? tCommon('nav.darkMode') : tCommon('nav.lightMode')}
                    >
                        {theme === 'light' ? <Moon className="h-5 w-5" /> : <Sun className="h-5 w-5" />}
                    </button>

                    <div className="relative">
                        <button
                            onClick={() => setIsProfileOpen((current) => !current)}
                            className="flex items-center gap-2 rounded-lg border border-gray-200 bg-white px-3 py-2 transition-colors hover:border-[#00b14f]/30 hover:bg-gray-50"
                        >
                            <div className="flex h-7 w-7 items-center justify-center rounded-full bg-[#00b14f] text-xs font-semibold text-white">
                                {displayInitial}
                            </div>
                            <span className="hidden text-sm font-medium text-gray-700 sm:block">
                                {displayFirstName}
                            </span>
                            <ChevronDown className="h-4 w-4 text-gray-400" />
                        </button>

                        {isProfileOpen && (
                            <div className="absolute right-0 mt-2 w-56 rounded-xl border border-gray-100 bg-white py-2 shadow-xl">
                                <div className="border-b border-gray-100 px-4 pb-3 pt-1">
                                    <p className="font-semibold text-gray-900">{displayName}</p>
                                    <p className="text-sm text-gray-500">{user?.email}</p>
                                </div>

                                <Link
                                    to="/company/dashboard"
                                    className="flex items-center gap-3 px-4 py-2.5 text-sm text-gray-700 transition-colors hover:bg-gray-50 hover:text-[#00b14f]"
                                    onClick={() => setIsProfileOpen(false)}
                                >
                                    <User className="h-4 w-4" />
                                    {t('navbar.dashboardLink')}
                                </Link>

                                <div className="my-1 border-t border-gray-100" />

                                <button
                                    onClick={handleLogout}
                                    className="flex w-full items-center gap-3 px-4 py-2.5 text-sm text-red-600 transition-colors hover:bg-red-50"
                                >
                                    <LogOut className="h-4 w-4" />
                                    {t('navbar.logout')}
                                </button>
                            </div>
                        )}
                    </div>
                </div>
            </div>
        </nav>
    );
}
