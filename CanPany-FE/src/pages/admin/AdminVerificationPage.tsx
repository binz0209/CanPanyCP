import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import { adminApi, companiesApi } from '../../api';
import { Button, Card } from '../../components/ui';
import { adminKeys, companiesKeys } from '../../lib/queryKeys';
import type { Company } from '../../types';

export function AdminVerificationPage() {
    const { t } = useTranslation('admin');
    const { t: tCommon } = useTranslation('common');
    const queryClient = useQueryClient();
    const [rejectFor, setRejectFor] = useState<Company | null>(null);
    const [rejectReason, setRejectReason] = useState('');

    const pendingQuery = useQuery({
        queryKey: adminKeys.verification(),
        queryFn: async () => {
            const res = await companiesApi.getAll({ page: 1, pageSize: 200 });
            return res.companies.filter((c) => c.verificationStatus === 'Pending');
        },
    });

    const approveMutation = useMutation({
        mutationFn: (companyId: string) => adminApi.approveVerification(companyId),
        onSuccess: () => {
            void queryClient.invalidateQueries({ queryKey: adminKeys.verification() });
            void queryClient.invalidateQueries({ queryKey: companiesKeys.list() });
            void queryClient.invalidateQueries({ queryKey: adminKeys.dashboard() });
            toast.success(t('verification.approveSuccess'));
        },
        onError: () => toast.error(t('verification.actionError')),
    });

    const rejectMutation = useMutation({
        mutationFn: ({ companyId, reason }: { companyId: string; reason: string }) =>
            adminApi.rejectVerification(companyId, reason),
        onSuccess: () => {
            void queryClient.invalidateQueries({ queryKey: adminKeys.verification() });
            void queryClient.invalidateQueries({ queryKey: companiesKeys.list() });
            void queryClient.invalidateQueries({ queryKey: adminKeys.dashboard() });
            toast.success(t('verification.rejectSuccess'));
            setRejectFor(null);
            setRejectReason('');
        },
        onError: () => toast.error(t('verification.actionError')),
    });

    const busy = approveMutation.isPending || rejectMutation.isPending;

    if (pendingQuery.isError) {
        return (
            <div className="rounded-xl border border-red-100 bg-red-50 p-6 text-sm text-red-800">
                {t('verification.loadError')}
            </div>
        );
    }

    const list = pendingQuery.data ?? [];

    return (
        <div className="space-y-6">
            <div>
                <h1 className="text-2xl font-bold text-gray-900">{t('verification.title')}</h1>
                <p className="mt-1 text-sm text-gray-600">{t('verification.subtitle')}</p>
                <p className="mt-2 text-xs text-gray-500">{t('verification.sourceNote')}</p>
                <p className="mt-1 text-xs text-amber-800/90">{t('verification.apiEmptyNote')}</p>
            </div>

            <Card className="overflow-hidden p-0">
                <div className="overflow-x-auto">
                    <table className="w-full min-w-[640px] text-left text-sm">
                        <thead className="border-b border-gray-100 bg-gray-50 text-xs font-semibold uppercase text-gray-500">
                            <tr>
                                <th className="px-4 py-3">{t('verification.company')}</th>
                                <th className="px-4 py-3">{t('verification.status')}</th>
                                <th className="px-4 py-3 text-right">{/* actions */}</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-gray-100">
                            {pendingQuery.isLoading ? (
                                <tr>
                                    <td colSpan={3} className="px-4 py-12 text-center text-gray-500">
                                        {tCommon('app.loading')}
                                    </td>
                                </tr>
                            ) : list.length === 0 ? (
                                <tr>
                                    <td colSpan={3} className="px-4 py-12 text-center text-gray-500">
                                        {t('verification.empty')}
                                    </td>
                                </tr>
                            ) : (
                                list.map((c) => (
                                    <tr key={c.id} className="hover:bg-gray-50/80">
                                        <td className="px-4 py-3">
                                            <div className="font-medium text-gray-900">{c.name}</div>
                                            {c.website && (
                                                <div className="text-xs text-gray-500">{c.website}</div>
                                            )}
                                        </td>
                                        <td className="px-4 py-3">
                                            <span className="rounded-full bg-amber-100 px-2.5 py-0.5 text-xs font-medium text-amber-800">
                                                {t('verification.pending')}
                                            </span>
                                        </td>
                                        <td className="px-4 py-3 text-right">
                                            <div className="flex flex-wrap justify-end gap-2">
                                                <Button
                                                    size="sm"
                                                    className="bg-[#00b14f] hover:bg-[#00b14f]/90"
                                                    disabled={busy}
                                                    onClick={() => approveMutation.mutate(c.id)}
                                                >
                                                    {t('verification.approve')}
                                                </Button>
                                                <Button
                                                    size="sm"
                                                    variant="outline"
                                                    className="border-red-200 text-red-600 hover:bg-red-50"
                                                    disabled={busy}
                                                    onClick={() => {
                                                        setRejectFor(c);
                                                        setRejectReason('');
                                                    }}
                                                >
                                                    {t('verification.reject')}
                                                </Button>
                                            </div>
                                        </td>
                                    </tr>
                                ))
                            )}
                        </tbody>
                    </table>
                </div>
            </Card>

            {rejectFor && (
                <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
                    <div className="max-h-[90vh] w-full max-w-md overflow-y-auto rounded-2xl bg-white p-6 shadow-xl">
                        <h2 className="text-lg font-semibold text-gray-900">{t('verification.reject')}</h2>
                        <p className="mt-1 text-sm text-gray-600">{rejectFor.name}</p>
                        <label htmlFor="reject-reason" className="mt-4 block text-sm font-medium text-gray-700">
                            {t('verification.rejectReason')}
                        </label>
                        <textarea
                            id="reject-reason"
                            value={rejectReason}
                            onChange={(e) => setRejectReason(e.target.value)}
                            rows={4}
                            placeholder={t('verification.rejectReasonPlaceholder')}
                            className="mt-1 w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                        />
                        <div className="mt-4 flex justify-end gap-2">
                            <Button
                                type="button"
                                variant="outline"
                                disabled={busy}
                                onClick={() => {
                                    setRejectFor(null);
                                    setRejectReason('');
                                }}
                            >
                                {t('verification.cancel')}
                            </Button>
                            <Button
                                type="button"
                                className="bg-red-600 hover:bg-red-700"
                                disabled={busy || !rejectReason.trim()}
                                onClick={() =>
                                    rejectMutation.mutate({
                                        companyId: rejectFor.id,
                                        reason: rejectReason.trim(),
                                    })
                                }
                            >
                                {t('verification.confirmReject')}
                            </Button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}
