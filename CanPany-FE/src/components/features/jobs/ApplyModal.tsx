import { useState, useEffect } from 'react';
import { X, Loader2, CheckCircle, DollarSign, FileText } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { Button } from '../../ui';
import { applicationsApi, cvApi } from '../../../api';
import type { CV } from '../../../types';

interface ApplyModalProps {
    jobId: string;
    jobTitle: string;
    isOpen: boolean;
    onClose: () => void;
    onSuccess: () => void;
}

export function ApplyModal({ jobId, jobTitle, isOpen, onClose, onSuccess }: ApplyModalProps) {
    const { t } = useTranslation('public');
    const [cvs, setCvs] = useState<CV[]>([]);
    const [selectedCvId, setSelectedCvId] = useState<string>('');
    const [coverLetter, setCoverLetter] = useState('');
    const [expectedSalary, setExpectedSalary] = useState('');
    const [isLoadingCvs, setIsLoadingCvs] = useState(false);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [isSuccess, setIsSuccess] = useState(false);

    useEffect(() => {
        if (!isOpen) return;
        setIsLoadingCvs(true);
        cvApi.getCVs()
            .then(data => {
                setCvs(data);
                const defaultCv = data.find(cv => cv.isDefault);
                if (defaultCv) setSelectedCvId(defaultCv.id);
            })
            .catch(() => {})
            .finally(() => setIsLoadingCvs(false));
    }, [isOpen]);

    const handleClose = () => {
        if (isSubmitting) return;
        setCoverLetter('');
        setExpectedSalary('');
        setError(null);
        setIsSuccess(false);
        onClose();
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError(null);
        setIsSubmitting(true);
        try {
            await applicationsApi.apply(
                jobId,
                selectedCvId || undefined,
                coverLetter.trim() || undefined,
                expectedSalary ? Number(expectedSalary) : undefined
            );
            setIsSuccess(true);
            onSuccess();
        } catch (err: unknown) {
            const msg = (err as { response?: { data?: { message?: string } } })
                ?.response?.data?.message;
            setError(msg || t('applyModal.failedError'));
        } finally {
            setIsSubmitting(false);
        }
    };

    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
            <div className="absolute inset-0 bg-black/50" onClick={handleClose} />
            <div className="relative w-full max-w-md rounded-xl bg-white shadow-xl dark:bg-slate-900">
                {/* Header */}
                <div className="flex items-center justify-between border-b border-gray-200 px-6 py-4 dark:border-slate-800">
                    <h2 className="text-lg font-semibold text-gray-900 dark:text-slate-100">{t('applyModal.title')}</h2>
                    <button
                        onClick={handleClose}
                        disabled={isSubmitting}
                        className="rounded-full p-1.5 text-gray-400 hover:bg-gray-100 hover:text-gray-600 disabled:opacity-50 dark:hover:bg-slate-800 dark:hover:text-slate-200"
                    >
                        <X className="h-5 w-5" />
                    </button>
                </div>

                {/* Body */}
                <div className="px-6 py-4">
                    {isSuccess ? (
                        <div className="flex flex-col items-center gap-3 py-6 text-center">
                            <CheckCircle className="h-12 w-12 text-green-500" />
                            <p className="font-semibold text-gray-900 dark:text-slate-100">{t('applyModal.successTitle')}</p>
                            <p className="text-sm text-gray-500 dark:text-slate-400">{t('applyModal.successDesc')}</p>
                            <Button className="mt-2" onClick={handleClose}>{t('applyModal.closeButton')}</Button>
                        </div>
                    ) : (
                        <form onSubmit={handleSubmit} className="space-y-4">
                            <p className="text-sm text-gray-600 dark:text-slate-400">
                                {t('applyModal.jobContext', { jobTitle })}
                            </p>

                            {/* CV Selection */}
                            <div>
                                <label className="mb-1.5 block text-sm font-medium text-gray-700 dark:text-slate-200">
                                    <FileText className="mr-1.5 inline h-4 w-4" />
                                    {t('applyModal.cvLabel')}
                                </label>
                                {isLoadingCvs ? (
                                    <div className="flex items-center gap-2 text-sm text-gray-500 dark:text-slate-400">
                                        <Loader2 className="h-4 w-4 animate-spin" />
                                        {t('applyModal.loadingCVs')}
                                    </div>
                                ) : cvs.length === 0 ? (
                                    <p className="text-sm text-gray-500 dark:text-slate-400">{t('applyModal.noCVs')}</p>
                                ) : (
                                    <select
                                        value={selectedCvId}
                                        onChange={e => setSelectedCvId(e.target.value)}
                                        className="w-full rounded-lg border border-gray-300 bg-white px-3 py-2.5 text-sm text-gray-900 focus:border-[#00b14f] focus:outline-none focus:ring-2 focus:ring-[#00b14f]/20 dark:border-slate-700 dark:bg-slate-800 dark:text-slate-100"
                                    >
                                        <option value="">{t('applyModal.noAttachCV')}</option>
                                        {cvs.map(cv => (
                                            <option key={cv.id} value={cv.id}>
                                                {cv.fileName}{cv.isDefault ? ` ${t('applyModal.defaultSuffix')}` : ''}
                                            </option>
                                        ))}
                                    </select>
                                )}
                            </div>

                            {/* Cover Letter */}
                            <div>
                                <label className="mb-1.5 block text-sm font-medium text-gray-700 dark:text-slate-200">
                                    {t('applyModal.coverLetterLabel')}
                                    <span className="ml-1 text-gray-400">{t('applyModal.coverLetterOptional')}</span>
                                </label>
                                <textarea
                                    value={coverLetter}
                                    onChange={e => setCoverLetter(e.target.value)}
                                    rows={4}
                                    placeholder={t('applyModal.coverLetterPlaceholder')}
                                    className="w-full resize-none rounded-lg border border-gray-300 bg-white px-3 py-2.5 text-sm text-gray-900 placeholder:text-gray-400 focus:border-[#00b14f] focus:outline-none focus:ring-2 focus:ring-[#00b14f]/20 dark:border-slate-700 dark:bg-slate-800 dark:text-slate-100 dark:placeholder:text-slate-500"
                                />
                            </div>

                            {/* Expected Salary */}
                            <div>
                                <label className="mb-1.5 block text-sm font-medium text-gray-700 dark:text-slate-200">
                                    <DollarSign className="mr-1 inline h-4 w-4" />
                                    {t('applyModal.salaryLabel')}
                                    <span className="ml-1 text-gray-400">{t('applyModal.salaryOptional')}</span>
                                </label>
                                <input
                                    type="number"
                                    min={0}
                                    value={expectedSalary}
                                    onChange={e => setExpectedSalary(e.target.value)}
                                    placeholder={t('applyModal.salaryPlaceholder')}
                                    className="w-full rounded-lg border border-gray-300 bg-white px-3 py-2.5 text-sm text-gray-900 placeholder:text-gray-400 focus:border-[#00b14f] focus:outline-none focus:ring-2 focus:ring-[#00b14f]/20 dark:border-slate-700 dark:bg-slate-800 dark:text-slate-100 dark:placeholder:text-slate-500"
                                />
                            </div>

                            {error && (
                                <p className="rounded-lg bg-red-50 px-3 py-2 text-sm text-red-600 dark:bg-red-900/30 dark:text-red-400">
                                    {error}
                                </p>
                            )}

                            {/* Actions */}
                            <div className="flex gap-3 pt-1">
                                <Button type="button" variant="outline" className="flex-1" onClick={handleClose} disabled={isSubmitting}>
                                    {t('applyModal.cancelButton')}
                                </Button>
                                <Button type="submit" className="flex-1" disabled={isSubmitting}>
                                    {isSubmitting ? (
                                        <>
                                            <Loader2 className="h-4 w-4 animate-spin" />
                                            {t('applyModal.submitting')}
                                        </>
                                    ) : (
                                        t('applyModal.submitButton')
                                    )}
                                </Button>
                            </div>
                        </form>
                    )}
                </div>
            </div>
        </div>
    );
}
