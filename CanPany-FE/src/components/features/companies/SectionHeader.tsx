import type { ReactNode } from 'react';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { ArrowLeft } from 'lucide-react';

interface SectionHeaderProps {
    title: string;
    description?: string;
    eyebrow?: string;
    actions?: ReactNode;
    backLink?: string;
    backLabel?: string;
    tone?: 'default' | 'hero';
}

export function SectionHeader({
    title,
    description,
    eyebrow,
    actions,
    backLink,
    backLabel,
    tone = 'default',
}: SectionHeaderProps) {
    const isHero = tone === 'hero';
    const { t } = useTranslation('common');
    const resolvedBackLabel = backLabel ?? t('buttons.back');

    return (
        <section
            className={
                isHero
                    ? 'rounded-2xl bg-gradient-to-r from-[#00b14f] to-[#009245] p-6 text-white shadow-lg'
                    : 'rounded-2xl bg-white p-6 shadow-sm'
            }
        >
            <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
                <div>
                    {backLink && (
                        <Link
                            to={backLink}
                            className={isHero
                                ? 'inline-flex items-center gap-2 text-sm text-white/85 hover:text-white'
                                : 'inline-flex items-center gap-2 text-sm text-gray-500 hover:text-gray-700'}
                        >
                            <ArrowLeft className="h-4 w-4" />
                            {resolvedBackLabel}
                        </Link>
                    )}
                    {eyebrow && (
                        <p className={backLink ? 'mt-4 text-sm' : 'text-sm'}>
                            {eyebrow}
                        </p>
                    )}
                    <h1 className={isHero ? 'mt-2 text-3xl font-bold text-white' : 'text-3xl font-bold text-gray-900'}>
                        {title}
                    </h1>
                    {description && (
                        <p className={isHero ? 'mt-3 max-w-3xl text-sm leading-6 text-white/90' : 'mt-2 max-w-3xl text-sm leading-6 text-gray-600'}>
                            {description}
                        </p>
                    )}
                </div>

                {actions && <div className="flex flex-wrap gap-3">{actions}</div>}
            </div>
        </section>
    );
}
