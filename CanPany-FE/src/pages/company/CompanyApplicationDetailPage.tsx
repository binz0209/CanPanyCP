import { useEffect, useMemo, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { isAxiosError } from 'axios';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { FileText, MessageSquare, UserRound } from 'lucide-react';
import toast from 'react-hot-toast';
import { applicationsApi, candidateApi, jobsApi } from '../../api';
import { conversationsApi } from '../../api/conversations.api';
import { Button, Card } from '../../components/ui';
import { companyPaths } from '../../lib/companyNavigation';
import {
    ApplicationNotesCard,
    ApplicationStatusActionsCard,
    CandidateCVsCard,
    CompanyWorkspaceErrorState,
    EmptyState,
    SectionHeader,
    StatusBadge,
} from '../../components/features/companies';
import type { Application } from '../../types';
import { applicationKeys, candidateKeys, companyKeys, conversationKeys } from '../../lib/queryKeys';
import { formatCurrency, formatDateTime } from '../../utils';


function getPersistedPrivateNotes(application: unknown): string[] {
    const source = application as { privateNotes?: string; PrivateNotes?: string } | null | undefined;
    const raw = source?.privateNotes ?? source?.PrivateNotes;
    if (!raw || !raw.trim()) return [];

    // BE appends notes by newline; show newest first for reviewer convenience.
    return raw
        .split('\n')
        .map((note) => note.trim())
        .filter(Boolean)
        .reverse();
}

export function CompanyApplicationDetailPage() {
    const { applicationId } = useParams<{ applicationId: string }>();
    const navigate = useNavigate();
    const queryClient = useQueryClient();
    const [rejectReason, setRejectReason] = useState('');
    const [noteDraft, setNoteDraft] = useState('');
    const [sessionNotes, setSessionNotes] = useState<string[]>([]);
    const sessionNoteStorageKey = applicationId ? `company-application-notes:${applicationId}` : null;

    const applicationQuery = useQuery({
        queryKey: applicationKeys.detail(applicationId!),
        queryFn: () => applicationsApi.getDetails(applicationId!),
        enabled: !!applicationId,
    });

    const candidateProfileQuery = useQuery({
        queryKey: candidateKeys.profile(applicationQuery.data?.candidateId || ''),
        queryFn: () => candidateApi.getCandidateProfile(applicationQuery.data!.candidateId),
        enabled: !!applicationQuery.data?.candidateId,
    });

    const candidateCVsQuery = useQuery({
        queryKey: candidateKeys.cvs(applicationQuery.data?.candidateId || ''),
        queryFn: () => candidateApi.getCandidateCVs(applicationQuery.data!.candidateId),
        enabled: !!applicationQuery.data?.candidateId,
        retry: false,
    });

    const jobQuery = useQuery({
        queryKey: companyKeys.workspaceJobDetail(applicationQuery.data?.jobId || ''),
        queryFn: () => jobsApi.getById(applicationQuery.data!.jobId),
        enabled: !!applicationQuery.data?.jobId,
    });

    // ── Start conversation with the candidate ─────────────────────────────────
    const startConversationMutation = useMutation({
        mutationFn: async () => {
            const candidateId = applicationQuery.data?.candidateId;
            const jobId = applicationQuery.data?.jobId;
            if (!candidateId) throw new Error('Missing candidateId');
            return conversationsApi.getOrCreateConversation(candidateId, jobId);
        },
        onSuccess: (conversation) => {
            // Add the new conversation to the cache so it shows up immediately in the messages list
            queryClient.setQueryData<unknown[]>(conversationKeys.list(), (old) => {
                if (!old) return [conversation];
                // Avoid duplicates
                if (old.some((c: unknown) => (c as { id: string }).id === conversation.id)) {
                    return old;
                }
                return [conversation, ...old];
            });
            navigate(companyPaths.messageThread(conversation.id));
        },
        onError: (error) => {
            const message = isAxiosError(error)
                ? error.response?.data?.message || 'Không thể tạo cuộc trò chuyện'
                : 'Không thể tạo cuộc trò chuyện';
            toast.error(message);
        },
    });

    useEffect(() => {
        if (!sessionNoteStorageKey) return;
        const storedNotes = sessionStorage.getItem(sessionNoteStorageKey);
        if (!storedNotes) return;

        try {
            const parsedNotes = JSON.parse(storedNotes) as string[];
            if (Array.isArray(parsedNotes)) {
                setSessionNotes(parsedNotes);
            }
        } catch {
            sessionStorage.removeItem(sessionNoteStorageKey);
        }
    }, [sessionNoteStorageKey]);

    useEffect(() => {
        if (!sessionNoteStorageKey) return;
        sessionStorage.setItem(sessionNoteStorageKey, JSON.stringify(sessionNotes));
    }, [sessionNotes, sessionNoteStorageKey]);

    const acceptMutation = useMutation({
        mutationFn: () => applicationsApi.accept(applicationId!),
        onSuccess: async () => {
            syncApplicationCaches((application) => ({
                ...application,
                status: 'Accepted',
                rejectedReason: undefined,
            }));
            await Promise.all([
                queryClient.invalidateQueries({ queryKey: applicationKeys.detail(applicationId!), exact: true }),
                applicationQuery.data?.jobId
                    ? queryClient.invalidateQueries({ queryKey: applicationKeys.byJob(applicationQuery.data.jobId), exact: true })
                    : Promise.resolve(),
            ]);
            toast.success('Đã cập nhật trạng thái Accepted');
        },
        onError: (error) => {
            const message = isAxiosError(error)
                ? error.response?.data?.message || 'Không thể accept application'
                : 'Không thể accept application';
            toast.error(message);
        },
    });

    const rejectMutation = useMutation({
        mutationFn: () => applicationsApi.reject(applicationId!, rejectReason.trim()),
        onSuccess: async () => {
            setRejectReason('');
            syncApplicationCaches((application) => ({
                ...application,
                status: 'Rejected',
                rejectedReason: rejectReason.trim(),
            }));
            await Promise.all([
                queryClient.invalidateQueries({ queryKey: applicationKeys.detail(applicationId!), exact: true }),
                applicationQuery.data?.jobId
                    ? queryClient.invalidateQueries({ queryKey: applicationKeys.byJob(applicationQuery.data.jobId), exact: true })
                    : Promise.resolve(),
            ]);
            toast.success('Đã cập nhật trạng thái Rejected');
        },
        onError: (error) => {
            const message = isAxiosError(error)
                ? error.response?.data?.message || 'Không thể reject application'
                : 'Không thể reject application';
            toast.error(message);
        },
    });

    const noteMutation = useMutation({
        mutationFn: () => applicationsApi.addNote(applicationId!, noteDraft.trim()),
        onSuccess: async () => {
            setSessionNotes((previous) => [noteDraft.trim(), ...previous]);
            setNoteDraft('');
            await queryClient.invalidateQueries({ queryKey: applicationKeys.detail(applicationId!), exact: true });
            toast.success('Đã gửi private note');
        },
        onError: (error) => {
            const message = isAxiosError(error)
                ? error.response?.data?.message || 'Không thể lưu private note'
                : 'Không thể lưu private note';
            toast.error(message);
        },
    });

    const cvAccessMessage = useMemo(() => {
        if (!candidateCVsQuery.error) return null;
        if (isAxiosError(candidateCVsQuery.error) && candidateCVsQuery.error.response?.status === 403) {
            return 'Vui lòng mở khóa liên hệ ứng viên trước khi xem danh sách CV.';
        }
        return 'Không thể tải danh sách CV của ứng viên. Vui lòng thử lại sau.';
    }, [candidateCVsQuery.error]);

    const syncApplicationCaches = (updater: (application: Application) => Application) => {
        if (!applicationId) return;

        queryClient.setQueryData<Application>(applicationKeys.detail(applicationId), (currentApplication) =>
            currentApplication ? updater(currentApplication) : currentApplication
        );

        const currentJobId = applicationQuery.data?.jobId;
        if (!currentJobId) return;

        queryClient.setQueryData<Application[]>(applicationKeys.byJob(currentJobId), (currentApplications = []) =>
            currentApplications.map((application) =>
                application.id === applicationId
                    ? updater(application)
                    : application
            )
        );
    };

    if (!applicationId) {
        return (
            <EmptyState
                title="Thiếu applicationId"
                description="Đường dẫn hiện tại chưa có mã application hợp lệ."
                icon={<FileText className="h-6 w-6" />}
            />
        );
    }

    if (applicationQuery.isLoading) {
        return (
            <div className="space-y-4">
                <div className="h-32 animate-pulse rounded-2xl bg-gray-100" />
                <div className="grid gap-4 lg:grid-cols-2">
                    <div className="h-72 animate-pulse rounded-2xl bg-gray-100" />
                    <div className="h-72 animate-pulse rounded-2xl bg-gray-100" />
                </div>
            </div>
        );
    }

    if (applicationQuery.error || !applicationQuery.data) {
        return (
            <CompanyWorkspaceErrorState
                title="Không thể tải chi tiết application"
                description="Vui lòng kiểm tra lại route hoặc thử reload trang."
                icon={<FileText className="h-6 w-6" />}
            />
        );
    }

    const application = applicationQuery.data;
    const candidate = candidateProfileQuery.data;
    const job = jobQuery.data?.job;
    const canReviewStatus = application.status === 'Pending';
    const persistedNotes = getPersistedPrivateNotes(application);
    const notesForDisplay = Array.from(new Set([...sessionNotes, ...persistedNotes]));

    return (
        <div className="space-y-6">
            <SectionHeader
                title="Chi tiết hồ sơ ứng tuyển"
                description="Xem đầy đủ thông tin ứng viên, job liên quan, cover letter và lịch sử xử lý; cập nhật trạng thái hồ sơ và ghi lại private note cho phiên review hiện tại."
                backLink="/company/applications"
                actions={
                    <Button
                        variant="outline"
                        onClick={() => startConversationMutation.mutate()}
                        isLoading={startConversationMutation.isPending}
                        disabled={startConversationMutation.isPending}
                    >
                        <MessageSquare className="h-4 w-4" />
                        Nhắn tin với ứng viên
                    </Button>
                }
            />

            <div className="grid gap-6 xl:grid-cols-[1.1fr_0.9fr]">
                <Card className="p-6">
                    <div className="flex flex-wrap items-center gap-3">
                        <StatusBadge status={application.status} kind="application" />
                        <span className="text-sm text-gray-500">
                            Ứng tuyển lúc {formatDateTime(application.createdAt)}
                        </span>
                    </div>

                    <div className="mt-6 grid gap-4 md:grid-cols-2">
                        <div className="rounded-xl bg-gray-50 p-4">
                            <p className="text-xs font-semibold uppercase tracking-wide text-gray-500">Ứng viên</p>
                            <p className="mt-2 text-lg font-semibold text-gray-900">
                                {candidate?.user.fullName || application.candidateId}
                            </p>
                            <p className="mt-1 text-sm text-gray-500">
                                {candidate?.profile?.title || 'Chưa cập nhật vị trí'}
                            </p>
                            <p className="mt-3 text-sm text-gray-600">
                                {candidate?.profile?.location || 'Chưa cập nhật địa điểm'}
                            </p>
                        </div>

                        <div className="rounded-xl bg-gray-50 p-4">
                            <p className="text-xs font-semibold uppercase tracking-wide text-gray-500">Tin tuyển dụng</p>
                            <p className="mt-2 text-lg font-semibold text-gray-900">
                                {job?.title || application.jobId}
                            </p>
                            <p className="mt-1 text-sm text-gray-500">
                                {job?.location || 'Chưa cập nhật địa điểm'}
                            </p>
                            <p className="mt-3 text-sm text-gray-600">
                                Mức phù hợp: {Math.round(Number(application.matchScore || 0))}%
                            </p>
                        </div>
                    </div>

                    <div className="mt-6 grid gap-4 md:grid-cols-2">
                        <div className="rounded-xl border border-gray-100 p-4">
                            <p className="text-sm font-semibold text-gray-900">Mức lương đề xuất</p>
                            <p className="mt-2 text-sm text-gray-600">
                                {application.proposedAmount
                                    ? formatCurrency(application.proposedAmount)
                                    : 'Ứng viên chưa nhập mức đề xuất'}
                            </p>
                        </div>

                        <div className="rounded-xl border border-gray-100 p-4">
                            <p className="text-sm font-semibold text-gray-900">CV được dùng khi apply</p>
                            <p className="mt-2 text-sm text-gray-600">
                                {application.cvId || 'Ứng viên không chỉ định CV'}
                            </p>
                        </div>
                    </div>

                    <div className="mt-6 rounded-xl border border-gray-100 p-4">
                        <p className="text-sm font-semibold text-gray-900">Thư xin việc</p>
                        <p className="mt-3 whitespace-pre-wrap text-sm leading-6 text-gray-600">
                            {application.coverLetter || 'Ứng viên chưa nhập thư xin việc.'}
                        </p>
                    </div>

                    {application.rejectedReason && (
                        <div className="mt-4 rounded-xl border border-red-100 bg-red-50 p-4">
                            <p className="text-sm font-semibold text-red-700">Lý do từ chối</p>
                            <p className="mt-2 text-sm text-red-700">{application.rejectedReason}</p>
                        </div>
                    )}

                    <div className="mt-6 rounded-xl border border-gray-100 p-4">
                        <div className="flex items-center gap-2 text-gray-900">
                            <UserRound className="h-5 w-5" />
                            <p className="text-sm font-semibold">Hồ sơ tóm tắt</p>
                        </div>
                        <p className="mt-3 text-sm leading-6 text-gray-600">
                            {candidate?.profile?.bio || candidate?.profile?.experience || 'Ứng viên chưa cập nhật mô tả bản thân.'}
                        </p>
                        <div className="mt-4 flex flex-wrap gap-2">
                            {(candidate?.profile?.skillIds || []).slice(0, 10).map((skillId) => (
                                <span
                                    key={skillId}
                                    className="rounded-full bg-[#00b14f]/10 px-3 py-1 text-xs font-semibold text-[#00b14f]"
                                >
                                    {skillId}
                                </span>
                            ))}
                        </div>
                    </div>
                </Card>

                <div className="space-y-6">
                    <ApplicationStatusActionsCard
                        canReviewStatus={canReviewStatus}
                        rejectReason={rejectReason}
                        onRejectReasonChange={setRejectReason}
                        onAccept={() => acceptMutation.mutate()}
                        onReject={() => {
                            if (!rejectReason.trim()) {
                                toast.error('Vui lòng nhập lý do trước khi từ chối.');
                                return;
                            }
                            rejectMutation.mutate();
                        }}
                        isAccepting={acceptMutation.isPending}
                        isRejecting={rejectMutation.isPending}
                    />

                    <ApplicationNotesCard
                        noteDraft={noteDraft}
                        onNoteDraftChange={setNoteDraft}
                        onSubmit={() => {
                            if (!noteDraft.trim()) {
                                toast.error('Vui lòng nhập ghi chú trước khi lưu.');
                                return;
                            }
                            noteMutation.mutate();
                        }}
                        isSubmitting={noteMutation.isPending}
                        sessionNotes={notesForDisplay}
                    />

                    <CandidateCVsCard
                        isLoading={candidateCVsQuery.isLoading}
                        accessMessage={cvAccessMessage}
                        cvs={candidateCVsQuery.data || []}
                        hasError={Boolean(candidateCVsQuery.error)}
                    />
                </div>
            </div>
        </div>
    );
}
