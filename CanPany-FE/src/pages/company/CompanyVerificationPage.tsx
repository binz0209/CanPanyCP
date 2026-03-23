import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { z } from 'zod';
import { isAxiosError } from 'axios';
import toast from 'react-hot-toast';
import { CheckCircle2, FileText, ShieldCheck } from 'lucide-react';
import { Button, Card } from '../../components/ui';
import { companiesApi } from '../../api';
import { formatDateTime } from '../../utils';
import {
    CompanyProfileRequiredState,
    CompanyWorkspaceErrorState,
    CompanyWorkspaceLoader,
    SectionHeader,
    StatusBadge,
} from '../../components/features/companies';
import { useCompanyWorkspace } from '../../hooks/company/useCompanyWorkspace';
import { companyKeys } from '../../lib/queryKeys';
import { useTranslation } from 'react-i18next';

const createVerificationSchema = (t: (key: string) => string) =>
    z.object({
        documentUrlsText: z.string().trim().min(1, t('verification.validDocRequired')),
    });

type VerificationFormValues = z.infer<ReturnType<typeof createVerificationSchema>>;

export function CompanyVerificationPage() {
    const queryClient = useQueryClient();
    const { t } = useTranslation('company');
    const { companyId, isLoading: isWorkspaceLoading, isMissingProfile, hasFatalError } = useCompanyWorkspace();

    const verificationSchema = createVerificationSchema(t as unknown as (key: string) => string);

    const verificationQuery = useQuery({
        queryKey: companyKeys.verification(companyId!),
        queryFn: () => companiesApi.getVerificationStatus(companyId!),
        enabled: !!companyId,
    });

    const {
        register,
        handleSubmit,
        reset,
        formState: { errors },
    } = useForm<VerificationFormValues>({
        resolver: zodResolver(verificationSchema),
        defaultValues: { documentUrlsText: '' },
    });

    useEffect(() => {
        if (verificationQuery.data?.verificationDocuments?.length) {
            reset({
                documentUrlsText: verificationQuery.data.verificationDocuments.join('\n'),
            });
        }
    }, [verificationQuery.data, reset]);

    const requestMutation = useMutation({
        mutationFn: async (values: VerificationFormValues) => {
            const documentUrls = values.documentUrlsText
                .split('\n')
                .map((item) => item.trim())
                .filter(Boolean);

            await companiesApi.requestVerification({ documentUrls });
        },
        onSuccess: async () => {
            await Promise.all([
                queryClient.invalidateQueries({ queryKey: companyKeys.me() }),
                queryClient.invalidateQueries({ queryKey: companyKeys.verification(companyId!), exact: true }),
                queryClient.invalidateQueries({ queryKey: companyKeys.statistics(companyId!), exact: true }),
            ]);
            toast.success(t('verification.toastSuccess'));
        },
        onError: (error) => {
            const message = isAxiosError(error)
                ? error.response?.data?.message || t('verification.toastFailed')
                : t('verification.toastFailed');
            toast.error(message);
        },
    });

    if (isWorkspaceLoading || (companyId && verificationQuery.isLoading)) {
        return <CompanyWorkspaceLoader />;
    }

    if (isMissingProfile) {
        return (
            <CompanyProfileRequiredState
                title={t('verification.profileRequired')}
                description={t('verification.profileRequiredDesc')}
                icon={<ShieldCheck className="h-6 w-6" />}
            />
        );
    }

    if (hasFatalError || verificationQuery.error) {
        return (
            <CompanyWorkspaceErrorState
                title={t('verification.errorTitle')}
                description={t('verification.errorDesc')}
                icon={<ShieldCheck className="h-6 w-6" />}
            />
        );
    }

    const verification = verificationQuery.data;
    const isApproved = verification?.isVerified;

    return (
        <div className="space-y-6">
            <SectionHeader
                title={t('verification.title')}
                description={t('verification.description')}
            />

            <div className="grid gap-6 xl:grid-cols-[0.95fr_1.05fr]">
                <Card className="p-6">
                    <h2 className="text-lg font-semibold text-gray-900">{t('verification.statusTitle')}</h2>
                    <div className="mt-5 space-y-4">
                        <div className="rounded-xl bg-gray-50 p-4">
                            <p className="text-sm text-gray-500">{t('verification.statusCurrent')}</p>
                            <div className="mt-2">
                                <StatusBadge status={verification?.verificationStatus || 'Pending'} kind="verification" />
                            </div>
                        </div>

                        <div className="rounded-xl bg-gray-50 p-4">
                            <p className="text-sm text-gray-500">{t('verification.isVerified')}</p>
                            <p className="mt-2 text-sm font-semibold text-gray-900">
                                {verification?.isVerified ? t('verification.verifiedYes') : t('verification.verifiedNo')}
                            </p>
                        </div>

                        <div className="rounded-xl bg-gray-50 p-4">
                            <p className="text-sm text-gray-500">{t('verification.verifiedAt')}</p>
                            <p className="mt-2 text-sm font-semibold text-gray-900">
                                {verification?.verifiedAt ? formatDateTime(verification.verifiedAt) : t('verification.verifiedAtFallback')}
                            </p>
                        </div>

                        <div className="rounded-xl border border-dashed border-gray-300 p-4 text-sm text-gray-600">
                            {isApproved
                                ? t('verification.alreadyVerifiedHint')
                                : t('verification.reviewHint')}
                        </div>
                    </div>
                </Card>

                <Card className="p-6">
                    <div className="flex items-start gap-3">
                        <div className="rounded-lg bg-[#00b14f]/10 p-2 text-[#00b14f]">
                            <ShieldCheck className="h-5 w-5" />
                        </div>
                        <div>
                            <h2 className="text-lg font-semibold text-gray-900">{t('verification.formTitle')}</h2>
                            <p className="mt-1 text-sm text-gray-500">{t('verification.formHint')}</p>
                        </div>
                    </div>

                    <form onSubmit={handleSubmit((values) => requestMutation.mutate(values))} className="mt-6 space-y-5">
                        <div>
                            <label className="mb-2 block text-sm font-medium text-gray-700">
                                {t('verification.documentsLabel')}
                            </label>
                            <textarea
                                rows={10}
                                className="w-full rounded-lg border border-gray-300 px-4 py-3 text-sm text-gray-900 outline-none transition focus:border-[#00b14f] focus:ring-2 focus:ring-[#00b14f]/20"
                                placeholder={'https://storage.example.com/business-license.pdf\nhttps://storage.example.com/tax-registration.pdf'}
                                {...register('documentUrlsText')}
                            />
                            {errors.documentUrlsText?.message && (
                                <p className="mt-1.5 text-sm text-red-600">{errors.documentUrlsText.message}</p>
                            )}
                        </div>

                        <div className="flex flex-wrap gap-3">
                            <Button type="submit" isLoading={requestMutation.isPending}>
                                {t('verification.btnSubmit')}
                            </Button>
                            <Button
                                type="button"
                                variant="outline"
                                onClick={() => reset({ documentUrlsText: verification?.verificationDocuments?.join('\n') || '' })}
                                disabled={requestMutation.isPending}
                            >
                                {t('verification.btnReset')}
                            </Button>
                        </div>
                    </form>

                    <div className="mt-6 rounded-xl bg-gray-50 p-4">
                        <div className="flex items-center gap-2">
                            <FileText className="h-4 w-4 text-gray-500" />
                            <p className="text-sm font-semibold text-gray-900">{t('verification.existingDocs')}</p>
                        </div>

                        {verification?.verificationDocuments?.length ? (
                            <div className="mt-4 space-y-2">
                                {verification.verificationDocuments.map((documentUrl: string) => (
                                    <a
                                        key={documentUrl}
                                        href={documentUrl}
                                        target="_blank"
                                        rel="noopener noreferrer"
                                        className="block rounded-lg border border-gray-200 bg-white px-3 py-2 text-sm text-[#00b14f] hover:underline"
                                    >
                                        {documentUrl}
                                    </a>
                                ))}
                            </div>
                        ) : (
                            <p className="mt-4 text-sm text-gray-500">{t('verification.noDocuments')}</p>
                        )}

                        {isApproved && (
                            <div className="mt-4 flex items-start gap-2 rounded-lg bg-green-50 p-3 text-sm text-green-700">
                                <CheckCircle2 className="mt-0.5 h-4 w-4 shrink-0" />
                                <span>{t('verification.alreadyVerifiedHint')}</span>
                            </div>
                        )}
                    </div>
                </Card>
            </div>
        </div>
    );
}
