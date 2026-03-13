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
} as const;

export interface CompanyNavItem {
    label: string;
    path?: string;
    items?: { label: string; path: string }[];
}

export const companyNavigationItems: CompanyNavItem[] = [
    {
        label: 'Dashboard',
        path: companyPaths.dashboard,
    },
    {
        label: 'Company Profile',
        items: [
            { label: 'Thông tin công ty', path: companyPaths.profile },
            { label: 'Xác minh công ty', path: companyPaths.verification },
        ],
    },
    {
        label: 'Job Management',
        items: [
            { label: 'Danh sách tin tuyển dụng', path: companyPaths.jobs },
            { label: 'Tạo tin tuyển dụng', path: companyPaths.newJob },
            { label: 'Review applications', path: companyPaths.applications },
        ],
    },
    {
        label: 'Candidate Search',
        path: companyPaths.candidateSearch,
    },
] as const;
