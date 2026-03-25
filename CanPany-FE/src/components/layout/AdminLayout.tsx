import { useState } from 'react';
import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { AdminNavbar, AdminSidebar } from '../features/admin';
import { useAuthStore } from '../../stores/auth.store';
import { isAppRole } from '@/lib/userRole';

export function AdminLayout() {
    const [sidebarOpen, setSidebarOpen] = useState(false);
    const location = useLocation();
    const { isAuthenticated, isLoading, user } = useAuthStore();

    if (isLoading) {
        return (
            <div className="flex min-h-screen items-center justify-center bg-gray-50">
                <div className="h-10 w-10 animate-spin rounded-full border-b-2 border-slate-800" />
            </div>
        );
    }

    if (!isAuthenticated || !user) {
        return <Navigate to="/auth/login" replace state={{ from: location }} />;
    }

    if (!isAppRole(user.role, 'Admin')) {
        const fallback = isAppRole(user.role, 'Candidate')
            ? '/candidate/dashboard'
            : isAppRole(user.role, 'Company')
              ? '/company/dashboard'
              : '/';
        return <Navigate to={fallback} replace />;
    }

    return (
        <div className="min-h-screen bg-gray-50">
            <AdminNavbar onMenuClick={() => setSidebarOpen((current) => !current)} isMenuOpen={sidebarOpen} />
            <div className="flex">
                <AdminSidebar isOpen={sidebarOpen} onClose={() => setSidebarOpen(false)} />
                <main className="flex-1 pt-16 md:pl-64">
                    <div className="p-4 md:p-8">
                        <Outlet />
                    </div>
                </main>
            </div>
        </div>
    );
}
