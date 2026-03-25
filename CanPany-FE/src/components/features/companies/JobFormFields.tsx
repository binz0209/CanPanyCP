import type { FieldErrors, UseFormRegister } from 'react-hook-form';
import { useTranslation } from 'react-i18next';
import { BriefcaseBusiness, MapPin, Wallet } from 'lucide-react';
import { Input } from '../../ui';
import type { BudgetType, JobLevel } from '../../../types';

export interface CompanyJobFormValues {
    title: string;
    description: string;
    categoryId?: string;
    skillIdsText?: string;
    budgetType: BudgetType;
    budgetAmount?: string;
    level?: JobLevel;
    location?: string;
    isRemote: boolean;
    deadline?: string;
}

interface JobFormFieldsProps {
    register: UseFormRegister<CompanyJobFormValues>;
    errors: FieldErrors<CompanyJobFormValues>;
    isEditMode: boolean;
    budgetTypeOptions: BudgetType[];
    levelOptions: JobLevel[];
    categoryIdValue?: string;
}

export function JobFormFields({
    register,
    errors,
    isEditMode,
    budgetTypeOptions,
    levelOptions,
    categoryIdValue,
}: JobFormFieldsProps) {
    const { t } = useTranslation('company');
    return (
        <>
            <Input
                label={t('jobForm.titleLabel')}
                placeholder={t('jobForm.titlePlaceholder')}
                icon={<BriefcaseBusiness className="h-4 w-4" />}
                error={errors.title?.message}
                {...register('title')}
            />

            <div>
                <label className="mb-2 block text-sm font-medium text-gray-700">{t('jobForm.descriptionLabel')}</label>
                <textarea
                    rows={10}
                    className="w-full rounded-lg border border-gray-300 px-4 py-3 text-sm text-gray-900 outline-none transition focus:border-[#00b14f] focus:ring-2 focus:ring-[#00b14f]/20"
                    placeholder={t('jobForm.descriptionPlaceholder')}
                    {...register('description')}
                />
                {errors.description?.message && (
                    <p className="mt-1.5 text-sm text-red-600">{errors.description.message}</p>
                )}
            </div>

            {!isEditMode && (
                <div className="space-y-1.5">
                    <Input
                        label={t('jobForm.categoryLabel')}
                        placeholder={t('jobForm.categoryPlaceholder')}
                        error={errors.categoryId?.message}
                        {...register('categoryId')}
                    />
                    <p className="text-xs text-gray-500">{t('jobForm.categoryHelp')}</p>
                    {categoryIdValue && categoryIdValue.trim().includes(' ') && (
                        <p className="text-xs text-amber-600">{t('jobForm.categoryWarningSpaces')}</p>
                    )}
                    {!categoryIdValue && (
                        <p className="text-xs text-gray-500">{t('jobForm.categoryHelpWhenEmpty')}</p>
                    )}
                </div>
            )}

            <Input
                label={t('jobForm.skillsLabel')}
                placeholder={t('jobForm.skillsPlaceholder')}
                error={errors.skillIdsText?.message}
                {...register('skillIdsText')}
            />

            <div className="grid gap-5 md:grid-cols-2">
                <div>
                    <label className="mb-2 block text-sm font-medium text-gray-700">{t('jobForm.budgetTypeLabel')}</label>
                    <select
                        className="h-11 w-full rounded-lg border border-gray-300 bg-white px-4 text-sm text-gray-900 outline-none transition focus:border-[#00b14f] focus:ring-2 focus:ring-[#00b14f]/20 disabled:bg-gray-50"
                        disabled={isEditMode}
                        {...register('budgetType')}
                    >
                        {budgetTypeOptions.map((option) => (
                            <option key={option} value={option}>{option}</option>
                        ))}
                    </select>
                </div>

                <Input
                    label={t('jobForm.budgetAmountLabel')}
                    placeholder={t('jobForm.budgetAmountPlaceholder')}
                    icon={<Wallet className="h-4 w-4" />}
                    error={errors.budgetAmount?.message}
                    {...register('budgetAmount')}
                />
            </div>

            <div className="grid gap-5 md:grid-cols-2">
                <div>
                    <label className="mb-2 block text-sm font-medium text-gray-700">{t('jobForm.levelLabel')}</label>
                    <select
                        className="h-11 w-full rounded-lg border border-gray-300 bg-white px-4 text-sm text-gray-900 outline-none transition focus:border-[#00b14f] focus:ring-2 focus:ring-[#00b14f]/20"
                        {...register('level')}
                    >
                        <option value="">{t('jobForm.levelSelectPlaceholder')}</option>
                        {levelOptions.map((option) => (
                            <option key={option} value={option}>{option}</option>
                        ))}
                    </select>
                </div>

                <Input
                    label={t('jobForm.locationLabel')}
                    placeholder={t('jobForm.locationPlaceholder')}
                    icon={<MapPin className="h-4 w-4" />}
                    error={errors.location?.message}
                    {...register('location')}
                />
            </div>

            <div className="grid gap-5 md:grid-cols-2">
                <Input
                    label={t('jobForm.deadlineLabel')}
                    type="date"
                    error={errors.deadline?.message}
                    {...register('deadline')}
                />

                <label className="flex items-center gap-3 rounded-lg border border-gray-200 px-4 py-3 text-sm text-gray-700">
                    <input
                        type="checkbox"
                        className="h-4 w-4 rounded border-gray-300 text-[#00b14f] focus:ring-[#00b14f]"
                        disabled={isEditMode}
                        {...register('isRemote')}
                    />
                    {t('jobForm.isRemoteLabel')}
                </label>
            </div>

            {isEditMode && (
                <div className="rounded-lg border border-dashed border-gray-300 bg-gray-50 p-4 text-sm text-gray-600">
                    {t('jobForm.editModeRestrictedFields')}
                </div>
            )}
        </>
    );
}
