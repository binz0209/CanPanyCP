import { useState, useEffect } from 'react';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useTranslation } from 'react-i18next';
import { Mail, Lock, Briefcase, Eye, EyeOff, ArrowRight } from 'lucide-react';
import { Button, Input } from '../../components/ui';
import { authApi } from '../../api';
import { useAuthStore } from '../../stores/auth.store';
import toast from 'react-hot-toast';

const createLoginSchema = (t: (key: string) => string) =>
    z.object({
        email: z.string().email(t('login.emailInvalid')),
        password: z.string().min(6, t('login.passwordMin')),
    });

type LoginForm = z.infer<ReturnType<typeof createLoginSchema>>;

export function LoginPage() {
    const { t } = useTranslation('auth');
    const loginSchema = createLoginSchema(t as unknown as (key: string) => string);
    const [showPassword, setShowPassword] = useState(false);
    const [isLoading, setIsLoading] = useState(false);
    const navigate = useNavigate();
    const location = useLocation();
    const setAuth = useAuthStore((state) => state.setAuth);

    const from = (location.state as { from?: Location })?.from?.pathname || '/';

    const {
        register,
        handleSubmit,
        formState: { errors },
        watch,
    } = useForm<LoginForm>({
        resolver: zodResolver(loginSchema),
        defaultValues: {
            email: localStorage.getItem('loginEmail') || '',
            password: '',
        },
    });

    // Persist email to localStorage
    useEffect(() => {
        const subscription = watch((value: Partial<LoginForm>) => {
            if (value.email !== undefined) {
                localStorage.setItem('loginEmail', value.email);
            }
        });
        return () => subscription.unsubscribe();
    }, [watch]);

    // Show persisted error toast on mount
    useEffect(() => {
        const errorMsg = localStorage.getItem('loginError');
        if (errorMsg) {
            toast.error(errorMsg, { duration: 8000 });
            localStorage.removeItem('loginError');
        }
    }, []);

    const onSubmit = async (data: LoginForm) => {
        setIsLoading(true);
        try {
            const response = await authApi.login(data);
            localStorage.removeItem('loginError'); // Clear any previous error
            setAuth(response.user, response.accessToken);
            toast.success(t('login.success'));

            const redirectPath = response.user.role === 'Candidate'
                ? '/candidate/dashboard'
                : response.user.role === 'Company'
                    ? '/company/dashboard'
                    : response.user.role === 'Admin'
                        ? '/admin/dashboard'
                        : from;

            setIsLoading(false);
            navigate(redirectPath, { replace: true });
        } catch (error: any) {
            setIsLoading(false);
            localStorage.setItem('loginError', error.response?.data?.message || t('login.failed'));
            // Toast will be shown on next mount due to page refresh
        }
    };

    const handleGoogleLogin = async () => {
        try {
            const { oauthUrl } = await authApi.getGoogleLinkUrl();
            window.location.href = oauthUrl;
        } catch (error) {
            toast.error(t('login.failed'));
        }
    };

    return (
        <div className="flex min-h-screen">
            {/* Left Side - Form */}
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
                            <h1 className="text-2xl font-bold text-gray-900">{t('login.title')}</h1>
                            <p className="mt-2 text-gray-600">{t('login.description')}</p>
                        </div>

                    <form onSubmit={handleSubmit(onSubmit)} className="mt-8 space-y-5">
                            <Input
                            label={t('login.emailLabel')}
                            type="email"
                            placeholder={t('login.emailPlaceholder')}
                            icon={<Mail className="h-5 w-5" />}
                            error={errors.email?.message}
                            {...register('email')}
                        />

                        <div className="relative">
                            <Input
                            label={t('login.passwordLabel')}
                                type={showPassword ? 'text' : 'password'}
                            placeholder={t('login.passwordPlaceholder')}
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

                        <div className="flex items-center justify-between">
                            <label className="flex items-center gap-2">
                                <input type="checkbox" className="h-4 w-4 rounded border-gray-300 text-[#00b14f] focus:ring-[#00b14f]" />
                                <span className="text-sm text-gray-600">{t('login.rememberMe')}</span>
                            </label>
                            <Link
                                to="/auth/forgot-password"
                                className="text-sm font-medium text-[#00b14f] hover:text-[#008f3c]"
                            >
                                {t('login.forgotPassword')}
                            </Link>
                        </div>

                        <Button type="submit" className="w-full" size="lg" isLoading={isLoading}>
                            {t('login.submit')}
                            <ArrowRight className="h-4 w-4" />
                        </Button>
                    </form>

                    <div className="mt-8">
                        <div className="relative">
                            <div className="absolute inset-0 flex items-center">
                                <div className="w-full border-t border-gray-200" />
                            </div>
                            <div className="relative flex justify-center text-sm">
                                <span className="bg-white px-4 text-gray-500">{t('login.orLoginWith')}</span>
                            </div>
                        </div>

                        <div className="mt-6 flex justify-center">
                            <button 
                                type="button"
                                onClick={handleGoogleLogin}
                                className="flex items-center justify-center gap-2 rounded-lg border border-gray-200 bg-white px-6 py-2.5 text-sm font-medium text-gray-700 transition hover:bg-gray-50"
                            >
                                <svg className="h-5 w-5" viewBox="0 0 24 24"><path fill="#4285F4" d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z" /><path fill="#34A853" d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z" /><path fill="#FBBC05" d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z" /><path fill="#EA4335" d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z" /></svg>
                                Google
                            </button>
                        </div>
                    </div>

                    <p className="mt-8 text-center text-sm text-gray-600">
                        {t('login.noAccount')}{' '}
                        <Link to="/auth/register" className="font-semibold text-[#00b14f] hover:text-[#008f3c]">
                            {t('login.registerNow')}
                        </Link>
                    </p>
                </div>
            </div>

            {/* Right Side - Image/Promo */}
            <div className="hidden bg-gradient-to-br from-[#00b14f] via-[#00a045] to-[#008f3c] lg:flex lg:w-1/2 lg:items-center lg:justify-center lg:p-12">
                <div className="max-w-lg text-center text-white">
                    <h2 className="text-3xl font-bold">{t('login.promoTitle')}</h2>
                    <p className="mt-4 text-lg text-white/90">
                        {t('login.promoDescription')}
                    </p>
                    <div className="mt-8 grid grid-cols-2 gap-4 text-left">
                        <div className="rounded-xl bg-white/10 p-4 backdrop-blur-sm">
                            <div className="text-2xl font-bold">50,000+</div>
                            <div className="text-sm text-white/80">{t('login.statJobs')}</div>
                        </div>
                        <div className="rounded-xl bg-white/10 p-4 backdrop-blur-sm">
                            <div className="text-2xl font-bold">10,000+</div>
                            <div className="text-sm text-white/80">{t('login.statCompanies')}</div>
                        </div>
                        <div className="rounded-xl bg-white/10 p-4 backdrop-blur-sm">
                            <div className="text-2xl font-bold">100,000+</div>
                            <div className="text-sm text-white/80">{t('login.statCandidates')}</div>
                        </div>
                        <div className="rounded-xl bg-white/10 p-4 backdrop-blur-sm">
                            <div className="text-2xl font-bold">95%</div>
                            <div className="text-sm text-white/80">{t('login.statSatisfaction')}</div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}
