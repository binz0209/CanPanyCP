import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import { Button, Card } from '../../components/ui';
import { adminApi } from '../../api';

export function AdminPaymentsPage() {
    const { t } = useTranslation('admin');

    const queryClient = useQueryClient();

    const [status, setStatus] = useState<string>('Pending');
    const [paymentIdApprove, setPaymentIdApprove] = useState('');
    const [paymentIdReject, setPaymentIdReject] = useState('');
    const [rejectReason, setRejectReason] = useState('');

    const paymentsKey = ['admin', 'payments'] as const;

    const paymentsQuery = useQuery({
        queryKey: [...paymentsKey, status],
        queryFn: () => adminApi.getPayments(status),
        enabled: false,
    });

    const approveMutation = useMutation({
        mutationFn: (id: string) => adminApi.approvePayment(id),
        onSuccess: () => {
            toast.success(t('payments.approve.success'));
            void queryClient.invalidateQueries();
            setPaymentIdApprove('');
        },
        onError: () => toast.error(t('payments.approve.error')),
    });

    const rejectMutation = useMutation({
        mutationFn: ({ id, reason }: { id: string; reason: string }) => adminApi.rejectPayment(id, reason),
        onSuccess: () => {
            toast.success(t('payments.reject.success'));
            void queryClient.invalidateQueries();
            setPaymentIdReject('');
            setRejectReason('');
        },
        onError: () => toast.error(t('payments.reject.error')),
    });

    const title = t('placeholders.payments.title');
    const desc = t('placeholders.payments.description');

    const payments = paymentsQuery.data ?? [];

    return (
        <div className="space-y-6">
            <div>
                <h1 className="text-2xl font-bold text-gray-900">{title}</h1>
                <p className="mt-1 text-sm text-gray-600">{desc}</p>
            </div>

            <div className="grid gap-4 lg:grid-cols-2">
                <Card className="space-y-4 p-5">
                    <h2 className="text-lg font-semibold text-gray-900">{t('payments.list.sectionTitle')}</h2>

                    <div className="space-y-2">
                        <label className="block text-sm font-medium text-gray-700">{t('payments.list.statusLabel')}</label>
                        <select
                            value={status}
                            onChange={(e) => setStatus(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                        >
                            <option value="Pending">{t('payments.status.pending')}</option>
                            <option value="Approved">{t('payments.status.approved')}</option>
                            <option value="Rejected">{t('payments.status.rejected')}</option>
                        </select>
                    </div>

                    <div className="flex justify-end">
                        <Button
                            className="bg-[#00b14f] hover:bg-[#00b14f]/90"
                            disabled={paymentsQuery.isFetching}
                            onClick={() => void paymentsQuery.refetch()}
                        >
                            {t('payments.list.loadButton')}
                        </Button>
                    </div>

                    {paymentsQuery.isFetching ? (
                        <div className="text-sm text-gray-500">{t('payments.list.loading')}</div>
                    ) : payments.length === 0 ? (
                        <div className="rounded-lg border border-dashed border-gray-200 bg-gray-50 p-4 text-sm text-gray-500">
                            {t('payments.list.empty')}
                        </div>
                    ) : (
                        <pre className="max-h-96 overflow-auto rounded-lg bg-gray-50 p-3 text-xs text-gray-700">
                            {JSON.stringify(payments, null, 2)}
                        </pre>
                    )}
                </Card>

                <Card className="space-y-4 p-5">
                    <h2 className="text-lg font-semibold text-gray-900">{t('payments.actions.sectionTitle')}</h2>

                    <div className="space-y-2">
                        <label className="block text-sm font-medium text-gray-700">
                            {t('payments.actions.approve.idLabel')}
                        </label>
                        <input
                            value={paymentIdApprove}
                            onChange={(e) => setPaymentIdApprove(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder={t('payments.actions.approve.idPlaceholder')}
                        />
                        <div className="flex justify-end">
                            <Button
                                className="bg-[#00b14f] hover:bg-[#00b14f]/90"
                                disabled={approveMutation.isPending || !paymentIdApprove.trim()}
                                onClick={() => approveMutation.mutate(paymentIdApprove.trim())}
                            >
                                {t('payments.actions.approve.button')}
                            </Button>
                        </div>
                    </div>

                    <div className="h-px bg-gray-200" />

                    <div className="space-y-2">
                        <label className="block text-sm font-medium text-gray-700">
                            {t('payments.actions.reject.idLabel')}
                        </label>
                        <input
                            value={paymentIdReject}
                            onChange={(e) => setPaymentIdReject(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder={t('payments.actions.reject.idPlaceholder')}
                        />
                        <label className="block text-sm font-medium text-gray-700">{t('payments.actions.reject.reasonLabel')}</label>
                        <textarea
                            value={rejectReason}
                            onChange={(e) => setRejectReason(e.target.value)}
                            rows={3}
                            className="w-full resize-y rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder={t('payments.actions.reject.reasonPlaceholder')}
                        />
                        <div className="flex justify-end">
                            <Button
                                variant="outline"
                                className="border-red-200 text-red-600 hover:bg-red-50"
                                disabled={rejectMutation.isPending || !paymentIdReject.trim() || !rejectReason.trim()}
                                onClick={() =>
                                    rejectMutation.mutate({
                                        id: paymentIdReject.trim(),
                                        reason: rejectReason.trim(),
                                    })
                                }
                            >
                                {t('payments.actions.reject.button')}
                            </Button>
                        </div>
                    </div>
                </Card>
            </div>
        </div>
    );
}

