import apiClient from './axios.config';

export interface Contract {
    id: string;
    jobId: string;
    applicationId: string;
    companyId: string;
    candidateId: string;
    agreedAmount: number;
    status: string;
    startDate?: string;
    endDate?: string;
    completedAt?: string;
    cancelledAt?: string;
    cancellationReason?: string;
    createdAt: string;
}

export interface Review {
    id: string;
    contractId: string;
    reviewerId: string;
    revieweeId: string;
    rating: number;
    comment?: string;
    createdAt: string;
}

export const contractsApi = {
    /** GET /api/contracts/my */
    getMyContracts: async (role: 'candidate' | 'company' = 'candidate'): Promise<Contract[]> => {
        const res = await apiClient.get<{ data: Contract[] }>('/contracts/my', { params: { role } });
        return res.data.data || [];
    },

    /** GET /api/contracts/{id} */
    getContract: async (id: string): Promise<Contract> => {
        const res = await apiClient.get<{ data: Contract }>(`/contracts/${id}`);
        return res.data.data;
    },

    /** POST /api/contracts */
    createContract: async (data: { applicationId: string; agreedAmount: number; startDate?: string; endDate?: string }): Promise<Contract> => {
        const res = await apiClient.post<{ data: Contract }>('/contracts', data);
        return res.data.data;
    },

    /** PUT /api/contracts/{id}/status */
    updateStatus: async (id: string, status: string, cancellationReason?: string): Promise<void> => {
        await apiClient.put(`/contracts/${id}/status`, { status, cancellationReason });
    },
};

export const reviewsApi = {
    /** GET /api/reviews/contract/{contractId} */
    getByContract: async (contractId: string): Promise<Review[]> => {
        const res = await apiClient.get<{ data: Review[] }>(`/reviews/contract/${contractId}`);
        return res.data.data || [];
    },

    /** GET /api/reviews/user/{userId} */
    getByUser: async (userId: string): Promise<Review[]> => {
        const res = await apiClient.get<{ data: Review[] }>(`/reviews/user/${userId}`);
        return res.data.data || [];
    },

    /** POST /api/reviews */
    create: async (data: { contractId: string; revieweeId: string; rating: number; comment?: string }): Promise<Review> => {
        const res = await apiClient.post<{ data: Review }>('/reviews', data);
        return res.data.data;
    },
};
