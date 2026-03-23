import { useEffect, useMemo, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { isAxiosError } from 'axios';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { FileText, MessageSquare, UserRound } from 'lucide-react';
import toast from 'react-hot-toast';
import { applicationsApi, candidateApi, jobsApi } from '../../api';
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
import { applicationKeys, candidateKeys, companyKeys } from '../../lib/queryKeys';
import { formatCurrency, formatDateTime } from '../../utils';
import { useTranslation } from 'react-i18next';

export function CompanyApplicationDetailPage() {
    const { applicationId } = useParams<{ applicationId: string }>();
    const queryClient = useQueryClient();
    const { t } = useTranslation('company');
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
            toast.success(t('applicationDetail.toastAccepted'));
        },
        onError: (error) => {
            const message = isAxiosError(error)
                ? error.response?.data?.message || t('applicationDetail.toastAcceptFailed')
                : t('applicationDetail.toastAcceptFailed');
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
            toast.success(t('applicationDetail.toastRejected'));
        },
        onError: (error) => {
            const message = isAxiosError(error)
                ? error.response?.data?.message || t('applicationDetail.toastRejectFailed')
                : t('applicationDetail.toastRejectFailed');
            toast.error(message);
        },
    });

    const noteMutation = useMutation({
        mutationFn: () => applicationsApi.addNote(applicationId!, noteDraft.trim()),
        onSuccess: () => {
            setSessionNotes((previous) => [noteDraft.trim(), ...previous]);
            setNoteDraft('');
            toast.success(t('applicationDetail.toastNoteSaved'));
        },
        onError: (error) => {
            const message = isAxiosError(error)
                ? error.response?.data?.message || t('applicationDetail.toastNoteFailed')
                : t('applicationDetail.toastNoteFailed');
            toast.error(message);
        },
    });

    const cvAccessMessage = useMemo(() => {
        if (!candidateCVsQuery.error) return null;
        if (isAxiosError(candidateCVsQuery.error) && candidateCVsQuery.error.response?.status === 403) {
            return t('applicationDetail.toastUnlockCVFirst');
        }
        return t('applicationDetail.toastLoadCVFailed');
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
                title={t('applicationDetail.missingIdTitle')}
                description={t('applicationDetail.missingIdDesc')}
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
                title={t('applicationDetail.errorTitle')}
                description={t('applicationDetail.errorDesc')}
                icon={<FileText className="h-6 w-6" />}
            />
        );
    }

    const application = applicationQuery.data;
    const candidate = candidateProfileQuery.data;
    const job = jobQuery.data?.job;
    const canReviewStatus = application.status === 'Pending';
    // UC-36 (private note) hiện BE đang TODO/chưa persist. Tạm ẩn UI note để tránh hiểu nhầm.
    const enableNotes = false;

    // Build the messaging URL using the application's candidateId as a conversation
    // routing key.  The full conversationId comes from the server; for now we
    // navigate to the messages page with an identifier the company can use.
    // Replace with a real conversationId once the BE exposes a conversations endpoint.
    const messagingPath = companyPaths.messageThread(application.candidateId);

    return (
        <div className="space-y-6">
            <SectionHeader
                title={t('applicationDetail.title')}
                description={t('applicationDetail.description')}
                backLink="/company/applications"
                actions={
                    <Link to={messagingPath}>
                        <Button variant="outline">
                            <MessageSquare className="h-4 w-4" />
                            {t('applicationDetail.btnMessage')}
                        </Button>
                    </Link>
                }
            />

            <div className="grid gap-6 xl:grid-cols-[1.1fr_0.9fr]">
                <Card className="p-6">
                    <div className="flex flex-wrap items-center gap-3">
                        <StatusBadge status={application.status} kind="application" />
                        <span className="text-sm text-gray-500">
                            {t('applicationDetail.appliedAt', { datetime: formatDateTime(application.createdAt) })}
                        </span>
                    </div>

                    <div className="mt-6 grid gap-4 md:grid-cols-2">
                        <div className="rounded-xl bg-gray-50 p-4">
                            <p className="text-xs font-semibold uppercase tracking-wide text-gray-500">{t('applicationDetail.labelCandidate')}</p>
                            <p className="mt-2 text-lg font-semibold text-gray-900">
                                {candidate?.user.fullName || application.candidateId}
                            </p>
                            <p className="mt-1 text-sm text-gray-500">
                                {candidate?.profile?.title || t('candidateSearch.positionPlaceholder')}
                            </p>
                            <p className="mt-3 text-sm text-gray-600">
                                {candidate?.profile?.location || t('applicationDetail.noLocation')}
                            </p>
                        </div>

                        <div className="rounded-xl bg-gray-50 p-4">
                            <p className="text-xs font-semibold uppercase tracking-wide text-gray-500">{t('applicationDetail.labelJob')}</p>
                            <p className="mt-2 text-lg font-semibold text-gray-900">
                                {job?.title || application.jobId}
                            </p>
                            <p className="mt-1 text-sm text-gray-500">
                                {job?.location || t('applicationDetail.noJobLocation')}
                            </p>
                            <p className="mt-3 text-sm text-gray-600">
                                {t('applicationDetail.matchScore', { score: Math.round(Number(application.matchScore || 0)) })}
                            </p>
                        </div>
                    </div>

                    <div className="mt-6 grid gap-4 md:grid-cols-2">
                        <div className="rounded-xl border border-gray-100 p-4">
                            <p className="text-sm font-semibold text-gray-900">{t('applicationDetail.salaryLabel')}</p>
                            <p className="mt-2 text-sm text-gray-600">
                                {application.proposedAmount
                                    ? formatCurrency(application.proposedAmount)
                                    : t('applicationDetail.noSalary')}
                            </p>
                        </div>

                        <div className="rounded-xl border border-gray-100 p-4">
                            <p className="text-sm font-semibold text-gray-900">{t('applicationDetail.cvLabel')}</p>
                            <p className="mt-2 text-sm text-gray-600">
                                {application.cvId || t('applicationDetail.noCV')}
                            </p>
                        </div>
                    </div>

                    <div className="mt-6 rounded-xl border border-gray-100 p-4">
                        <p className="text-sm font-semibold text-gray-900">{t('applicationDetail.coverLetterLabel')}</p>
                        <p className="mt-3 whitespace-pre-wrap text-sm leading-6 text-gray-600">
                            {application.coverLetter || t('applicationDetail.noCoverLetter')}
                        </p>
                    </div>

                    {application.rejectedReason && (
                        <div className="mt-4 rounded-xl border border-red-100 bg-red-50 p-4">
                            <p className="text-sm font-semibold text-red-700">{t('applicationDetail.rejectReasonLabel')}</p>
                            <p className="mt-2 text-sm text-red-700">{application.rejectedReason}</p>
                        </div>
                    )}

                    <div className="mt-6 rounded-xl border border-gray-100 p-4">
                        <div className="flex items-center gap-2 text-gray-900">
                            <UserRound className="h-5 w-5" />
                            <p className="text-sm font-semibold">{t('applicationDetail.profileSummary')}</p>
                        </div>
                        <p className="mt-3 text-sm leading-6 text-gray-600">
                            {candidate?.profile?.bio || candidate?.profile?.experience || t('applicationDetail.noProfileSummary')}
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
                                toast.error(t('applicationDetail.toastEnterRejectReason'));
                                return;
                            }
                            rejectMutation.mutate();
                        }}
                        isAccepting={acceptMutation.isPending}
                        isRejecting={rejectMutation.isPending}
                    />

                    {enableNotes && (
                        <ApplicationNotesCard
                            noteDraft={noteDraft}
                            onNoteDraftChange={setNoteDraft}
                            onSubmit={() => {
                                if (!noteDraft.trim()) {
                                    toast.error(t('applicationDetail.toastEnterNote'));
                                    return;
                                }
                                noteMutation.mutate();
                            }}
                            isSubmitting={noteMutation.isPending}
                            sessionNotes={sessionNotes}
                        />
                    )}

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
