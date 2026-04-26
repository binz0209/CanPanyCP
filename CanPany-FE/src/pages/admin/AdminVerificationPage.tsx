import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import { ExternalLink, Eye, FileSpreadsheet, FileText, FileType, Image as ImageIcon, X } from 'lucide-react';
import { adminApi, companiesApi } from '../../api';
import { Button, Card } from '../../components/ui';
import { adminKeys, companiesKeys } from '../../lib/queryKeys';
import type { Company } from '../../types';

// ─── helpers ────────────────────────────────────────────────────────────────

/**
 * Cloudinary raw files lose extension in URL path for some account settings.
 * We check both the path and attempt to parse resource type from URL segments.
 */
function detectFileCategory(url: string): 'image' | 'pdf' | 'word' | 'excel' | 'other' {
    const lower = url.toLowerCase().split('?')[0]; // strip query string

    const imageExts = ['.jpg', '.jpeg', '.png', '.webp', '.gif', '.bmp', '.svg'];
    const pdfExts = ['.pdf'];
    const wordExts = ['.doc', '.docx'];
    const excelExts = ['.xls', '.xlsx'];

    // Check for image upload resource type in Cloudinary URL path
    // Cloudinary image URLs contain /image/upload/ segment
    // Raw URLs contain /raw/upload/ segment
    if (lower.includes('/image/upload/')) return 'image';

    // Check extension after stripping Cloudinary transformation params
    // URL format: .../upload/v123456/folder/filename.ext
    const pathPart = lower.split('?')[0];
    const lastSlash = pathPart.lastIndexOf('/');
    const fileName = pathPart.slice(lastSlash + 1);
    const dotIdx = fileName.lastIndexOf('.');
    const ext = dotIdx >= 0 ? fileName.slice(dotIdx) : '';

    if (imageExts.some((e) => ext === e || lower.includes(e))) return 'image';
    if (pdfExts.some((e) => ext === e || pathPart.includes(e))) return 'pdf';
    if (wordExts.some((e) => ext === e || pathPart.includes(e))) return 'word';
    if (excelExts.some((e) => ext === e || pathPart.includes(e))) return 'excel';

    return 'other';
}

function FileIcon({ category, className = 'h-5 w-5' }: { category: ReturnType<typeof detectFileCategory>; className?: string }) {
    switch (category) {
        case 'image':  return <ImageIcon className={`${className} text-blue-500`} />;
        case 'pdf':    return <FileType className={`${className} text-red-500`} />;
        case 'word':   return <FileText className={`${className} text-blue-600`} />;
        case 'excel':  return <FileSpreadsheet className={`${className} text-green-600`} />;
        default:       return <FileText className={`${className} text-gray-400`} />;
    }
}

function getReadableName(url: string): string {
    const path = url.split('?')[0];
    return decodeURIComponent(path.split('/').pop() ?? url);
}

// ─── Document lightbox / modal ───────────────────────────────────────────────

function DocModal({ docs, company, onClose }: { docs: string[]; company: Company; onClose: () => void }) {
    const [active, setActive] = useState(0);
    const [isLoadingUrl, setIsLoadingUrl] = useState(false);
    
    const currentUrl = docs[active];
    const category = detectFileCategory(currentUrl);

    const handleOpenDocument = async (url: string) => {
        try {
            setIsLoadingUrl(true);
            const res = await companiesApi.getVerificationDocumentDownloadUrl(url);
            window.open(res.url, '_blank');
        } catch (error) {
            toast.error('Không thể lấy đường dẫn tải xuống');
            // Fallback just in case
            window.open(url, '_blank');
        } finally {
            setIsLoadingUrl(false);
        }
    };

    return (
        <div
            className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 p-4 backdrop-blur-sm"
            onClick={(e) => e.target === e.currentTarget && onClose()}
        >
            <div className="relative flex flex-col w-full max-w-4xl max-h-[92vh] rounded-2xl bg-white shadow-2xl overflow-hidden">
                {/* Header */}
                <div className="flex items-center gap-3 border-b border-gray-100 px-5 py-3 shrink-0">
                    <div className="flex-1 min-w-0">
                        <p className="text-xs text-gray-500">Tài liệu xác minh của</p>
                        <p className="truncate text-sm font-semibold text-gray-900">{company.name}</p>
                    </div>
                    {/* Tab picker */}
                    {docs.length > 1 && (
                        <div className="flex items-center gap-1 overflow-x-auto max-w-xs">
                            {docs.map((url, i) => {
                                const cat = detectFileCategory(url);
                                return (
                                    <button
                                        key={url}
                                        onClick={() => setActive(i)}
                                        title={getReadableName(url)}
                                        className={`flex shrink-0 items-center gap-1.5 rounded-lg border px-2.5 py-1.5 text-xs font-medium transition-colors ${
                                            i === active
                                                ? 'border-[#00b14f] bg-[#00b14f]/10 text-[#00b14f]'
                                                : 'border-gray-200 bg-white text-gray-600 hover:border-gray-300'
                                        }`}
                                    >
                                        <FileIcon category={cat} className="h-3.5 w-3.5" />
                                        File {i + 1}
                                    </button>
                                );
                            })}
                        </div>
                    )}
                    <button
                        onClick={() => handleOpenDocument(currentUrl)}
                        disabled={isLoadingUrl}
                        className="flex shrink-0 items-center gap-1.5 rounded-lg border border-gray-200 px-3 py-1.5 text-xs font-medium text-gray-600 hover:border-[#00b14f] hover:text-[#00b14f] transition-colors disabled:opacity-50"
                    >
                        {isLoadingUrl ? (
                            <div className="animate-spin rounded-full h-3.5 w-3.5 border-2 border-gray-400 border-t-transparent"></div>
                        ) : (
                            <ExternalLink className="h-3.5 w-3.5" />
                        )}
                        Mở tab mới
                    </button>
                    <button
                        onClick={onClose}
                        className="rounded-lg p-1.5 text-gray-400 hover:bg-gray-100 hover:text-gray-700 transition-colors"
                    >
                        <X className="h-5 w-5" />
                    </button>
                </div>

                {/* Content */}
                <div className="flex-1 overflow-auto bg-gray-50">
                    {category === 'image' ? (
                        <div className="flex items-center justify-center min-h-[400px] p-4">
                            <img
                                src={currentUrl}
                                alt={getReadableName(currentUrl)}
                                className="max-h-[70vh] max-w-full rounded-xl object-contain shadow-lg"
                            />
                        </div>
                    ) : (
                        <div className="flex flex-col items-center justify-center h-[65vh] p-4 text-center bg-white">
                            <FileIcon category={category} className="h-16 w-16 mb-4 opacity-80" />
                            <p className="text-gray-900 mb-2 font-semibold text-lg">{getReadableName(currentUrl)}</p>
                            <p className="text-sm text-gray-500 mb-6 max-w-sm">
                                Tài liệu này được lưu trữ để bảo mật. Vui lòng bấm vào nút bên dưới để mở hoặc tải xuống an toàn.
                            </p>
                            <button
                                onClick={() => handleOpenDocument(currentUrl)}
                                disabled={isLoadingUrl}
                                className="inline-flex items-center gap-2 rounded-lg bg-[#00b14f] px-6 py-3 text-sm font-semibold text-white hover:bg-[#00a045] transition-colors shadow-md disabled:opacity-50"
                            >
                                {isLoadingUrl ? (
                                    <>
                                        <div className="animate-spin rounded-full h-4 w-4 border-2 border-white border-t-transparent"></div>
                                        Đang tải dữ liệu...
                                    </>
                                ) : (
                                    <>
                                        <ExternalLink className="h-4 w-4" />
                                        Mở / Tải xuống tệp này
                                    </>
                                )}
                            </button>
                        </div>
                    )}
                </div>

                {/* Footer info */}
                <div className="border-t border-gray-100 px-5 py-2 shrink-0">
                    <p className="truncate text-xs text-gray-400">{getReadableName(currentUrl)}</p>
                </div>
            </div>
        </div>
    );
}

// ─── Row ────────────────────────────────────────────────────────────────────

function VerificationRow({
    c,
    busy,
    onApprove,
    onReject,
    onViewDocs,
}: {
    c: Company;
    busy: boolean;
    onApprove: () => void;
    onReject: () => void;
    onViewDocs: () => void;
}) {
    const { t } = useTranslation('admin');
    const docs: string[] = c.verificationDocuments ?? [];

    return (
        <tr className="hover:bg-gray-50/80">
            <td className="px-4 py-3">
                <div className="font-medium text-gray-900">{c.name}</div>
                {c.website && <div className="text-xs text-gray-500">{c.website}</div>}
            </td>
            <td className="px-4 py-3">
                <span className="rounded-full bg-amber-100 px-2.5 py-0.5 text-xs font-medium text-amber-800">
                    {t('verification.pending')}
                </span>
            </td>
            <td className="px-4 py-3">
                {docs.length > 0 ? (
                    <div className="flex items-center gap-2">
                        {/* Mini preview chips */}
                        <div className="flex -space-x-1">
                            {docs.slice(0, 4).map((url, i) => {
                                const cat = detectFileCategory(url);
                                return (
                                    <div
                                        key={i}
                                        className="flex h-7 w-7 items-center justify-center rounded-full border-2 border-white bg-gray-100 shadow-sm"
                                        title={getReadableName(url)}
                                    >
                                        {cat === 'image' ? (
                                            <img
                                                src={url}
                                                alt=""
                                                className="h-full w-full rounded-full object-cover"
                                                onError={(e) => {
                                                    (e.target as HTMLImageElement).style.display = 'none';
                                                }}
                                            />
                                        ) : (
                                            <FileIcon category={cat} className="h-3.5 w-3.5" />
                                        )}
                                    </div>
                                );
                            })}
                            {docs.length > 4 && (
                                <div className="flex h-7 w-7 items-center justify-center rounded-full border-2 border-white bg-gray-200 text-[10px] font-semibold text-gray-600 shadow-sm">
                                    +{docs.length - 4}
                                </div>
                            )}
                        </div>
                        <button
                            type="button"
                            onClick={onViewDocs}
                            className="inline-flex items-center gap-1 rounded-md border border-gray-200 bg-white px-2 py-1 text-xs font-medium text-gray-600 hover:border-[#00b14f]/60 hover:text-[#00b14f] transition-colors"
                        >
                            <Eye className="h-3.5 w-3.5" />
                            {t('verification.viewDocs')} ({docs.length})
                        </button>
                    </div>
                ) : (
                    <span className="text-xs text-gray-400 italic">{t('verification.noDocs')}</span>
                )}
            </td>
            <td className="px-4 py-3 text-right">
                <div className="flex flex-wrap justify-end gap-2">
                    <Button
                        size="sm"
                        className="bg-[#00b14f] hover:bg-[#00b14f]/90"
                        disabled={busy}
                        onClick={onApprove}
                    >
                        {t('verification.approve')}
                    </Button>
                    <Button
                        size="sm"
                        variant="outline"
                        className="border-red-200 text-red-600 hover:bg-red-50"
                        disabled={busy}
                        onClick={onReject}
                    >
                        {t('verification.reject')}
                    </Button>
                </div>
            </td>
        </tr>
    );
}

// ─── Page ────────────────────────────────────────────────────────────────────

export function AdminVerificationPage() {
    const { t } = useTranslation('admin');
    const { t: tCommon } = useTranslation('common');
    const queryClient = useQueryClient();
    const [rejectFor, setRejectFor] = useState<Company | null>(null);
    const [rejectReason, setRejectReason] = useState('');
    const [docModal, setDocModal] = useState<Company | null>(null);

    const pendingQuery = useQuery({
        queryKey: adminKeys.verification(),
        queryFn: async () => {
            const res = await companiesApi.getAll({ page: 1, pageSize: 200 });
            return res.companies.filter((c) => c.verificationStatus === 'Pending');
        },
    });

    const approveMutation = useMutation({
        mutationFn: (companyId: string) => adminApi.approveVerification(companyId),
        onSuccess: () => {
            void queryClient.invalidateQueries({ queryKey: adminKeys.verification() });
            void queryClient.invalidateQueries({ queryKey: companiesKeys.list() });
            void queryClient.invalidateQueries({ queryKey: adminKeys.dashboard() });
            toast.success(t('verification.approveSuccess'));
        },
        onError: () => toast.error(t('verification.actionError')),
    });

    const rejectMutation = useMutation({
        mutationFn: ({ companyId, reason }: { companyId: string; reason: string }) =>
            adminApi.rejectVerification(companyId, reason),
        onSuccess: () => {
            void queryClient.invalidateQueries({ queryKey: adminKeys.verification() });
            void queryClient.invalidateQueries({ queryKey: companiesKeys.list() });
            void queryClient.invalidateQueries({ queryKey: adminKeys.dashboard() });
            toast.success(t('verification.rejectSuccess'));
            setRejectFor(null);
            setRejectReason('');
        },
        onError: () => toast.error(t('verification.actionError')),
    });

    const busy = approveMutation.isPending || rejectMutation.isPending;

    if (pendingQuery.isError) {
        return (
            <div className="rounded-xl border border-red-100 bg-red-50 p-6 text-sm text-red-800">
                {t('verification.loadError')}
            </div>
        );
    }

    const list = pendingQuery.data ?? [];

    return (
        <div className="space-y-6">
            <div>
                <h1 className="text-2xl font-bold text-gray-900">{t('verification.title')}</h1>
                <p className="mt-1 text-sm text-gray-600">{t('verification.subtitle')}</p>
            </div>

            <Card className="overflow-hidden p-0">
                <div className="overflow-x-auto">
                    <table className="w-full min-w-[700px] text-left text-sm">
                        <thead className="border-b border-gray-100 bg-gray-50 text-xs font-semibold uppercase text-gray-500">
                            <tr>
                                <th className="px-4 py-3">{t('verification.company')}</th>
                                <th className="px-4 py-3">{t('verification.status')}</th>
                                <th className="px-4 py-3">{t('verification.documents')}</th>
                                <th className="px-4 py-3 text-right">{/* actions */}</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-gray-100">
                            {pendingQuery.isLoading ? (
                                <tr>
                                    <td colSpan={4} className="px-4 py-12 text-center text-gray-500">
                                        {tCommon('app.loading')}
                                    </td>
                                </tr>
                            ) : list.length === 0 ? (
                                <tr>
                                    <td colSpan={4} className="px-4 py-12 text-center text-gray-500">
                                        {t('verification.empty')}
                                    </td>
                                </tr>
                            ) : (
                                list.map((c) => (
                                    <VerificationRow
                                        key={c.id}
                                        c={c}
                                        busy={busy}
                                        onApprove={() => approveMutation.mutate(c.id)}
                                        onReject={() => {
                                            setRejectFor(c);
                                            setRejectReason('');
                                        }}
                                        onViewDocs={() => setDocModal(c)}
                                    />
                                ))
                            )}
                        </tbody>
                    </table>
                </div>
            </Card>

            {/* Doc modal (global, outside table) */}
            {docModal && (
                <DocModal
                    docs={docModal.verificationDocuments ?? []}
                    company={docModal}
                    onClose={() => setDocModal(null)}
                />
            )}

            {/* Reject modal */}
            {rejectFor && (
                <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
                    <div className="max-h-[90vh] w-full max-w-md overflow-y-auto rounded-2xl bg-white p-6 shadow-xl">
                        <h2 className="text-lg font-semibold text-gray-900">{t('verification.reject')}</h2>
                        <p className="mt-1 text-sm text-gray-600">{rejectFor.name}</p>
                        <label
                            htmlFor="reject-reason"
                            className="mt-4 block text-sm font-medium text-gray-700"
                        >
                            {t('verification.rejectReason')}
                        </label>
                        <textarea
                            id="reject-reason"
                            value={rejectReason}
                            onChange={(e) => setRejectReason(e.target.value)}
                            rows={4}
                            placeholder={t('verification.rejectReasonPlaceholder')}
                            className="mt-1 w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                        />
                        <div className="mt-4 flex justify-end gap-2">
                            <Button
                                type="button"
                                variant="outline"
                                disabled={busy}
                                onClick={() => {
                                    setRejectFor(null);
                                    setRejectReason('');
                                }}
                            >
                                {t('verification.cancel')}
                            </Button>
                            <Button
                                type="button"
                                className="bg-red-600 hover:bg-red-700"
                                disabled={busy || !rejectReason.trim()}
                                onClick={() =>
                                    rejectMutation.mutate({
                                        companyId: rejectFor.id,
                                        reason: rejectReason.trim(),
                                    })
                                }
                            >
                                {t('verification.confirmReject')}
                            </Button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}
