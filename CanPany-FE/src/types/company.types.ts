export type VerificationStatus = 'Pending' | 'Approved' | 'Rejected';

export interface Company {
    id: string;
    userId: string;
    name: string;
    description?: string;
    logoUrl?: string;
    website?: string;
    phone?: string;
    address?: string;
    isVerified: boolean;
    verificationStatus: VerificationStatus;
    verificationDocuments: string[];
    verifiedAt?: Date;
    createdAt: Date;
    updatedAt?: Date;
}

export interface CompanySearchParams {
    keyword?: string;
    isVerified?: boolean;
    location?: string;
    page?: number;
    pageSize?: number;
}

export interface CreateCompanyRequest {
    name: string;
    description?: string;
    logoUrl?: string;
    website?: string;
    phone?: string;
    address?: string;
}

export interface UpdateCompanyRequest {
    name?: string;
    description?: string;
    logoUrl?: string;
    website?: string;
    phone?: string;
    address?: string;
}
