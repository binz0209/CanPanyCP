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

const verificationSchema = z.object({
    documentUrlsText: z.string().trim().min(1, 'Vui lòng nhập ít nhất một document URL'),
});

type VerificationFormValues = z.infer<typeof verificationSchema>;

export function CompanyVerificationPage() {
    const queryClient = useQueryClient();
    const { companyId, isLoading: isWorkspaceLoading, isMissingProfile, hasFatalError } = useCompanyWorkspace();

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
            toast.success('Gửi yêu cầu xác minh thành công');
        },
        onError: (error) => {
            const message = isAxiosError(error)
                ? error.response?.data?.message || 'Không thể gửi yêu cầu xác minh'
                : 'Không thể gửi yêu cầu xác minh';
            toast.error(message);
        },
    });

    if (isWorkspaceLoading || (companyId && verificationQuery.isLoading)) {
        return <CompanyWorkspaceLoader />;
    }

    if (isMissingProfile) {
        return (
            <CompanyProfileRequiredState
                title="Bạn chưa có hồ sơ công ty"
                description="Hãy hoàn thiện hồ sơ công ty trước khi gửi yêu cầu xác minh."
                icon={<ShieldCheck className="h-6 w-6" />}
            />
        );
    }

    if (hasFatalError || verificationQuery.error) {
        return (
            <CompanyWorkspaceErrorState
                title="Không thể tải thông tin xác minh"
                description="Đã xảy ra lỗi khi tải thông tin xác minh. Vui lòng thử lại sau hoặc liên hệ quản trị viên nếu cần thêm hỗ trợ."
                icon={<ShieldCheck className="h-6 w-6" />}
            />
        );
    }

    const verification = verificationQuery.data;
    const isApproved = verification?.isVerified;

    return (
        <div className="space-y-6">
            <SectionHeader
                title="Xác minh doanh nghiệp"
                description="Gửi tài liệu pháp lý (giấy phép kinh doanh, mã số thuế, v.v.) để đội ngũ CanPany xác minh và gắn nhãn doanh nghiệp uy tín trên hệ thống."
            />

            <div className="grid gap-6 xl:grid-cols-[0.95fr_1.05fr]">
                <Card className="p-6">
                    <h2 className="text-lg font-semibold text-gray-900">Trạng thái xác minh</h2>
                    <div className="mt-5 space-y-4">
                        <div className="rounded-xl bg-gray-50 p-4">
                            <p className="text-sm text-gray-500">Trạng thái hiện tại</p>
                            <div className="mt-2">
                                <StatusBadge status={verification?.verificationStatus || 'Pending'} kind="verification" />
                            </div>
                        </div>

                        <div className="rounded-xl bg-gray-50 p-4">
                            <p className="text-sm text-gray-500">Đã xác minh</p>
                            <p className="mt-2 text-sm font-semibold text-gray-900">
                                {verification?.isVerified ? 'Có' : 'Chưa'}
                            </p>
                        </div>

                        <div className="rounded-xl bg-gray-50 p-4">
                            <p className="text-sm text-gray-500">Thời điểm duyệt</p>
                            <p className="mt-2 text-sm font-semibold text-gray-900">
                                {verification?.verifiedAt ? formatDateTime(verification.verifiedAt) : 'Chưa có'}
                            </p>
                        </div>

                        <div className="rounded-xl border border-dashed border-gray-300 p-4 text-sm text-gray-600">
                            {isApproved
                                ? 'Công ty đã được xác minh. Bạn vẫn có thể gửi lại hồ sơ nếu muốn cập nhật tài liệu.'
                                : 'Nếu công ty đang Pending hoặc Rejected, nên rà soát lại URL tài liệu để tránh admin phải yêu cầu bổ sung.'}
                        </div>
                    </div>
                </Card>

                <Card className="p-6">
                    <div className="flex items-start gap-3">
                        <div className="rounded-lg bg-[#00b14f]/10 p-2 text-[#00b14f]">
                            <ShieldCheck className="h-5 w-5" />
                        </div>
                        <div>
                            <h2 className="text-lg font-semibold text-gray-900">Gửi hồ sơ xác minh</h2>
                            <p className="mt-1 text-sm text-gray-500">
                                Mỗi dòng là một đường dẫn tới tài liệu (PDF, hình ảnh...) được lưu trữ trên hệ thống của bạn.
                            </p>
                        </div>
                    </div>

                    <form onSubmit={handleSubmit((values) => requestMutation.mutate(values))} className="mt-6 space-y-5">
                        <div>
                            <label className="mb-2 block text-sm font-medium text-gray-700">
                                Danh sách tài liệu
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
                                Gửi yêu cầu xác minh
                            </Button>
                            <Button
                                type="button"
                                variant="outline"
                                onClick={() => reset({ documentUrlsText: verification?.verificationDocuments?.join('\n') || '' })}
                                disabled={requestMutation.isPending}
                            >
                                Khôi phục dữ liệu hiện tại
                            </Button>
                        </div>
                    </form>

                    <div className="mt-6 rounded-xl bg-gray-50 p-4">
                        <div className="flex items-center gap-2">
                            <FileText className="h-4 w-4 text-gray-500" />
                            <p className="text-sm font-semibold text-gray-900">Documents hiện có</p>
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
                            <p className="mt-4 text-sm text-gray-500">Chưa có tài liệu nào được gửi.</p>
                        )}

                        {isApproved && (
                            <div className="mt-4 flex items-start gap-2 rounded-lg bg-green-50 p-3 text-sm text-green-700">
                                <CheckCircle2 className="mt-0.5 h-4 w-4 shrink-0" />
                                <span>Company đã ở trạng thái verified.</span>
                            </div>
                        )}
                    </div>
                </Card>
            </div>
        </div>
    );
}
