import type { FieldErrors, UseFormRegister } from 'react-hook-form';
import { BriefcaseBusiness, MapPin, Wallet } from 'lucide-react';
import { Input } from '../../ui';
import type { BudgetType, JobLevel } from '../../../types';

export interface CompanyJobFormValues {
    title: string;
    description: string;
    categoryId?: string;
    skillIdsText?: string;
    budgetType: BudgetType;
    budgetAmount?: string;
    level?: JobLevel;
    location?: string;
    isRemote: boolean;
    deadline?: string;
}

interface JobFormFieldsProps {
    register: UseFormRegister<CompanyJobFormValues>;
    errors: FieldErrors<CompanyJobFormValues>;
    isEditMode: boolean;
    budgetTypeOptions: BudgetType[];
    levelOptions: JobLevel[];
}

export function JobFormFields({
    register,
    errors,
    isEditMode,
    budgetTypeOptions,
    levelOptions,
}: JobFormFieldsProps) {
    return (
        <>
            <Input
                label="Tiêu đề job"
                placeholder="Senior Frontend Developer"
                icon={<BriefcaseBusiness className="h-4 w-4" />}
                error={errors.title?.message}
                {...register('title')}
            />

            <div>
                <label className="mb-2 block text-sm font-medium text-gray-700">Mô tả công việc</label>
                <textarea
                    rows={10}
                    className="w-full rounded-lg border border-gray-300 px-4 py-3 text-sm text-gray-900 outline-none transition focus:border-[#00b14f] focus:ring-2 focus:ring-[#00b14f]/20"
                    placeholder="Mô tả trách nhiệm, yêu cầu, quyền lợi..."
                    {...register('description')}
                />
                {errors.description?.message && (
                    <p className="mt-1.5 text-sm text-red-600">{errors.description.message}</p>
                )}
            </div>

            {!isEditMode && (
                <Input
                    label="Category ID"
                    placeholder="Nhập mã danh mục (Category ID)"
                    error={errors.categoryId?.message}
                    {...register('categoryId')}
                />
            )}

            <Input
                label="Skills"
                placeholder="React, TypeScript, Tailwind CSS"
                error={errors.skillIdsText?.message}
                {...register('skillIdsText')}
            />

            <div className="grid gap-5 md:grid-cols-2">
                <div>
                    <label className="mb-2 block text-sm font-medium text-gray-700">Loại ngân sách</label>
                    <select
                        className="h-11 w-full rounded-lg border border-gray-300 bg-white px-4 text-sm text-gray-900 outline-none transition focus:border-[#00b14f] focus:ring-2 focus:ring-[#00b14f]/20 disabled:bg-gray-50"
                        disabled={isEditMode}
                        {...register('budgetType')}
                    >
                        {budgetTypeOptions.map((option) => (
                            <option key={option} value={option}>{option}</option>
                        ))}
                    </select>
                </div>

                <Input
                    label="Mức lương / Ngân sách"
                    placeholder="30000000"
                    icon={<Wallet className="h-4 w-4" />}
                    error={errors.budgetAmount?.message}
                    {...register('budgetAmount')}
                />
            </div>

            <div className="grid gap-5 md:grid-cols-2">
                <div>
                    <label className="mb-2 block text-sm font-medium text-gray-700">Cấp độ kinh nghiệm</label>
                    <select
                        className="h-11 w-full rounded-lg border border-gray-300 bg-white px-4 text-sm text-gray-900 outline-none transition focus:border-[#00b14f] focus:ring-2 focus:ring-[#00b14f]/20"
                        {...register('level')}
                    >
                        <option value="">Chọn cấp độ</option>
                        {levelOptions.map((option) => (
                            <option key={option} value={option}>{option}</option>
                        ))}
                    </select>
                </div>

                <Input
                    label="Địa điểm làm việc"
                    placeholder="Hồ Chí Minh"
                    icon={<MapPin className="h-4 w-4" />}
                    error={errors.location?.message}
                    {...register('location')}
                />
            </div>

            <div className="grid gap-5 md:grid-cols-2">
                <Input
                    label="Deadline"
                    type="date"
                    error={errors.deadline?.message}
                    {...register('deadline')}
                />

                <label className="flex items-center gap-3 rounded-lg border border-gray-200 px-4 py-3 text-sm text-gray-700">
                    <input
                        type="checkbox"
                        className="h-4 w-4 rounded border-gray-300 text-[#00b14f] focus:ring-[#00b14f]"
                        disabled={isEditMode}
                        {...register('isRemote')}
                    />
                    Làm việc từ xa (Remote)
                </label>
            </div>

            {isEditMode && (
                <div className="rounded-lg border border-dashed border-gray-300 bg-gray-50 p-4 text-sm text-gray-600">
                    Một số trường cơ bản như <code>categoryId</code>, <code>budgetType</code> và <code>isRemote</code> không thể thay đổi sau khi job đã được tạo. Nếu cần chỉnh sửa, hãy tạo tin tuyển dụng mới với thông tin chính xác.
                </div>
            )}
        </>
    );
}
