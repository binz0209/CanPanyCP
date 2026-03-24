import { useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import toast from 'react-hot-toast';
import { Badge, Button, Card, CardContent, CardHeader, CardTitle, Input } from '../../components/ui';
import { walletApi, type WalletTransaction } from '../../api/wallet.api';
import { paymentsApi, type PaymentItem } from '../../api/payments.api';
import { useAuthStore } from '../../stores/auth.store';
import { formatCurrency, formatDateTime } from '../../utils';

type WalletType = 'TopUp' | 'Withdraw' | 'Hold' | 'Release' | string;

type PaymentStatus = 'Pending' | 'Paid' | 'Failed' | string;

export function WalletPage() {
  const { t } = useTranslation('candidate');
  const { token } = useAuthStore();
  const queryClient = useQueryClient();

  const [take, setTake] = useState(20);
  const [depositAmount, setDepositAmount] = useState('100000');

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

  const depositMutation = useMutation({
    mutationFn: (amount: number) => paymentsApi.createDeposit(amount),
    onSuccess: () => {
      toast.success(t('wallet.paymentSection.createSuccess', { defaultValue: 'Đã tạo yêu cầu nạp tiền.' }));
      queryClient.invalidateQueries({ queryKey: ['payments', 'list'] });
      queryClient.invalidateQueries({ queryKey: ['wallet', 'transactions'] });
      queryClient.invalidateQueries({ queryKey: ['wallet', 'balance'] });
    },
    onError: () => {
      toast.error(t('wallet.paymentSection.createError', { defaultValue: 'Không thể tạo yêu cầu nạp tiền.' }));
    },
  });

  const balance = balanceQuery.data?.balance ?? 0;
  const currency = balanceQuery.data?.wallet?.currency ?? 'VND';
  const transactions = transactionsQuery.data ?? [];
  const payments = paymentsQuery.data ?? [];

  const typeBadgeVariant = (type: WalletType) => {
    switch (type) {
      case 'TopUp':
        return 'success';
      case 'Withdraw':
        return 'destructive';
      case 'Hold':
        return 'warning';
      case 'Release':
        return 'secondary';
      default:
        return 'secondary';
    }
  };

  const statusBadgeVariant = (status: PaymentStatus) => {
    switch (status) {
      case 'Paid':
        return 'success';
      case 'Failed':
        return 'destructive';
      case 'Pending':
      default:
        return 'warning';
    }
  };

  const getTypeLabel = (type: WalletType) => {
    if (type === 'TopUp') return t('wallet.transactionTypes.topUp', { defaultValue: 'Nạp tiền' });
    if (type === 'Withdraw') return t('wallet.transactionTypes.withdraw', { defaultValue: 'Rút tiền' });
    if (type === 'Hold') return t('wallet.transactionTypes.hold', { defaultValue: 'Giữ' });
    if (type === 'Release') return t('wallet.transactionTypes.release', { defaultValue: 'Phát hành' });
    return type;
  };

  const getPaymentStatusLabel = (status: PaymentStatus) => {
    if (status === 'Paid') return t('wallet.paymentSection.status.paid', { defaultValue: 'Thành công' });
    if (status === 'Failed') return t('wallet.paymentSection.status.failed', { defaultValue: 'Thất bại' });
    return t('wallet.paymentSection.status.pending', { defaultValue: 'Đang chờ' });
  };

  const displayedTransactions = useMemo(() => transactions, [transactions]);

  const onCreateDeposit = () => {
    const amount = Number(depositAmount.replace(/,/g, '').trim());
    if (!Number.isFinite(amount) || amount <= 0) {
      toast.error(t('wallet.paymentSection.invalidAmount', { defaultValue: 'Vui lòng nhập số tiền hợp lệ.' }));
      return;
    }
    depositMutation.mutate(amount);
  };

  return (
    <div className="space-y-4">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">{t('wallet.title', { defaultValue: 'Ví của tôi' })}</h1>
          <p className="mt-1 text-sm text-gray-600">{t('wallet.subtitle', { defaultValue: 'Quản lý số dư và lịch sử giao dịch.' })}</p>
        </div>

        <div className="flex items-center gap-2">
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
            {t('wallet.refresh', { defaultValue: 'Làm mới' })}
          </Button>
        </div>
      </div>

      <Card>
        <CardHeader className="pb-3">
          <CardTitle className="text-base text-gray-800">{t('wallet.balanceCard.title', { defaultValue: 'Số dư hiện tại' })}</CardTitle>
        </CardHeader>
        <CardContent>
          {balanceQuery.isLoading ? (
            <div className="flex items-center justify-center py-10">
              <div className="h-8 w-8 animate-spin rounded-full border-2 border-[#00b14f]/30 border-t-[#00b14f]" />
            </div>
          ) : (
            <div className="flex flex-col gap-2 sm:flex-row sm:items-end sm:justify-between">
              <div>
                <div className="text-sm text-gray-600">{t('wallet.balanceCard.label', { defaultValue: 'Tổng số dư' })}</div>
                <div className="mt-1 text-3xl font-bold text-gray-900">
                  {formatCurrency(balance, currency)}
                </div>
              </div>
              <div className="text-sm text-gray-500">
                {t('wallet.balanceCard.note', { defaultValue: 'Số dư được cập nhật theo hệ thống thanh toán.' })}
              </div>
            </div>
          )}
        </CardContent>
      </Card>

      <Card>
        <CardHeader className="pb-3">
          <CardTitle className="text-base text-gray-800">
            {t('wallet.paymentSection.title', { defaultValue: 'Nạp tiền & thanh toán' })}
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid gap-2 sm:grid-cols-[1fr_auto] sm:items-end">
            <Input
              type="number"
              min={1000}
              step={1000}
              value={depositAmount}
              onChange={(e) => setDepositAmount(e.target.value)}
              label={t('wallet.paymentSection.amountLabel', { defaultValue: 'Số tiền nạp (VND)' })}
              placeholder={t('wallet.paymentSection.amountPlaceholder', { defaultValue: 'Ví dụ: 100000' })}
            />
            <Button onClick={onCreateDeposit} isLoading={depositMutation.isPending}>
              {t('wallet.paymentSection.createButton', { defaultValue: 'Tạo yêu cầu nạp tiền' })}
            </Button>
          </div>

          <div className="space-y-2">
            <h3 className="text-sm font-semibold text-gray-800">
              {t('wallet.paymentSection.historyTitle', { defaultValue: 'Lịch sử yêu cầu thanh toán' })}
            </h3>
            {paymentsQuery.isLoading ? (
              <div className="flex items-center justify-center py-6">
                <div className="h-6 w-6 animate-spin rounded-full border-2 border-[#00b14f]/30 border-t-[#00b14f]" />
              </div>
            ) : payments.length === 0 ? (
              <p className="text-sm text-gray-500">
                {t('wallet.paymentSection.historyEmpty', { defaultValue: 'Chưa có yêu cầu thanh toán nào.' })}
              </p>
            ) : (
              <div className="divide-y divide-gray-200 rounded-xl border border-gray-100">
                {payments.slice(0, 8).map((payment: PaymentItem) => (
                  <div key={payment.id} className="flex flex-col gap-2 px-4 py-3 sm:flex-row sm:items-center sm:justify-between">
                    <div className="min-w-0">
                      <div className="flex flex-wrap items-center gap-2">
                        <Badge variant={statusBadgeVariant(payment.status)}>{getPaymentStatusLabel(payment.status)}</Badge>
                        <span className="text-sm font-medium text-gray-800">{payment.purpose}</span>
                      </div>
                      <div className="mt-1 text-xs text-gray-500">{formatDateTime(payment.createdAt)}</div>
                    </div>
                    <div className="text-right text-sm font-semibold text-gray-900">
                      {formatCurrency((payment.amount ?? 0) / 100, payment.currency || 'VND')}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader className="pb-3">
          <CardTitle className="text-base text-gray-800">
            {t('wallet.transactionsCard.title', { defaultValue: 'Lịch sử giao dịch' })}
          </CardTitle>
        </CardHeader>

        <CardContent className="space-y-3">
          {transactionsQuery.isLoading ? (
            <div className="flex items-center justify-center py-10">
              <div className="h-8 w-8 animate-spin rounded-full border-2 border-[#00b14f]/30 border-t-[#00b14f]" />
            </div>
          ) : displayedTransactions.length === 0 ? (
            <div className="py-8 text-center">
              <p className="text-sm text-gray-500">{t('wallet.transactionsCard.empty', { defaultValue: 'Chưa có giao dịch nào.' })}</p>
            </div>
          ) : (
            <div className="divide-y divide-gray-200">
              {displayedTransactions.map((tx: WalletTransaction) => (
                <div key={tx.id} className="flex flex-col gap-3 py-4 sm:flex-row sm:items-start sm:justify-between">
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
                      {t('wallet.transactionsCard.balanceAfter', { defaultValue: 'Số dư sau' })}: {formatCurrency(tx.balanceAfter, currency)}
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
              {t('wallet.transactionsCard.loadMore', { defaultValue: 'Xem thêm' })}
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
