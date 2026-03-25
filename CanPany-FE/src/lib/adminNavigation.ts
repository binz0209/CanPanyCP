import type adminVi from '../i18n/locales/vi/admin.json';

type AdminSidebarLabelKey = {
    [K in keyof typeof adminVi.sidebar]: `sidebar.${K & string}`;
}[keyof typeof adminVi.sidebar];

export const adminPaths = {
    root: '/admin',
    dashboard: '/admin/dashboard',
    users: '/admin/users',
    verification: '/admin/verification',
    jobs: '/admin/jobs',
    catalog: '/admin/catalog',
    payments: '/admin/payments',
    auditLogs: '/admin/audit-logs',
    reports: '/admin/reports',
    broadcast: '/admin/broadcast',
} as const;

export type AdminNavItem = {
    id: string;
    labelKey: AdminSidebarLabelKey;
    path: string;
};

export const adminNavigationItems: AdminNavItem[] = [
    { id: 'dashboard', labelKey: 'sidebar.dashboard', path: adminPaths.dashboard },
    { id: 'users', labelKey: 'sidebar.users', path: adminPaths.users },
    { id: 'verification', labelKey: 'sidebar.verification', path: adminPaths.verification },
    { id: 'jobs', labelKey: 'sidebar.jobs', path: adminPaths.jobs },
    { id: 'catalog', labelKey: 'sidebar.catalog', path: adminPaths.catalog },
    { id: 'payments', labelKey: 'sidebar.payments', path: adminPaths.payments },
    { id: 'auditLogs', labelKey: 'sidebar.auditLogs', path: adminPaths.auditLogs },
    { id: 'reports', labelKey: 'sidebar.reports', path: adminPaths.reports },
    { id: 'broadcast', labelKey: 'sidebar.broadcast', path: adminPaths.broadcast },
];
