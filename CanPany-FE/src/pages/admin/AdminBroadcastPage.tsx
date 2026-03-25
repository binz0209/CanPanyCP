import { useMutation } from '@tanstack/react-query';
import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import { Button, Card } from '../../components/ui';
import { adminApi } from '../../api';

export function AdminBroadcastPage() {
    const { t } = useTranslation('admin');

    const title = t('placeholders.broadcast.title');
    const desc = t('placeholders.broadcast.description');

    const [broadcastTitle, setBroadcastTitle] = useState('');
    const [message, setMessage] = useState('');
    const [targetRole, setTargetRole] = useState<string>(''); // optional

    const sendMutation = useMutation({
        mutationFn: () =>
            adminApi.sendBroadcastNotification({
                title: broadcastTitle.trim(),
                message: message.trim(),
                targetRole: targetRole.trim() || undefined,
            }),
        onSuccess: () => {
            toast.success('Đã gửi broadcast.');
            setBroadcastTitle('');
            setMessage('');
            setTargetRole('');
        },
        onError: () => toast.error('Không thể gửi broadcast.'),
    });

    return (
        <div className="space-y-6">
            <div>
                <h1 className="text-2xl font-bold text-gray-900">{title}</h1>
                <p className="mt-1 text-sm text-gray-600">{desc}</p>
            </div>

            <div className="grid gap-4 lg:grid-cols-2">
                <Card className="space-y-4 p-5 lg:col-span-2">
                    <div className="space-y-2">
                        <label className="block text-sm font-medium text-gray-700">Title</label>
                        <input
                            value={broadcastTitle}
                            onChange={(e) => setBroadcastTitle(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder="Tiêu đề broadcast"
                        />
                    </div>
                    <div className="space-y-2">
                        <label className="block text-sm font-medium text-gray-700">Message</label>
                        <textarea
                            value={message}
                            onChange={(e) => setMessage(e.target.value)}
                            rows={6}
                            className="w-full resize-y rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder="Nội dung thông báo"
                        />
                    </div>
                    <div className="space-y-2">
                        <label className="block text-sm font-medium text-gray-700">Target role (optional)</label>
                        <select
                            value={targetRole}
                            onChange={(e) => setTargetRole(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                        >
                            <option value="">All</option>
                            <option value="Candidate">Candidate</option>
                            <option value="Company">Company</option>
                            <option value="Admin">Admin</option>
                            <option value="Guest">Guest</option>
                        </select>
                    </div>
                    <div className="flex justify-end">
                        <Button
                            className="bg-[#00b14f] hover:bg-[#00b14f]/90"
                            disabled={sendMutation.isPending || !broadcastTitle.trim() || !message.trim()}
                            onClick={() => sendMutation.mutate()}
                        >
                            Gửi broadcast
                        </Button>
                    </div>
                </Card>
            </div>
        </div>
    );
}

