import { useMutation } from '@tanstack/react-query';
import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import { Button, Card } from '../../components/ui';
import { adminApi } from '../../api';

export function AdminJobsPage() {
    const { t } = useTranslation('admin');

    const [hideJobId, setHideJobId] = useState('');
    const [hideReason, setHideReason] = useState('');

    const [deleteJobId, setDeleteJobId] = useState('');

    const hideMutation = useMutation({
        mutationFn: () => adminApi.hideJob(hideJobId.trim(), hideReason.trim()),
        onSuccess: () => {
            toast.success('Job đã được ẩn.');
            setHideJobId('');
            setHideReason('');
        },
        onError: () => toast.error('Không thể ẩn job.'),
    });

    const deleteMutation = useMutation({
        mutationFn: () => adminApi.deleteJob(deleteJobId.trim()),
        onSuccess: () => {
            toast.success('Job đã được xóa.');
            setDeleteJobId('');
        },
        onError: () => toast.error('Không thể xóa job.'),
    });

    const title = t('placeholders.jobs.title');
    const desc = t('placeholders.jobs.description');

    return (
        <div className="space-y-6">
            <div>
                <h1 className="text-2xl font-bold text-gray-900">{title}</h1>
                <p className="mt-1 text-sm text-gray-600">{desc}</p>
            </div>

            <div className="grid gap-4 lg:grid-cols-2">
                <Card className="space-y-4 p-5">
                    <h2 className="text-lg font-semibold text-gray-900">Ẩn job</h2>
                    <div className="space-y-2">
                        <label className="block text-sm font-medium text-gray-700">Job ID</label>
                        <input
                            value={hideJobId}
                            onChange={(e) => setHideJobId(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder="VD: 65f6... (jobId)"
                        />
                    </div>
                    <div className="space-y-2">
                        <label className="block text-sm font-medium text-gray-700">Lý do</label>
                        <textarea
                            value={hideReason}
                            onChange={(e) => setHideReason(e.target.value)}
                            rows={4}
                            className="w-full resize-y rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder="Nhập lý do ẩn job..."
                        />
                    </div>
                    <div className="flex justify-end">
                        <Button
                            onClick={() => hideMutation.mutate()}
                            disabled={hideMutation.isPending || !hideJobId.trim() || !hideReason.trim()}
                            className="bg-[#00b14f] hover:bg-[#00b14f]/90"
                        >
                            Ẩn job
                        </Button>
                    </div>
                </Card>

                <Card className="space-y-4 p-5">
                    <h2 className="text-lg font-semibold text-gray-900">Xóa job</h2>
                    <div className="space-y-2">
                        <label className="block text-sm font-medium text-gray-700">Job ID</label>
                        <input
                            value={deleteJobId}
                            onChange={(e) => setDeleteJobId(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder="VD: 65f6... (jobId)"
                        />
                    </div>
                    <div className="flex justify-end">
                        <Button
                            variant="outline"
                            onClick={() => deleteMutation.mutate()}
                            disabled={deleteMutation.isPending || !deleteJobId.trim()}
                            className="border-red-200 text-red-600 hover:bg-red-50"
                        >
                            Xóa job
                        </Button>
                    </div>
                </Card>
            </div>
        </div>
    );
}

