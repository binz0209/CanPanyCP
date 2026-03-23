import { useEffect } from 'react';
import { Link, Navigate, useNavigate, useParams } from 'react-router-dom';
import { isAxiosError } from 'axios';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { z } from 'zod';
import toast from 'react-hot-toast';
import { useTranslation } from 'react-i18next';
import { BriefcaseBusiness } from 'lucide-react';
import { Button, Card } from '../../components/ui';
import { jobsApi } from '../../api';
import type { BudgetType, JobLevel } from '../../types';
import {
    CompanyProfileRequiredState,
    CompanyWorkspaceErrorState,
    CompanyWorkspaceLoader,
    CompanyJobPreviewCard,
    JobFormFields,
    SectionHeader,
} from '../../components/features/companies';
import type { CompanyJobFormValues } from '../../components/features/companies';
import { useCompanyWorkspace } from '../../hooks/company/useCompanyWorkspace';
import { companyKeys } from '../../lib/queryKeys';

const createJobFormSchema = (t: (key: string) => string) => z.object({
    title: z.string().trim().min(5, t('jobForm.validTitleMin')),
    description: z.string().trim().min(20, t('jobForm.validDescriptionMin')),
    categoryId: z.string().trim().optional(),
    skillIdsText: z.string().trim().optional(),
    budgetType: z.enum(['Fixed', 'Hourly']),
    budgetAmount: z
        .string()
        .trim()
        .optional()
        .refine((value) => !value || !Number.isNaN(Number(value)), t('jobForm.validBudgetNumber')),
    level: z.enum(['Junior', 'Mid', 'Senior', 'Expert']).optional(),
    location: z.string().trim().optional(),
    isRemote: z.boolean(),
    deadline: z.string().optional(),
});

const levelOptions: JobLevel[] = ['Junior', 'Mid', 'Senior', 'Expert'];
const budgetTypeOptions: BudgetType[] = ['Fixed', 'Hourly'];

function toDateInputValue(value?: Date | string) {
    if (!value) return '';
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return '';
    return date.toISOString().split('T')[0];
}

export function CompanyJobFormPage() {
    const { t } = useTranslation('company');
    const jobFormSchema = createJobFormSchema(t as (key: string) => string);
    const { jobId } = useParams<{ jobId: string }>();
    const isEditMode = Boolean(jobId);
    const navigate = useNavigate();
    const queryClient = useQueryClient();
    const { company, companyId, isLoading: isWorkspaceLoading, isMissingProfile, hasFatalError } = useCompanyWorkspace();

    const jobQuery = useQuery({
        queryKey: companyKeys.workspaceJobDetail(jobId!),
        queryFn: () => jobsApi.getById(jobId!),
        enabled: isEditMode,
    });

    const {
        register,
        handleSubmit,
        reset,
        control,
        watch,
        formState: { errors, isDirty },
    } = useForm<CompanyJobFormValues>({
        resolver: zodResolver(jobFormSchema),
        defaultValues: {
            title: '',
            description: '',
            categoryId: '',
            skillIdsText: '',
            budgetType: 'Fixed',
            budgetAmount: '',
            level: undefined,
            location: '',
            isRemote: false,
            deadline: '',
        },
    });

    useEffect(() => {
        if (!jobQuery.data?.job) return;

        const job = jobQuery.data.job;
        reset({
            title: job.title,
            description: job.description,
            categoryId: job.categoryId || '',
            skillIdsText: job.skillIds.join(', '),
            budgetType: job.budgetType,
            budgetAmount: job.budgetAmount ? String(job.budgetAmount) : '',
            level: job.level || undefined,
            location: job.location || '',
            isRemote: job.isRemote,
            deadline: toDateInputValue(job.deadline),
        });
    }, [jobQuery.data, reset]);

    const saveMutation = useMutation({
        mutationFn: async (values: CompanyJobFormValues) => {
            if (!company) {
                throw new Error('Company profile is required before managing jobs');
            }

            const normalizedSkillIds = (values.skillIdsText || '')
                .split(',')
                .map((item) => item.trim())
                .filter(Boolean);

            if (!isEditMode) {
                await jobsApi.create({
                    companyId: company.id,
                    title: values.title.trim(),
                    description: values.description.trim(),
                    categoryId: values.categoryId?.trim() || undefined,
                    skillIds: normalizedSkillIds,
                    budgetType: values.budgetType,
                    budgetAmount: values.budgetAmount ? Number(values.budgetAmount) : undefined,
                    level: values.level,
                    location: values.location?.trim() || undefined,
                    isRemote: values.isRemote,
                    deadline: values.deadline ? new Date(values.deadline) : undefined,
                });
                return;
            }

            await jobsApi.update(jobId!, {
                title: values.title.trim(),
                description: values.description.trim(),
                skillIds: normalizedSkillIds,
                budgetAmount: values.budgetAmount ? Number(values.budgetAmount) : undefined,
                level: values.level,
                location: values.location?.trim() || undefined,
                deadline: values.deadline ? new Date(values.deadline) : undefined,
            });
        },
        onSuccess: async () => {
            await Promise.all([
                companyId
                    ? queryClient.invalidateQueries({ queryKey: companyKeys.workspaceJobs(companyId), exact: true })
                    : Promise.resolve(),
                isEditMode && jobId
                    ? queryClient.invalidateQueries({ queryKey: companyKeys.workspaceJobDetail(jobId), exact: true })
                    : Promise.resolve(),
            ]);
            toast.success(isEditMode ? t('jobForm.updateSuccess') : t('jobForm.createSuccess'));
            navigate('/company/jobs', { replace: true });
        },
        onError: (error) => {
            const message = isAxiosError(error)
                ? error.response?.data?.message || t('jobForm.saveFailed')
                : error instanceof Error
                    ? error.message
                    : t('jobForm.saveFailed');
            toast.error(message);
        },
    });

    if (isWorkspaceLoading || (isEditMode && jobQuery.isLoading)) {
        return <CompanyWorkspaceLoader />;
    }

    if (isMissingProfile) {
        return (
            <CompanyProfileRequiredState
                title={t('jobForm.profileRequiredTitle')}
                description={t('jobForm.profileRequiredDesc')}
                icon={<BriefcaseBusiness className="h-6 w-6" />}
                action={
                    <Link to="/company/profile">
                        <Button>{t('jobForm.btnGoProfile')}</Button>
                    </Link>
                }
            />
        );
    }

    if (hasFatalError || jobQuery.error) {
        return (
            <CompanyWorkspaceErrorState
                title={t('jobForm.errorLoadTitle')}
                description={t('jobForm.errorLoadDesc')}
                icon={<BriefcaseBusiness className="h-6 w-6" />}
            />
        );
    }

    const editingJob = jobQuery.data?.job;

    if (editingJob && company && editingJob.companyId !== company.id) {
        return <Navigate to="/company/jobs" replace />;
    }

    const handleReset = () => {
        if (editingJob) {
            reset({
                title: editingJob.title,
                description: editingJob.description,
                categoryId: editingJob.categoryId || '',
                skillIdsText: editingJob.skillIds.join(', '),
                budgetType: editingJob.budgetType,
                budgetAmount: editingJob.budgetAmount ? String(editingJob.budgetAmount) : '',
                level: editingJob.level || undefined,
                location: editingJob.location || '',
                isRemote: editingJob.isRemote,
                deadline: toDateInputValue(editingJob.deadline),
            });
            return;
        }

        reset({
            title: '',
            description: '',
            categoryId: '',
            skillIdsText: '',
            budgetType: 'Fixed',
            budgetAmount: '',
            level: undefined,
            location: '',
            isRemote: false,
            deadline: '',
        });
    };

    return (
        <div className="space-y-6">
            <SectionHeader
                backLink="/company/jobs"
                backLabel={t('jobForm.backLabel')}
                title={isEditMode ? t('jobForm.pageTitleEdit') : t('jobForm.pageTitleCreate')}
                description={t('jobForm.pageDescription')}
            />

            <div className="grid gap-6 xl:grid-cols-[1.15fr_0.85fr]">
                <Card className="p-6">
                    <form onSubmit={handleSubmit((values) => saveMutation.mutate(values))} className="space-y-5">
                        <JobFormFields
                            register={register}
                            errors={errors}
                            isEditMode={isEditMode}
                            budgetTypeOptions={budgetTypeOptions}
                            levelOptions={levelOptions}
                            categoryIdValue={watch('categoryId')}
                        />

                        <div className="flex flex-wrap gap-3 border-t border-gray-100 pt-4">
                            <Button
                                type="submit"
                                isLoading={saveMutation.isPending}
                                disabled={isEditMode ? !isDirty : false}
                            >
                                {isEditMode ? t('jobForm.btnSave') : t('jobForm.btnCreate')}
                            </Button>
                            <Button
                                type="button"
                                variant="outline"
                                onClick={handleReset}
                                disabled={saveMutation.isPending}
                            >
                                {t('jobForm.btnReset')}
                            </Button>
                        </div>
                    </form>
                </Card>

                <div className="space-y-6">
                    <CompanyJobPreviewCard control={control} />

                    <Card className="p-6">
                        <h2 className="text-lg font-semibold text-gray-900">{t('jobForm.tipsTitle')}</h2>
                        <ul className="mt-4 space-y-2 text-sm leading-6 text-gray-600">
                            <li>{t('jobForm.tip1')}</li>
                            <li>{t('jobForm.tip2')}</li>
                            <li>{t('jobForm.tip3')}</li>
                        </ul>
                    </Card>
                </div>
            </div>
        </div>
    );
}
