export const companyPaths = {
    root: '/company',
    dashboard: '/company/dashboard',
    profile: '/company/profile',
    verification: '/company/verification',
    jobs: '/company/jobs',
    newJob: '/company/jobs/new',
    editJob: (jobId: string) => `/company/jobs/${jobId}/edit`,
    applications: '/company/applications',
    applicationDetail: (applicationId: string) => `/company/applications/${applicationId}`,
    messages: '/company/messages',
    notifications: '/company/notifications',
    messageThread: (conversationId: string) => `/company/messages/${conversationId}`,
    wallet: '/company/wallet',
    premium: '/company/premium',
    settingsAccount: '/company/settings/account',
} as const;

export interface CompanyNavItem {
    labelKey: string;
    path?: string;
    items?: { labelKey: string; path: string }[];
}

// Labels are i18n keys under the 'company' namespace
export const companyNavigationItems: CompanyNavItem[] = [
    {
        labelKey: 'sidebar.dashboard',
        path: companyPaths.dashboard,
    },
    {
        labelKey: 'sidebar.companyProfile',
        items: [
            { labelKey: 'sidebar.companyInfo', path: companyPaths.profile },
            { labelKey: 'sidebar.verification', path: companyPaths.verification },
        ],
    },
    {
        labelKey: 'sidebar.jobManagement',
        items: [
            { labelKey: 'sidebar.jobList', path: companyPaths.jobs },
            { labelKey: 'sidebar.createJob', path: companyPaths.newJob },
            { labelKey: 'sidebar.reviewApplications', path: companyPaths.applications },
        ],
    },
    {
        labelKey: 'sidebar.messages',
        path: companyPaths.messages,
    },
    {
        labelKey: 'sidebar.notifications',
        path: companyPaths.notifications,
    },
    {
        labelKey: 'sidebar.billing',
        items: [
            { labelKey: 'sidebar.wallet', path: companyPaths.wallet },
            { labelKey: 'sidebar.premium', path: companyPaths.premium },
        ],
    },
    {
        labelKey: 'sidebar.settings',
        items: [
            { labelKey: 'sidebar.settingsAccount', path: companyPaths.settingsAccount },
        ],
    },
] as const;
