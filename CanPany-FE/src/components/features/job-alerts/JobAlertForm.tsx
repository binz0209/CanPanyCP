import { useState, useEffect } from 'react';
import { X, Loader2 } from 'lucide-react';
import type { JobAlertResponse, JobAlertCreateDto, JobAlertUpdateDto } from '../../../api/jobAlerts.api';
import { Button } from '../../ui/Button';
import { useQuery } from '@tanstack/react-query';
import { catalogApi } from '../../../api/catalog.api';

interface JobAlertFormProps {
    isOpen: boolean;
    onClose: () => void;
    onSubmit: (dto: JobAlertCreateDto | JobAlertUpdateDto) => Promise<void>;
    initialData?: JobAlertResponse | null;
    isSubmitting?: boolean;
}

const JOB_TYPES = ['FullTime', 'PartTime', 'Freelance'] as const;
const JOB_TYPE_LABELS: Record<string, string> = {
    FullTime: 'Full-time',
    PartTime: 'Part-time',
    Freelance: 'Freelance',
};
const EXPERIENCE_LEVELS = ['Intern', 'Junior', 'Mid', 'Senior', 'Lead', 'Manager'];
const FREQUENCIES = [
    { value: 'Immediate', label: 'Ngay lập tức', desc: 'Nhận thông báo ngay khi có job phù hợp' },
    { value: 'Daily', label: 'Hàng ngày', desc: 'Tổng hợp job mỗi ngày 8:00 sáng' },
    { value: 'Weekly', label: 'Hàng tuần', desc: 'Digest email vào thứ 2 hàng tuần' },
] as const;

const emptyForm = {
    title: '',
    location: '',
    jobType: '' as string,
    minBudget: '' as string | number,
    maxBudget: '' as string | number,
    experienceLevel: '',
    skillIds: [] as string[],
    categoryIds: [] as string[],
    frequency: 'Daily' as 'Immediate' | 'Daily' | 'Weekly',
    emailEnabled: true,
    inAppEnabled: true,
};

export function JobAlertForm({ isOpen, onClose, onSubmit, initialData, isSubmitting }: JobAlertFormProps) {
    const [form, setForm] = useState(emptyForm);

    const { data: skills = [] } = useQuery({
        queryKey: ['catalog', 'skills'],
        queryFn: catalogApi.getSkills,
        enabled: isOpen,
    });

    const { data: categories = [] } = useQuery({
        queryKey: ['catalog', 'categories'],
        queryFn: catalogApi.getCategories,
        enabled: isOpen,
    });

    useEffect(() => {
        if (initialData) {
            setForm({
                title: initialData.title ?? '',
                location: initialData.location ?? '',
                jobType: initialData.jobType ?? '',
                minBudget: initialData.minBudget ?? '',
                maxBudget: initialData.maxBudget ?? '',
                experienceLevel: initialData.experienceLevel ?? '',
                skillIds: initialData.skillIds ?? [],
                categoryIds: initialData.categoryIds ?? [],
                frequency: (initialData.frequency as any) ?? 'Daily',
                emailEnabled: initialData.emailEnabled,
                inAppEnabled: initialData.inAppEnabled,
            });
        } else {
            setForm(emptyForm);
        }
    }, [initialData, isOpen]);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        const dto: JobAlertCreateDto = {
            title: form.title || (undefined as any),
            location: form.location || undefined,
            jobType: (form.jobType || undefined) as any,
            minBudget: form.minBudget !== '' ? Number(form.minBudget) : undefined,
            maxBudget: form.maxBudget !== '' ? Number(form.maxBudget) : undefined,
            experienceLevel: form.experienceLevel || undefined,
            skillIds: form.skillIds.length > 0 ? form.skillIds : undefined,
            categoryIds: form.categoryIds.length > 0 ? form.categoryIds : undefined,
            frequency: form.frequency,
            emailEnabled: form.emailEnabled,
            inAppEnabled: form.inAppEnabled,
        };
        await onSubmit(dto);
        onClose();
    };

    if (!isOpen) return null;

    const isEdit = Boolean(initialData);

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
            <div className="absolute inset-0 bg-black/50" onClick={onClose} />
            <div className="relative w-full max-w-lg rounded-xl bg-white shadow-xl max-h-[90vh] overflow-y-auto">
                {/* Header */}
                <div className="flex items-center justify-between border-b border-gray-100 px-6 py-4">
                    <h2 className="text-lg font-semibold text-gray-900">
                        {isEdit ? 'Chỉnh sửa Job Alert' : 'Tạo Job Alert mới'}
                    </h2>
                    <button onClick={onClose} className="rounded-lg p-1 text-gray-400 hover:bg-gray-100 hover:text-gray-600">
                        <X className="h-5 w-5" />
                    </button>
                </div>

                <form onSubmit={handleSubmit} className="p-6 space-y-5">
                    {/* Title */}
                    <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">
                            Tên alert <span className="text-gray-400">(không bắt buộc)</span>
                        </label>
                        <input
                            type="text"
                            placeholder="VD: Senior Frontend tại Hà Nội"
                            value={form.title}
                            onChange={(e) => setForm((p) => ({ ...p, title: e.target.value }))}
                            className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-[#00b14f] focus:outline-none focus:ring-2 focus:ring-[#00b14f]/20"
                        />
                    </div>

                    {/* Location */}
                    <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">Địa điểm</label>
                        <input
                            type="text"
                            placeholder="VD: Hà Nội, TP.HCM, Remote..."
                            value={form.location}
                            onChange={(e) => setForm((p) => ({ ...p, location: e.target.value }))}
                            className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-[#00b14f] focus:outline-none focus:ring-2 focus:ring-[#00b14f]/20"
                        />
                    </div>

                    {/* Job Type + Experience Level */}
                    <div className="grid grid-cols-2 gap-4">
                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-1">Loại công việc</label>
                            <select
                                value={form.jobType}
                                onChange={(e) => setForm((p) => ({ ...p, jobType: e.target.value }))}
                                className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-[#00b14f] focus:outline-none focus:ring-2 focus:ring-[#00b14f]/20"
                            >
                                <option value="">Tất cả</option>
                                {JOB_TYPES.map((t) => (
                                    <option key={t} value={t}>{JOB_TYPE_LABELS[t]}</option>
                                ))}
                            </select>
                        </div>
                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-1">Level kinh nghiệm</label>
                            <select
                                value={form.experienceLevel}
                                onChange={(e) => setForm((p) => ({ ...p, experienceLevel: e.target.value }))}
                                className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-[#00b14f] focus:outline-none focus:ring-2 focus:ring-[#00b14f]/20"
                            >
                                <option value="">Tất cả</option>
                                {EXPERIENCE_LEVELS.map((l) => (
                                    <option key={l} value={l}>{l}</option>
                                ))}
                            </select>
                        </div>
                    </div>

                    {/* Budget Range */}
                    <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">
                            Mức lương (VND/tháng)
                        </label>
                        <div className="flex items-center gap-3">
                            <input
                                type="number"
                                placeholder="Tối thiểu"
                                min={0}
                                value={form.minBudget}
                                onChange={(e) => setForm((p) => ({ ...p, minBudget: e.target.value }))}
                                className="flex-1 rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-[#00b14f] focus:outline-none focus:ring-2 focus:ring-[#00b14f]/20"
                            />
                            <span className="text-gray-400">–</span>
                            <input
                                type="number"
                                placeholder="Tối đa"
                                min={0}
                                value={form.maxBudget}
                                onChange={(e) => setForm((p) => ({ ...p, maxBudget: e.target.value }))}
                                className="flex-1 rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-[#00b14f] focus:outline-none focus:ring-2 focus:ring-[#00b14f]/20"
                            />
                        </div>
                    </div>

                    {/* Category Selection */}
                    <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">Danh mục nghề nghiệp</label>
                        <div className="flex flex-wrap gap-2 mb-2">
                            {form.categoryIds.map((id) => {
                                const cat = categories.find((c) => c.id === id);
                                return (
                                    <span
                                        key={id}
                                        className="inline-flex items-center gap-1 rounded-full bg-blue-50 px-2.5 py-1 text-xs font-medium text-blue-700"
                                    >
                                        {cat?.name || id}
                                        <button
                                            type="button"
                                            onClick={() => setForm((p) => ({ ...p, categoryIds: p.categoryIds.filter((cid) => cid !== id) }))}
                                            className="hover:text-blue-900"
                                        >
                                            <X className="h-3 w-3" />
                                        </button>
                                    </span>
                                );
                            })}
                        </div>
                        <select
                            onChange={(e) => {
                                const id = e.target.value;
                                if (id && !form.categoryIds.includes(id)) {
                                    setForm((p) => ({ ...p, categoryIds: [...p.categoryIds, id] }));
                                }
                                e.target.value = '';
                            }}
                            className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-[#00b14f] focus:outline-none focus:ring-2 focus:ring-[#00b14f]/20"
                        >
                            <option value="">Thêm danh mục...</option>
                            {categories
                                .filter((c) => !form.categoryIds.includes(c.id))
                                .map((c) => (
                                    <option key={c.id} value={c.id}>
                                        {c.name}
                                    </option>
                                ))}
                        </select>
                    </div>

                    {/* Skill Selection */}
                    <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">Kỹ năng</label>
                        <div className="flex flex-wrap gap-2 mb-2">
                            {form.skillIds.map((id) => {
                                const skill = skills.find((s) => s.id === id);
                                return (
                                    <span
                                        key={id}
                                        className="inline-flex items-center gap-1 rounded-full bg-[#00b14f]/10 px-2.5 py-1 text-xs font-medium text-[#00b14f]"
                                    >
                                        {skill?.name || id}
                                        <button
                                            type="button"
                                            onClick={() => setForm((p) => ({ ...p, skillIds: p.skillIds.filter((sid) => sid !== id) }))}
                                            className="hover:text-[#009940]"
                                        >
                                            <X className="h-3 w-3" />
                                        </button>
                                    </span>
                                );
                            })}
                        </div>
                        <select
                            onChange={(e) => {
                                const id = e.target.value;
                                if (id && !form.skillIds.includes(id)) {
                                    setForm((p) => ({ ...p, skillIds: [...p.skillIds, id] }));
                                }
                                e.target.value = '';
                            }}
                            className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-[#00b14f] focus:outline-none focus:ring-2 focus:ring-[#00b14f]/20"
                        >
                            <option value="">Thêm kỹ năng...</option>
                            {skills
                                .filter((s) => !form.skillIds.includes(s.id))
                                .map((s) => (
                                    <option key={s.id} value={s.id}>
                                        {s.name}
                                    </option>
                                ))}
                        </select>
                    </div>

                    {/* Frequency */}
                    <div>
                        <label className="block text-sm font-medium text-gray-700 mb-2">Tần suất nhận alert</label>
                        <div className="space-y-2">
                            {FREQUENCIES.map((f) => (
                                <label
                                    key={f.value}
                                    className={`flex cursor-pointer items-start gap-3 rounded-lg border p-3 transition-colors ${
                                        form.frequency === f.value
                                            ? 'border-[#00b14f] bg-[#00b14f]/5'
                                            : 'border-gray-200 hover:border-gray-300'
                                    }`}
                                >
                                    <input
                                        type="radio"
                                        name="frequency"
                                        value={f.value}
                                        checked={form.frequency === f.value}
                                        onChange={() => setForm((p) => ({ ...p, frequency: f.value }))}
                                        className="mt-0.5 accent-[#00b14f]"
                                    />
                                    <div>
                                        <div className="text-sm font-medium text-gray-800">{f.label}</div>
                                        <div className="text-xs text-gray-500">{f.desc}</div>
                                    </div>
                                </label>
                            ))}
                        </div>
                    </div>

                    {/* Notification Channels */}
                    <div>
                        <label className="block text-sm font-medium text-gray-700 mb-2">Kênh thông báo</label>
                        <div className="flex gap-4">
                            <label className="flex cursor-pointer items-center gap-2 text-sm">
                                <input
                                    type="checkbox"
                                    checked={form.emailEnabled}
                                    onChange={(e) => setForm((p) => ({ ...p, emailEnabled: e.target.checked }))}
                                    className="accent-[#00b14f]"
                                />
                                📧 Email
                            </label>
                            <label className="flex cursor-pointer items-center gap-2 text-sm">
                                <input
                                    type="checkbox"
                                    checked={form.inAppEnabled}
                                    onChange={(e) => setForm((p) => ({ ...p, inAppEnabled: e.target.checked }))}
                                    className="accent-[#00b14f]"
                                />
                                🔔 In-app notification
                            </label>
                        </div>
                    </div>

                    {/* Submit */}
                    <div className="flex justify-end gap-3 pt-2 border-t border-gray-100">
                        <Button type="button" variant="outline" onClick={onClose} disabled={isSubmitting}>
                            Hủy
                        </Button>
                        <Button
                            type="submit"
                            disabled={isSubmitting}
                            className="bg-[#00b14f] hover:bg-[#009940] text-white"
                        >
                            {isSubmitting ? (
                                <span className="flex items-center gap-2">
                                    <Loader2 className="h-4 w-4 animate-spin" />
                                    Đang lưu...
                                </span>
                            ) : isEdit ? (
                                'Cập nhật'
                            ) : (
                                'Tạo Alert'
                            )}
                        </Button>
                    </div>
                </form>
            </div>
        </div>
    );
}
