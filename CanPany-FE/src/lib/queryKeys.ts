export const companyKeys = {
    all: ['company'] as const,
    me: () => [...companyKeys.all, 'me'] as const,
    statistics: (companyId: string) => [...companyKeys.all, 'statistics', companyId] as const,
    verification: (companyId: string) => [...companyKeys.all, 'verification', companyId] as const,
    workspaceJobs: (companyId: string) => [...companyKeys.all, 'workspace', 'jobs', companyId] as const,
    workspaceJobDetail: (jobId: string) => [...companyKeys.all, 'workspace', 'job-detail', jobId] as const,
};

export const companiesKeys = {
    all: ['companies'] as const,
    detail: (companyId: string) => [...companiesKeys.all, 'detail', companyId] as const,
    list: (params?: unknown) => [...companiesKeys.all, 'list', params ?? {}] as const,
    search: (params?: unknown) => [...companiesKeys.all, 'search', params ?? {}] as const,
    publicJobs: (companyId: string, status?: string) =>
        [...companiesKeys.detail(companyId), 'jobs', { status: status ?? 'all' }] as const,
};

export const applicationKeys = {
    all: ['applications'] as const,
    byJob: (jobId: string) => [...applicationKeys.all, 'by-job', jobId] as const,
    detail: (applicationId: string) => [...applicationKeys.all, 'detail', applicationId] as const,
};

export const candidateKeys = {
    all: ['candidates'] as const,
    profile: (candidateId: string) => [...candidateKeys.all, 'profile', candidateId] as const,
    cvs: (candidateId: string) => [...candidateKeys.all, 'cvs', candidateId] as const,
    search: (mode: string, params: unknown) => [...candidateKeys.all, 'search', mode, params] as const,
    unlocked: () => [...candidateKeys.all, 'unlocked'] as const,
};

export const messageKeys = {
    all: ['messages'] as const,
    byConversation: (conversationId: string) =>
        [...messageKeys.all, 'conversation', conversationId] as const,
};

export const bookmarkKeys = {
    all: ['bookmarks'] as const,
    list: () => [...bookmarkKeys.all, 'list'] as const,
};

export const jobAlertKeys = {
    all: ['jobAlerts'] as const,
    list: () => [...jobAlertKeys.all, 'list'] as const,
    detail: (id: string) => [...jobAlertKeys.all, 'detail', id] as const,
    preview: (id: string) => [...jobAlertKeys.all, 'preview', id] as const,
    stats: () => [...jobAlertKeys.all, 'stats'] as const,
};

export const notificationKeys = {
    all: ['notifications'] as const,
    list: (params?: unknown) => [...notificationKeys.all, 'list', params ?? {}] as const,
    detail: (id: string) => [...notificationKeys.all, 'detail', id] as const,
};

export const conversationKeys = {
    all: ['conversations'] as const,
    list: () => [...conversationKeys.all, 'list'] as const,
    unreadCount: () => [...conversationKeys.all, 'unread-count'] as const,
    detail: (id: string) => [...conversationKeys.all, 'detail', id] as const,
};
