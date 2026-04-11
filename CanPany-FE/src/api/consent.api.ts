import apiClient from './axios.config';

export interface UserConsent {
    id: string;
    userId: string;
    consentType: string;
    isGranted: boolean;
    grantedAt?: string;
    revokedAt?: string;
    policyVersion?: string;
    ipAddress?: string;
    createdAt: string;
    updatedAt?: string;
}

export const consentApi = {
    /** GET /api/consent — all consents for current user */
    getConsents: async (): Promise<UserConsent[]> => {
        const res = await apiClient.get<{ data: UserConsent[] }>('/consent');
        return res.data.data || [];
    },

    /** POST /api/consent — grant a consent */
    grantConsent: async (consentType: string, policyVersion?: string): Promise<UserConsent> => {
        const res = await apiClient.post<{ data: UserConsent }>('/consent', { consentType, policyVersion });
        return res.data.data;
    },

    /** DELETE /api/consent/{type} — revoke a consent */
    revokeConsent: async (consentType: string): Promise<void> => {
        await apiClient.delete(`/consent/${consentType}`);
    },

    /** GET /api/consent/check/{type} — check if a consent is granted */
    checkConsent: async (consentType: string): Promise<boolean> => {
        const res = await apiClient.get<{ data: { consentType: string; isGranted: boolean } }>(`/consent/check/${consentType}`);
        return res.data.data?.isGranted ?? false;
    },
};
