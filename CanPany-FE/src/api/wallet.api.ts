import apiClient from './axios.config';
import type { ApiResponse } from '../types';

export interface Wallet {
    id: string;
    userId: string;
    balance: number;
    currency: string;
    isActive: boolean;
    createdAt: string; // ISO string
    updatedAt?: string | null; // ISO string
}

export interface WalletTransaction {
    id: string;
    walletId: string;
    userId: string;
    paymentId?: string | null;
    type: string; // TopUp / Withdraw / Hold / Release...
    amount: number;
    balanceAfter: number;
    note?: string | null;
    createdAt: string; // ISO string
}

export interface WalletBalanceResponse {
    balance: number;
    wallet?: Wallet;
}

function toNumber(value: any): number {
    const n = typeof value === 'number' ? value : Number(value);
    return Number.isFinite(n) ? n : 0;
}

function normalizeWallet(dto: any): Wallet {
    return {
        id: dto?.id ?? dto?.Id ?? '',
        userId: dto?.userId ?? dto?.UserId ?? '',
        balance: toNumber(dto?.balance ?? dto?.Balance),
        currency: dto?.currency ?? dto?.Currency ?? 'VND',
        isActive: dto?.isActive ?? dto?.IsActive ?? true,
        createdAt: dto?.createdAt ?? dto?.CreatedAt ?? new Date().toISOString(),
        updatedAt: dto?.updatedAt ?? dto?.UpdatedAt ?? null,
    };
}

function normalizeTransaction(dto: any): WalletTransaction {
    return {
        id: dto?.id ?? dto?.Id ?? '',
        walletId: dto?.walletId ?? dto?.WalletId ?? '',
        userId: dto?.userId ?? dto?.UserId ?? '',
        paymentId: dto?.paymentId ?? dto?.PaymentId ?? null,
        type: dto?.type ?? dto?.Type ?? '',
        amount: toNumber(dto?.amount ?? dto?.Amount),
        balanceAfter: toNumber(dto?.balanceAfter ?? dto?.BalanceAfter),
        note: dto?.note ?? dto?.Note ?? null,
        createdAt: dto?.createdAt ?? dto?.CreatedAt ?? new Date().toISOString(),
    };
}

export const walletApi = {
    _normalizeBalance: (dto: any): WalletBalanceResponse => {
        return {
            balance: toNumber(dto?.balance ?? dto?.Balance),
            wallet: dto?.wallet ?? dto?.Wallet ? normalizeWallet(dto.wallet ?? dto.Wallet) : undefined,
        };
    },

    getBalance: async (): Promise<WalletBalanceResponse> => {
        const response = await apiClient.get<ApiResponse<any>>('/wallet/balance');
        return walletApi._normalizeBalance(response.data.data);
    },

    getTransactions: async (take: number = 20): Promise<WalletTransaction[]> => {
        const response = await apiClient.get<ApiResponse<any[]>>('/wallet/transactions', {
            params: { take },
        });
        const list = response.data.data ?? [];
        return list.map(normalizeTransaction);
    },
};

