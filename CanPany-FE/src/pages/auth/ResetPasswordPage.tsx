import { useState, useEffect } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Mail, Lock, Briefcase, Eye, EyeOff, ArrowLeft, CheckCircle, KeyRound, ShieldCheck } from 'lucide-react';
import { Button, Input } from '../../components/ui';
import { authApi } from '../../api';
import toast from 'react-hot-toast';

const resetPasswordSchema = z.object({
    code: z.string().min(6, 'Mã xác nhận phải có 6 chữ số'),
    newPassword: z.string().min(6, 'Mật khẩu tối thiểu 6 ký tự'),
    confirmPassword: z.string(),
}).refine((data) => data.newPassword === data.confirmPassword, {
    message: 'Mật khẩu không khớp',
    path: ['confirmPassword'],
});

type ResetPasswordForm = z.infer<typeof resetPasswordSchema>;

export function ResetPasswordPage() {
    const [showPassword, setShowPassword] = useState(false);
    const [isLoading, setIsLoading] = useState(false);
    const [isSuccess, setIsSuccess] = useState(false);
    const navigate = useNavigate();
    const [searchParams] = useSearchParams();

    const email = searchParams.get('email') || '';

    const {
        register,
        handleSubmit,
        formState: { errors },
    } = useForm<ResetPasswordForm>({
        resolver: zodResolver(resetPasswordSchema),
    });

    useEffect(() => {
        if (!email) {
            toast.error('Email không hợp lệ');
            navigate('/auth/forgot-password');
        }
    }, [email, navigate]);

    const onSubmit = async (data: ResetPasswordForm) => {
        setIsLoading(true);
        try {
            await authApi.resetPassword({
                email,
                code: data.code,
                newPassword: data.newPassword,
            });
            setIsSuccess(true);
            toast.success('Đặt lại mật khẩu thành công!');

            // Redirect to login after 3 seconds
            setTimeout(() => {
                navigate('/auth/login');
            }, 3000);
        } catch (error: any) {
            toast.error(error.response?.data?.message || 'Có lỗi xảy ra');
        } finally {
            setIsLoading(false);
        }
    };

    if (!email) {
        return null; // Will redirect in useEffect
    }

    return (
        <div className="flex min-h-screen items-center justify-center bg-gradient-to-br from-gray-50 to-gray-100 px-4 py-12 dark:from-gray-900 dark:to-gray-800">
            <div className="w-full max-w-md">
                {/* Card Container */}
                <div className="rounded-2xl border border-gray-200 bg-white p-8 shadow-xl dark:border-gray-700 dark:bg-gray-800">
                    {/* Logo - Centered */}
                    <div className="mb-8 text-center">
                        <Link to="/" className="inline-flex items-center gap-2">
                            <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-[#00b14f]">
                                <Briefcase className="h-5 w-5 text-white" />
                            </div>
                            <span className="text-2xl font-bold">
                                <span className="text-gray-900 dark:text-white">Can</span>
                                <span className="text-[#00b14f]">Pany</span>
                            </span>
                        </Link>
                    </div>

                    {!isSuccess ? (
                        <>
                            {/* Icon & Header - Centered */}
                            <div className="mb-8 text-center">
                                <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-gradient-to-br from-[#00b14f]/20 to-[#00b14f]/5">
                                    <div className="flex h-12 w-12 items-center justify-center rounded-full bg-[#00b14f]/10">
                                        <KeyRound className="h-6 w-6 text-[#00b14f]" />
                                    </div>
                                </div>
                                <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Đặt lại mật khẩu</h1>
                                <p className="mt-2 text-sm text-gray-500 dark:text-gray-400">
                                    Nhập mã xác nhận và mật khẩu mới cho
                                </p>
                                <p className="mt-1 font-semibold text-[#00b14f]">{email}</p>
                            </div>

                            {/* Form */}
                            <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
                                <Input
                                    label="Mã xác nhận (6 chữ số)"
                                    type="text"
                                    placeholder="123456"
                                    icon={<ShieldCheck className="h-5 w-5" />}
                                    error={errors.code?.message}
                                    {...register('code')}
                                    maxLength={6}
                                />

                                <div className="relative">
                                    <Input
                                        label="Mật khẩu mới"
                                        type={showPassword ? 'text' : 'password'}
                                        placeholder="••••••••"
                                        icon={<Lock className="h-5 w-5" />}
                                        error={errors.newPassword?.message}
                                        {...register('newPassword')}
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
                                    label="Xác nhận mật khẩu mới"
                                    type={showPassword ? 'text' : 'password'}
                                    placeholder="••••••••"
                                    icon={<Lock className="h-5 w-5" />}
                                    error={errors.confirmPassword?.message}
                                    {...register('confirmPassword')}
                                />

                                <Button type="submit" className="w-full" size="lg" isLoading={isLoading}>
                                    Đặt lại mật khẩu
                                </Button>
                            </form>

                            {/* Back Link */}
                            <div className="mt-6 text-center">
                                <Link
                                    to="/auth/forgot-password"
                                    className="inline-flex items-center gap-2 text-sm font-medium text-gray-500 transition-colors hover:text-[#00b14f] dark:text-gray-400"
                                >
                                    <ArrowLeft className="h-4 w-4" />
                                    Quay lại quên mật khẩu
                                </Link>
                            </div>
                        </>
                    ) : (
                        /* Success State */
                        <div className="text-center">
                            <div className="mx-auto mb-6 flex h-20 w-20 items-center justify-center rounded-full bg-[#00b14f]/10">
                                <CheckCircle className="h-10 w-10 text-[#00b14f]" />
                            </div>
                            <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Thành công!</h1>
                            <p className="mt-3 text-sm text-gray-500 dark:text-gray-400">
                                Mật khẩu của bạn đã được đặt lại thành công.
                            </p>
                            <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">
                                Bạn sẽ được chuyển hướng đến trang đăng nhập trong 3 giây...
                            </p>

                            <div className="mt-8">
                                <Link to="/auth/login" className="block">
                                    <Button variant="outline" className="w-full">
                                        <ArrowLeft className="h-4 w-4" />
                                        Đăng nhập ngay
                                    </Button>
                                </Link>
                            </div>
                        </div>
                    )}
                </div>

                {/* Footer Info */}
                <p className="mt-6 text-center text-xs text-gray-400 dark:text-gray-500">
                    Bảo mật bởi mã hóa AES-256 · <a href="#" className="hover:text-[#00b14f]">Điều khoản</a> · <a href="#" className="hover:text-[#00b14f]">Bảo mật</a>
                </p>
            </div>
        </div>
    );
}
