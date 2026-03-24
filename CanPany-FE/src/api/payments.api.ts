import apiClient from './axios.config';
import type { ApiResponse } from '../types';

export interface PaymentItem {
  id: string;
  userId: string;
  purpose: string;
  amount: number;
  currency: string;
  status: string;
  paidAt?: string | null;
  createdAt: string;
}

function toNumber(value: unknown): number {
  const n = typeof value === 'number' ? value : Number(value);
  return Number.isFinite(n) ? n : 0;
}

function normalizePayment(dto: any): PaymentItem {
  return {
    id: dto?.id ?? dto?.Id ?? '',
    userId: dto?.userId ?? dto?.UserId ?? '',
    purpose: dto?.purpose ?? dto?.Purpose ?? '',
    amount: toNumber(dto?.amount ?? dto?.Amount),
    currency: dto?.currency ?? dto?.Currency ?? 'VND',
    status: dto?.status ?? dto?.Status ?? 'Pending',
    paidAt: dto?.paidAt ?? dto?.PaidAt ?? null,
    createdAt: dto?.createdAt ?? dto?.CreatedAt ?? new Date().toISOString(),
  };
}

export const paymentsApi = {
  createDeposit: async (amount: number): Promise<PaymentItem> => {
    const response = await apiClient.post<ApiResponse<any>>('/payments/deposit', { amount });
    return normalizePayment(response.data.data);
  },

  getPayments: async (): Promise<PaymentItem[]> => {
    const response = await apiClient.get<ApiResponse<any[]>>('/payments');
    const list = response.data.data ?? [];
    return list.map(normalizePayment);
  },
};
