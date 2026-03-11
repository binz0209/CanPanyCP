export type ApplicationStatus = 'Pending' | 'Accepted' | 'Rejected' | 'Withdrawn';

export interface Application {
    id: string;
    jobId: string;
    candidateId: string;
    cvId?: string;
    coverLetter?: string;
    proposedAmount?: number;
    status: ApplicationStatus;
    matchScore?: number;
    rejectedReason?: string;
    privateNotes?: string;
    contractId?: string;
    createdAt: string | Date;
    updatedAt?: string | Date;
    // Populated fields
    job?: import('./job.types').Job;
}

export interface CreateApplicationRequest {
    jobId: string;
    cvId?: string;
    coverLetter?: string;
    expectedSalary?: number;
}
