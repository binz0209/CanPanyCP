import { useTranslation } from 'react-i18next';
import { Button, Card } from '../../ui';

interface ApplicationStatusActionsCardProps {
    canReviewStatus: boolean;
    rejectReason: string;
    onRejectReasonChange: (value: string) => void;
    onAccept: () => void;
    onReject: () => void;
    isAccepting: boolean;
    isRejecting: boolean;
}

export function ApplicationStatusActionsCard({
    canReviewStatus,
    rejectReason,
    onRejectReasonChange,
    onAccept,
    onReject,
    isAccepting,
    isRejecting,
}: ApplicationStatusActionsCardProps) {
    const { t } = useTranslation('company');

    return (
        <Card className="p-6">
            <h2 className="text-lg font-semibold text-gray-900">{t('applicationStatus.title')}</h2>
            {canReviewStatus ? (
                <div className="mt-5 space-y-4">
                    <div className="flex flex-wrap gap-3">
                        <Button onClick={onAccept} isLoading={isAccepting}>
                            {t('applicationStatus.btnAccept')}
                        </Button>
                    </div>

                    <div>
                        <label className="mb-2 block text-sm font-medium text-gray-700">
                            {t('applicationStatus.rejectReasonLabel')}
                        </label>
                        <textarea
                            rows={4}
                            value={rejectReason}
                            onChange={(event) => onRejectReasonChange(event.target.value)}
                            placeholder={t('applicationStatus.rejectPlaceholder')}
                            className="w-full rounded-lg border border-gray-300 px-4 py-3 text-sm text-gray-900 outline-none transition focus:border-[#00b14f] focus:ring-2 focus:ring-[#00b14f]/20"
                        />
                        <div className="mt-3">
                            <Button variant="outline" isLoading={isRejecting} onClick={onReject}>
                                {t('applicationStatus.btnReject')}
                            </Button>
                        </div>
                    </div>
                </div>
            ) : (
                <p className="mt-4 text-sm text-gray-600">{t('applicationStatus.alreadyProcessed')}</p>
            )}
        </Card>
    );
}
