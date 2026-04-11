import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Shield, ShieldCheck, ShieldOff, Loader2, Info, Clock, Globe, Brain, Github, Mail } from 'lucide-react';
import toast from 'react-hot-toast';
import { consentApi, type UserConsent } from '../../../api/consent.api';

// ─── consent type metadata ───────────────────────────────────────────────────
const CONSENT_TYPES = [
    {
        type: 'DataProcessing',
        icon: Shield,
        color: 'text-blue-600',
        bg: 'bg-blue-50',
        title: 'Xử lý dữ liệu cá nhân',
        titleEn: 'Personal Data Processing',
        description: 'Cho phép hệ thống xử lý dữ liệu cá nhân của bạn để cung cấp dịch vụ tuyển dụng, gợi ý việc làm và quản lý hồ sơ.',
        descriptionEn: 'Allow the system to process your personal data to provide recruitment services, job recommendations, and profile management.',
        required: true,
    },
    {
        type: 'AIAnalysis',
        icon: Brain,
        color: 'text-purple-600',
        bg: 'bg-purple-50',
        title: 'Phân tích AI',
        titleEn: 'AI Analysis',
        description: 'Cho phép sử dụng trí tuệ nhân tạo để phân tích CV, đánh giá kỹ năng và tạo gợi ý nghề nghiệp cá nhân hóa.',
        descriptionEn: 'Allow AI to analyze your CV, assess skills, and create personalized career suggestions.',
        required: false,
    },
    {
        type: 'ExternalSync_GitHub',
        icon: Github,
        color: 'text-gray-800',
        bg: 'bg-gray-100',
        title: 'Đồng bộ GitHub',
        titleEn: 'GitHub Sync',
        description: 'Cho phép hệ thống truy cập và phân tích dữ liệu GitHub của bạn để trích xuất kỹ năng kỹ thuật và cập nhật hồ sơ.',
        descriptionEn: 'Allow the system to access and analyze your GitHub data to extract technical skills and update your profile.',
        required: false,
    },
    {
        type: 'CrossBorderTransfer',
        icon: Globe,
        color: 'text-emerald-600',
        bg: 'bg-emerald-50',
        title: 'Chuyển dữ liệu xuyên biên giới',
        titleEn: 'Cross-Border Data Transfer',
        description: 'Cho phép chuyển dữ liệu cá nhân sang máy chủ nước ngoài để xử lý AI và lưu trữ (tuân thủ Nghị định 13/2023/NĐ-CP).',
        descriptionEn: 'Allow personal data transfer to overseas servers for AI processing and storage (compliant with Decree 13/2023/ND-CP).',
        required: false,
    },
    {
        type: 'Marketing',
        icon: Mail,
        color: 'text-amber-600',
        bg: 'bg-amber-50',
        title: 'Thông báo & tiếp thị',
        titleEn: 'Notifications & Marketing',
        description: 'Nhận email thông báo việc làm mới, khuyến mãi và tin tức từ CanPany.',
        descriptionEn: 'Receive email notifications about new jobs, promotions, and news from CanPany.',
        required: false,
    },
];

// ─── page ────────────────────────────────────────────────────────────────────
export function PrivacyConsentPage() {
    const queryClient = useQueryClient();

    const { data: consents = [], isLoading } = useQuery({
        queryKey: ['user-consents'],
        queryFn: consentApi.getConsents,
    });

    const grantMutation = useMutation({
        mutationFn: (type: string) => consentApi.grantConsent(type, '1.0'),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['user-consents'] });
            toast.success('Đã cấp quyền');
        },
        onError: () => toast.error('Lỗi khi cấp quyền'),
    });

    const revokeMutation = useMutation({
        mutationFn: (type: string) => consentApi.revokeConsent(type),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['user-consents'] });
            toast.success('Đã thu hồi quyền');
        },
        onError: () => toast.error('Lỗi khi thu hồi quyền'),
    });

    const findConsent = (type: string): UserConsent | undefined =>
        consents.find((c) => c.consentType === type);

    const isGranted = (type: string) => {
        const c = findConsent(type);
        return c?.isGranted ?? false;
    };

    const handleToggle = (type: string) => {
        if (isGranted(type)) {
            revokeMutation.mutate(type);
        } else {
            grantMutation.mutate(type);
        }
    };

    const isMutating = grantMutation.isPending || revokeMutation.isPending;
    const grantedCount = consents.filter((c) => c.isGranted).length;

    return (
        <div className="space-y-6">
            {/* Header */}
            <div className="rounded-2xl bg-gradient-to-br from-blue-600 via-indigo-600 to-purple-700 p-6 text-white shadow-xl">
                <div className="flex items-center gap-3 mb-2">
                    <div className="rounded-xl bg-white/15 p-2.5">
                        <Shield className="h-6 w-6" />
                    </div>
                    <div>
                        <h1 className="text-2xl font-bold">Quyền riêng tư & Đồng ý</h1>
                        <p className="text-sm text-white/70">Privacy & Consent Management</p>
                    </div>
                </div>
                <p className="mt-3 text-sm text-white/80 max-w-2xl">
                    Quản lý quyền đồng ý xử lý dữ liệu cá nhân của bạn theo quy định của Nghị định 13/2023/NĐ-CP
                    về bảo vệ dữ liệu cá nhân. Bạn có thể bật/tắt bất kỳ quyền nào dưới đây.
                </p>

                {/* Stats */}
                <div className="mt-4 flex items-center gap-6 text-sm">
                    <div className="flex items-center gap-2">
                        <ShieldCheck className="h-4 w-4 text-emerald-300" />
                        <span>{grantedCount} quyền đã cấp</span>
                    </div>
                    <div className="flex items-center gap-2">
                        <ShieldOff className="h-4 w-4 text-white/50" />
                        <span>{CONSENT_TYPES.length - grantedCount} chưa cấp</span>
                    </div>
                </div>
            </div>

            {/* Legal Notice */}
            <div className="flex items-start gap-3 rounded-xl border border-blue-200 bg-blue-50/50 p-4">
                <Info className="h-5 w-5 text-blue-600 mt-0.5 shrink-0" />
                <div className="text-sm text-blue-800">
                    <p className="font-medium mb-1">Nghị định 13/2023/NĐ-CP — Bảo vệ dữ liệu cá nhân</p>
                    <p className="text-blue-700">
                        Bạn có quyền đồng ý hoặc từ chối xử lý dữ liệu cá nhân bất kỳ lúc nào.
                        Việc thu hồi đồng ý có thể ảnh hưởng đến một số tính năng của hệ thống.
                    </p>
                </div>
            </div>

            {/* Loading */}
            {isLoading ? (
                <div className="flex items-center justify-center py-16">
                    <Loader2 className="h-8 w-8 animate-spin text-indigo-500" />
                </div>
            ) : (
                /* Consent Cards */
                <div className="space-y-4">
                    {CONSENT_TYPES.map((ct) => {
                        const Icon = ct.icon;
                        const granted = isGranted(ct.type);
                        const consent = findConsent(ct.type);
                        return (
                            <div
                                key={ct.type}
                                className={`rounded-2xl border bg-white p-5 shadow-sm transition-all ${
                                    granted ? 'border-emerald-200 ring-1 ring-emerald-100' : 'border-gray-200'
                                }`}
                            >
                                <div className="flex items-start gap-4">
                                    {/* Icon */}
                                    <div className={`rounded-xl ${ct.bg} p-3 shrink-0`}>
                                        <Icon className={`h-5 w-5 ${ct.color}`} />
                                    </div>

                                    {/* Content */}
                                    <div className="flex-1 min-w-0">
                                        <div className="flex items-center gap-2 mb-1">
                                            <h3 className="text-base font-semibold text-gray-900">{ct.title}</h3>
                                            {ct.required && (
                                                <span className="text-[10px] font-medium px-1.5 py-0.5 rounded bg-red-100 text-red-600 uppercase">
                                                    Bắt buộc
                                                </span>
                                            )}
                                        </div>
                                        <p className="text-sm text-gray-500 leading-relaxed">{ct.description}</p>

                                        {/* Meta info */}
                                        {consent && granted && (
                                            <div className="mt-2 flex flex-wrap gap-3 text-xs text-gray-400">
                                                {consent.grantedAt && (
                                                    <span className="flex items-center gap-1">
                                                        <Clock className="h-3 w-3" />
                                                        Đã cấp: {new Date(consent.grantedAt).toLocaleDateString('vi-VN')}
                                                    </span>
                                                )}
                                                {consent.policyVersion && (
                                                    <span>Phiên bản: v{consent.policyVersion}</span>
                                                )}
                                            </div>
                                        )}
                                    </div>

                                    {/* Toggle */}
                                    <button
                                        onClick={() => handleToggle(ct.type)}
                                        disabled={isMutating}
                                        className={`relative inline-flex h-7 w-12 items-center rounded-full transition-colors duration-200 shrink-0 ${
                                            granted ? 'bg-emerald-500' : 'bg-gray-300'
                                        } ${isMutating ? 'opacity-50 cursor-not-allowed' : 'cursor-pointer'}`}
                                    >
                                        <span
                                            className={`inline-block h-5 w-5 transform rounded-full bg-white shadow-sm transition-transform duration-200 ${
                                                granted ? 'translate-x-6' : 'translate-x-1'
                                            }`}
                                        />
                                    </button>
                                </div>
                            </div>
                        );
                    })}
                </div>
            )}
        </div>
    );
}
