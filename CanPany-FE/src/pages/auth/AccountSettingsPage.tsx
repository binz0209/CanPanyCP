import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Shield, KeyRound, Save, Mail, User, ShieldAlert } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import { isAxiosError } from 'axios';
import { useAuthStore as useAuth } from '../../stores/auth.store';
import { authApi } from '../../api';
import { Card, Button, Input } from '../../components/ui';

const createPasswordSchema = (t: (key: string) => string) =>
    z.object({
        oldPassword: z.string().min(1, { message: t('accountSettings.requiredOldPassword') }),
        newPassword: z.string().min(8, { message: t('accountSettings.minLengthNewPassword') }),
        confirmPassword: z.string().min(1, { message: t('accountSettings.requiredConfirmPassword') }),
    }).refine((data) => data.newPassword === data.confirmPassword, {
        message: t('accountSettings.passwordMismatch'),
        path: ['confirmPassword'],
    });

type PasswordFormValues = z.infer<ReturnType<typeof createPasswordSchema>>;

export function AccountSettingsPage() {
    const { user } = useAuth();
    const { t } = useTranslation('common');
    const [isLoading, setIsLoading] = useState(false);

    const passwordSchema = createPasswordSchema(t as unknown as (key: string) => string);

    const {
        register,
        handleSubmit,
        reset,
        formState: { errors, isDirty },
    } = useForm<PasswordFormValues>({
        resolver: zodResolver(passwordSchema),
        defaultValues: {
            oldPassword: '',
            newPassword: '',
            confirmPassword: '',
        },
    });

    const onSubmit = async (data: PasswordFormValues) => {
        setIsLoading(true);
        try {
            await authApi.changePassword(data.oldPassword, data.newPassword);
            toast.success(t('accountSettings.changePasswordSuccess'));
            reset();
        } catch (error) {
            if (isAxiosError(error)) {
                toast.error(error.response?.data?.message || t('accountSettings.changePasswordFailed'));
            } else {
                toast.error(t('accountSettings.changePasswordFailed'));
            }
        } finally {
            setIsLoading(false);
        }
    };

    if (!user) {
        return null; // Should be handled by PrivateRoute
    }

    return (
        <div className="space-y-6">
            <div>
                <h1 className="text-2xl font-bold text-gray-900">{t('accountSettings.title')}</h1>
                <p className="mt-1 text-sm text-gray-500">
                    {t('accountSettings.subtitle')}
                </p>
            </div>

            <div className="grid gap-6 lg:grid-cols-[1fr_300px]">
                <div className="space-y-6">
                    {/* Basic Info (Readonly for now as API handles name/email change differently) */}
                    <Card className="p-6">
                        <div className="mb-4 flex items-center gap-2">
                            <User className="h-5 w-5 text-[#00b14f]" />
                            <h2 className="text-lg font-semibold text-gray-900">{t('accountSettings.basicInfo')}</h2>
                        </div>
                        <div className="grid gap-4 sm:grid-cols-2">
                            <div>
                                <label className="mb-1.5 block text-sm font-medium text-gray-700">
                                    {t('accountSettings.fullName')}
                                </label>
                                <Input
                                    readOnly
                                    disabled
                                    value={user.fullName}
                                    icon={<User className="h-4 w-4" />}
                                />
                                <p className="mt-1 text-xs text-gray-500">{t('accountSettings.nameHint')}</p>
                            </div>
                            <div>
                                <label className="mb-1.5 block text-sm font-medium text-gray-700">
                                    {t('accountSettings.email')}
                                </label>
                                <Input
                                    readOnly
                                    disabled
                                    value={user.email}
                                    icon={<Mail className="h-4 w-4" />}
                                />
                                <p className="mt-1 text-xs text-gray-500">{t('accountSettings.emailHint')}</p>
                            </div>
                        </div>
                    </Card>

                    {/* Change Password */}
                    <Card className="p-6">
                        <div className="mb-4 flex items-center gap-2">
                            <KeyRound className="h-5 w-5 text-[#00b14f]" />
                            <h2 className="text-lg font-semibold text-gray-900">{t('accountSettings.changePassword')}</h2>
                        </div>
                        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
                            <Input
                                type="password"
                                label={t('accountSettings.oldPassword')}
                                placeholder="••••••••"
                                error={errors.oldPassword?.message}
                                {...register('oldPassword')}
                            />
                            <div className="grid gap-4 sm:grid-cols-2">
                                <Input
                                    type="password"
                                    label={t('accountSettings.newPassword')}
                                    placeholder="••••••••"
                                    error={errors.newPassword?.message}
                                    {...register('newPassword')}
                                />
                                <Input
                                    type="password"
                                    label={t('accountSettings.confirmPassword')}
                                    placeholder="••••••••"
                                    error={errors.confirmPassword?.message}
                                    {...register('confirmPassword')}
                                />
                            </div>

                            <div className="pt-4">
                                <Button
                                    type="submit"
                                    isLoading={isLoading}
                                    disabled={!isDirty || isLoading}
                                    className="flex items-center gap-2"
                                >
                                    <Save className="h-4 w-4" />
                                    {t('accountSettings.savePassword')}
                                </Button>
                            </div>
                        </form>
                    </Card>
                </div>

                <div className="space-y-6">
                    <Card className="bg-gray-50 p-5">
                        <div className="flex items-start gap-3">
                            <ShieldAlert className="mt-0.5 h-5 w-5 text-gray-600" />
                            <div>
                                <h3 className="font-medium text-gray-900">{t('accountSettings.securityTipsTitle')}</h3>
                                <ul className="mt-2 list-inside list-disc space-y-1.5 text-sm text-gray-600">
                                    <li>{t('accountSettings.securityTip1')}</li>
                                    <li>{t('accountSettings.securityTip2')}</li>
                                    <li>{t('accountSettings.securityTip3')}</li>
                                </ul>
                            </div>
                        </div>
                    </Card>

                    <Card className="p-5">
                         <div className="flex items-center gap-3 border-b border-gray-100 pb-4">
                            <Shield className="h-5 w-5 text-gray-600" />
                            <div>
                                <h3 className="font-medium text-gray-900">{t('accountSettings.roleTitle')}</h3>
                                <p className="text-sm text-gray-500">{t('accountSettings.roleSubtitle')}</p>
                            </div>
                        </div>
                        <div className="mt-4">
                            <div className="inline-flex items-center rounded-full bg-[#00b14f]/10 px-3 py-1.5 text-sm font-medium text-[#00b14f]">
                                {user.role === 'Company' ? t('accountSettings.roleCompany') : t('accountSettings.roleCandidate')}
                            </div>
                        </div>
                    </Card>
                </div>
            </div>
        </div>
    );
}
