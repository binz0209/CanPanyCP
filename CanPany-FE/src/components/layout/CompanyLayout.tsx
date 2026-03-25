import { useState } from 'react';
import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { CompanyNavbar, CompanySidebar } from '../features/companies';
import { companiesApi } from '../../api';
import { companyKeys } from '../../lib/queryKeys';
import { useAuthStore } from '../../stores/auth.store';
import { isAppRole } from '@/lib/userRole';

export function CompanyLayout() {
    const [sidebarOpen, setSidebarOpen] = useState(false);
    const location = useLocation();
    const { isAuthenticated, isLoading, user } = useAuthStore();
    useQuery({
        queryKey: companyKeys.me(),
        queryFn: () => companiesApi.getMe(),
        enabled: isAuthenticated && isAppRole(user?.role, 'Company'),
        retry: false,
    });

    if (isLoading) {
        return (
            <div className="flex min-h-screen items-center justify-center bg-gray-50">
                <div className="h-10 w-10 animate-spin rounded-full border-b-2 border-[#00b14f]" />
            </div>
        );
    }

    if (!isAuthenticated || !user) {
        return <Navigate to="/auth/login" replace state={{ from: location }} />;
    }

    if (!isAppRole(user.role, 'Company')) {
        if (isAppRole(user.role, 'Admin')) {
            return <Navigate to="/admin/dashboard" replace />;
        }
        const fallbackPath = isAppRole(user.role, 'Candidate') ? '/candidate/dashboard' : '/';
        return <Navigate to={fallbackPath} replace />;
    }

    return (
        <div className="min-h-screen bg-gray-50">
            <CompanyNavbar
                onMenuClick={() => setSidebarOpen((current) => !current)}
                isMenuOpen={sidebarOpen}
            />
            <div className="flex">
                <CompanySidebar
                    isOpen={sidebarOpen}
                    onClose={() => setSidebarOpen(false)}
                />
                <main className="flex-1 pt-16 md:pl-64">
                    <div className="p-4 md:p-8">
                        <Outlet />
                    </div>
                </main>
            </div>
        </div>
    );
}
