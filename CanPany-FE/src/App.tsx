import { Suspense, lazy } from 'react';
import type { ReactNode } from 'react';
import { BrowserRouter, Routes, Route, Navigate, useLocation, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { QueryClientProvider } from '@tanstack/react-query';
import { Toaster } from 'react-hot-toast';
import { queryClient } from '@/lib/queryClient';
import { companyPaths } from '@/lib/companyNavigation';
import { PublicLayout, CandidateLayout, CompanyLayout, AdminLayout } from '@/components/layout';

// ── Public ────────────────────────────────────────────────────────────────────
const HomePage               = lazy(() => import('@/pages/public/HomePage').then(m => ({ default: m.HomePage })));
const JobsPage               = lazy(() => import('@/pages/public/JobsPage').then(m => ({ default: m.JobsPage })));
const JobDetailPage          = lazy(() => import('@/pages/public/JobDetailPage').then(m => ({ default: m.JobDetailPage })));
const CompaniesPage          = lazy(() => import('@/pages/public/CompaniesPage').then(m => ({ default: m.CompaniesPage })));
const CompanyDetailPage      = lazy(() => import('@/pages/public/CompanyDetailPage').then(m => ({ default: m.CompanyDetailPage })));

// ── Auth ──────────────────────────────────────────────────────────────────────
const LoginPage              = lazy(() => import('@/pages/auth/LoginPage').then(m => ({ default: m.LoginPage })));
const RegisterPage           = lazy(() => import('@/pages/auth/RegisterPage').then(m => ({ default: m.RegisterPage })));
const ForgotPasswordPage     = lazy(() => import('@/pages/auth/ForgotPasswordPage').then(m => ({ default: m.ForgotPasswordPage })));
const ResetPasswordPage      = lazy(() => import('@/pages/auth/ResetPasswordPage').then(m => ({ default: m.ResetPasswordPage })));
const AccountSettingsPage    = lazy(() => import('@/pages/auth/AccountSettingsPage').then(m => ({ default: m.AccountSettingsPage })));
const AuthCallbackPage       = lazy(() => import('@/pages/auth/AuthCallbackPage').then(m => ({ default: m.AuthCallbackPage })));

// ── Candidate ─────────────────────────────────────────────────────────────────
const CandidateProfilePage   = lazy(() => import('@/pages/candidate/CandidateProfilePage').then(m => ({ default: m.CandidateProfilePage })));
const CandidateDashboardPage = lazy(() => import('@/pages/candidate/CandidateDashboardPage').then(m => ({ default: m.CandidateDashboardPage })));
const CVListPage             = lazy(() => import('@/pages/candidate/CVListPage').then(m => ({ default: m.CVListPage })));
const AICVPage               = lazy(() => import('@/pages/candidate/AICVPage').then(m => ({ default: m.AICVPage })));
const ApplicationHistoryPage = lazy(() => import('@/pages/candidate/ApplicationHistoryPage').then(m => ({ default: m.ApplicationHistoryPage })));
const SavedJobsPage          = lazy(() => import('@/pages/candidate/SavedJobsPage').then(m => ({ default: m.SavedJobsPage })));
const NotificationsPage      = lazy(() => import('@/pages/candidate/settings/NotificationsPage').then(m => ({ default: m.NotificationsPage })));
const WalletPage             = lazy(() => import('@/pages/candidate/WalletPage').then(m => ({ default: m.WalletPage })));
const PremiumPage            = lazy(() => import('@/pages/candidate/PremiumPage').then(m => ({ default: m.PremiumPage })));
const JobAlertsPage          = lazy(() => import('@/pages/candidate/JobAlertsPage').then(m => ({ default: m.JobAlertsPage })));
const NotificationCenterPage = lazy(() => import('@/pages/candidate/NotificationCenterPage').then(m => ({ default: m.NotificationCenterPage })));
const BackgroundJobsPage     = lazy(() => import('@/pages/candidate/BackgroundJobsPage').then(m => ({ default: m.BackgroundJobsPage })));
const RecommendedJobsPage    = lazy(() => import('@/pages/candidate/RecommendedJobsPage').then(m => ({ default: m.RecommendedJobsPage })));
const CandidateMessagesPage  = lazy(() => import('@/pages/candidate/CandidateMessagesPage').then(m => ({ default: m.CandidateMessagesPage })));
const CVEditorPage           = lazy(() => import('@/pages/candidate/CVEditorPage').then(m => ({ default: m.CVEditorPage })));
const GitHubAnalysisPage     = lazy(() => import('@/pages/candidate/GitHubAnalysisPage').then(m => ({ default: m.GitHubAnalysisPage })));
const ContractsPage          = lazy(() => import('@/pages/candidate/ContractsPage').then(m => ({ default: m.ContractsPage })));

// ── Company ───────────────────────────────────────────────────────────────────
const CompanyDashboardPage          = lazy(() => import('@/pages/company/CompanyDashboardPage').then(m => ({ default: m.CompanyDashboardPage })));
const CompanyProfilePage            = lazy(() => import('@/pages/company/CompanyProfilePage').then(m => ({ default: m.CompanyProfilePage })));
const CompanyVerificationPage       = lazy(() => import('@/pages/company/CompanyVerificationPage').then(m => ({ default: m.CompanyVerificationPage })));
const CompanyJobsPage               = lazy(() => import('@/pages/company/CompanyJobsPage').then(m => ({ default: m.CompanyJobsPage })));
const CompanyJobFormPage            = lazy(() => import('@/pages/company/CompanyJobFormPage').then(m => ({ default: m.CompanyJobFormPage })));
const CompanyApplicationsPage       = lazy(() => import('@/pages/company/CompanyApplicationsPage').then(m => ({ default: m.CompanyApplicationsPage })));
const CompanyApplicationDetailPage  = lazy(() => import('@/pages/company/CompanyApplicationDetailPage').then(m => ({ default: m.CompanyApplicationDetailPage })));
const CompanyMessagesPage           = lazy(() => import('@/pages/company/CompanyMessagesPage').then(m => ({ default: m.CompanyMessagesPage })));
const CompanyNotificationsPage      = lazy(() => import('@/pages/company/CompanyNotificationsPage').then(m => ({ default: m.CompanyNotificationsPage })));
const CompanyWalletPage             = lazy(() => import('@/pages/company/CompanyWalletPage').then(m => ({ default: m.CompanyWalletPage })));
const CompanyPremiumPage            = lazy(() => import('@/pages/company/CompanyPremiumPage').then(m => ({ default: m.CompanyPremiumPage })));

// ── Admin ─────────────────────────────────────────────────────────────────────
const AdminDashboardPage    = lazy(() => import('@/pages/admin/AdminDashboardPage').then(m => ({ default: m.AdminDashboardPage })));
const AdminUsersPage        = lazy(() => import('@/pages/admin/AdminUsersPage').then(m => ({ default: m.AdminUsersPage })));
const AdminVerificationPage = lazy(() => import('@/pages/admin/AdminVerificationPage').then(m => ({ default: m.AdminVerificationPage })));
const AdminCompaniesPage    = lazy(() => import('@/pages/admin/AdminCompaniesPage').then(m => ({ default: m.AdminCompaniesPage })));
const AdminJobsPage         = lazy(() => import('@/pages/admin/AdminJobsPage').then(m => ({ default: m.AdminJobsPage })));
const AdminCatalogPage      = lazy(() => import('@/pages/admin/AdminCatalogPage').then(m => ({ default: m.AdminCatalogPage })));
const AdminPaymentsPage     = lazy(() => import('@/pages/admin/AdminPaymentsPage').then(m => ({ default: m.AdminPaymentsPage })));
const AdminAuditLogsPage    = lazy(() => import('@/pages/admin/AdminAuditLogsPage').then(m => ({ default: m.AdminAuditLogsPage })));
const AdminReportsPage      = lazy(() => import('@/pages/admin/AdminReportsPage').then(m => ({ default: m.AdminReportsPage })));
const AdminBroadcastPage    = lazy(() => import('@/pages/admin/AdminBroadcastPage').then(m => ({ default: m.AdminBroadcastPage })));

// ── Payment ───────────────────────────────────────────────────────────────────
const PaymentResultPage     = lazy(() => import('@/pages/payment/PaymentResultPage').then(m => ({ default: m.PaymentResultPage })));

// ── Helpers ───────────────────────────────────────────────────────────────────
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
            <Route path="/" element={<LazyRoute><HomePage /></LazyRoute>} />
            <Route path="/jobs" element={<LazyRoute><JobsPage /></LazyRoute>} />
            <Route path="/jobs/:id" element={<LazyRoute><JobDetailPage /></LazyRoute>} />
            <Route path="/companies" element={<LazyRoute><CompaniesPage /></LazyRoute>} />
            <Route path="/companies/:id" element={<LazyRoute><CompanyDetailPage /></LazyRoute>} />
          </Route>

          {/* Candidate Routes */}
          <Route element={<CandidateLayout />}>
            <Route path="/candidate/dashboard" element={<LazyRoute><CandidateDashboardPage /></LazyRoute>} />
            <Route path="/candidate/profile" element={<LazyRoute><CandidateProfilePage /></LazyRoute>} />
            <Route path="/candidate/cv/list" element={<LazyRoute><CVListPage /></LazyRoute>} />
            <Route path="/candidate/cv/ai" element={<LazyRoute><AICVPage /></LazyRoute>} />
            <Route path="/candidate/applications/history" element={<LazyRoute><ApplicationHistoryPage /></LazyRoute>} />
            <Route path="/candidate/jobs/bookmarks" element={<LazyRoute><SavedJobsPage /></LazyRoute>} />
            <Route path="/candidate/notifications" element={<LazyRoute><NotificationCenterPage /></LazyRoute>} />
            <Route path="/candidate/settings/notifications" element={<LazyRoute><NotificationsPage /></LazyRoute>} />
            <Route path="/candidate/wallet" element={<LazyRoute><WalletPage /></LazyRoute>} />
            <Route path="/candidate/premium" element={<LazyRoute><PremiumPage /></LazyRoute>} />
            <Route path="/candidate/job-alerts" element={<LazyRoute><JobAlertsPage /></LazyRoute>} />
            <Route path="/candidate/background-jobs" element={<LazyRoute><BackgroundJobsPage /></LazyRoute>} />
            <Route path="/candidate/jobs/recommended" element={<LazyRoute><RecommendedJobsPage /></LazyRoute>} />
            <Route path="/candidate/messages" element={<LazyRoute><CandidateMessagesPage /></LazyRoute>} />
            <Route path="/candidate/messages/:conversationId" element={<LazyRoute><CandidateMessagesPage /></LazyRoute>} />
            <Route path="/candidate/ai/skills" element={<LazyRoute><GitHubAnalysisPage /></LazyRoute>} />
            <Route path="/candidate/cv/editor/:id" element={<LazyRoute><CVEditorPage /></LazyRoute>} />
            <Route path="/candidate/settings/account" element={<LazyRoute><AccountSettingsPage /></LazyRoute>} />
            <Route path="/candidate/contracts" element={<LazyRoute><ContractsPage /></LazyRoute>} />
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
            <Route path={companyPaths.messages} element={<LazyRoute><CompanyMessagesPage /></LazyRoute>} />
            <Route path="/company/messages/:conversationId" element={<LazyRoute><CompanyMessagesPage /></LazyRoute>} />
            <Route path={companyPaths.notifications} element={<LazyRoute><CompanyNotificationsPage /></LazyRoute>} />
            <Route path={companyPaths.wallet} element={<LazyRoute><CompanyWalletPage /></LazyRoute>} />
            <Route path={companyPaths.premium} element={<LazyRoute><CompanyPremiumPage /></LazyRoute>} />
            <Route path={companyPaths.settingsAccount} element={<LazyRoute><AccountSettingsPage /></LazyRoute>} />
          </Route>

          {/* Admin Routes */}
          <Route path="/admin" element={<AdminLayout />}>
            <Route index element={<Navigate to="/admin/dashboard" replace />} />
            <Route path="dashboard" element={<LazyRoute><AdminDashboardPage /></LazyRoute>} />
            <Route path="users" element={<LazyRoute><AdminUsersPage /></LazyRoute>} />
            <Route path="verification" element={<LazyRoute><AdminVerificationPage /></LazyRoute>} />
            <Route path="companies" element={<LazyRoute><AdminCompaniesPage /></LazyRoute>} />
            <Route path="jobs" element={<LazyRoute><AdminJobsPage /></LazyRoute>} />
            <Route path="catalog" element={<LazyRoute><AdminCatalogPage /></LazyRoute>} />
            <Route path="payments" element={<LazyRoute><AdminPaymentsPage /></LazyRoute>} />
            <Route path="audit-logs" element={<LazyRoute><AdminAuditLogsPage /></LazyRoute>} />
            <Route path="reports" element={<LazyRoute><AdminReportsPage /></LazyRoute>} />
            <Route path="broadcast" element={<LazyRoute><AdminBroadcastPage /></LazyRoute>} />
          </Route>

          {/* GitHub OAuth callback — BE redirects to /profile?github_linked=... */}
          <Route path="/profile" element={<ProfileRedirect />} />

          {/* Auth Routes (no layout) */}
          <Route path="/auth/login" element={<LazyRoute><LoginPage /></LazyRoute>} />
          <Route path="/auth/register" element={<LazyRoute><RegisterPage /></LazyRoute>} />
          <Route path="/auth/forgot-password" element={<LazyRoute><ForgotPasswordPage /></LazyRoute>} />
          <Route path="/auth/reset-password" element={<LazyRoute><ResetPasswordPage /></LazyRoute>} />
          <Route path="/auth/callback" element={<LazyRoute><AuthCallbackPage /></LazyRoute>} />

          {/* Payment Callback Pages */}
          <Route path="/payment/:status" element={<LazyRoute><PaymentResultPage /></LazyRoute>} />

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
