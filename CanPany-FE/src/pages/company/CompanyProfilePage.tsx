import { useEffect, useMemo } from 'react';
import { isAxiosError } from 'axios';
import { useForm, useWatch } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { z } from 'zod';
import { Building2, Globe, Image as ImageIcon, MapPin, Phone } from 'lucide-react';
import toast from 'react-hot-toast';
import { useTranslation } from 'react-i18next';
import { Button, Card, Input } from '../../components/ui';
import { companiesApi } from '../../api';
import type { Company } from '../../types';
import {
    CompanyProfilePreviewCard,
    CompanyWorkspaceErrorState,
    CompanyWorkspaceLoader,
    SectionHeader,
} from '../../components/features/companies';
import { useCompanyWorkspace } from '../../hooks/company/useCompanyWorkspace';
import { companyKeys } from '../../lib/queryKeys';

const createCompanyProfileSchema = (t: (key: string) => string) =>
    z.object({
        name: z.string().trim().min(2, t('profile.validNameMin')),
        logoUrl: z
            .string()
            .trim()
            .optional()
            .or(z.literal(''))
            .refine((value) => !value || /^https?:\/\//.test(value), t('profile.validLogoUrl')),
        website: z
            .string()
            .trim()
            .optional()
            .or(z.literal(''))
            .refine((value) => !value || /^https?:\/\//.test(value), t('profile.validWebsiteUrl')),
        phone: z.string().trim().max(20, t('profile.validPhoneMax')).optional(),
        address: z.string().trim().max(255, t('profile.validAddressMax')).optional(),
        description: z.string().trim().max(2000, t('profile.validDescMax')).optional(),
    });

type CompanyProfileFormValues = z.infer<ReturnType<typeof createCompanyProfileSchema>>;

function getDefaultValues(company?: Company): CompanyProfileFormValues {
    return {
        name: company?.name || '',
        logoUrl: company?.logoUrl || '',
        website: company?.website || '',
        phone: company?.phone || '',
        address: company?.address || '',
        description: company?.description || '',
    };
}

export function CompanyProfilePage() {
    const queryClient = useQueryClient();
    const { t } = useTranslation('company');
    const { company, isLoading, isMissingProfile, hasFatalError } = useCompanyWorkspace();

    const companyProfileSchema = createCompanyProfileSchema(t as unknown as (key: string) => string);

    const {
        register,
        handleSubmit,
        reset,
        control,
        formState: { errors, isDirty },
    } = useForm<CompanyProfileFormValues>({
        resolver: zodResolver(companyProfileSchema),
        defaultValues: getDefaultValues(),
    });

    useEffect(() => {
        if (company) {
            reset(getDefaultValues(company));
        }
    }, [company, reset]);

    const [logoPreview, previewName, previewAddress, previewDescription] = useWatch({
        control,
        name: ['logoUrl', 'name', 'address', 'description'],
    });

    const profileMutation = useMutation({
        mutationFn: async (values: CompanyProfileFormValues) => {
            const payload = {
                name: values.name.trim(),
                logoUrl: values.logoUrl?.trim() || undefined,
                website: values.website?.trim() || undefined,
                phone: values.phone?.trim() || undefined,
                address: values.address?.trim() || undefined,
                description: values.description?.trim() || undefined,
            };

            // Company account có thể đã đăng ký nhưng chưa có bản ghi company.
            // FE cần xử lý cả 2 nhánh create và update để luồng profile không bị gãy.
            if (company) {
                await companiesApi.updateMe(payload);
                return;
            }

            await companiesApi.create({
                name: payload.name,
                logoUrl: payload.logoUrl,
                website: payload.website,
                phone: payload.phone,
                address: payload.address,
                description: payload.description,
            });
        },
        onSuccess: async () => {
            await queryClient.invalidateQueries({ queryKey: companyKeys.me() });
            toast.success(company ? t('profile.toastUpdateSuccess') : t('profile.toastCreateSuccess'));
        },
        onError: (error) => {
            const message = isAxiosError(error)
                ? error.response?.data?.message || t('profile.toastSaveFailed')
                : t('profile.toastSaveFailed');
            toast.error(message);
        },
    });

    const verificationSummary = useMemo(() => {
        if (!company) {
            return t('profile.statusNotCreated');
        }

        if (company.isVerified) {
            return t('profile.statusVerified');
        }

        const statusLabel = (() => {
            if (company.verificationStatus === 'Pending') return t('verification.statusPending');
            if (company.verificationStatus === 'Approved') return t('verification.statusApproved');
            if (company.verificationStatus === 'Rejected') return t('verification.statusRejected');
            return company.verificationStatus;
        })();

        return `${t('profile.statusCurrent')}: ${statusLabel}`;
    }, [company]);

    if (isLoading) {
        return <CompanyWorkspaceLoader />;
    }

    if (hasFatalError) {
        return (
            <CompanyWorkspaceErrorState
                title={t('profile.errorTitle')}
                description={t('profile.errorDesc')}
                icon={<Building2 className="h-6 w-6" />}
            />
        );
    }

    return (
        <div className="space-y-6">
            <SectionHeader
                title={t('profile.title')}
                description={t('profile.description')}
            />

            <div className="grid gap-6 xl:grid-cols-[1.2fr_0.8fr]">
                <Card className="p-6">
                    <div className="mb-6 flex items-start justify-between gap-4">
                        <div>
                            <h2 className="text-xl font-semibold text-gray-900">
                                {company ? t('profile.formTitleUpdate') : t('profile.formTitleCreate')}
                            </h2>
                            <p className="mt-1 text-sm text-gray-500">
                                {t('profile.formSubtitle')}
                            </p>
                        </div>
                        {isMissingProfile && (
                            <span className="rounded-full bg-yellow-100 px-3 py-1 text-xs font-semibold text-yellow-700">
                                {t('profile.noBadge')}
                            </span>
                        )}
                    </div>

                    <form onSubmit={handleSubmit((values) => profileMutation.mutate(values))} className="space-y-5">
                        <Input
                            label={t('profile.nameLabel')}
                            placeholder={t('profile.namePlaceholder')}
                            icon={<Building2 className="h-4 w-4" />}
                            error={errors.name?.message}
                            {...register('name')}
                        />

                        <div className="grid gap-5 md:grid-cols-2">
                            <Input
                                label={t('profile.logoLabel')}
                                placeholder={t('profile.logoPlaceholder')}
                                icon={<ImageIcon className="h-4 w-4" />}
                                error={errors.logoUrl?.message}
                                {...register('logoUrl')}
                            />
                            <Input
                                label={t('profile.websiteLabel')}
                                placeholder={t('profile.websitePlaceholder')}
                                icon={<Globe className="h-4 w-4" />}
                                error={errors.website?.message}
                                {...register('website')}
                            />
                        </div>

                        <div className="grid gap-5 md:grid-cols-2">
                            <Input
                                label={t('profile.phoneLabel')}
                                placeholder={t('profile.phonePlaceholder')}
                                icon={<Phone className="h-4 w-4" />}
                                error={errors.phone?.message}
                                {...register('phone')}
                            />
                            <Input
                                label={t('profile.addressLabel')}
                                placeholder={t('profile.addressPlaceholder')}
                                icon={<MapPin className="h-4 w-4" />}
                                error={errors.address?.message}
                                {...register('address')}
                            />
                        </div>

                        <div>
                            <label className="mb-2 block text-sm font-medium text-gray-700">{t('profile.descriptionLabel')}</label>
                            <textarea
                                rows={8}
                                className="w-full rounded-lg border border-gray-300 px-4 py-3 text-sm text-gray-900 outline-none transition focus:border-[#00b14f] focus:ring-2 focus:ring-[#00b14f]/20"
                                placeholder={t('profile.descriptionPlaceholder')}
                                {...register('description')}
                            />
                            {errors.description?.message && (
                                <p className="mt-1.5 text-sm text-red-600">{errors.description.message}</p>
                            )}
                        </div>

                        <div className="flex flex-wrap gap-3 border-t border-gray-100 pt-4">
                            <Button
                                type="submit"
                                isLoading={profileMutation.isPending}
                                disabled={!isDirty && !!company}
                            >
                                {company ? t('profile.btnSave') : t('profile.btnCreate')}
                            </Button>
                            <Button
                                type="button"
                                variant="outline"
                                onClick={() => reset(getDefaultValues(company))}
                                disabled={profileMutation.isPending}
                            >
                                {t('profile.btnReset')}
                            </Button>
                        </div>
                    </form>
                </Card>

                <div className="space-y-6">
                    <CompanyProfilePreviewCard
                        logoUrl={logoPreview}
                        name={previewName}
                        address={previewAddress}
                        description={previewDescription}
                    />

                    <Card className="p-6">
                        <h2 className="text-lg font-semibold text-gray-900">{t('profile.previewTitle')}</h2>
                        <div className="mt-4 space-y-3 text-sm text-gray-600">
                            <p>{verificationSummary}</p>
                            <p>
                                {t('profile.previewHint')}
                            </p>
                        </div>
                    </Card>
                </div>
            </div>
        </div>
    );
}
