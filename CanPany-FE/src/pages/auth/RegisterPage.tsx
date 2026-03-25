import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useTranslation } from 'react-i18next';
import { Mail, Lock, User, Briefcase, Eye, EyeOff, Building2, UserCircle, ArrowRight, CheckCircle } from 'lucide-react';
import { Button, Input } from '../../components/ui';
import { authApi } from '../../api';
import { useAuthStore } from '../../stores/auth.store';
import { cn } from '../../utils';
import toast from 'react-hot-toast';

const createRegisterSchema = (t: (key: string) => string) =>
    z
        .object({
            fullName: z.string().min(2, t('register.fullNameMin')),
            email: z.string().email(t('register.emailInvalid')),
            password: z
                .string()
                .min(8, t('register.passwordMin'))
                .regex(/[A-Z]/, t('register.passwordUppercase'))
                .regex(/[a-z]/, t('register.passwordLowercase'))
                .regex(/[0-9]/, t('register.passwordNumber')),
            confirmPassword: z.string(),
            role: z.enum(['Candidate', 'Company']),
        })
        .refine((data) => data.password === data.confirmPassword, {
            message: t('register.passwordMismatch'),
            path: ['confirmPassword'],
        });

type RegisterForm = z.infer<ReturnType<typeof createRegisterSchema>>;

const benefitsKey = [
    'register.benefit1',
    'register.benefit2',
    'register.benefit3',
    'register.benefit4',
];

export function RegisterPage() {
    const { t } = useTranslation('auth');
    const registerSchema = createRegisterSchema(t as unknown as (key: string) => string);
    const [showPassword, setShowPassword] = useState(false);
    const [isLoading, setIsLoading] = useState(false);
    const navigate = useNavigate();
    const setAuth = useAuthStore((state) => state.setAuth);

    const {
        register,
        handleSubmit,
        watch,
        setValue,
        formState: { errors },
    } = useForm<RegisterForm>({
        resolver: zodResolver(registerSchema),
        defaultValues: {
            role: 'Candidate',
        },
    });

    const selectedRole = watch('role');

    const onSubmit = async (data: RegisterForm) => {
        setIsLoading(true);
        try {
            const response = await authApi.register({
                fullName: data.fullName,
                email: data.email,
                password: data.password,
                confirmPassword: data.confirmPassword,
                role: data.role,
            });
            setAuth(response.user, response.accessToken);
            toast.success(t('register.success'));

            const redirectPath = data.role === 'Candidate'
                ? '/candidate/dashboard'
                : '/company/dashboard';

            navigate(redirectPath, { replace: true });
        } catch (error: any) {
            toast.error(error.response?.data?.message || t('register.failed'));
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="flex min-h-screen">
            {/* Left Side - Promo */}
            <div className="hidden bg-gradient-to-br from-[#00b14f] via-[#00a045] to-[#008f3c] lg:flex lg:w-1/2 lg:flex-col lg:justify-center lg:p-12">
                <div className="max-w-lg text-white">
                    <h2 className="text-3xl font-bold">{t('register.promoTitle')}</h2>
                    <p className="mt-4 text-lg text-white/90">
                        {t('register.promoDescription')}
                    </p>

                    <div className="mt-8 space-y-4">
                        {benefitsKey.map((key) => (
                            <div key={key} className="flex items-center gap-3">
                                <div className="flex h-6 w-6 items-center justify-center rounded-full bg-white/20">
                                    <CheckCircle className="h-4 w-4" />
                                </div>
                                <span className="text-white/90">{t(key as any)}</span>
                            </div>
                        ))}
                    </div>

                    <div className="mt-12 rounded-2xl bg-white/10 p-6 backdrop-blur-sm">
                        <p className="text-lg font-medium">{t('register.testimonialQuote')}</p>
                        <div className="mt-4 flex items-center gap-3">
                            <div className="h-10 w-10 rounded-full bg-white/20" />
                            <div>
                                <p className="font-medium">{t('register.testimonialName')}</p>
                                <p className="text-sm text-white/70">{t('register.testimonialTitle')}</p>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            {/* Right Side - Form */}
            <div className="flex flex-1 items-center justify-center px-4 py-12">
                <div className="w-full max-w-md">
                    {/* Logo */}
                    <div className="mb-8">
                        <Link to="/" className="inline-flex items-center gap-2">
                            <div className="flex h-11 w-11 items-center justify-center rounded-xl bg-gradient-to-br from-[#00b14f] to-[#008f3c]">
                                <Briefcase className="h-6 w-6 text-white" />
                            </div>
                            <span className="text-2xl font-bold text-gray-900">
                                Can<span className="text-[#00b14f]">Pany</span>
                            </span>
                        </Link>
                    </div>

                        <div>
                            <h1 className="text-2xl font-bold text-gray-900">{t('register.title')}</h1>
                            <p className="mt-2 text-gray-600">{t('register.description')}</p>
                        </div>

                    {/* Role Selection */}
                    <div className="mt-6">
                        <div className="grid grid-cols-2 gap-3">
                            <button
                                type="button"
                                onClick={() => setValue('role', 'Candidate')}
                                className={cn(
                                    'flex flex-col items-center gap-2 rounded-xl border-2 p-4 transition-all',
                                    selectedRole === 'Candidate'
                                        ? 'border-[#00b14f] bg-[#00b14f]/5'
                                        : 'border-gray-200 hover:border-gray-300'
                                )}
                            >
                                <div className={cn(
                                    'flex h-12 w-12 items-center justify-center rounded-full',
                                    selectedRole === 'Candidate' ? 'bg-[#00b14f]/10 text-[#00b14f]' : 'bg-gray-100 text-gray-500'
                                )}>
                                    <UserCircle className="h-6 w-6" />
                                </div>
                                <span className={cn(
                                    'text-sm font-medium',
                                    selectedRole === 'Candidate' ? 'text-[#00b14f]' : 'text-gray-700'
                                )}>{t('register.roleCandidate')}</span>
                            </button>
                            <button
                                type="button"
                                onClick={() => setValue('role', 'Company')}
                                className={cn(
                                    'flex flex-col items-center gap-2 rounded-xl border-2 p-4 transition-all',
                                    selectedRole === 'Company'
                                        ? 'border-[#00b14f] bg-[#00b14f]/5'
                                        : 'border-gray-200 hover:border-gray-300'
                                )}
                            >
                                <div className={cn(
                                    'flex h-12 w-12 items-center justify-center rounded-full',
                                    selectedRole === 'Company' ? 'bg-[#00b14f]/10 text-[#00b14f]' : 'bg-gray-100 text-gray-500'
                                )}>
                                    <Building2 className="h-6 w-6" />
                                </div>
                                <span className={cn(
                                    'text-sm font-medium',
                                    selectedRole === 'Company' ? 'text-[#00b14f]' : 'text-gray-700'
                                )}>{t('register.roleCompany')}</span>
                            </button>
                        </div>
                        <input type="hidden" {...register('role')} />
                    </div>

                    <form onSubmit={handleSubmit(onSubmit)} className="mt-6 space-y-4">
                        <Input
                            label={t('register.fullNameLabel')}
                            type="text"
                            placeholder={t('register.fullNamePlaceholder')}
                            icon={<User className="h-5 w-5" />}
                            error={errors.fullName?.message}
                            {...register('fullName')}
                        />

                        <Input
                            label={t('register.emailLabel')}
                            type="email"
                            placeholder={t('register.emailPlaceholder')}
                            icon={<Mail className="h-5 w-5" />}
                            error={errors.email?.message}
                            {...register('email')}
                        />

                        <div className="relative">
                            <Input
                                label={t('register.passwordLabel')}
                                type={showPassword ? 'text' : 'password'}
                                placeholder={t('register.passwordPlaceholder')}
                                icon={<Lock className="h-5 w-5" />}
                                error={errors.password?.message}
                                {...register('password')}
                            />
                            <button
                                type="button"
                                className="absolute right-3 top-10 text-gray-400 hover:text-gray-600"
                                onClick={() => setShowPassword(!showPassword)}
                            >
                                {showPassword ? <EyeOff className="h-5 w-5" /> : <Eye className="h-5 w-5" />}
                            </button>
                        </div>

                        <Input
                            label={t('register.passwordConfirmLabel')}
                            type={showPassword ? 'text' : 'password'}
                                placeholder={t('register.passwordPlaceholder')}
                            icon={<Lock className="h-5 w-5" />}
                            error={errors.confirmPassword?.message}
                            {...register('confirmPassword')}
                        />

                        <div className="pt-2">
                            <Button type="submit" className="w-full" size="lg" isLoading={isLoading}>
                                {t('register.submit')}
                                <ArrowRight className="h-4 w-4" />
                            </Button>
                        </div>
                    </form>

                    <p className="mt-6 text-center text-sm text-gray-600">
                        {t('register.termsPrefix')}{' '}
                        <a href="#" className="font-medium text-[#00b14f] hover:underline">{t('register.termsLink')}</a>
                        {' '}{t('register.and')}{' '}
                        <a href="#" className="font-medium text-[#00b14f] hover:underline">{t('register.privacyLink')}</a>
                    </p>

                    <p className="mt-6 text-center text-sm text-gray-600">
                        {t('register.haveAccount')}{' '}
                        <Link to="/auth/login" className="font-semibold text-[#00b14f] hover:text-[#008f3c]">
                            {t('login.title')}
                        </Link>
                    </p>
                </div>
            </div>
        </div>
    );
}
