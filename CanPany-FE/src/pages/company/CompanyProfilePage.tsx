import { useEffect, useMemo } from 'react';
import { isAxiosError } from 'axios';
import { useForm, useWatch } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { z } from 'zod';
import { Building2, Globe, Image as ImageIcon, MapPin, Phone } from 'lucide-react';
import toast from 'react-hot-toast';
import { Button, Card, Input } from '../../components/ui';
import { companiesApi } from '../../api';
import type { Company } from '../../types';
import {
    CompanyProfilePreviewCard,
    CompanyWorkspaceErrorState,
    CompanyWorkspaceLoader,
    SectionHeader,
} from '../../components/features/companies';
import { useCompanyWorkspace } from '../../hooks/company/useCompanyWorkspace';
import { companyKeys } from '../../lib/queryKeys';

const companyProfileSchema = z.object({
    name: z.string().trim().min(2, 'Tên công ty tối thiểu 2 ký tự'),
    logoUrl: z
        .string()
        .trim()
        .optional()
        .or(z.literal(''))
        .refine((value) => !value || /^https?:\/\//.test(value), 'Logo URL phải là đường dẫn hợp lệ'),
    website: z
        .string()
        .trim()
        .optional()
        .or(z.literal(''))
        .refine((value) => !value || /^https?:\/\//.test(value), 'Website phải là đường dẫn hợp lệ'),
    phone: z.string().trim().max(20, 'Số điện thoại quá dài').optional(),
    address: z.string().trim().max(255, 'Địa chỉ quá dài').optional(),
    description: z.string().trim().max(2000, 'Mô tả tối đa 2000 ký tự').optional(),
});

type CompanyProfileFormValues = z.infer<typeof companyProfileSchema>;

function getDefaultValues(company?: Company): CompanyProfileFormValues {
    return {
        name: company?.name || '',
        logoUrl: company?.logoUrl || '',
        website: company?.website || '',
        phone: company?.phone || '',
        address: company?.address || '',
        description: company?.description || '',
    };
}

export function CompanyProfilePage() {
    const queryClient = useQueryClient();
    const { company, isLoading, isMissingProfile, hasFatalError } = useCompanyWorkspace();

    const {
        register,
        handleSubmit,
        reset,
        control,
        formState: { errors, isDirty },
    } = useForm<CompanyProfileFormValues>({
        resolver: zodResolver(companyProfileSchema),
        defaultValues: getDefaultValues(),
    });

    useEffect(() => {
        if (company) {
            reset(getDefaultValues(company));
        }
    }, [company, reset]);

    const [logoPreview, previewName, previewAddress, previewDescription] = useWatch({
        control,
        name: ['logoUrl', 'name', 'address', 'description'],
    });

    const profileMutation = useMutation({
        mutationFn: async (values: CompanyProfileFormValues) => {
            const payload = {
                name: values.name.trim(),
                logoUrl: values.logoUrl?.trim() || undefined,
                website: values.website?.trim() || undefined,
                phone: values.phone?.trim() || undefined,
                address: values.address?.trim() || undefined,
                description: values.description?.trim() || undefined,
            };

            // Company account có thể đã đăng ký nhưng chưa có bản ghi company.
            // FE cần xử lý cả 2 nhánh create và update để luồng profile không bị gãy.
            if (company) {
                await companiesApi.updateMe(payload);
                return;
            }

            await companiesApi.create({
                name: payload.name,
                logoUrl: payload.logoUrl,
                website: payload.website,
                phone: payload.phone,
                address: payload.address,
                description: payload.description,
            });
        },
        onSuccess: async () => {
            await queryClient.invalidateQueries({ queryKey: companyKeys.me() });
            toast.success(company ? 'Cập nhật hồ sơ công ty thành công' : 'Tạo hồ sơ công ty thành công');
        },
        onError: (error) => {
            const message = isAxiosError(error)
                ? error.response?.data?.message || 'Không thể lưu hồ sơ công ty'
                : 'Không thể lưu hồ sơ công ty';
            toast.error(message);
        },
    });

    const verificationSummary = useMemo(() => {
        if (!company) {
            return 'Bạn chưa tạo hồ sơ công ty.';
        }

        if (company.isVerified) {
            return 'Công ty đã được xác minh.';
        }

        return `Trạng thái hiện tại: ${company.verificationStatus}`;
    }, [company]);

    if (isLoading) {
        return <CompanyWorkspaceLoader />;
    }

    if (hasFatalError) {
        return (
            <CompanyWorkspaceErrorState
                title="Không thể tải hồ sơ công ty"
                description="Đã xảy ra lỗi khi tải dữ liệu. Vui lòng thử lại sau hoặc liên hệ quản trị viên nếu vấn đề lặp lại."
                icon={<Building2 className="h-6 w-6" />}
            />
        );
    }

    return (
        <div className="space-y-6">
            <SectionHeader
                title="Hồ sơ công ty"
                description="Quản lý thông tin giới thiệu doanh nghiệp (logo, website, địa chỉ, mô tả) để ứng viên hiểu rõ hơn về thương hiệu và môi trường làm việc."
            />

            <div className="grid gap-6 xl:grid-cols-[1.2fr_0.8fr]">
                <Card className="p-6">
                    <div className="mb-6 flex items-start justify-between gap-4">
                        <div>
                            <h2 className="text-xl font-semibold text-gray-900">
                                {company ? 'Cập nhật hồ sơ công ty' : 'Tạo hồ sơ công ty'}
                            </h2>
                            <p className="mt-1 text-sm text-gray-500">
                                Điền đầy đủ và chính xác thông tin giúp tin tuyển dụng chuyên nghiệp hơn và tăng độ tin cậy với ứng viên.
                            </p>
                        </div>
                        {isMissingProfile && (
                            <span className="rounded-full bg-yellow-100 px-3 py-1 text-xs font-semibold text-yellow-700">
                                Chưa có hồ sơ
                            </span>
                        )}
                    </div>

                    <form onSubmit={handleSubmit((values) => profileMutation.mutate(values))} className="space-y-5">
                        <Input
                            label="Tên công ty"
                            placeholder="Ví dụ: CanPany Technology"
                            icon={<Building2 className="h-4 w-4" />}
                            error={errors.name?.message}
                            {...register('name')}
                        />

                        <div className="grid gap-5 md:grid-cols-2">
                            <Input
                                label="Logo URL"
                                placeholder="https://..."
                                icon={<ImageIcon className="h-4 w-4" />}
                                error={errors.logoUrl?.message}
                                {...register('logoUrl')}
                            />
                            <Input
                                label="Website"
                                placeholder="https://company.com"
                                icon={<Globe className="h-4 w-4" />}
                                error={errors.website?.message}
                                {...register('website')}
                            />
                        </div>

                        <div className="grid gap-5 md:grid-cols-2">
                            <Input
                                label="Số điện thoại"
                                placeholder="0123456789"
                                icon={<Phone className="h-4 w-4" />}
                                error={errors.phone?.message}
                                {...register('phone')}
                            />
                            <Input
                                label="Địa chỉ"
                                placeholder="Hồ Chí Minh"
                                icon={<MapPin className="h-4 w-4" />}
                                error={errors.address?.message}
                                {...register('address')}
                            />
                        </div>

                        <div>
                            <label className="mb-2 block text-sm font-medium text-gray-700">Mô tả công ty</label>
                            <textarea
                                rows={8}
                                className="w-full rounded-lg border border-gray-300 px-4 py-3 text-sm text-gray-900 outline-none transition focus:border-[#00b14f] focus:ring-2 focus:ring-[#00b14f]/20"
                                placeholder="Giới thiệu ngắn gọn về công ty, lĩnh vực hoạt động, quy mô, văn hoá làm việc..."
                                {...register('description')}
                            />
                            {errors.description?.message && (
                                <p className="mt-1.5 text-sm text-red-600">{errors.description.message}</p>
                            )}
                        </div>

                        <div className="flex flex-wrap gap-3 border-t border-gray-100 pt-4">
                            <Button
                                type="submit"
                                isLoading={profileMutation.isPending}
                                disabled={!isDirty && !!company}
                            >
                                {company ? 'Lưu thay đổi' : 'Tạo hồ sơ công ty'}
                            </Button>
                            <Button
                                type="button"
                                variant="outline"
                                onClick={() => reset(getDefaultValues(company))}
                                disabled={profileMutation.isPending}
                            >
                                Đặt lại form
                            </Button>
                        </div>
                    </form>
                </Card>

                <div className="space-y-6">
                    <CompanyProfilePreviewCard
                        logoUrl={logoPreview}
                        name={previewName}
                        address={previewAddress}
                        description={previewDescription}
                    />

                    <Card className="p-6">
                        <h2 className="text-lg font-semibold text-gray-900">Tình trạng hồ sơ</h2>
                        <div className="mt-4 space-y-3 text-sm text-gray-600">
                            <p>{verificationSummary}</p>
                            <p>
                                Hồ sơ công ty đầy đủ là điều kiện tiên quyết trước khi gửi yêu cầu xác minh và đăng tin tuyển dụng.
                            </p>
                        </div>
                    </Card>
                </div>
            </div>
        </div>
    );
}
