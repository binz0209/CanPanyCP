export type UserRole = 'Guest' | 'Candidate' | 'Company' | 'Admin';

export interface User {
    id: string;
    fullName: string;
    email: string;
    role: UserRole;
    avatarUrl?: string;
    isLocked: boolean;
    lockedUntil?: Date;
    createdAt: Date;
    updatedAt?: Date;
}

export interface UserProfile {
    id: string;
    userId: string;
    bio?: string;
    phone?: string;
    address?: string;
    dateOfBirth?: Date;
    skillIds: string[];
    experience?: string;
    education?: string;
    portfolio?: string;
    linkedInUrl?: string;
    gitHubUrl?: string;
    title?: string;
    location?: string;
    hourlyRate?: number;
    languages: string[];
    certifications: string[];
    createdAt: Date;
    updatedAt?: Date;
}

export interface LoginRequest {
    email: string;
    password: string;
}

export interface RegisterRequest {
    fullName: string;
    email: string;
    password: string;
    role: 'Candidate' | 'Company';
}

export interface AuthResponse {
    accessToken: string;
    user: User;
}

export interface ChangePasswordRequest {
    oldPassword: string;
    newPassword: string;
}

export interface ForgotPasswordRequest {
    email: string;
}

export interface ResetPasswordRequest {
    email: string;
    code: string;
    newPassword: string;
}
