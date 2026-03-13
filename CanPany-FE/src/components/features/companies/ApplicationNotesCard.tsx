import { MessageSquareText } from 'lucide-react';
import { Button, Card } from '../../ui';

interface ApplicationNotesCardProps {
    noteDraft: string;
    onNoteDraftChange: (value: string) => void;
    onSubmit: () => void;
    isSubmitting: boolean;
    sessionNotes: string[];
}

export function ApplicationNotesCard({
    noteDraft,
    onNoteDraftChange,
    onSubmit,
    isSubmitting,
    sessionNotes,
}: ApplicationNotesCardProps) {
    return (
        <Card className="p-6">
            <div className="flex items-center gap-2 text-gray-900">
                <MessageSquareText className="h-5 w-5" />
                <h2 className="text-lg font-semibold">Ghi chú nội bộ</h2>
            </div>

            <div className="mt-4 rounded-xl border border-amber-200 bg-amber-50 p-4 text-sm text-amber-800">
                Private note chỉ được lưu tạm thời trong phiên làm việc hiện tại để phục vụ việc review. Nếu cần lưu trữ dài hạn, hãy ghi chú thêm ở hệ thống quản lý nội bộ của công ty.
            </div>

            <div className="mt-4">
                <textarea
                    rows={5}
                    value={noteDraft}
                    onChange={(event) => onNoteDraftChange(event.target.value)}
                    placeholder="Nhập ghi chú nội bộ cho hồ sơ ứng tuyển này"
                    className="w-full rounded-lg border border-gray-300 px-4 py-3 text-sm text-gray-900 outline-none transition focus:border-[#00b14f] focus:ring-2 focus:ring-[#00b14f]/20"
                />
                <div className="mt-3">
                    <Button
                        variant="outline"
                        isLoading={isSubmitting}
                        onClick={onSubmit}
                    >
                        Lưu ghi chú
                    </Button>
                </div>
            </div>

            {sessionNotes.length > 0 && (
                <div className="mt-5 space-y-3">
                    {sessionNotes.map((note, index) => (
                        <div key={`${note}-${index}`} className="rounded-xl border border-gray-100 bg-gray-50 p-4">
                            <p className="text-xs font-semibold uppercase tracking-wide text-gray-500">
                                Ghi chú phiên làm việc
                            </p>
                            <p className="mt-2 whitespace-pre-wrap text-sm leading-6 text-gray-600">{note}</p>
                        </div>
                    ))}
                </div>
            )}
        </Card>
    );
}
