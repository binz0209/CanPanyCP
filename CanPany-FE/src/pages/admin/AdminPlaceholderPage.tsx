import { useTranslation } from 'react-i18next';
import { Card } from '../../components/ui';
import type adminEn from '../../i18n/locales/en/admin.json';

export type AdminPlaceholderSection = keyof typeof adminEn.placeholders;

interface AdminPlaceholderPageProps {
    section: AdminPlaceholderSection;
}

export function AdminPlaceholderPage({ section }: AdminPlaceholderPageProps) {
    const { t } = useTranslation('admin');
    const titleKey = `placeholders.${section}.title` as const;
    const descKey = `placeholders.${section}.description` as const;
    return (
        <div className="space-y-4">
            <div>
                <h1 className="text-2xl font-bold text-gray-900">{t(titleKey)}</h1>
                <p className="mt-2 max-w-2xl text-sm text-gray-600">{t(descKey)}</p>
            </div>
            <Card className="border-2 border-dashed border-gray-200 bg-gray-50/50 p-8 text-center text-sm text-gray-500">
                {t(descKey)}
            </Card>
        </div>
    );
}
