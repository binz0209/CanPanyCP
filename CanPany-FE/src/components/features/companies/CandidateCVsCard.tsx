import { AlertTriangle, FileText } from 'lucide-react';
import { Card } from '../../ui';
import type { CandidateCVSummary } from '../../../api/candidate.api';

interface CandidateCVsCardProps {
    isLoading: boolean;
    accessMessage: string | null;
    cvs: CandidateCVSummary[];
    hasError: boolean;
}

export function CandidateCVsCard({
    isLoading,
    accessMessage,
    cvs,
    hasError,
}: CandidateCVsCardProps) {
    return (
        <>
            <Card className="p-6">
                <div className="flex items-center gap-2 text-gray-900">
                    <FileText className="h-5 w-5" />
                    <h2 className="text-lg font-semibold">Candidate CVs</h2>
                </div>

                {isLoading ? (
                    <div className="mt-4 h-20 animate-pulse rounded-xl bg-gray-100" />
                ) : accessMessage ? (
                    <div className="mt-4 rounded-xl border border-amber-200 bg-amber-50 p-4 text-sm text-amber-800">
                        {accessMessage}
                    </div>
                ) : cvs.length === 0 ? (
                    <div className="mt-4 rounded-xl border border-gray-100 bg-gray-50 p-4 text-sm text-gray-600">
                        Ứng viên chưa có CV khả dụng.
                    </div>
                ) : (
                    <div className="mt-4 space-y-3">
                        {cvs.map((cv) => (
                            <div key={cv.id} className="rounded-xl border border-gray-100 p-4">
                                <p className="font-medium text-gray-900">{cv.fileName}</p>
                                <p className="mt-1 text-sm text-gray-500">
                                    {cv.isDefault ? 'CV mặc định' : 'CV bổ sung'}
                                </p>
                            </div>
                        ))}
                    </div>
                )}
            </Card>

            {hasError && (
                <Card className="border-amber-200 bg-amber-50 p-6">
                    <div className="flex items-start gap-3">
                        <AlertTriangle className="mt-0.5 h-5 w-5 text-amber-700" />
                        <p className="text-sm leading-6 text-amber-800">
                            Luồng review application đã hoạt động, nhưng quyền xem CV hiện còn phụ thuộc backend unlock candidate.
                        </p>
                    </div>
                </Card>
            )}
        </>
    );
}
