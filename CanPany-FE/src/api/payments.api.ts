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

export interface SePayCheckoutResult {
  paymentId: string;
  checkoutUrl: string;
  fields: Record<string, string>;
}

export interface CreateSePayCheckoutRequest {
  amount: number;       // VND, ví dụ: 100000 = 100,000đ
  purpose?: string;     // "TopUp" | "Premium"
  packageId?: string;
  successUrl?: string;
  errorUrl?: string;
  cancelUrl?: string;
}

export interface PremiumPackage {
  id: string;
  name: string;
  description?: string;
  userType: string;       // "Candidate" | "Company" | any
  packageType: string;    // "Monthly" | "Yearly"
  price: number;          // VND
  durationDays: number;
  features: string[];
  isActive: boolean;
  createdAt: string;
}

export interface UserSubscription {
  id: string;
  packageId: string;
  status: string;        // "Active" | "Expired" | "Cancelled"
  startDate: string;
  endDate: string;
  features: string[];
}

export interface PremiumStatus {
  isPremium: boolean;
  subscription: UserSubscription | null;
}

export interface PurchasePremiumResult {
  subscriptionId: string;
  startDate: string;
  endDate: string;
  status: string;
  features: string[];
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

function normalizePackage(dto: any): PremiumPackage {
  return {
    id: dto?.id ?? dto?.Id ?? '',
    name: dto?.name ?? dto?.Name ?? '',
    description: dto?.description ?? dto?.Description ?? '',
    userType: dto?.userType ?? dto?.UserType ?? '',
    packageType: dto?.packageType ?? dto?.PackageType ?? '',
    price: toNumber(dto?.price ?? dto?.Price),
    durationDays: toNumber(dto?.durationDays ?? dto?.DurationDays),
    features: dto?.features ?? dto?.Features ?? [],
    isActive: dto?.isActive ?? dto?.IsActive ?? true,
    createdAt: dto?.createdAt ?? dto?.CreatedAt ?? new Date().toISOString(),
  };
}

export const paymentsApi = {
  /** Tạo phiên thanh toán SePay – trả về checkoutUrl + form fields cần POST */
  createSePayCheckout: async (
    req: CreateSePayCheckoutRequest,
  ): Promise<SePayCheckoutResult> => {
    const origin = window.location.origin;
    const response = await apiClient.post<ApiResponse<SePayCheckoutResult>>(
      '/payments/sepay/checkout',
      {
        amount: req.amount,
        purpose: req.purpose ?? 'TopUp',
        packageId: req.packageId ?? null,
        successUrl: req.successUrl ?? `${origin}/payment/success`,
        errorUrl: req.errorUrl ?? `${origin}/payment/error`,
        cancelUrl: req.cancelUrl ?? `${origin}/payment/cancel`,
      },
    );
    return response.data.data as SePayCheckoutResult;
  },

  /** Lấy danh sách gói premium khả dụng */
  getPremiumPackages: async (userType?: string): Promise<PremiumPackage[]> => {
    const url = userType ? `/payments/premium/packages?userType=${userType}` : '/payments/premium/packages';
    const response = await apiClient.get<ApiResponse<any[]>>(url);
    const list = response.data.data ?? [];
    return list.map(normalizePackage);
  },

  /** Lấy trạng thái premium hiện tại */
  getPremiumStatus: async (): Promise<PremiumStatus> => {
    const response = await apiClient.get<ApiResponse<any>>('/payments/premium/status');
    const data = response.data.data;
    return {
      isPremium: data?.isPremium ?? false,
      subscription: data?.subscription ?? null,
    };
  },

  /** Mua gói premium bằng số dư ví */
  purchasePremium: async (packageId: string): Promise<PurchasePremiumResult> => {
    const response = await apiClient.post<ApiResponse<PurchasePremiumResult>>(
      '/payments/premium/purchase',
      { packageId },
    );
    return response.data.data as PurchasePremiumResult;
  },

  /** Legacy: tạo yêu cầu nạp tiền thủ công (admin duyệt) */
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
