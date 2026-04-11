import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
    FileSignature, CheckCircle2, XCircle, Clock, DollarSign,
    Star, ChevronRight, Loader2, AlertCircle, MessageSquare, Send
} from 'lucide-react';
import toast from 'react-hot-toast';
import { contractsApi, reviewsApi, type Contract, type Review } from '../../api/contracts.api';

const STATUS_CONFIG: Record<string, { label: string; color: string; bg: string; icon: any }> = {
    Active: { label: 'Đang hiệu lực', color: 'text-blue-700', bg: 'bg-blue-50 border-blue-200', icon: Clock },
    InProgress: { label: 'Đang thực hiện', color: 'text-amber-700', bg: 'bg-amber-50 border-amber-200', icon: Clock },
    Completed: { label: 'Hoàn thành', color: 'text-emerald-700', bg: 'bg-emerald-50 border-emerald-200', icon: CheckCircle2 },
    Cancelled: { label: 'Đã hủy', color: 'text-red-700', bg: 'bg-red-50 border-red-200', icon: XCircle },
    Disputed: { label: 'Tranh chấp', color: 'text-orange-700', bg: 'bg-orange-50 border-orange-200', icon: AlertCircle },
    Resolved: { label: 'Đã giải quyết', color: 'text-gray-700', bg: 'bg-gray-50 border-gray-200', icon: CheckCircle2 },
};

export function ContractsPage() {
    const queryClient = useQueryClient();
    const [selectedContract, setSelectedContract] = useState<Contract | null>(null);
    const [reviewModal, setReviewModal] = useState<{ contractId: string; revieweeId: string } | null>(null);
    const [rating, setRating] = useState(5);
    const [comment, setComment] = useState('');

    const { data: contracts = [], isLoading } = useQuery({
        queryKey: ['my-contracts'],
        queryFn: () => contractsApi.getMyContracts('candidate'),
    });

    const { data: reviews = [] } = useQuery({
        queryKey: ['contract-reviews', selectedContract?.id],
        queryFn: () => reviewsApi.getByContract(selectedContract!.id),
        enabled: !!selectedContract,
    });

    const reviewMutation = useMutation({
        mutationFn: (data: { contractId: string; revieweeId: string; rating: number; comment?: string }) =>
            reviewsApi.create(data),
        onSuccess: () => {
            toast.success('Đánh giá đã được gửi!');
            queryClient.invalidateQueries({ queryKey: ['contract-reviews'] });
            setReviewModal(null);
            setRating(5);
            setComment('');
        },
        onError: (err: any) => {
            const msg = err?.response?.data?.message || 'Không thể gửi đánh giá';
            toast.error(msg);
        },
    });

    const formatCurrency = (amount: number) =>
        new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);

    const formatDate = (d?: string) =>
        d ? new Date(d).toLocaleDateString('vi-VN') : '—';

    return (
        <div className="space-y-6">
            {/* Header */}
            <div className="rounded-2xl bg-gradient-to-br from-[#00b14f] via-emerald-600 to-teal-700 p-6 text-white shadow-xl">
                <div className="flex items-center gap-3 mb-1">
                    <div className="rounded-xl bg-white/15 p-2.5">
                        <FileSignature className="h-6 w-6" />
                    </div>
                    <div>
                        <h1 className="text-2xl font-bold">Hợp đồng & Đánh giá</h1>
                        <p className="text-sm text-white/70">Contracts & Reviews</p>
                    </div>
                </div>
                <p className="mt-2 text-sm text-white/80 max-w-2xl">
                    Quản lý các hợp đồng từ ứng tuyển thành công và đánh giá sau hoàn thành.
                </p>

                <div className="mt-4 flex items-center gap-6 text-sm">
                    <span>{contracts.length} hợp đồng</span>
                    <span>{contracts.filter(c => c.status === 'Completed').length} hoàn thành</span>
                    <span>{contracts.filter(c => c.status === 'Active' || c.status === 'InProgress').length} đang thực hiện</span>
                </div>
            </div>

            {/* List */}
            {isLoading ? (
                <div className="flex items-center justify-center py-16">
                    <Loader2 className="h-8 w-8 animate-spin text-emerald-500" />
                </div>
            ) : contracts.length === 0 ? (
                <div className="rounded-2xl border border-dashed border-gray-200 bg-white p-16 text-center">
                    <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-gray-50">
                        <FileSignature className="h-8 w-8 text-gray-400" />
                    </div>
                    <h2 className="text-lg font-semibold text-gray-900">Chưa có hợp đồng nào</h2>
                    <p className="mt-2 text-sm text-gray-500">Hợp đồng sẽ xuất hiện khi đơn ứng tuyển được chấp nhận.</p>
                </div>
            ) : (
                <div className="space-y-3">
                    {contracts.map((contract) => {
                        const sc = STATUS_CONFIG[contract.status] || STATUS_CONFIG.Active;
                        const Icon = sc.icon;
                        return (
                            <div
                                key={contract.id}
                                onClick={() => setSelectedContract(contract)}
                                className="rounded-2xl border border-gray-100 bg-white p-5 shadow-sm hover:shadow-md transition-all cursor-pointer group"
                            >
                                <div className="flex items-center justify-between">
                                    <div className="flex items-center gap-4">
                                        <div className={`rounded-xl ${sc.bg} border p-3`}>
                                            <Icon className={`h-5 w-5 ${sc.color}`} />
                                        </div>
                                        <div>
                                            <p className="font-semibold text-gray-900">
                                                Hợp đồng #{contract.id.slice(-6).toUpperCase()}
                                            </p>
                                            <div className="flex items-center gap-3 mt-1 text-xs text-gray-500">
                                                <span className="flex items-center gap-1">
                                                    <DollarSign className="h-3 w-3" />
                                                    {formatCurrency(contract.agreedAmount)}
                                                </span>
                                                <span>Tạo: {formatDate(contract.createdAt)}</span>
                                            </div>
                                        </div>
                                    </div>
                                    <div className="flex items-center gap-3">
                                        <span className={`px-2.5 py-1 rounded-full text-xs font-medium ${sc.bg} ${sc.color} border`}>
                                            {sc.label}
                                        </span>
                                        {contract.status === 'Completed' && (
                                            <button
                                                onClick={(e) => {
                                                    e.stopPropagation();
                                                    setReviewModal({ contractId: contract.id, revieweeId: contract.companyId });
                                                }}
                                                className="flex items-center gap-1 text-xs font-medium text-amber-600 hover:text-amber-700 bg-amber-50 px-2.5 py-1 rounded-full border border-amber-200"
                                            >
                                                <Star className="h-3 w-3" />
                                                Đánh giá
                                            </button>
                                        )}
                                        <ChevronRight className="h-4 w-4 text-gray-400 group-hover:text-gray-600 transition-colors" />
                                    </div>
                                </div>
                            </div>
                        );
                    })}
                </div>
            )}

            {/* Contract Detail Modal */}
            {selectedContract && (
                <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
                    <div className="bg-white rounded-2xl shadow-2xl max-w-lg w-full max-h-[80vh] overflow-y-auto p-6">
                        <div className="flex items-center justify-between mb-4">
                            <h2 className="text-lg font-bold text-gray-900">
                                Hợp đồng #{selectedContract.id.slice(-6).toUpperCase()}
                            </h2>
                            <button onClick={() => setSelectedContract(null)} className="p-2 hover:bg-gray-100 rounded-full">
                                <XCircle className="h-5 w-5 text-gray-400" />
                            </button>
                        </div>

                        <div className="space-y-3">
                            {[
                                ['Trạng thái', STATUS_CONFIG[selectedContract.status]?.label || selectedContract.status],
                                ['Số tiền', formatCurrency(selectedContract.agreedAmount)],
                                ['Ngày tạo', formatDate(selectedContract.createdAt)],
                                ['Ngày bắt đầu', formatDate(selectedContract.startDate)],
                                ['Ngày kết thúc', formatDate(selectedContract.endDate)],
                                ['Hoàn thành', formatDate(selectedContract.completedAt)],
                            ].map(([label, value]) => (
                                <div key={label as string} className="flex justify-between text-sm">
                                    <span className="text-gray-500">{label}</span>
                                    <span className="font-medium text-gray-900">{value}</span>
                                </div>
                            ))}
                        </div>

                        {/* Reviews */}
                        {reviews.length > 0 && (
                            <div className="mt-6 pt-4 border-t">
                                <h3 className="text-sm font-semibold text-gray-900 mb-3 flex items-center gap-2">
                                    <MessageSquare className="h-4 w-4 text-amber-500" />
                                    Đánh giá ({reviews.length})
                                </h3>
                                {reviews.map((r) => (
                                    <div key={r.id} className="mb-3 rounded-xl bg-gray-50 p-3">
                                        <div className="flex items-center gap-1 mb-1">
                                            {[1, 2, 3, 4, 5].map((s) => (
                                                <Star
                                                    key={s}
                                                    className={`h-3.5 w-3.5 ${s <= r.rating ? 'text-amber-400 fill-amber-400' : 'text-gray-300'}`}
                                                />
                                            ))}
                                            <span className="ml-2 text-xs text-gray-400">{formatDate(r.createdAt)}</span>
                                        </div>
                                        {r.comment && <p className="text-sm text-gray-700">{r.comment}</p>}
                                    </div>
                                ))}
                            </div>
                        )}
                    </div>
                </div>
            )}

            {/* Review Modal */}
            {reviewModal && (
                <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
                    <div className="bg-white rounded-2xl shadow-2xl max-w-md w-full p-6">
                        <h2 className="text-lg font-bold text-gray-900 mb-4 flex items-center gap-2">
                            <Star className="h-5 w-5 text-amber-500" />
                            Đánh giá hợp đồng
                        </h2>

                        {/* Star rating */}
                        <div className="flex items-center gap-1 mb-4">
                            {[1, 2, 3, 4, 5].map((s) => (
                                <button key={s} onClick={() => setRating(s)} className="p-1">
                                    <Star
                                        className={`h-8 w-8 transition-colors ${
                                            s <= rating ? 'text-amber-400 fill-amber-400' : 'text-gray-300 hover:text-amber-200'
                                        }`}
                                    />
                                </button>
                            ))}
                            <span className="ml-3 text-sm text-gray-500">{rating}/5</span>
                        </div>

                        {/* Comment */}
                        <textarea
                            value={comment}
                            onChange={(e) => setComment(e.target.value)}
                            placeholder="Nhận xét (tùy chọn)..."
                            rows={3}
                            className="w-full rounded-xl border border-gray-200 px-4 py-3 text-sm focus:ring-2 focus:ring-emerald-500 focus:border-transparent resize-none"
                        />

                        <div className="flex gap-3 mt-4">
                            <button
                                onClick={() => {
                                    reviewMutation.mutate({
                                        contractId: reviewModal.contractId,
                                        revieweeId: reviewModal.revieweeId,
                                        rating,
                                        comment: comment || undefined,
                                    });
                                }}
                                disabled={reviewMutation.isPending}
                                className="flex-1 flex items-center justify-center gap-2 rounded-xl bg-emerald-600 text-white py-2.5 text-sm font-medium hover:bg-emerald-700 disabled:opacity-50 transition-colors"
                            >
                                {reviewMutation.isPending ? (
                                    <Loader2 className="h-4 w-4 animate-spin" />
                                ) : (
                                    <Send className="h-4 w-4" />
                                )}
                                Gửi đánh giá
                            </button>
                            <button
                                onClick={() => {
                                    setReviewModal(null);
                                    setRating(5);
                                    setComment('');
                                }}
                                className="px-4 py-2.5 rounded-xl border border-gray-200 text-sm text-gray-600 hover:bg-gray-50"
                            >
                                Hủy
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}
