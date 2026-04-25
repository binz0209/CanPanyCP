import { Suspense, lazy } from 'react';
import type { ReactNode } from 'react';
import { BrowserRouter, Routes, Route, Navigate, useLocation, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { QueryClientProvider } from '@tanstack/react-query';
import { Toaster } from 'react-hot-toast';
import { queryClient } from '@/lib/queryClient';
import { companyPaths } from '@/lib/companyNavigation';
import { PublicLayout, CandidateLayout, CompanyLayout, AdminLayout } from '@/components/layout';
import { HomePage, JobsPage, JobDetailPage, CompaniesPage, CompanyDetailPage } from '@/pages/public';
import { LoginPage, RegisterPage, ForgotPasswordPage, ResetPasswordPage, AuthCallbackPage } from '@/pages/auth';
import { 
  CandidateProfilePage, CandidateDashboardPage, CVListPage, AICVPage, 
  ApplicationHistoryPage, SavedJobsPage, NotificationsPage, WalletPage, 
  PremiumPage, JobAlertsPage, NotificationCenterPage, BackgroundJobsPage, 
  RecommendedJobsPage, CandidateMessagesPage, CVEditorPage, GitHubAnalysisPage,
  PrivacyConsentPage, ContractsPage
} from '@/pages/candidate';
import {
  AdminDashboardPage,
  AdminUsersPage,
  AdminVerificationPage,
  AdminCompaniesPage,
  AdminJobsPage,
  AdminCatalogPage,
  AdminPaymentsPage,
  AdminAuditLogsPage,
  AdminReportsPage,
  AdminBroadcastPage,
} from '@/pages/admin';
import { PaymentResultPage } from '@/pages/payment/PaymentResultPage';

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
const CompanyNotificationsPage = lazy(() =>
  import('@/pages/company/CompanyNotificationsPage').then((module) => ({
    default: module.CompanyNotificationsPage,
  }))
);
const CompanyWalletPage = lazy(() =>
  import('@/pages/company/CompanyWalletPage').then((module) => ({
    default: module.CompanyWalletPage,
  }))
);
const CompanyPremiumPage = lazy(() =>
  import('@/pages/company/CompanyPremiumPage').then((module) => ({
    default: module.CompanyPremiumPage,
  }))
);

function RouteLoader() {
  const { t } = useTranslation('common');
  return (
    <div className="flex min-h-[40vh] items-center justify-center px-4">
      <div className="text-center">
        <div className="mx-auto h-10 w-10 animate-spin rounded-full border-4 border-[#00b14f]/20 border-t-[#00b14f]" />
        <p className="mt-4 text-sm text-gray-500">{t('app.loading')}</p>
      </div>
    </div>
  );
}

function NotFoundPage() {
  const { t } = useTranslation('common');
  return (
    <div className="flex min-h-screen items-center justify-center">
      <div className="text-center">
        <h1 className="text-6xl font-bold text-gray-900">404</h1>
        <p className="mt-4 text-gray-600">{t('app.notFoundTitle')}</p>
        <p className="mt-2 max-w-md text-sm text-gray-500">{t('app.notFoundDescription')}</p>
        <Link to="/" className="mt-4 inline-block text-blue-600 hover:underline">
          {t('app.backToHome')}
        </Link>
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
            <Route path="/" element={<HomePage />} />
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
            {/* Some links point here directly (e.g. sidebar). Keep as alias. */}
            <Route path="/candidate/notifications" element={<NotificationCenterPage />} />
            <Route path="/candidate/settings/notifications" element={<NotificationsPage />} />
            <Route path="/candidate/wallet" element={<WalletPage />} />
            <Route path="/candidate/premium" element={<PremiumPage />} />
            <Route path="/candidate/job-alerts" element={<JobAlertsPage />} />
            <Route path="/candidate/background-jobs" element={<BackgroundJobsPage />} />
            <Route path="/candidate/jobs/recommended" element={<RecommendedJobsPage />} />
            <Route path="/candidate/messages" element={<CandidateMessagesPage />} />
            <Route path="/candidate/messages/:conversationId" element={<CandidateMessagesPage />} />
            <Route path="/candidate/ai/skills" element={<GitHubAnalysisPage />} />
            <Route path="/candidate/cv/editor/:id" element={<CVEditorPage />} />
            <Route path="/candidate/settings/privacy" element={<PrivacyConsentPage />} />
            <Route path="/candidate/contracts" element={<ContractsPage />} />
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
            <Route path={companyPaths.applications} element={<LazyRoute><CompanyApplicationsPage /></LazyRoute>} />
            <Route path="/company/applications/:applicationId" element={<LazyRoute><CompanyApplicationDetailPage /></LazyRoute>} />
            {/* No conversationId → landing page; with conversationId → chat thread */}
            <Route path={companyPaths.messages} element={<LazyRoute><CompanyMessagesPage /></LazyRoute>} />
            <Route path="/company/messages/:conversationId" element={<LazyRoute><CompanyMessagesPage /></LazyRoute>} />
            {/* Notifications */}
            <Route path={companyPaths.notifications} element={<LazyRoute><CompanyNotificationsPage /></LazyRoute>} />
            {/* Wallet & Premium */}
            <Route path={companyPaths.wallet} element={<LazyRoute><CompanyWalletPage /></LazyRoute>} />
            <Route path={companyPaths.premium} element={<LazyRoute><CompanyPremiumPage /></LazyRoute>} />
          </Route>

          {/* Admin — RR7: bọc path="/admin" + segment con tương đối (layout không path + path tuyệt đối trên con dễ không khớp → 404 *) */}
          <Route path="/admin" element={<AdminLayout />}>
            <Route index element={<Navigate to="/admin/dashboard" replace />} />
            <Route path="dashboard" element={<AdminDashboardPage />} />
            <Route path="users" element={<AdminUsersPage />} />
            <Route path="verification" element={<AdminVerificationPage />} />
            <Route path="companies" element={<AdminCompaniesPage />} />
            <Route path="jobs" element={<AdminJobsPage />} />
            <Route path="catalog" element={<AdminCatalogPage />} />
            <Route path="payments" element={<AdminPaymentsPage />} />
            <Route path="audit-logs" element={<AdminAuditLogsPage />} />
            <Route path="reports" element={<AdminReportsPage />} />
            <Route path="broadcast" element={<AdminBroadcastPage />} />
          </Route>

          {/* GitHub OAuth callback — BE redirects to /profile?github_linked=... */}
          <Route path="/profile" element={<ProfileRedirect />} />

          {/* Auth Routes (no layout) */}
          <Route path="/auth/login" element={<LoginPage />} />
          <Route path="/auth/register" element={<RegisterPage />} />
          <Route path="/auth/forgot-password" element={<ForgotPasswordPage />} />
          <Route path="/auth/reset-password" element={<ResetPasswordPage />} />
          <Route path="/auth/callback" element={<AuthCallbackPage />} />

          {/* Payment Callback Pages */}
          <Route path="/payment/:status" element={<PaymentResultPage />} />

          {/* 404 */}
          <Route path="*" element={<NotFoundPage />} />
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
