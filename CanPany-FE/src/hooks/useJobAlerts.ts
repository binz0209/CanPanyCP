import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import toast from 'react-hot-toast';
import { jobAlertsApi } from '../api/jobAlerts.api';
import type { JobAlertCreateDto, JobAlertUpdateDto } from '../api/jobAlerts.api';
import { jobAlertKeys } from '../lib/queryKeys';

export function useJobAlerts() {
    const queryClient = useQueryClient();

    const listQuery = useQuery({
        queryKey: jobAlertKeys.list(),
        queryFn: () => jobAlertsApi.getAll(),
    });

    const statsQuery = useQuery({
        queryKey: jobAlertKeys.stats(),
        queryFn: () => jobAlertsApi.getStats(),
    });

    const createMutation = useMutation({
        mutationFn: (dto: JobAlertCreateDto) => jobAlertsApi.create(dto),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: jobAlertKeys.list() });
            queryClient.invalidateQueries({ queryKey: jobAlertKeys.stats() });
            toast.success('Tạo job alert thành công!');
        },
        onError: () => toast.error('Không thể tạo job alert. Vui lòng thử lại.'),
    });

    const updateMutation = useMutation({
        mutationFn: ({ id, dto }: { id: string; dto: JobAlertUpdateDto }) =>
            jobAlertsApi.update(id, dto),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: jobAlertKeys.list() });
            toast.success('Cập nhật job alert thành công!');
        },
        onError: () => toast.error('Không thể cập nhật job alert.'),
    });

    const deleteMutation = useMutation({
        mutationFn: (id: string) => jobAlertsApi.delete(id),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: jobAlertKeys.list() });
            queryClient.invalidateQueries({ queryKey: jobAlertKeys.stats() });
            toast.success('Đã xóa job alert.');
        },
        onError: () => toast.error('Không thể xóa job alert.'),
    });

    const pauseMutation = useMutation({
        mutationFn: (id: string) => jobAlertsApi.pause(id),
        onSuccess: (_, id) => {
            queryClient.setQueryData(jobAlertKeys.list(), (prev: any) =>
                prev?.map((a: any) => (a.id === id ? { ...a, isActive: false } : a))
            );
            toast.success('Đã tạm dừng job alert.');
        },
        onError: () => toast.error('Không thể tạm dừng job alert.'),
    });

    const resumeMutation = useMutation({
        mutationFn: (id: string) => jobAlertsApi.resume(id),
        onSuccess: (_, id) => {
            queryClient.setQueryData(jobAlertKeys.list(), (prev: any) =>
                prev?.map((a: any) => (a.id === id ? { ...a, isActive: true } : a))
            );
            toast.success('Job alert đã được kích hoạt lại.');
        },
        onError: () => toast.error('Không thể kích hoạt lại job alert.'),
    });

    return {
        alerts: listQuery.data ?? [],
        stats: statsQuery.data,
        isLoading: listQuery.isLoading,
        isStatsLoading: statsQuery.isLoading,
        create: (dto: JobAlertCreateDto) => createMutation.mutateAsync(dto),
        update: (id: string, dto: JobAlertUpdateDto) => updateMutation.mutateAsync({ id, dto }),
        remove: (id: string) => deleteMutation.mutate(id),
        pause: (id: string) => pauseMutation.mutate(id),
        resume: (id: string) => resumeMutation.mutate(id),
        isCreating: createMutation.isPending,
        isUpdating: updateMutation.isPending,
        isDeleting: deleteMutation.isPending,
    };
}

export function useJobAlertPreview(id: string, enabled: boolean) {
    return useQuery({
        queryKey: jobAlertKeys.preview(id),
        queryFn: () => jobAlertsApi.preview(id),
        enabled,
    });
}
