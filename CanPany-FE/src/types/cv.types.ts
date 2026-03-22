// CV Types for Candidate CV Management

export interface CV {
    id: string;
    userId: string;
    fileName: string;
    fileUrl: string;
    fileSize: number;
    mimeType: string;
    isDefault: boolean;
    isAIGenerated?: boolean;
    structuredData?: Record<string, unknown>;
    latestAnalysisId?: string;
    extractedSkills?: string[];
    createdAt: string;
    updatedAt?: string;
}

export interface UploadCVRequest {
    file: File;
    isDefault?: boolean;
}

export interface UpdateCVRequest {
    fileName?: string;
}

export interface CVAnalysis {
    id: string;
    cvId: string;
    extractedSkills: string[];
    atsScore?: number;
    analysisSummary?: string;
    suggestedImprovements?: string[];
    analyzedAt: string;
}
