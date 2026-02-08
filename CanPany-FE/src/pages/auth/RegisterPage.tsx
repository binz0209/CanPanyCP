import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Mail, Lock, User, Briefcase, Eye, EyeOff, Building2, UserCircle, ArrowRight, CheckCircle } from 'lucide-react';
import { Button, Input, Card, CardHeader, CardTitle, CardDescription, CardContent } from '../../components/ui';
import { authApi } from '../../api';
import { useAuthStore } from '../../stores/auth.store';
import { cn } from '../../utils';
import toast from 'react-hot-toast';

const registerSchema = z.object({
    fullName: z.string().min(2, 'Họ tên tối thiểu 2 ký tự'),
    email: z.string().email('Email không hợp lệ'),
    password: z.string().min(6, 'Mật khẩu tối thiểu 6 ký tự'),
    confirmPassword: z.string(),
    role: z.enum(['Candidate', 'Company']),
}).refine((data) => data.password === data.confirmPassword, {
    message: 'Mật khẩu không khớp',
    path: ['confirmPassword'],
});

type RegisterForm = z.infer<typeof registerSchema>;

const benefits = [
    'Tiếp cận 50,000+ việc làm mới mỗi ngày',
    'Nhận gợi ý việc làm phù hợp qua AI',
    'Tạo CV chuyên nghiệp miễn phí',
    'Theo dõi trạng thái ứng tuyển',
];

export function RegisterPage() {
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
                role: data.role,
            });
            setAuth(response.user, response.accessToken);
            toast.success('Đăng ký thành công!');

            const redirectPath = data.role === 'Candidate'
                ? '/candidate/dashboard'
                : '/company/dashboard';

            navigate(redirectPath, { replace: true });
        } catch (error: any) {
            toast.error(error.response?.data?.message || 'Đăng ký thất bại');
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="flex min-h-screen">
            {/* Left Side - Promo */}
            <div className="hidden bg-gradient-to-br from-[#00b14f] via-[#00a045] to-[#008f3c] lg:flex lg:w-1/2 lg:flex-col lg:justify-center lg:p-12">
                <div className="max-w-lg text-white">
                    <h2 className="text-3xl font-bold">Bắt đầu hành trình sự nghiệp của bạn</h2>
                    <p className="mt-4 text-lg text-white/90">
                        Đăng ký ngay để khám phá hàng nghìn cơ hội việc làm hấp dẫn
                    </p>

                    <div className="mt-8 space-y-4">
                        {benefits.map((benefit) => (
                            <div key={benefit} className="flex items-center gap-3">
                                <div className="flex h-6 w-6 items-center justify-center rounded-full bg-white/20">
                                    <CheckCircle className="h-4 w-4" />
                                </div>
                                <span className="text-white/90">{benefit}</span>
                            </div>
                        ))}
                    </div>

                    <div className="mt-12 rounded-2xl bg-white/10 p-6 backdrop-blur-sm">
                        <p className="text-lg font-medium">"CanPany đã giúp tôi tìm được công việc mơ ước chỉ trong 2 tuần!"</p>
                        <div className="mt-4 flex items-center gap-3">
                            <div className="h-10 w-10 rounded-full bg-white/20" />
                            <div>
                                <p className="font-medium">Nguyễn Văn A</p>
                                <p className="text-sm text-white/70">Frontend Developer tại FPT</p>
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
                        <h1 className="text-2xl font-bold text-gray-900">Tạo tài khoản mới</h1>
                        <p className="mt-2 text-gray-600">Điền thông tin để bắt đầu</p>
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
                                )}>Ứng viên</span>
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
                                )}>Nhà tuyển dụng</span>
                            </button>
                        </div>
                        <input type="hidden" {...register('role')} />
                    </div>

                    <form onSubmit={handleSubmit(onSubmit)} className="mt-6 space-y-4">
                        <Input
                            label="Họ và tên"
                            type="text"
                            placeholder="Nguyễn Văn A"
                            icon={<User className="h-5 w-5" />}
                            error={errors.fullName?.message}
                            {...register('fullName')}
                        />

                        <Input
                            label="Email"
                            type="email"
                            placeholder="you@example.com"
                            icon={<Mail className="h-5 w-5" />}
                            error={errors.email?.message}
                            {...register('email')}
                        />

                        <div className="relative">
                            <Input
                                label="Mật khẩu"
                                type={showPassword ? 'text' : 'password'}
                                placeholder="••••••••"
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
                            label="Xác nhận mật khẩu"
                            type={showPassword ? 'text' : 'password'}
                            placeholder="••••••••"
                            icon={<Lock className="h-5 w-5" />}
                            error={errors.confirmPassword?.message}
                            {...register('confirmPassword')}
                        />

                        <div className="pt-2">
                            <Button type="submit" className="w-full" size="lg" isLoading={isLoading}>
                                Đăng ký
                                <ArrowRight className="h-4 w-4" />
                            </Button>
                        </div>
                    </form>

                    <p className="mt-6 text-center text-sm text-gray-600">
                        Bằng việc đăng ký, bạn đồng ý với{' '}
                        <a href="#" className="font-medium text-[#00b14f] hover:underline">Điều khoản</a>
                        {' '}và{' '}
                        <a href="#" className="font-medium text-[#00b14f] hover:underline">Chính sách bảo mật</a>
                    </p>

                    <p className="mt-6 text-center text-sm text-gray-600">
                        Đã có tài khoản?{' '}
                        <Link to="/auth/login" className="font-semibold text-[#00b14f] hover:text-[#008f3c]">
                            Đăng nhập
                        </Link>
                    </p>
                </div>
            </div>
        </div>
    );
}
