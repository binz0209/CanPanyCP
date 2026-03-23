import { MessageSquareText } from 'lucide-react';
import { Button, Card } from '../../ui';
import { useTranslation } from 'react-i18next';

interface ApplicationNotesCardProps {
    noteDraft: string;
    onNoteDraftChange: (value: string) => void;
    onSubmit: () => void;
    isSubmitting: boolean;
    sessionNotes: string[];
}

export function ApplicationNotesCard({
    noteDraft,
    onNoteDraftChange,
    onSubmit,
    isSubmitting,
    sessionNotes,
}: ApplicationNotesCardProps) {
    const { t } = useTranslation('company');
    return (
        <Card className="p-6">
            <div className="flex items-center gap-2 text-gray-900">
                <MessageSquareText className="h-5 w-5" />
                <h2 className="text-lg font-semibold">{t('applicationNotes.title')}</h2>
            </div>

            <div className="mt-4 rounded-xl border border-amber-200 bg-amber-50 p-4 text-sm text-amber-800">
                {t('applicationNotes.hint')}
            </div>

            <div className="mt-4">
                <textarea
                    rows={5}
                    value={noteDraft}
                    onChange={(event) => onNoteDraftChange(event.target.value)}
                    placeholder={t('applicationNotes.placeholder')}
                    className="w-full rounded-lg border border-gray-300 px-4 py-3 text-sm text-gray-900 outline-none transition focus:border-[#00b14f] focus:ring-2 focus:ring-[#00b14f]/20"
                />
                <div className="mt-3">
                    <Button
                        variant="outline"
                        isLoading={isSubmitting}
                        onClick={onSubmit}
                    >
                        {t('applicationNotes.saveButton')}
                    </Button>
                </div>
            </div>

            {sessionNotes.length > 0 && (
                <div className="mt-5 space-y-3">
                    {sessionNotes.map((note, index) => (
                        <div key={`${note}-${index}`} className="rounded-xl border border-gray-100 bg-gray-50 p-4">
                            <p className="text-xs font-semibold uppercase tracking-wide text-gray-500">
                                {t('applicationNotes.sessionNote')}
                            </p>
                            <p className="mt-2 whitespace-pre-wrap text-sm leading-6 text-gray-600">{note}</p>
                        </div>
                    ))}
                </div>
            )}
        </Card>
    );
}
