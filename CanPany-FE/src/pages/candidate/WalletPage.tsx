import { useEffect, useMemo, useRef, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import { Badge, Button, Card, CardContent, CardHeader, CardTitle, Input } from '../../components/ui';
import { walletApi, type WalletTransaction } from '../../api/wallet.api';
import { paymentsApi, type PaymentItem, type SePayCheckoutResult } from '../../api/payments.api';
import { useAuthStore } from '../../stores/auth.store';
import { formatCurrency, formatDateTime } from '../../utils';

type WalletType = 'TopUp' | 'Withdraw' | 'Hold' | 'Release' | string;
type PaymentStatus = 'Pending' | 'Paid' | 'Failed' | string;

// Preset amount options (VND)
const PRESET_AMOUNTS = [50_000, 100_000, 200_000, 500_000, 1_000_000, 2_000_000];

export function WalletPage() {
  const { token } = useAuthStore();
  const { t } = useTranslation('candidate');
  const queryClient = useQueryClient();
  const [searchParams] = useSearchParams();

  const [take, setTake] = useState(20);
  const [depositAmount, setDepositAmount] = useState('100000');

  // Hidden form ref for SePay POST submission
  const sepayFormRef = useRef<HTMLFormElement>(null);
  const [checkoutData, setCheckoutData] = useState<SePayCheckoutResult | null>(null);

  // Handle SePay redirect-back result from URL params
  useEffect(() => {
    const paymentStatus = searchParams.get('payment');
    if (paymentStatus === 'success') {
      toast.success(t('paymentResult.success.message'));
      queryClient.invalidateQueries({ queryKey: ['wallet'] });
      queryClient.invalidateQueries({ queryKey: ['payments'] });
    } else if (paymentStatus === 'error') {
      toast.error(t('paymentResult.error.message'));
    } else if (paymentStatus === 'cancel') {
      toast(t('paymentResult.cancelledToast'), { icon: 'ℹ️' });
    }
  }, [searchParams, queryClient, t]);

  // Auto-submit hidden form when checkoutData is ready
  useEffect(() => {
    if (checkoutData && sepayFormRef.current) {
      sepayFormRef.current.submit();
    }
  }, [checkoutData]);

  const balanceQuery = useQuery({
    queryKey: ['wallet', 'balance'],
    enabled: !!token,
    queryFn: () => walletApi.getBalance(),
  });

  const transactionsQuery = useQuery({
    queryKey: ['wallet', 'transactions', take],
    enabled: !!token,
    queryFn: () => walletApi.getTransactions(take),
    placeholderData: (prev) => prev,
  });

  const paymentsQuery = useQuery({
    queryKey: ['payments', 'list'],
    enabled: !!token,
    queryFn: () => paymentsApi.getPayments(),
  });

  // SePay checkout mutation
  const sePayMutation = useMutation({
    mutationFn: (amount: number) =>
      paymentsApi.createSePayCheckout({ amount, purpose: 'TopUp' }),
    onSuccess: (data) => {
      toast.success(t('wallet.paymentSection.createSuccess'));
      setCheckoutData(data);
    },
    onError: () => {
      toast.error(t('wallet.paymentSection.createError'));
    },
  });

  const balance = balanceQuery.data?.balance ?? 0;
  const currency = balanceQuery.data?.wallet?.currency ?? 'VND';
  const transactions = transactionsQuery.data ?? [];
  const payments = paymentsQuery.data ?? [];

  const typeBadgeVariant = (type: WalletType) => {
    switch (type) {
      case 'TopUp': return 'success';
      case 'Withdraw': return 'destructive';
      case 'Hold': return 'warning';
      case 'Release': return 'secondary';
      default: return 'secondary';
    }
  };

  const statusBadgeVariant = (status: PaymentStatus) => {
    switch (status) {
      case 'Paid': return 'success';
      case 'Failed': return 'destructive';
      default: return 'warning';
    }
  };

  const getTypeLabel = (type: WalletType) => {
    const typeMap: Record<string, string> = {
      TopUp: t('wallet.transactionTypes.topUp'),
      Withdraw: t('wallet.transactionTypes.withdraw'),
      Hold: t('wallet.transactionTypes.hold'),
      Release: t('wallet.transactionTypes.release'),
    };
    return typeMap[type] ?? type;
  };

  const getPaymentStatusLabel = (status: PaymentStatus) => {
    const statusMap: Record<string, string> = {
      Paid: t('wallet.paymentSection.status.paid'),
      Failed: t('wallet.paymentSection.status.failed'),
    };
    return statusMap[status] ?? t('wallet.paymentSection.status.pending');
  };

  const displayedTransactions = useMemo(() => transactions, [transactions]);

  const onPayWithSePay = () => {
    const amount = Number(depositAmount.replace(/,/g, '').trim());
    if (!Number.isFinite(amount) || amount < 1000) {
      toast.error(t('wallet.paymentSection.invalidAmount'));
      return;
    }
    sePayMutation.mutate(amount);
  };

  return (
    <div className="space-y-4">
      {/* Hidden SePay POST form – auto-submitted when checkoutData is set */}
      {checkoutData && (
        <form
          ref={sepayFormRef}
          action={checkoutData.checkoutUrl}
          method="POST"
          style={{ display: 'none' }}
        >
          {Object.entries(checkoutData.fields).map(([name, value]) => (
            <input key={name} type="hidden" name={name} value={value} />
          ))}
        </form>
      )}

      {/* Header */}
      <div className="flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">{t('wallet.title')}</h1>
          <p className="mt-1 text-sm text-gray-600">{t('wallet.subtitle')}</p>
        </div>
        <Button
          variant="outline"
          size="sm"
          onClick={() => {
            transactionsQuery.refetch();
            paymentsQuery.refetch();
            balanceQuery.refetch();
          }}
          disabled={!token || transactionsQuery.isPending}
        >
          {t('wallet.refresh')}
        </Button>
      </div>

      {/* Balance card */}
      <Card>
        <CardHeader className="pb-3">
          <CardTitle className="text-base text-gray-800">{t('wallet.balanceCard.title')}</CardTitle>
        </CardHeader>
        <CardContent>
          {balanceQuery.isLoading ? (
            <div className="flex items-center justify-center py-10">
              <div className="h-8 w-8 animate-spin rounded-full border-2 border-[#00b14f]/30 border-t-[#00b14f]" />
            </div>
          ) : (
            <div className="flex flex-col gap-2 sm:flex-row sm:items-end sm:justify-between">
              <div>
                <div className="text-sm text-gray-600">{t('wallet.balanceCard.label')}</div>
                <div className="mt-1 text-3xl font-bold text-gray-900">
                  {formatCurrency(balance, currency)}
                </div>
              </div>
              <div className="text-sm text-gray-500">
                {t('wallet.balanceCard.note')}
              </div>
            </div>
          )}
        </CardContent>
      </Card>

      {/* SePay Deposit card */}
      <Card className="border-[#00b14f]/30">
        <CardHeader className="pb-3">
          <div className="flex items-center gap-2">
            {/* SePay logo badge */}
            <span className="inline-flex items-center rounded-full bg-[#00b14f]/10 px-2 py-0.5 text-xs font-semibold text-[#00b14f]">
              SePay
            </span>
            <CardTitle className="text-base text-gray-800">{t('wallet.paymentSection.title')}</CardTitle>
          </div>
        </CardHeader>
        <CardContent className="space-y-4">
          {/* Preset amounts */}
          <div>
            <p className="mb-2 text-sm text-gray-600">{t('wallet.paymentSection.amountLabel')}:</p>
            <div className="flex flex-wrap gap-2">
              {PRESET_AMOUNTS.map((amt) => (
                <button
                  key={amt}
                  type="button"
                  onClick={() => setDepositAmount(String(amt))}
                  className={`rounded-lg border px-3 py-1.5 text-sm font-medium transition-colors ${
                    depositAmount === String(amt)
                      ? 'border-[#00b14f] bg-[#00b14f] text-white'
                      : 'border-gray-200 bg-white text-gray-700 hover:border-[#00b14f] hover:text-[#00b14f]'
                  }`}
                >
                  {formatCurrency(amt, 'VND')}
                </button>
              ))}
            </div>
          </div>

          {/* Custom amount input */}
          <div className="grid gap-2 sm:grid-cols-[1fr_auto] sm:items-end">
            <Input
              type="number"
              min={1000}
              step={1000}
              value={depositAmount}
              onChange={(e) => setDepositAmount(e.target.value)}
              label={t('wallet.paymentSection.amountLabel')}
              placeholder={t('wallet.paymentSection.amountPlaceholder')}
            />
            <Button
              onClick={onPayWithSePay}
              isLoading={sePayMutation.isPending}
              className="bg-[#00b14f] hover:bg-[#00953f] text-white"
            >
              {sePayMutation.isPending ? t('wallet.paymentSection.createButton') + '...' : '💳 ' + t('wallet.paymentSection.createButton')}
            </Button>
          </div>

          <p className="text-xs text-gray-400">
            {t('wallet.balanceCard.note')}
          </p>
        </CardContent>
      </Card>

      {/* Payment history */}
      <Card>
        <CardHeader className="pb-3">
          <CardTitle className="text-base text-gray-800">{t('wallet.paymentSection.historyTitle')}</CardTitle>
        </CardHeader>
        <CardContent>
          {paymentsQuery.isLoading ? (
            <div className="flex items-center justify-center py-6">
              <div className="h-6 w-6 animate-spin rounded-full border-2 border-[#00b14f]/30 border-t-[#00b14f]" />
            </div>
          ) : payments.length === 0 ? (
            <p className="text-sm text-gray-500">{t('wallet.paymentSection.historyEmpty')}</p>
          ) : (
            <div className="divide-y divide-gray-200 rounded-xl border border-gray-100">
              {payments.slice(0, 8).map((payment: PaymentItem) => (
                <div
                  key={payment.id}
                  className="flex flex-col gap-2 px-4 py-3 sm:flex-row sm:items-center sm:justify-between"
                >
                  <div className="min-w-0">
                    <div className="flex flex-wrap items-center gap-2">
                      <Badge variant={statusBadgeVariant(payment.status)}>
                        {getPaymentStatusLabel(payment.status)}
                      </Badge>
                      <span className="text-sm font-medium text-gray-800">{payment.purpose}</span>
                    </div>
                    <div className="mt-1 text-xs text-gray-500">{formatDateTime(payment.createdAt)}</div>
                  </div>
                  <div className="text-right text-sm font-semibold text-gray-900">
                    {formatCurrency(payment.amount ?? 0, payment.currency || 'VND')}
                  </div>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>

      {/* Transaction history */}
      <Card>
        <CardHeader className="pb-3">
          <CardTitle className="text-base text-gray-800">{t('wallet.transactionsCard.title')}</CardTitle>
        </CardHeader>
        <CardContent className="space-y-3">
          {transactionsQuery.isLoading ? (
            <div className="flex items-center justify-center py-10">
              <div className="h-8 w-8 animate-spin rounded-full border-2 border-[#00b14f]/30 border-t-[#00b14f]" />
            </div>
          ) : displayedTransactions.length === 0 ? (
            <div className="py-8 text-center">
              <p className="text-sm text-gray-500">{t('wallet.transactionsCard.empty')}</p>
            </div>
          ) : (
            <div className="divide-y divide-gray-200">
              {displayedTransactions.map((tx: WalletTransaction) => (
                <div
                  key={tx.id}
                  className="flex flex-col gap-3 py-4 sm:flex-row sm:items-start sm:justify-between"
                >
                  <div className="min-w-0">
                    <div className="flex flex-wrap items-center gap-2">
                      <Badge variant={typeBadgeVariant(tx.type)}>{getTypeLabel(tx.type)}</Badge>
                      <div className="truncate text-sm font-semibold text-gray-900">
                        {tx.note ? tx.note : tx.type}
                      </div>
                    </div>
                    <div className="mt-1 text-xs text-gray-500">{formatDateTime(tx.createdAt)}</div>
                  </div>
                  <div className="text-right">
                    <div className="text-sm font-semibold text-gray-900">
                      {formatCurrency(tx.amount, currency)}
                    </div>
                    <div className="mt-1 text-xs text-gray-500">
                      {t('wallet.transactionsCard.balanceAfter')}: {formatCurrency(tx.balanceAfter, currency)}
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}

          <div className="flex items-center justify-center">
            <Button
              variant="secondary"
              onClick={() => setTake((prev) => prev + 20)}
              disabled={!token || transactionsQuery.isLoading || displayedTransactions.length < take}
            >
              {t('wallet.transactionsCard.loadMore')}
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
