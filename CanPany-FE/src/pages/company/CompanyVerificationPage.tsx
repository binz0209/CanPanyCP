import { useCallback, useEffect, useRef, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { isAxiosError } from 'axios';
import toast from 'react-hot-toast';
import {
    CheckCircle2,
    FileText,
    Image as ImageIcon,
    Loader2,
    ShieldCheck,
    Trash2,
    Upload,
    X,
} from 'lucide-react';
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
import { useTranslation } from 'react-i18next';

// ─── helpers ───────────────────────────────────────────────────────────────

const IMAGE_EXTS = ['.jpg', '.jpeg', '.png', '.webp', '.gif'];
const ALLOWED_EXTS = [...IMAGE_EXTS, '.pdf', '.doc', '.docx', '.xls', '.xlsx', '.txt'];
const MAX_SIZE_MB = 10;

function isImageUrl(url: string) {
    const lower = url.toLowerCase();
    return IMAGE_EXTS.some((ext) => lower.includes(ext));
}

function getFileExt(name: string) {
    return name.slice(name.lastIndexOf('.')).toLowerCase();
}

function formatBytes(bytes: number) {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

interface PendingFile {
    id: string;
    file: File;
    preview?: string; // object URL for images
    uploading: boolean;
    uploaded: boolean;
    uploadedUrl?: string;
    error?: string;
}

// ─── component ─────────────────────────────────────────────────────────────

export function CompanyVerificationPage() {
    const queryClient = useQueryClient();
    const { t } = useTranslation('company');
    const { companyId, isLoading: isWorkspaceLoading, isMissingProfile, hasFatalError } =
        useCompanyWorkspace();

    const fileInputRef = useRef<HTMLInputElement>(null);
    const [dragOver, setDragOver] = useState(false);
    const [pendingFiles, setPendingFiles] = useState<PendingFile[]>([]);
    // submitted URLs that will be sent to the API
    const [submittedUrls, setSubmittedUrls] = useState<string[]>([]);

    const verificationQuery = useQuery({
        queryKey: companyKeys.verification(companyId!),
        queryFn: () => companiesApi.getVerificationStatus(companyId!),
        enabled: !!companyId,
    });

    // Initialise submittedUrls from existing server data
    useEffect(() => {
        if (verificationQuery.data?.verificationDocuments?.length) {
            setSubmittedUrls(verificationQuery.data.verificationDocuments);
        }
    }, [verificationQuery.data]);

    // ── upload single file ──────────────────────────────────────────────────
    const uploadFile = useCallback(async (pending: PendingFile) => {
        setPendingFiles((prev) =>
            prev.map((p) => (p.id === pending.id ? { ...p, uploading: true, error: undefined } : p))
        );
        try {
            const [url] = await companiesApi.uploadVerificationDocuments([pending.file]);
            setPendingFiles((prev) =>
                prev.map((p) =>
                    p.id === pending.id
                        ? { ...p, uploading: false, uploaded: true, uploadedUrl: url }
                        : p
                )
            );
            setSubmittedUrls((prev) => [...prev, url]);
        } catch {
            setPendingFiles((prev) =>
                prev.map((p) =>
                    p.id === pending.id
                        ? { ...p, uploading: false, error: t('verification.uploadFailed') }
                        : p
                )
            );
        }
    }, [t]);

    // ── add files ───────────────────────────────────────────────────────────
    const addFiles = useCallback(
        (files: FileList | File[]) => {
            const arr = Array.from(files);
            const newPending: PendingFile[] = [];
            for (const file of arr) {
                const ext = getFileExt(file.name);
                if (!ALLOWED_EXTS.includes(ext)) {
                    toast.error(`${file.name}: ${t('verification.invalidType')}`);
                    continue;
                }
                if (file.size > MAX_SIZE_MB * 1024 * 1024) {
                    toast.error(`${file.name}: ${t('verification.fileTooLarge', { mb: MAX_SIZE_MB })}`);
                    continue;
                }
                const id = `${Date.now()}-${Math.random()}`;
                const preview = IMAGE_EXTS.includes(ext) ? URL.createObjectURL(file) : undefined;
                newPending.push({ id, file, preview, uploading: false, uploaded: false });
            }
            setPendingFiles((prev) => [...prev, ...newPending]);
            // auto-upload each
            newPending.forEach((p) => void uploadFile(p));
        },
        [t, uploadFile]
    );

    const handleDrop = useCallback(
        (e: React.DragEvent) => {
            e.preventDefault();
            setDragOver(false);
            addFiles(e.dataTransfer.files);
        },
        [addFiles]
    );

    const removePending = (id: string) => {
        setPendingFiles((prev) => {
            const removed = prev.find((p) => p.id === id);
            if (removed?.preview) URL.revokeObjectURL(removed.preview);
            const next = prev.filter((p) => p.id !== id);
            // also remove its URL from submittedUrls if already uploaded
            if (removed?.uploadedUrl) {
                setSubmittedUrls((urls) => urls.filter((u) => u !== removed.uploadedUrl));
            }
            return next;
        });
    };

    const removeExistingUrl = (url: string) => {
        setSubmittedUrls((prev) => prev.filter((u) => u !== url));
    };

    // ── submit verification request ─────────────────────────────────────────
    const requestMutation = useMutation({
        mutationFn: async () => {
            if (submittedUrls.length === 0) {
                throw new Error('no-docs');
            }
            await companiesApi.requestVerification({ documentUrls: submittedUrls });
        },
        onSuccess: async () => {
            setPendingFiles([]);
            await Promise.all([
                queryClient.invalidateQueries({ queryKey: companyKeys.me() }),
                queryClient.invalidateQueries({
                    queryKey: companyKeys.verification(companyId!),
                    exact: true,
                }),
                queryClient.invalidateQueries({
                    queryKey: companyKeys.statistics(companyId!),
                    exact: true,
                }),
            ]);
            toast.success(t('verification.toastSuccess'));
        },
        onError: (error) => {
            if ((error as Error).message === 'no-docs') {
                toast.error(t('verification.validDocRequired'));
                return;
            }
            const message = isAxiosError(error)
                ? error.response?.data?.message || t('verification.toastFailed')
                : t('verification.toastFailed');
            toast.error(message);
        },
    });

    // ── guards ──────────────────────────────────────────────────────────────
    if (isWorkspaceLoading || (companyId && verificationQuery.isLoading)) {
        return <CompanyWorkspaceLoader />;
    }
    if (isMissingProfile) {
        return (
            <CompanyProfileRequiredState
                title={t('verification.profileRequired')}
                description={t('verification.profileRequiredDesc')}
                icon={<ShieldCheck className="h-6 w-6" />}
            />
        );
    }
    if (hasFatalError || verificationQuery.error) {
        return (
            <CompanyWorkspaceErrorState
                title={t('verification.errorTitle')}
                description={t('verification.errorDesc')}
                icon={<ShieldCheck className="h-6 w-6" />}
            />
        );
    }

    const verification = verificationQuery.data;
    const isApproved = verification?.isVerified;
    const anyUploading = pendingFiles.some((p) => p.uploading);

    // ── existing docs (from server) that are not already in submittedUrls due to removal ──
    const existingServerDocs = verification?.verificationDocuments ?? [];

    return (
        <div className="space-y-6">
            <SectionHeader
                title={t('verification.title')}
                description={t('verification.description')}
            />

            <div className="grid gap-6 xl:grid-cols-[0.95fr_1.05fr]">
                {/* ─── Status card ─── */}
                <Card className="p-6">
                    <h2 className="text-lg font-semibold text-gray-900">{t('verification.statusTitle')}</h2>
                    <div className="mt-5 space-y-4">
                        <div className="rounded-xl bg-gray-50 p-4">
                            <p className="text-sm text-gray-500">{t('verification.statusCurrent')}</p>
                            <div className="mt-2">
                                <StatusBadge
                                    status={verification?.verificationStatus || 'Pending'}
                                    kind="verification"
                                />
                            </div>
                        </div>

                        <div className="rounded-xl bg-gray-50 p-4">
                            <p className="text-sm text-gray-500">{t('verification.isVerified')}</p>
                            <p className="mt-2 text-sm font-semibold text-gray-900">
                                {verification?.isVerified
                                    ? t('verification.verifiedYes')
                                    : t('verification.verifiedNo')}
                            </p>
                        </div>

                        <div className="rounded-xl bg-gray-50 p-4">
                            <p className="text-sm text-gray-500">{t('verification.verifiedAt')}</p>
                            <p className="mt-2 text-sm font-semibold text-gray-900">
                                {verification?.verifiedAt
                                    ? formatDateTime(verification.verifiedAt)
                                    : t('verification.verifiedAtFallback')}
                            </p>
                        </div>

                        <div className="rounded-xl border border-dashed border-gray-300 p-4 text-sm text-gray-600">
                            {isApproved
                                ? t('verification.alreadyVerifiedHint')
                                : t('verification.reviewHint')}
                        </div>

                        {/* Existing docs on server */}
                        {existingServerDocs.length > 0 && (
                            <div className="rounded-xl bg-gray-50 p-4">
                                <div className="flex items-center gap-2 mb-3">
                                    <FileText className="h-4 w-4 text-gray-500" />
                                    <p className="text-sm font-semibold text-gray-900">
                                        {t('verification.existingDocs')}
                                    </p>
                                </div>
                                <div className="space-y-2">
                                    {existingServerDocs.map((url: string) => (
                                        <ExistingDocRow key={url} url={url} />
                                    ))}
                                </div>
                            </div>
                        )}
                    </div>
                </Card>

                {/* ─── Upload card ─── */}
                <Card className="p-6">
                    <div className="flex items-start gap-3">
                        <div className="rounded-lg bg-[#00b14f]/10 p-2 text-[#00b14f]">
                            <ShieldCheck className="h-5 w-5" />
                        </div>
                        <div>
                            <h2 className="text-lg font-semibold text-gray-900">
                                {t('verification.formTitle')}
                            </h2>
                            <p className="mt-1 text-sm text-gray-500">{t('verification.uploadHint')}</p>
                        </div>
                    </div>

                    <div className="mt-6 space-y-4">
                        {/* Drop zone */}
                        <div
                            className={`relative flex flex-col items-center justify-center rounded-xl border-2 border-dashed px-6 py-10 text-center transition-colors cursor-pointer
                                ${dragOver ? 'border-[#00b14f] bg-[#00b14f]/5' : 'border-gray-300 bg-gray-50 hover:border-[#00b14f]/60 hover:bg-[#00b14f]/5'}`}
                            onDragOver={(e) => { e.preventDefault(); setDragOver(true); }}
                            onDragLeave={() => setDragOver(false)}
                            onDrop={handleDrop}
                            onClick={() => fileInputRef.current?.click()}
                        >
                            <Upload className={`h-9 w-9 mb-3 ${dragOver ? 'text-[#00b14f]' : 'text-gray-400'}`} />
                            <p className="text-sm font-medium text-gray-700">
                                {t('verification.dropZoneTitle')}
                            </p>
                            <p className="mt-1 text-xs text-gray-500">
                                {t('verification.dropZoneHint', { exts: 'JPG, PNG, PDF, DOCX, XLSX…', mb: MAX_SIZE_MB })}
                            </p>
                            <input
                                ref={fileInputRef}
                                type="file"
                                multiple
                                accept={ALLOWED_EXTS.join(',')}
                                className="hidden"
                                onChange={(e) => e.target.files && addFiles(e.target.files)}
                            />
                        </div>

                        {/* Files actively uploading or with errors — uploaded ones move to 'ready' section */}
                        {pendingFiles.some((p) => p.uploading || !!p.error) && (
                            <div className="space-y-2">
                                <p className="text-xs font-medium text-gray-500 uppercase tracking-wide">
                                    {t('verification.pendingFiles')}
                                </p>
                                {pendingFiles
                                    .filter((p) => p.uploading || !!p.error)
                                    .map((pf) => (
                                        <PendingFileRow
                                            key={pf.id}
                                            pf={pf}
                                            onRemove={() => removePending(pf.id)}
                                            onRetry={() => void uploadFile(pf)}
                                        />
                                    ))}
                            </div>
                        )}

                        {/* URLs to be submitted */}
                        {submittedUrls.length > 0 && (
                            <div className="space-y-2">
                                <p className="text-xs font-medium text-gray-500 uppercase tracking-wide">
                                    {t('verification.readyToSubmit')} ({submittedUrls.length})
                                </p>
                                {submittedUrls.map((url) => (
                                    <div
                                        key={url}
                                        className="flex items-center gap-3 rounded-lg border border-gray-200 bg-white px-3 py-2"
                                    >
                                        {isImageUrl(url) ? (
                                            <img
                                                src={url}
                                                alt=""
                                                className="h-9 w-9 rounded object-cover shrink-0 border border-gray-100"
                                            />
                                        ) : (
                                            <FileText className="h-5 w-5 text-[#00b14f] shrink-0" />
                                        )}
                                        <a
                                            href={url}
                                            target="_blank"
                                            rel="noopener noreferrer"
                                            className="flex-1 min-w-0 text-xs text-[#00b14f] hover:underline truncate"
                                        >
                                            {url.split('/').pop()}
                                        </a>
                                        <button
                                            type="button"
                                            onClick={() => removeExistingUrl(url)}
                                            className="text-gray-400 hover:text-red-500 transition-colors shrink-0"
                                        >
                                            <X className="h-4 w-4" />
                                        </button>
                                    </div>
                                ))}
                            </div>
                        )}

                        {/* Actions */}
                        <div className="flex flex-wrap gap-3 pt-2">
                            <Button
                                type="button"
                                isLoading={requestMutation.isPending}
                                disabled={anyUploading || submittedUrls.length === 0}
                                onClick={() => requestMutation.mutate()}
                            >
                                {anyUploading
                                    ? t('verification.uploading')
                                    : t('verification.btnSubmit')}
                            </Button>
                            <Button
                                type="button"
                                variant="outline"
                                disabled={requestMutation.isPending || anyUploading}
                                onClick={() => {
                                    setPendingFiles([]);
                                    setSubmittedUrls(existingServerDocs);
                                }}
                            >
                                {t('verification.btnReset')}
                            </Button>
                        </div>
                    </div>

                    {isApproved && (
                        <div className="mt-6 flex items-start gap-2 rounded-lg bg-green-50 p-3 text-sm text-green-700">
                            <CheckCircle2 className="mt-0.5 h-4 w-4 shrink-0" />
                            <span>{t('verification.alreadyVerifiedHint')}</span>
                        </div>
                    )}
                </Card>
            </div>
        </div>
    );
}

// ─── sub-components ──────────────────────────────────────────────────────────

function PendingFileRow({
    pf,
    onRemove,
    onRetry,
}: {
    pf: PendingFile;
    onRemove: () => void;
    onRetry: () => void;
}) {
    return (
        <div
            className={`flex items-center gap-3 rounded-lg border px-3 py-2 text-sm transition-colors
                ${pf.error ? 'border-red-200 bg-red-50' : 'border-gray-200 bg-white'}`}
        >
            {/* thumbnail / icon */}
            {pf.preview ? (
                <img
                    src={pf.preview}
                    alt=""
                    className="h-9 w-9 rounded object-cover shrink-0 border border-gray-100"
                />
            ) : (
                <div className="h-9 w-9 flex items-center justify-center rounded bg-gray-100 shrink-0">
                    {getFileExt(pf.file.name) === '.pdf' ? (
                        <FileText className="h-5 w-5 text-red-400" />
                    ) : (
                        <ImageIcon className="h-5 w-5 text-blue-400" />
                    )}
                </div>
            )}

            {/* info */}
            <div className="flex-1 min-w-0">
                <p className="truncate text-xs font-medium text-gray-800">{pf.file.name}</p>
                <p className="text-xs text-gray-400">{formatBytes(pf.file.size)}</p>
                {pf.error && (
                    <p className="text-xs text-red-500">
                        {pf.error}{' '}
                        <button
                            className="underline font-medium"
                            onClick={onRetry}
                        >
                            Retry
                        </button>
                    </p>
                )}
            </div>

            {/* status */}
            <div className="shrink-0">
                {pf.uploading && <Loader2 className="h-4 w-4 animate-spin text-[#00b14f]" />}
                {pf.uploaded && <CheckCircle2 className="h-4 w-4 text-green-500" />}
                {!pf.uploading && (
                    <button
                        type="button"
                        onClick={onRemove}
                        className="ml-1 text-gray-400 hover:text-red-500 transition-colors"
                    >
                        <Trash2 className="h-4 w-4" />
                    </button>
                )}
            </div>
        </div>
    );
}

function ExistingDocRow({ url }: { url: string }) {
    const fileName = url.split('/').pop() ?? url;
    const isImg = isImageUrl(url);
    return (
        <a
            href={url}
            target="_blank"
            rel="noopener noreferrer"
            className="flex items-center gap-2 rounded-lg border border-gray-200 bg-white px-3 py-2 hover:border-[#00b14f]/40 transition-colors"
        >
            {isImg ? (
                <img
                    src={url}
                    alt=""
                    className="h-8 w-8 rounded object-cover shrink-0 border border-gray-100"
                />
            ) : (
                <FileText className="h-4 w-4 text-[#00b14f] shrink-0" />
            )}
            <span className="flex-1 min-w-0 truncate text-xs text-[#00b14f]">{fileName}</span>
        </a>
    );
}
