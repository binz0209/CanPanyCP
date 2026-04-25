import { useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useAuthStore } from '../../stores/auth.store';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import type { User, UserRole } from '../../types';

export function AuthCallbackPage() {
    const [searchParams] = useSearchParams();
    const navigate = useNavigate();
    const setAuth = useAuthStore((state) => state.setAuth);
    const { t } = useTranslation('auth');

    useEffect(() => {
        const token = searchParams.get('token');
        const googleSso = searchParams.get('google_sso');
        const error = searchParams.get('error');

        if (error) {
            toast.error(t('login.failed'));
            navigate('/auth/login', { replace: true });
            return;
        }

        if (googleSso === 'true' && token) {
            try {
                // Decode JWT to get user info
                const payload = decodeToken(token);
                if (!payload) throw new Error('Invalid token');

                const user: User = {
                    id: payload.sub || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'],
                    fullName: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || '',
                    email: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] || '',
                    role: (payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] as UserRole) || 'Candidate',
                    isLocked: false,
                    createdAt: new Date(),
                };

                setAuth(user, token);
                toast.success(t('login.success'));

                // Redirect based on role
                const redirectPath = user.role === 'Candidate'
                    ? '/candidate/dashboard'
                    : user.role === 'Company'
                        ? '/company/dashboard'
                        : user.role === 'Admin'
                            ? '/admin/dashboard'
                            : '/';

                navigate(redirectPath, { replace: true });
            } catch (err) {
                console.error('Failed to decode token:', err);
                toast.error(t('login.failed'));
                navigate('/auth/login', { replace: true });
            }
        } else if (googleSso === 'false') {
            toast.error(t('login.failed'));
            navigate('/auth/login', { replace: true });
        }
    }, [searchParams, navigate, setAuth, t]);

    return (
        <div className="flex min-h-screen flex-col items-center justify-center bg-gray-50">
            <div className="text-center">
                <div className="mx-auto h-12 w-12 animate-spin rounded-full border-4 border-[#00b14f]/20 border-t-[#00b14f]" />
                <h2 className="mt-4 text-xl font-semibold text-gray-900">{t('login.processing' as any, { defaultValue: 'Đang xử lý đăng nhập...' })}</h2>
                <p className="mt-2 text-gray-600">{t('login.pleaseWait' as any, { defaultValue: 'Vui lòng chờ trong giây lát' })}</p>
            </div>
        </div>
    );
}

function decodeToken(token: string) {
    try {
        const base64Url = token.split('.')[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(
            atob(base64)
                .split('')
                .map((c) => {
                    return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
                })
                .join('')
        );
        return JSON.parse(jsonPayload);
    } catch (e) {
        return null;
    }
}
