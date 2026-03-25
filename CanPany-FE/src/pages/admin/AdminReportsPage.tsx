import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import { Button, Card } from '../../components/ui';
import { adminApi } from '../../api';
import type { AdminReportDetails } from '../../api/admin.api';

export function AdminReportsPage() {
    const { t } = useTranslation('admin');
    const queryClient = useQueryClient();

    const title = t('placeholders.reports.title');
    const desc = t('placeholders.reports.description');

    const [reportId, setReportId] = useState('');
    const [details, setDetails] = useState<AdminReportDetails | null>(null);

    const [resolveNote, setResolveNote] = useState('');
    const [banUser, setBanUser] = useState(false);

    const [rejectionReason, setRejectionReason] = useState('');

    const loadDetails = useMutation({
        mutationFn: () => adminApi.getReportDetails(reportId.trim()),
        onSuccess: (data) => {
            setDetails(data);
            setResolveNote('');
            setBanUser(false);
            setRejectionReason('');
        },
        onError: () => toast.error(t('reports.load.error')),
    });

    const resolveMutation = useMutation({
        mutationFn: () =>
            details
                ? adminApi.resolveReport(details.id, {
                      resolutionNote: resolveNote.trim(),
                      banUser,
                  })
                : Promise.reject(new Error('No report loaded')),
        onSuccess: () => {
            toast.success(t('reports.resolve.success'));
            void queryClient.invalidateQueries();
            setDetails(null);
            setReportId('');
            setResolveNote('');
            setBanUser(false);
        },
        onError: () => toast.error(t('reports.resolve.error')),
    });

    const rejectMutation = useMutation({
        mutationFn: () =>
            details
                ? adminApi.rejectReport(details.id, { rejectionReason: rejectionReason.trim() })
                : Promise.reject(new Error('No report loaded')),
        onSuccess: () => {
            toast.success(t('reports.reject.success'));
            void queryClient.invalidateQueries();
            setDetails(null);
            setReportId('');
            setRejectionReason('');
        },
        onError: () => toast.error(t('reports.reject.error')),
    });

    return (
        <div className="space-y-6">
            <div>
                <h1 className="text-2xl font-bold text-gray-900">{title}</h1>
                <p className="mt-1 text-sm text-gray-600">{desc}</p>
            </div>

            <Card className="space-y-4 p-5">
                <h2 className="text-lg font-semibold text-gray-900">{t('reports.load.sectionTitle')}</h2>
                <div className="space-y-2">
                    <label className="block text-sm font-medium text-gray-700">{t('reports.load.idLabel')}</label>
                    <input
                        value={reportId}
                        onChange={(e) => setReportId(e.target.value)}
                        className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                        placeholder={t('reports.load.idPlaceholder')}
                    />
                </div>
                <div className="flex justify-end">
                    <Button
                        className="bg-[#00b14f] hover:bg-[#00b14f]/90"
                        disabled={loadDetails.isPending || !reportId.trim()}
                        onClick={() => loadDetails.mutate()}
                    >
                        {t('reports.load.button')}
                    </Button>
                </div>
            </Card>

            {details && (
                <div className="space-y-4">
                    <Card className="space-y-4 p-5">
                        <h2 className="text-lg font-semibold text-gray-900">{t('reports.detail.sectionTitle')}</h2>
                        <div className="grid gap-4 md:grid-cols-2">
                            <div className="text-sm">
                                <div className="text-gray-500">{t('reports.detail.reporterLabel')}</div>
                                <div className="font-medium text-gray-900">
                                    {details.reporter.fullName} ({details.reporter.email})
                                </div>
                            </div>
                            <div className="text-sm">
                                <div className="text-gray-500">{t('reports.detail.statusLabel')}</div>
                                <div className="font-medium text-gray-900">{details.status}</div>
                            </div>
                        </div>

                        {details.reportedUser && (
                            <div className="text-sm">
                                <div className="text-gray-500">{t('reports.detail.reportedUserLabel')}</div>
                                <div className="font-medium text-gray-900">
                                    {details.reportedUser.fullName} ({details.reportedUser.email})
                                </div>
                            </div>
                        )}

                        <div className="text-sm">
                            <div className="text-gray-500">{t('reports.detail.reasonLabel')}</div>
                            <div className="whitespace-pre-wrap font-medium text-gray-900">{details.reason}</div>
                        </div>
                        <div className="text-sm">
                            <div className="text-gray-500">{t('reports.detail.descriptionLabel')}</div>
                            <div className="whitespace-pre-wrap text-gray-900">{details.description}</div>
                        </div>

                        {details.evidence && details.evidence.length > 0 && (
                            <div className="text-sm">
                                <div className="text-gray-500">{t('reports.detail.evidenceLabel')}</div>
                                <ul className="list-disc pl-5 text-gray-900">
                                    {details.evidence.map((e, idx) => (
                                        <li key={`${details.id}-e-${idx}`}>{e}</li>
                                    ))}
                                </ul>
                            </div>
                        )}
                    </Card>

                    <div className="grid gap-4 lg:grid-cols-2">
                        <Card className="space-y-3 p-5">
                            <h2 className="text-lg font-semibold text-gray-900">{t('reports.resolve.sectionTitle')}</h2>
                            <label className="block text-sm font-medium text-gray-700">{t('reports.resolve.noteLabel')}</label>
                            <textarea
                                value={resolveNote}
                                onChange={(e) => setResolveNote(e.target.value)}
                                rows={4}
                                className="w-full resize-y rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            />
                            <label className="flex items-center gap-2 text-sm text-gray-700">
                                <input
                                    type="checkbox"
                                    checked={banUser}
                                    onChange={(e) => setBanUser(e.target.checked)}
                                />
                                {t('reports.resolve.banUserLabel')}
                            </label>
                            <div className="flex justify-end">
                                <Button
                                    className="bg-[#00b14f] hover:bg-[#00b14f]/90"
                                    disabled={resolveMutation.isPending || !resolveNote.trim()}
                                    onClick={() => resolveMutation.mutate()}
                                >
                                    {t('reports.resolve.button')}
                                </Button>
                            </div>
                        </Card>

                        <Card className="space-y-3 p-5">
                            <h2 className="text-lg font-semibold text-gray-900">{t('reports.reject.sectionTitle')}</h2>
                            <label className="block text-sm font-medium text-gray-700">{t('reports.reject.reasonLabel')}</label>
                            <textarea
                                value={rejectionReason}
                                onChange={(e) => setRejectionReason(e.target.value)}
                                rows={4}
                                className="w-full resize-y rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            />
                            <div className="flex justify-end">
                                <Button
                                    variant="outline"
                                    className="border-red-200 text-red-600 hover:bg-red-50"
                                    disabled={rejectMutation.isPending || !rejectionReason.trim()}
                                    onClick={() => rejectMutation.mutate()}
                                >
                                    {t('reports.reject.button')}
                                </Button>
                            </div>
                        </Card>
                    </div>
                </div>
            )}
        </div>
    );
}

