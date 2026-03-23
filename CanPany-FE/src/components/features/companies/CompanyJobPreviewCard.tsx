import type { Control } from 'react-hook-form';
import { useWatch } from 'react-hook-form';
import { Card } from '../../ui';
import type { CompanyJobFormValues } from './JobFormFields';
import { useTranslation } from 'react-i18next';

interface CompanyJobPreviewCardProps {
    control: Control<CompanyJobFormValues>;
}

export function CompanyJobPreviewCard({ control }: CompanyJobPreviewCardProps) {
    const { t } = useTranslation('company');
    const [previewTitle, previewLocation, previewIsRemote, previewSkills, previewDescription] = useWatch({
        control,
        name: ['title', 'location', 'isRemote', 'skillIdsText', 'description'],
    });

    return (
        <Card className="p-6">
            <h2 className="text-lg font-semibold text-gray-900">{t('jobForm.previewTitle')}</h2>
            <div className="mt-4 space-y-3">
                <div>
                    <p className="text-sm text-gray-500">{t('jobForm.previewTitleLabel')}</p>
                    <p className="mt-1 font-semibold text-gray-900">{previewTitle || t('jobForm.previewTitleFallback')}</p>
                </div>
                <div>
                    <p className="text-sm text-gray-500">{t('jobForm.previewLocationLabel')}</p>
                    <p className="mt-1 text-sm text-gray-700">
                        {previewLocation || (previewIsRemote ? t('jobForm.previewLocationRemote') : t('jobForm.previewLocationFallbackNone'))}
                    </p>
                </div>
                <div>
                    <p className="text-sm text-gray-500">{t('jobForm.previewSkillsLabel')}</p>
                    <p className="mt-1 text-sm text-gray-700">
                        {previewSkills || t('jobForm.previewSkillsFallback')}
                    </p>
                </div>
                <div>
                    <p className="text-sm text-gray-500">{t('jobForm.previewDescriptionLabel')}</p>
                    <p className="mt-1 whitespace-pre-wrap text-sm leading-6 text-gray-700">
                        {previewDescription || t('jobForm.previewDescriptionFallback')}
                    </p>
                </div>
            </div>
        </Card>
    );
}
