export const companyPaths = {
    root: '/company',
    dashboard: '/company/dashboard',
    profile: '/company/profile',
    verification: '/company/verification',
    jobs: '/company/jobs',
    newJob: '/company/jobs/new',
    editJob: (jobId: string) => `/company/jobs/${jobId}/edit`,
    candidateSearch: '/company/candidates/search',
    applications: '/company/applications',
    applicationDetail: (applicationId: string) => `/company/applications/${applicationId}`,
    messages: '/company/messages',
    messageThread: (conversationId: string) => `/company/messages/${conversationId}`,
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
        labelKey: 'sidebar.candidateSearch',
        path: companyPaths.candidateSearch,
    },
    {
        labelKey: 'sidebar.messages',
        path: companyPaths.messages,
    },
] as const;
