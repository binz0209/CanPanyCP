import { Suspense, lazy } from 'react';
import type { ReactNode } from 'react';
import { BrowserRouter, Routes, Route, Navigate, useLocation } from 'react-router-dom';
import { QueryClientProvider } from '@tanstack/react-query';
import { Toaster } from 'react-hot-toast';
import { queryClient } from '@/lib/queryClient';
import { companyPaths } from '@/lib/companyNavigation';
import { PublicLayout, CandidateLayout, CompanyLayout } from '@/components/layout';
import { HomePageDemo, JobsPage, JobDetailPage, CompaniesPage, CompanyDetailPage } from '@/pages/public';
import { LoginPage, RegisterPage, ForgotPasswordPage, ResetPasswordPage } from '@/pages/auth';
import { CandidateProfilePage, CandidateDashboardPage, CVListPage, AICVPage, CVEditorPage, ApplicationHistoryPage, SavedJobsPage, JobAlertsPage, NotificationCenterPage, BackgroundJobsPage, RecommendedJobsPage, CandidateMessagesPage, WalletPage } from '@/pages/candidate';


const CompanyDashboardPage = lazy(() =>
  import('@/pages/company/CompanyDashboardPage').then((module) => ({
    default: module.CompanyDashboardPage,
  }))
);
const CompanyProfilePage = lazy(() =>
  import('@/pages/company/CompanyProfilePage').then((module) => ({
    default: module.CompanyProfilePage,
  }))
);
const CompanyVerificationPage = lazy(() =>
  import('@/pages/company/CompanyVerificationPage').then((module) => ({
    default: module.CompanyVerificationPage,
  }))
);
const CompanyJobsPage = lazy(() =>
  import('@/pages/company/CompanyJobsPage').then((module) => ({
    default: module.CompanyJobsPage,
  }))
);
const CompanyJobFormPage = lazy(() =>
  import('@/pages/company/CompanyJobFormPage').then((module) => ({
    default: module.CompanyJobFormPage,
  }))
);
const CompanyCandidateSearchPage = lazy(() =>
  import('@/pages/company/CompanyCandidateSearchPage').then((module) => ({
    default: module.CompanyCandidateSearchPage,
  }))
);
const CompanyApplicationsPage = lazy(() =>
  import('@/pages/company/CompanyApplicationsPage').then((module) => ({
    default: module.CompanyApplicationsPage,
  }))
);
const CompanyApplicationDetailPage = lazy(() =>
  import('@/pages/company/CompanyApplicationDetailPage').then((module) => ({
    default: module.CompanyApplicationDetailPage,
  }))
);
const CompanyMessagesPage = lazy(() =>
  import('@/pages/company/CompanyMessagesPage').then((module) => ({
    default: module.CompanyMessagesPage,
  }))
);

function RouteLoader() {
  return (
    <div className="flex min-h-[40vh] items-center justify-center px-4">
      <div className="text-center">
        <div className="mx-auto h-10 w-10 animate-spin rounded-full border-4 border-[#00b14f]/20 border-t-[#00b14f]" />
        <p className="mt-4 text-sm text-gray-500">Đang tải trang...</p>
      </div>
    </div>
  );
}

function LazyRoute({ children }: { children: ReactNode }) {
  return <Suspense fallback={<RouteLoader />}>{children}</Suspense>;
}

// Redirect /profile → /candidate/profile (preserving query params from GitHub OAuth callback)
function ProfileRedirect() {
  const { search } = useLocation();
  return <Navigate to={`/candidate/profile${search}`} replace />;
}

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Routes>
          {/* Public Routes with Layout */}
          <Route element={<PublicLayout />}>
            <Route path="/" element={<HomePageDemo />} />
            <Route path="/jobs" element={<JobsPage />} />
            <Route path="/jobs/:id" element={<JobDetailPage />} />
            <Route path="/companies" element={<CompaniesPage />} />
            <Route path="/companies/:id" element={<CompanyDetailPage />} />
          </Route>

          {/* Candidate Routes */}
          <Route element={<CandidateLayout />}>
            <Route path="/candidate/dashboard" element={<CandidateDashboardPage />} />
            <Route path="/candidate/profile" element={<CandidateProfilePage />} />
            <Route path="/candidate/cv/list" element={<CVListPage />} />
            <Route path="/candidate/cv/ai" element={<AICVPage />} />
            <Route path="/candidate/applications/history" element={<ApplicationHistoryPage />} />
            <Route path="/candidate/jobs/bookmarks" element={<SavedJobsPage />} />
            <Route path="/candidate/jobs/recommended" element={<RecommendedJobsPage />} />
            <Route path="/candidate/job-alerts" element={<JobAlertsPage />} />
            <Route path="/candidate/notifications" element={<NotificationCenterPage />} />
            <Route path="/candidate/settings/notifications" element={<NotificationCenterPage />} />
            <Route path="/candidate/wallet" element={<WalletPage />} />
            <Route path="/candidate/background-jobs" element={<BackgroundJobsPage />} />
            <Route path="/candidate/messages" element={<CandidateMessagesPage />} />
            <Route path="/candidate/messages/:conversationId" element={<CandidateMessagesPage />} />
          </Route>

          {/* Company Routes */}
          <Route element={<CompanyLayout />}>
            <Route path={companyPaths.root} element={<Navigate to={companyPaths.dashboard} replace />} />
            <Route path={companyPaths.dashboard} element={<LazyRoute><CompanyDashboardPage /></LazyRoute>} />
            <Route path={companyPaths.profile} element={<LazyRoute><CompanyProfilePage /></LazyRoute>} />
            <Route path={companyPaths.verification} element={<LazyRoute><CompanyVerificationPage /></LazyRoute>} />
            <Route path={companyPaths.jobs} element={<LazyRoute><CompanyJobsPage /></LazyRoute>} />
            <Route path={companyPaths.newJob} element={<LazyRoute><CompanyJobFormPage /></LazyRoute>} />
            <Route path="/company/jobs/:jobId/edit" element={<LazyRoute><CompanyJobFormPage /></LazyRoute>} />
            <Route path={companyPaths.candidateSearch} element={<LazyRoute><CompanyCandidateSearchPage /></LazyRoute>} />
            <Route path={companyPaths.applications} element={<LazyRoute><CompanyApplicationsPage /></LazyRoute>} />
            <Route path="/company/applications/:applicationId" element={<LazyRoute><CompanyApplicationDetailPage /></LazyRoute>} />
            {/* No conversationId → landing page; with conversationId → chat thread */}
            <Route path={companyPaths.messages} element={<LazyRoute><CompanyMessagesPage /></LazyRoute>} />
            <Route path="/company/messages/:conversationId" element={<LazyRoute><CompanyMessagesPage /></LazyRoute>} />
          </Route>

          {/* GitHub OAuth callback — BE redirects to /profile?github_linked=... */}
          <Route path="/profile" element={<ProfileRedirect />} />

          {/* Auth Routes (no layout) */}
          <Route path="/auth/login" element={<LoginPage />} />
          <Route path="/auth/register" element={<RegisterPage />} />
          <Route path="/auth/forgot-password" element={<ForgotPasswordPage />} />
          <Route path="/auth/reset-password" element={<ResetPasswordPage />} />

          {/* 404 */}
          <Route path="*" element={
            <div className="flex min-h-screen items-center justify-center">
              <div className="text-center">
                <h1 className="text-6xl font-bold text-gray-900">404</h1>
                <p className="mt-4 text-gray-600">Trang không tồn tại</p>
                <a href="/" className="mt-4 inline-block text-blue-600 hover:underline">
                  Về trang chủ
                </a>
              </div>
            </div>
          } />
        </Routes>
      </BrowserRouter>
      <Toaster
        position="top-right"
        toastOptions={{
          duration: 4000,
          style: {
            borderRadius: '10px',
            background: '#333',
            color: '#fff',
          },
        }}
      />
    </QueryClientProvider>
  );
}

export default App;
