import { useState } from 'react';
import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { CandidateNavbar, CandidateSidebar } from '../features/candidates';
import { useAuthStore } from '@/stores/auth.store';
import { isAppRole } from '@/lib/userRole';

export function CandidateLayout() {
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const location = useLocation();
  const { isAuthenticated, isLoading, user } = useAuthStore();

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

  if (!isAppRole(user.role, 'Candidate')) {
    if (isAppRole(user.role, 'Admin')) {
      return <Navigate to="/admin/dashboard" replace />;
    }
    if (isAppRole(user.role, 'Company')) {
      return <Navigate to="/company/dashboard" replace />;
    }
    return <Navigate to="/" replace />;
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <CandidateNavbar
        onMenuClick={() => setSidebarOpen(!sidebarOpen)}
        isMenuOpen={sidebarOpen}
      />
      <div className="flex">
        <CandidateSidebar
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