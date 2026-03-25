import { useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { Button, Badge, Card, CardContent, CardHeader, CardTitle } from '../../components/ui';
import { walletApi, type WalletTransaction } from '../../api/wallet.api';
import { useAuthStore } from '../../stores/auth.store';
import { formatCurrency, formatDateTime } from '../../utils';

type WalletType = 'TopUp' | 'Withdraw' | 'Hold' | 'Release' | string;

export function WalletPage() {
    const { t } = useTranslation('candidate');
    const { token } = useAuthStore();

    const [take, setTake] = useState(20);

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

    const balance = balanceQuery.data?.balance ?? 0;
    const currency = balanceQuery.data?.wallet?.currency ?? 'VND';
    const transactions = transactionsQuery.data ?? [];

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

    const getTypeLabel = (type: WalletType) => {
        if (type === 'TopUp') return t('wallet.transactionTypes.topUp');
        if (type === 'Withdraw') return t('wallet.transactionTypes.withdraw');
        if (type === 'Hold') return t('wallet.transactionTypes.hold');
        if (type === 'Release') return t('wallet.transactionTypes.release');
        return type;
    };

    const displayedTransactions = useMemo(() => transactions, [transactions]);

    return (
        <div className="space-y-4">
            <div className="flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
                <div>
                    <h1 className="text-2xl font-bold text-gray-900">{t('wallet.title')}</h1>
                    <p className="mt-1 text-sm text-gray-600">{t('wallet.subtitle')}</p>
                </div>

                <div className="flex items-center gap-2">
                    <Button
                        variant="outline"
                        size="sm"
                        onClick={() => transactionsQuery.refetch()}
                        disabled={!token || transactionsQuery.isPending}
                    >
                        {t('wallet.refresh')}
                    </Button>
                </div>
            </div>

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

            <Card>
                <CardHeader className="pb-3">
                    <CardTitle className="text-base text-gray-800">
                        {t('wallet.transactionsCard.title')}
                    </CardTitle>
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
                                <div key={tx.id} className="flex flex-col gap-3 py-4 sm:flex-row sm:items-start sm:justify-between">
                                    <div className="min-w-0">
                                        <div className="flex flex-wrap items-center gap-2">
                                            <Badge variant={typeBadgeVariant(tx.type)}>{getTypeLabel(tx.type)}</Badge>
                                            <div className="text-sm font-semibold text-gray-900 truncate">
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

