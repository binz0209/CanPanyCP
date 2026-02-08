import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Mail, Briefcase, ArrowLeft, CheckCircle, KeyRound, ShieldCheck } from 'lucide-react';
import { Button, Input } from '../../components/ui';
import { authApi } from '../../api';
import toast from 'react-hot-toast';

const forgotPasswordSchema = z.object({
    email: z.string().email('Email không hợp lệ'),
});

type ForgotPasswordForm = z.infer<typeof forgotPasswordSchema>;

export function ForgotPasswordPage() {
    const [isLoading, setIsLoading] = useState(false);
    const [isSuccess, setIsSuccess] = useState(false);
    const [submittedEmail, setSubmittedEmail] = useState('');

    const {
        register,
        handleSubmit,
        formState: { errors },
    } = useForm<ForgotPasswordForm>({
        resolver: zodResolver(forgotPasswordSchema),
    });

    const onSubmit = async (data: ForgotPasswordForm) => {
        setIsLoading(true);
        try {
            await authApi.forgotPassword(data);
            setSubmittedEmail(data.email);
            setIsSuccess(true);
            toast.success('Đã gửi hướng dẫn đặt lại mật khẩu!');
        } catch (error: any) {
            toast.error(error.response?.data?.message || 'Có lỗi xảy ra');
        } finally {
            setIsLoading(false);
        }
    };

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
                                        <ShieldCheck className="h-6 w-6 text-[#00b14f]" />
                                    </div>
                                </div>
                                <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Quên mật khẩu?</h1>
                                <p className="mt-2 text-sm text-gray-500 dark:text-gray-400">
                                    Nhập email đăng ký để nhận hướng dẫn đặt lại mật khẩu
                                </p>
                            </div>

                            {/* Form */}
                            <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
                                <Input
                                    label="Email đăng ký"
                                    type="email"
                                    placeholder="you@example.com"
                                    icon={<Mail className="h-5 w-5" />}
                                    error={errors.email?.message}
                                    {...register('email')}
                                />

                                <Button type="submit" className="w-full" size="lg" isLoading={isLoading}>
                                    Gửi hướng dẫn
                                </Button>
                            </form>

                            {/* Back Link */}
                            <div className="mt-6 text-center">
                                <Link
                                    to="/auth/login"
                                    className="inline-flex items-center gap-2 text-sm font-medium text-gray-500 transition-colors hover:text-[#00b14f] dark:text-gray-400"
                                >
                                    <ArrowLeft className="h-4 w-4" />
                                    Quay lại đăng nhập
                                </Link>
                            </div>
                        </>
                    ) : (
                        /* Success State */
                        <div className="text-center">
                            <div className="mx-auto mb-6 flex h-20 w-20 items-center justify-center rounded-full bg-[#00b14f]/10">
                                <CheckCircle className="h-10 w-10 text-[#00b14f]" />
                            </div>
                            <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Kiểm tra email!</h1>
                            <p className="mt-3 text-sm text-gray-500 dark:text-gray-400">
                                Hướng dẫn đặt lại mật khẩu đã được gửi đến
                            </p>
                            <p className="mt-1 font-semibold text-[#00b14f]">{submittedEmail}</p>

                            <div className="mt-6 rounded-xl bg-amber-50 p-4 text-left dark:bg-amber-900/20">
                                <p className="text-sm text-amber-800 dark:text-amber-200">
                                    💡 Không thấy email? Kiểm tra thư mục <strong>Spam</strong> hoặc <strong>Junk</strong>.
                                </p>
                            </div>

                            <div className="mt-8 space-y-3">
                                <Link to={`/auth/reset-password?email=${encodeURIComponent(submittedEmail)}`} className="block">
                                    <Button className="w-full" size="lg">
                                        <KeyRound className="h-4 w-4" />
                                        Đặt lại mật khẩu
                                    </Button>
                                </Link>
                                <Button
                                    variant="outline"
                                    className="w-full"
                                    onClick={() => setIsSuccess(false)}
                                >
                                    Thử email khác
                                </Button>
                                <Link to="/auth/login" className="block">
                                    <Button variant="ghost" className="w-full text-gray-500">
                                        <ArrowLeft className="h-4 w-4" />
                                        Quay lại đăng nhập
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
