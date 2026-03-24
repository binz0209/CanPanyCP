import { useState, useEffect } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
    Wand2,
    Sparkles,
    FileText,
    ArrowLeft,
    User,
    Briefcase,
    GraduationCap,
    Code2,
    Globe,
    Award,
    CheckCircle2,
    Loader2,
    ExternalLink,
    RefreshCw,
} from 'lucide-react';
import toast from 'react-hot-toast';
import { Link, useNavigate } from 'react-router-dom';
import { Button, Card, CardContent, CardHeader, CardTitle, Badge } from '../../components/ui';
import { cvApi } from '../../api';
import { candidateApi } from '../../api/candidate.api';
import { useAuthStore } from '../../stores/auth.store';
import { useTranslation } from 'react-i18next';

// ─── helpers ───────────────────────────────────────────────────────────────
// Helper to normalize job status - handles both string and integer values
const normalizeStatus = (status: string | number | undefined): string => {
    if (status === undefined || status === null) return 'Unknown';
    if (typeof status === 'number') {
        // JobStatus enum: 0=Pending, 1=Running, 2=Completed, 3=Failed, 4=Cancelled
        const statusMap: Record<number, string> = {
            0: 'Pending',
            1: 'Running',
            2: 'Completed',
            3: 'Failed',
            4: 'Cancelled',
            5: 'Retrying'
        };
        return statusMap[status] ?? String(status);
    }
    return String(status);
};

async function fetchJobStatus(jobId: string) {
    try {
        const token = localStorage.getItem('token');
        const res = await fetch(
            `${import.meta.env.VITE_API_URL}/api/background-jobs/my-jobs/${jobId}`,
            { headers: { Authorization: `Bearer ${token}` } }
        );
        if (!res.ok) return null;
        const body = await res.json();
        const data = body.data ?? body;
        // Normalize status to string
        if (data && data.status !== undefined) {
            data.status = normalizeStatus(data.status);
        }
        return data;
    } catch {
        return null;
    }
}

// ─── section component ──────────────────────────────────────────────────────
function InfoRow({ label, value }: { label: string; value?: string | null }) {
    if (!value) return null;
    return (
        <div className="flex gap-2 text-sm">
            <span className="text-gray-500 dark:text-slate-400 min-w-[110px] flex-shrink-0">{label}:</span>
            <span className="text-gray-800 dark:text-slate-100 break-words">{value}</span>
        </div>
    );
}

function ProfileSection({
    icon,
    title,
    children,
}: {
    icon: React.ReactNode;
    title: string;
    children: React.ReactNode;
}) {
    return (
        <div className="border border-gray-100 dark:border-slate-800 rounded-xl p-5 bg-gray-50/50 dark:bg-slate-900/60">
            <div className="flex items-center gap-2 mb-3">
                <span className="text-[#00b14f]">{icon}</span>
                <h3 className="font-semibold text-gray-800 dark:text-slate-100 text-sm">{title}</h3>
            </div>
            <div className="space-y-2">{children}</div>
        </div>
    );
}

// ─── main page ──────────────────────────────────────────────────────────────
export function AICVPage({ targetJobId }: { targetJobId?: string } = {}) {
    const { t } = useTranslation('candidate');
    const { user } = useAuthStore();
    const navigate = useNavigate();
    const queryClient = useQueryClient();
    const [activeJobId, setActiveJobId] = useState<string | null>(null);
    const [generationDone, setGenerationDone] = useState(false);
    const [generatedCVId, setGeneratedCVId] = useState<string | null>(null);

    // Load user profile
    const { data: profileData, isLoading: profileLoading } = useQuery({
        queryKey: ['candidate-profile', user?.id],
        queryFn: () => candidateApi.getCandidateProfile(user!.id),
        enabled: !!user?.id,
    });

    const profile = profileData?.profile;

    // Poll job status
    const { data: jobStatus } = useQuery({
        queryKey: ['job-status-cv-gen', activeJobId],
        queryFn: () => fetchJobStatus(activeJobId!),
        enabled: !!activeJobId && !generationDone,
        refetchInterval: (query) => {
            const d = query.state?.data;
            if (!d) return 2000;
            const done =
                d.status === 'Completed' ||
                d.status === 'Failed' ||
                d.status === 'Cancelled';
            if (done) return false;
            return 2000;
        },
    });

    // Watch for completion
    useEffect(() => {
        if (jobStatus?.status === 'Completed' && !generationDone) {
            setGenerationDone(true);
            const cvId = jobStatus?.result?.CVId || jobStatus?.result?.cvId;
            if (cvId) setGeneratedCVId(String(cvId));
            queryClient.invalidateQueries({ queryKey: ['candidate-cvs'] });
            toast.success(t('cv.ai.toast.completed'));
        }
    }, [jobStatus, generationDone, queryClient]);

    // Generate mutation
    const generateMutation = useMutation({
        mutationFn: () => cvApi.generateCV(targetJobId),
        onSuccess: (data) => {
            const id = data?.jobId;
            if (id) {
                setActiveJobId(id);
                setGenerationDone(false);
                setGeneratedCVId(null);
                toast.success(t('cv.ai.toast.started'));
            }
        },
        onError: () => {
            toast.error(t('cv.ai.toast.error'));
        },
    });

    const isGenerating =
        generateMutation.isPending ||
        (!!activeJobId &&
            !generationDone &&
            jobStatus?.status !== 'Failed');

    const handleGenerate = () => {
        if (isGenerating) return;
        generateMutation.mutate();
    };

    const handleRetry = () => {
        setActiveJobId(null);
        setGenerationDone(false);
        setGeneratedCVId(null);
        generateMutation.reset();
    };

    const skills = profile?.skillIds ?? [];
    const languages = profile?.languages ?? [];
    const certifications = profile?.certifications ?? [];

    const profileHasInfo = !!(
        profile?.title ||
        profile?.bio ||
        profile?.experience ||
        profile?.education ||
        skills.length > 0
    );

    return (
        <div className="min-h-screen bg-gradient-to-br from-gray-50 to-green-50/30 dark:from-slate-950 dark:to-slate-900">
            {/* Header */}
            <div className="bg-white dark:bg-slate-900 border-b border-gray-200 dark:border-slate-800 shadow-sm">
                <div className="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
                    <div className="flex items-center gap-3 mb-3">
                        <Link to="/candidate/cv/list">
                            <Button variant="ghost" size="sm" className="text-gray-500 hover:text-gray-700 dark:text-slate-300 dark:hover:text-white">
                                <ArrowLeft className="h-4 w-4 mr-1" />
                                {t('cv.ai.back')}
                            </Button>
                        </Link>
                    </div>
                    <div className="flex flex-col sm:flex-row sm:items-center gap-4">
                        <div className="flex items-center gap-3">
                            <div className="h-12 w-12 rounded-xl bg-gradient-to-br from-[#00b14f] to-[#00cc5e] flex items-center justify-center shadow-md">
                                <Wand2 className="h-6 w-6 text-white" />
                            </div>
                            <div>
                                <h1 className="text-2xl font-bold text-gray-900 dark:text-white">{t('cv.ai.title')}</h1>
                                <p className="text-sm text-gray-500 dark:text-slate-400">
                                    {t('cv.ai.subtitle')}
                                </p>
                            </div>
                        </div>
                        <div className="sm:ml-auto">
                            <Badge variant="secondary" className="text-xs bg-green-100 text-green-800 border-green-200 dark:bg-green-900/30 dark:text-green-100 dark:border-green-800">
                                <Sparkles className="h-3 w-3 mr-1" />
                                {t('cv.ai.badge')}
                            </Badge>
                        </div>
                    </div>
                </div>
            </div>

            <div className="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-6">

                {/* Warning if profile is incomplete */}
                {!profileLoading && !profileHasInfo && (
                    <div className="bg-amber-50 dark:bg-amber-900/30 border border-amber-200 dark:border-amber-800 rounded-xl p-4 flex items-start gap-3">
                        <span className="text-amber-500 dark:text-amber-300 text-xl mt-0.5">⚠️</span>
                        <div>
                            <p className="font-medium text-amber-800 dark:text-amber-100 text-sm">{t('cv.ai.warning.title')}</p>
                            <p className="text-amber-700 dark:text-amber-200 text-xs mt-0.5">
                                {t('cv.ai.warning.description')}{' '}
                                <Link to="/candidate/profile" className="underline font-medium dark:text-amber-100">
                                    {t('cv.ai.warning.cta')}
                                </Link>
                            </p>
                        </div>
                    </div>
                )}

                <div className="grid grid-cols-1 lg:grid-cols-5 gap-6">
                    {/* Left – Profile Preview */}
                    <div className="lg:col-span-3 space-y-4">
                        <Card className="dark:bg-slate-900 dark:border-slate-800">
                            <CardHeader className="pb-3">
                                <CardTitle className="text-base flex items-center gap-2 dark:text-white">
                                    <User className="h-4 w-4 text-[#00b14f]" />
                                    {t('cv.ai.profile.title')}
                                </CardTitle>
                                <p className="text-xs text-gray-500 dark:text-slate-400 mt-0.5">
                                    {t('cv.ai.profile.subtitle')}{' '}
                                    <Link to="/candidate/profile" className="text-[#00b14f] underline">
                                        {t('cv.ai.profile.editProfile')}
                                    </Link>
                                </p>
                            </CardHeader>
                            <CardContent className="space-y-4">
                                {profileLoading ? (
                                    <div className="flex justify-center py-8">
                                        <Loader2 className="h-8 w-8 text-[#00b14f] animate-spin" />
                                    </div>
                                ) : (
                                    <>
                                        {/* Personal Info */}
                                        <ProfileSection
                                            icon={<User className="h-4 w-4" />}
                                            title={t('cv.ai.sections.personal')}
                                        >
                                            <InfoRow label={t('cv.ai.fields.name')} value={user?.fullName} />
                                            <InfoRow label={t('cv.ai.fields.email')} value={user?.email} />
                                            <InfoRow label={t('cv.ai.fields.phone')} value={profile?.phone} />
                                            <InfoRow label={t('cv.ai.fields.address')} value={profile?.location || profile?.address} />
                                            <InfoRow label={t('cv.ai.fields.title')} value={profile?.title} />
                                            {profile?.bio && (
                                                <div className="text-sm">
                                                    <span className="text-gray-500 dark:text-slate-400 block mb-1">{t('cv.ai.fields.bio')}:</span>
                                                    <p className="text-gray-800 dark:text-slate-100 text-xs leading-relaxed line-clamp-3">
                                                        {profile.bio}
                                                    </p>
                                                </div>
                                            )}
                                        </ProfileSection>

                                        {/* Experience */}
                                        <ProfileSection
                                            icon={<Briefcase className="h-4 w-4" />}
                                            title={t('cv.ai.sections.experience')}
                                        >
                                            {profile?.experience ? (
                                                <p className="text-sm text-gray-800 dark:text-slate-100 whitespace-pre-line line-clamp-4">
                                                    {profile.experience}
                                                </p>
                                            ) : (
                                                <p className="text-sm text-gray-400 dark:text-slate-500 italic">{t('cv.ai.empty')}</p>
                                            )}
                                        </ProfileSection>

                                        {/* Education */}
                                        <ProfileSection
                                            icon={<GraduationCap className="h-4 w-4" />}
                                            title={t('cv.ai.sections.education')}
                                        >
                                            {profile?.education ? (
                                                <p className="text-sm text-gray-800 dark:text-slate-100 whitespace-pre-line line-clamp-3">
                                                    {profile.education}
                                                </p>
                                            ) : (
                                                <p className="text-sm text-gray-400 dark:text-slate-500 italic">{t('cv.ai.empty')}</p>
                                            )}
                                        </ProfileSection>

                                        {/* Skills */}
                                        <ProfileSection
                                            icon={<Code2 className="h-4 w-4" />}
                                            title={t('cv.ai.sections.skills')}
                                        >
                                            {skills.length > 0 ? (
                                                <div className="flex flex-wrap gap-1.5">
                                                    {skills.map((s, i) => (
                                                        <Badge key={i} variant="secondary" className="text-xs dark:bg-slate-800 dark:text-slate-100 dark:border-slate-700">
                                                            {s}
                                                        </Badge>
                                                    ))}
                                                </div>
                                            ) : (
                                                <p className="text-sm text-gray-400 dark:text-slate-500 italic">{t('cv.ai.emptySkills')}</p>
                                            )}
                                        </ProfileSection>

                                        {/* Languages & Certs */}
                                        {(languages.length > 0 || certifications.length > 0) && (
                                            <div className="grid grid-cols-2 gap-4">
                                                {languages.length > 0 && (
                                                    <ProfileSection
                                                        icon={<Globe className="h-4 w-4" />}
                                                        title={t('cv.ai.sections.languages')}
                                                    >
                                                        <div className="flex flex-wrap gap-1">
                                                            {languages.map((l, i) => (
                                                                <Badge key={i} variant="secondary" className="text-xs dark:bg-slate-800 dark:text-slate-100 dark:border-slate-700">
                                                                    {l}
                                                                </Badge>
                                                            ))}
                                                        </div>
                                                    </ProfileSection>
                                                )}
                                                {certifications.length > 0 && (
                                                    <ProfileSection
                                                        icon={<Award className="h-4 w-4" />}
                                                        title={t('cv.ai.sections.certifications')}
                                                    >
                                                        <div className="flex flex-col gap-1">
                                                            {certifications.map((c, i) => (
                                                                <span key={i} className="text-xs text-gray-700 dark:text-slate-200">
                                                                    • {c}
                                                                </span>
                                                            ))}
                                                        </div>
                                                    </ProfileSection>
                                                )}
                                            </div>
                                        )}

                                        {/* Links */}
                                        {(profile?.linkedInUrl ||
                                            profile?.gitHubUrl ||
                                            profile?.portfolio) && (
                                                <ProfileSection
                                                    icon={<ExternalLink className="h-4 w-4" />}
                                                    title={t('cv.ai.sections.links')}
                                                >
                                                    <InfoRow label="LinkedIn" value={profile?.linkedInUrl} />
                                                    <InfoRow label="GitHub" value={profile?.gitHubUrl} />
                                                    <InfoRow label={t('cv.ai.fields.portfolio')} value={profile?.portfolio} />
                                                </ProfileSection>
                                            )}
                                    </>
                                )}
                            </CardContent>
                        </Card>
                    </div>

                    {/* Right – Generate Panel */}
                    <div className="lg:col-span-2 space-y-4">
                        {/* Main CTA Card */}
                        <Card className="overflow-hidden dark:bg-slate-900 dark:border-slate-800">
                            <div className="bg-gradient-to-br from-[#00b14f] to-[#007c37] p-6 text-white">
                                <div className="flex items-center gap-2 mb-3">
                                    <Sparkles className="h-5 w-5" />
                                    <span className="font-semibold">{t('cv.ai.generate.title')}</span>
                                </div>
                                <p className="text-sm text-green-100 leading-relaxed">
                                    {t('cv.ai.generate.subtitle')}
                                </p>
                            </div>
                            <CardContent className="p-5 dark:bg-slate-900">
                                {/* Progress Indicator */}
                                {activeJobId && !generationDone && jobStatus && (
                                    <div className="mb-5 space-y-2">
                                        <div className="flex justify-between text-xs text-gray-600 dark:text-slate-300">
                                            <span>{jobStatus.currentStep || t('cv.ai.progress.pending')}</span>
                                            <span className="font-medium">
                                                {jobStatus?.percentComplete ?? 0}%
                                            </span>
                                        </div>
                                        <div className="w-full bg-gray-200 dark:bg-slate-800 rounded-full h-2 overflow-hidden">
                                            <div
                                                className="bg-gradient-to-r from-[#00b14f] to-[#00cc5e] dark:from-emerald-400 dark:to-emerald-300 h-2 rounded-full transition-all duration-500"
                                                style={{
                                                    width: `${jobStatus?.percentComplete ?? 0}%`,
                                                }}
                                            />
                                        </div>
                                        <p className="text-xs text-gray-400 dark:text-slate-500">
                                            {t('cv.ai.progress.wait')}
                                        </p>
                                    </div>
                                )}

                                {/* Error state */}
                                {jobStatus?.status === 'Failed' && (
                                    <div className="mb-4 bg-red-50 dark:bg-red-900/30 border border-red-200 dark:border-red-800 rounded-lg p-3">
                                        <p className="text-sm text-red-700 dark:text-red-200 font-medium">{t('cv.ai.progress.failed')}</p>
                                        <p className="text-xs text-red-500 dark:text-red-200/80 mt-1">
                                            {jobStatus.errorMessage || t('cv.ai.progress.unknown')}
                                        </p>
                                    </div>
                                )}

                                {/* Success state */}
                                {generationDone && (
                                    <div className="mb-4 bg-green-50 dark:bg-green-900/30 border border-green-200 dark:border-green-800 rounded-xl p-4 space-y-3">
                                        <div className="flex items-center gap-2 text-green-700 dark:text-green-200">
                                            <CheckCircle2 className="h-5 w-5" />
                                            <span className="font-semibold text-sm">{t('cv.ai.progress.success')}</span>
                                        </div>
                                        <p className="text-xs text-green-600 dark:text-green-200">
                                            {t('cv.ai.progress.saved')}
                                        </p>
                                        <div className="flex flex-col gap-2">
                                            {generatedCVId ? (
                                                <>
                                                    <Button
                                                        size="sm"
                                                        className="bg-[#00b14f] hover:bg-[#00a045] w-full"
                                                        onClick={() => navigate(`/candidate/cv/editor/${generatedCVId}`)}
                                                    >
                                                        <ExternalLink className="h-3.5 w-3.5 mr-1.5" />
                                                        {t('cv.ai.progress.openEditor')}
                                                    </Button>
                                                    <Button
                                                        size="sm"
                                                        variant="outline"
                                                        className="w-full"
                                                        onClick={() => navigate(`/candidate/cv/editor/${generatedCVId}?download=1`)}
                                                    >
                                                        {t('cv.ai.progress.downloadPdf')}
                                                    </Button>
                                                </>
                                            ) : (
                                                <>
                                                    <Link to="/candidate/cv/list" className="w-full">
                                                        <Button variant="outline" size="sm" className="w-full">
                                                            <FileText className="h-3.5 w-3.5 mr-1.5" />
                                                            {t('cv.ai.progress.viewList')}
                                                        </Button>
                                                    </Link>
                                                    <Button
                                                        variant="ghost"
                                                        size="sm"
                                                        className="w-full text-gray-500 dark:text-slate-300"
                                                        onClick={handleRetry}
                                                    >
                                                        <RefreshCw className="h-3.5 w-3.5 mr-1.5" />
                                                        {t('cv.ai.progress.retry')}
                                                    </Button>
                                                </>
                                            )}
                                        </div>
                                    </div>
                                )}

                                {/* Generate button */}
                                {!generationDone && (
                                    <Button
                                        className="w-full bg-[#00b14f] hover:bg-[#00a045] h-12 text-base font-semibold shadow-md hover:shadow-lg transition-all"
                                        onClick={handleGenerate}
                                        disabled={isGenerating || profileLoading}
                                    >
                                        {isGenerating ? (
                                            <>
                                                <Loader2 className="h-5 w-5 mr-2 animate-spin" />
                                                {t('cv.ai.generate.generating')}
                                            </>
                                        ) : (
                                            <>
                                                <Wand2 className="h-5 w-5 mr-2" />
                                                {t('cv.ai.generate.cta')}
                                            </>
                                        )}
                                    </Button>
                                )}
                            </CardContent>
                        </Card>

                        {/* How it works */}
                        <Card className="border-dashed border-gray-200 dark:border-slate-800 dark:bg-slate-900">
                            <CardHeader className="pb-3">
                                <CardTitle className="text-sm text-gray-700 dark:text-slate-200">{t('cv.ai.howItWorks.title')}</CardTitle>
                            </CardHeader>
                            <CardContent className="space-y-3">
                                {[
                                    {
                                        step: '1',
                                        label: t('cv.ai.howItWorks.steps.1.label'),
                                        desc: t('cv.ai.howItWorks.steps.1.desc'),
                                    },
                                    {
                                        step: '2',
                                        label: t('cv.ai.howItWorks.steps.2.label'),
                                        desc: t('cv.ai.howItWorks.steps.2.desc'),
                                    },
                                    {
                                        step: '3',
                                        label: t('cv.ai.howItWorks.steps.3.label'),
                                        desc: t('cv.ai.howItWorks.steps.3.desc'),
                                    },
                                ].map(({ step, label, desc }) => (
                                    <div key={step} className="flex items-start gap-3">
                                        <div className="h-6 w-6 rounded-full bg-[#00b14f]/10 dark:bg-emerald-500/10 text-[#00b14f] dark:text-emerald-200 flex items-center justify-center text-xs font-bold flex-shrink-0 mt-0.5">
                                            {step}
                                        </div>
                                        <div>
                                            <p className="text-sm font-medium text-gray-800 dark:text-slate-100">{label}</p>
                                            <p className="text-xs text-gray-500 dark:text-slate-400">{desc}</p>
                                        </div>
                                    </div>
                                ))}
                            </CardContent>
                        </Card>

                        {/* Tip */}
                        <div className="bg-blue-50 dark:bg-blue-900/30 border border-blue-100 dark:border-blue-800 rounded-xl p-4 text-sm">
                            <p className="font-medium text-blue-800 dark:text-blue-200 mb-1">{t('cv.ai.tip.title')}</p>
                            <p className="text-blue-700 dark:text-blue-200 text-xs leading-relaxed">
                                {t('cv.ai.tip.description')}{' '}
                                <Link to="/candidate/profile" className="underline dark:text-blue-100">
                                    {t('cv.ai.tip.cta')}
                                </Link>
                            </p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}
