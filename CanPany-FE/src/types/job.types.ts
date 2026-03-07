export type BudgetType = 'Fixed' | 'Hourly';
export type JobLevel = 'Junior' | 'Mid' | 'Senior' | 'Expert';
export type JobStatus = 'Open' | 'Closed' | 'Draft';

export interface Job {
    id: string;
    companyId: string;
    title: string;
    description: string;
    categoryId?: string;
    skillIds: string[];
    budgetType: BudgetType;
    budgetAmount?: number;
    level?: JobLevel;
    location?: string;
    isRemote: boolean;
    deadline?: Date;
    status: JobStatus;
    images: string[];
    viewCount: number;
    applicationCount: number;
    createdAt: Date;
    updatedAt?: Date;
    // Populated fields
    company?: import('./company.types').Company;
}

export interface JobSearchParams {
    keyword?: string;
    categoryId?: string;
    skillIds?: string[];
    minBudget?: number;
    maxBudget?: number;
    level?: JobLevel;
    isRemote?: boolean;
    location?: string;
    status?: JobStatus;
    budgetType?: BudgetType;
    page?: number;
    pageSize?: number;
    sortBy?: 'createdAt' | 'budgetAmount' | 'title';
    sortOrder?: 'asc' | 'desc';
}

export interface JobDetailsResponse {
    job: Job;
    isBookmarked: boolean;
}

export interface JobListResponse {
    jobs: Job[];
    total: number;
    page: number;
    pageSize: number;
    totalPages: number;
}

export interface CreateJobRequest {
    companyId: string;
    title: string;
    description: string;
    categoryId?: string;
    skillIds?: string[];
    budgetType?: BudgetType;
    budgetAmount?: number;
    level?: JobLevel;
    location?: string;
    isRemote?: boolean;
    deadline?: Date;
}

export interface UpdateJobRequest {
    title?: string;
    description?: string;
    skillIds?: string[];
    budgetAmount?: number;
    level?: JobLevel;
    location?: string;
    deadline?: Date;
}
