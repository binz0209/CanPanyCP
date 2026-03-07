import { useState, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { User, Edit, Save, X, Calendar, MapPin, Phone, Link as LinkIcon, Github, Linkedin, Mail, Briefcase, GraduationCap, ExternalLink, RefreshCw } from 'lucide-react';
import { Button, Card } from '../../components/ui';
import { candidateApi, authApi } from '../../api';
import { useAuthStore } from '../../stores/auth.store';
import type { UserProfile } from '../../types';
import toast from 'react-hot-toast';

export function CandidateProfilePage() {
    const [isEditing, setIsEditing] = useState(false);
    const [formData, setFormData] = useState<Partial<UserProfile>>({});
    const [showRepoModal, setShowRepoModal] = useState(false);
    const [selectedRepos, setSelectedRepos] = useState<string[]>([]);
    const [syncJobId, setSyncJobId] = useState<string | null>(null);
    const [isOAuthLoading, setIsOAuthLoading] = useState(false);
    const [showLinkedInSync, setShowLinkedInSync] = useState(false);
    const [linkedInData, setLinkedInData] = useState('');

    const queryClient = useQueryClient();
    const { user: currentUser, isAuthenticated } = useAuthStore();
    const [searchParams, setSearchParams] = useSearchParams();

    const userId = currentUser?.id;

    // Handle GitHub OAuth callback — BE redirects here with ?github_linked=true|false
    useEffect(() => {
        const githubLinked = searchParams.get('github_linked');
        const githubUsername = searchParams.get('github_username');
        const error = searchParams.get('error');
        if (githubLinked === 'true') {
            toast.success(`Đã liên kết GitHub thành công! (@${githubUsername})`);
            queryClient.invalidateQueries({ queryKey: ['candidate-profile'] });
            setSearchParams({}, { replace: true });
        } else if (githubLinked === 'false') {
            toast.error(`Liên kết GitHub thất bại: ${error || 'Unknown error'}`);
            setSearchParams({}, { replace: true });
        }
    // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    // Redirect if not authenticated
    if (!isAuthenticated || !userId) {
        return (
            <div className="min-h-screen bg-gray-50 flex items-center justify-center">
                <div className="text-center">
                    <p className="text-lg text-gray-600 mb-4">Vui lòng đăng nhập để xem profile</p>
                    <a href="/auth/login" className="text-blue-600 hover:underline">Đăng nhập</a>
                </div>
            </div>
        );
    }

    const { data: profileData, isLoading } = useQuery({
        queryKey: ['candidate-profile', userId],
        queryFn: () => candidateApi.getCandidateProfile(userId),
        enabled: !!userId,
    });

    const updateMutation = useMutation({
        mutationFn: (data: Partial<UserProfile>) => candidateApi.updateProfile(data),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['candidate-profile', userId] });
            setIsEditing(false);
        },
    });

    const syncLinkedInMutation = useMutation({
        mutationFn: (data: string) => candidateApi.syncLinkedInProfile(data),
        onSuccess: () => {
            toast.success('LinkedIn profile synced successfully!');
            queryClient.invalidateQueries({ queryKey: ['candidate-profile', userId] });
            setShowLinkedInSync(false);
            setLinkedInData('');
        },
        onError: (error: any) => {
            toast.error(error.response?.data?.message || 'Failed to sync LinkedIn profile');
        },
    });

    const { data: reposData, isLoading: isLoadingRepos } = useQuery({
        queryKey: ['github-repos'],
        queryFn: () => candidateApi.getGitHubRepos(),
        enabled: showRepoModal,
        staleTime: 60_000,
    });

    const syncSkillsMutation = useMutation({
        mutationFn: (repos: string[]) => candidateApi.syncSkillsFromRepos(repos),
        onSuccess: (data) => {
            setSyncJobId(data.jobId);
            setShowRepoModal(false);
            setSelectedRepos([]);
            toast.success('Đang phân tích repositories và trích xuất skills...');
        },
        onError: () => {
            toast.error('Không thể bắt đầu phân tích. Vui lòng thử lại.');
        },
    });

    const { data: jobStatus } = useQuery({
        queryKey: ['github-job-status', syncJobId],
        queryFn: () => candidateApi.getGitHubJobStatus(syncJobId!),
        enabled: !!syncJobId,
        refetchInterval: (query) => {
            const s = query.state.data?.status;
            if (s === 'Completed' || s === 'Failed') return false;
            return 3000;
        },
    });

    useEffect(() => {
        if (jobStatus?.status === 'Completed') {
            toast.success('Trích xuất skills hoàn thành! Hồ sơ đã được cập nhật.');
            queryClient.invalidateQueries({ queryKey: ['candidate-profile', userId] });
            setSyncJobId(null);
        } else if (jobStatus?.status === 'Failed') {
            toast.error('Phân tích thất bại. Vui lòng thử lại.');
            setSyncJobId(null);
        }
    }, [jobStatus?.status]); // eslint-disable-line react-hooks/exhaustive-deps

    const handleGitHubOAuth = async () => {
        setIsOAuthLoading(true);
        try {
            const { oauthUrl } = await authApi.getGitHubLinkUrl();
            window.location.href = oauthUrl;
        } catch {
            toast.error('Không thể tạo GitHub OAuth URL. Vui lòng thử lại.');
            setIsOAuthLoading(false);
        }
    };

    if (isLoading) {
        return (
            <div className="min-h-screen bg-gray-50 flex items-center justify-center">
                <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-blue-600"></div>
            </div>
        );
    }

    if (!profileData || !profileData.user) {
        return (
            <div className="min-h-screen bg-gray-50 flex items-center justify-center">
                <p>Không tìm thấy profile</p>
            </div>
        );
    }

    const { user, profile } = profileData;


    const handleEdit = () => {
        setFormData({ ...(profile || {}) });
        setIsEditing(true);
    };

    const handleSave = () => {
        updateMutation.mutate(formData);
    };

    const handleCancel = () => {
        setIsEditing(false);
        setFormData({});
    };

    const handleChange = (field: keyof UserProfile, value: any) => {
        setFormData(prev => ({ ...prev, [field]: value }));
    };

    // Calculate profile completion percentage
    const calculateProfileCompletion = () => {
        if (!profile) return 0;

        const fields = [
            profile.title,
            profile.bio,
            profile.phone,
            profile.address,
            profile.dateOfBirth,
            profile.skillIds?.length > 0,
            profile.experience,
            profile.education,
            profile.linkedInUrl,
            profile.gitHubUrl
        ];

        const filledFields = fields.filter(field =>
            field !== undefined && field !== null && field !== '' && field !== false
        ).length;

        return Math.round((filledFields / fields.length) * 100);
    };

    return (
        <div>
            {/* Hero Section */}
            <section className="relative overflow-hidden bg-gradient-to-br from-[#00b14f] via-[#00a045] to-[#008f3c]">
                <div className="relative mx-auto max-w-7xl px-4 py-16 sm:px-6 lg:px-8 lg:py-24">
                    <div className="text-center">
                        <h2 className="text-4xl font-bold tracking-tight text-white sm:text-5xl lg:text-6xl">
                            Hồ sơ cá nhân
                        </h2>
                        <p className="mt-6 max-w-xl mx-auto text-lg text-white/90">
                            Quản lý thông tin cá nhân của bạn để tăng cơ hội việc làm
                        </p>
                    </div>
                </div>
            </section>

            {/* Main Content */}
            <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
                {/* Header Card */}
                <Card className="mb-6 bg-white border border-gray-100 rounded-xl shadow-lg">
                    <div className="p-6 md:p-8">
                        <div className="flex flex-col items-start justify-between gap-6 md:flex-row md:items-center">
                            <div className="flex flex-col items-start gap-6 sm:flex-row sm:items-center">
                                {user.avatarUrl ? (
                                    <img
                                        src={user.avatarUrl}
                                        alt={user.fullName}
                                        className="w-24 h-24 md:w-32 md:h-32 rounded-full border-4 border-[#00b14f]"
                                    />
                                ) : (
                                    <div className="w-24 h-24 md:w-32 md:h-32 bg-[#00b14f] rounded-full flex items-center justify-center border-4 border-[#00b14f]/20">
                                        <User className="h-12 w-12 md:h-16 md:w-16 text-white" />
                                    </div>
                                )}

                                <div className="flex-1">
                                    {isEditing ? (
                                        <div className="space-y-3">
                                            <div className="text-lg font-bold">{user.fullName}</div>
                                            <input
                                                type="text"
                                                value={formData.title || ''}
                                                onChange={(e: React.ChangeEvent<HTMLInputElement>) => handleChange('title', e.target.value)}
                                                className="w-full border border-gray-300 rounded-lg py-2 px-3 focus:border-[#00b14f] focus:outline-none focus:ring-2 focus:ring-[#00b14f]/20"
                                                placeholder="Chuyên ngành"
                                            />
                                        </div>
                                    ) : (
                                        <>
                                            <h1 className="mb-2 text-balance text-3xl font-bold text-gray-900 md:text-4xl">
                                                {user.fullName}
                                            </h1>
                                            <p className="mb-4 text-pretty text-lg text-[#00b14f] md:text-xl">
                                                {profile?.title || 'Chưa cập nhật'}
                                            </p>
                                        </>
                                    )}

                                    <div className="flex flex-wrap items-center gap-4 text-sm text-gray-600">
                                        <div className="flex items-center gap-2">
                                            <MapPin className="size-4 text-[#00b14f]" />
                                            {isEditing ? (
                                                <input
                                                    type="text"
                                                    value={formData.address || ''}
                                                    onChange={(e: React.ChangeEvent<HTMLInputElement>) => handleChange('address', e.target.value)}
                                                    className="h-8 w-48 border border-gray-300 rounded px-2 focus:border-[#00b14f] focus:outline-none focus:ring-2 focus:ring-[#00b14f]/20"
                                                    placeholder="Địa chỉ"
                                                />
                                            ) : (
                                                <span>{profile?.address || 'Chưa cập nhật'}</span>
                                            )}
                                        </div>
                                    </div>
                                </div>
                            </div>

                            <div className="flex gap-2">
                                {isEditing ? (
                                    <>
                                        <Button onClick={handleSave} disabled={updateMutation.isPending} className="bg-[#00b14f] hover:bg-[#00a047] text-white">
                                            <Save className="h-4 w-4 mr-2" />
                                            Lưu
                                        </Button>
                                        <Button variant="outline" onClick={handleCancel} className="border-gray-300">
                                            <X className="h-4 w-4 mr-2" />
                                            Hủy
                                        </Button>
                                    </>
                                ) : (
                                    <Button onClick={handleEdit} className="bg-[#00b14f] hover:bg-[#00a047] text-white">
                                        <Edit className="h-4 w-4 mr-2" />
                                        Chỉnh sửa
                                    </Button>
                                )}
                            </div>
                        </div>
                    </div>
                </Card>

                {/* Profile Completion Progress */}
                <Card className="mb-6 bg-white border border-gray-100 rounded-xl shadow-lg">
                    <div className="p-6">
                        <div className="flex items-center justify-between mb-2">
                            <h3 className="text-lg font-semibold text-gray-900">Hoàn thiện hồ sơ</h3>
                            <span className="text-sm font-medium text-[#00b14f]">{calculateProfileCompletion()}%</span>
                        </div>
                        <div className="w-full bg-gray-200 rounded-full h-2">
                            <div
                                className="bg-[#00b14f] h-2 rounded-full transition-all duration-300"
                                style={{ width: `${calculateProfileCompletion()}%` }}
                            ></div>
                        </div>
                        <p className="text-sm text-gray-600 mt-2">
                            Hồ sơ hoàn thiện giúp tăng cơ hội được nhà tuyển dụng chú ý
                        </p>
                    </div>
                </Card>

                {/* Contact & Social Links */}
                <div className="mb-6 grid gap-6 md:grid-cols-2">
                    <Card className="bg-white border border-gray-100 rounded-xl shadow-lg hover:shadow-xl transition">
                        <div className="p-6">
                            <h3 className="text-lg font-semibold text-gray-900 flex items-center gap-2 mb-4">
                                <Phone className="h-5 w-5 text-[#00b14f]" />
                                Thông tin liên hệ
                            </h3>
                            <div className="space-y-3">
                                <div className="flex items-center gap-3">
                                    <Mail className="h-4 w-4 text-gray-500" />
                                    <span className="text-sm text-gray-700">{user.email}</span>
                                </div>
                                <div className="flex items-center gap-3">
                                    <Phone className="h-4 w-4 text-gray-500" />
                                    {isEditing ? (
                                        <input
                                            type="text"
                                            value={formData.phone || ''}
                                            onChange={(e: React.ChangeEvent<HTMLInputElement>) => handleChange('phone', e.target.value)}
                                            className="flex-1 border border-gray-300 rounded px-3 py-1 focus:border-[#00b14f] focus:outline-none focus:ring-2 focus:ring-[#00b14f]/20"
                                            placeholder="Số điện thoại"
                                        />
                                    ) : (
                                        <span className="text-sm text-gray-700">{profile?.phone || 'Chưa cập nhật'}</span>
                                    )}
                                </div>
                                <div className="flex items-center gap-3">
                                    <Calendar className="h-4 w-4 text-gray-500" />
                                    {isEditing ? (
                                        <input
                                            type="date"
                                            value={formData.dateOfBirth ? new Date(formData.dateOfBirth).toISOString().split('T')[0] : ''}
                                            onChange={(e: React.ChangeEvent<HTMLInputElement>) => handleChange('dateOfBirth', new Date(e.target.value))}
                                            className="flex-1 border border-gray-300 rounded px-3 py-1 focus:border-[#00b14f] focus:outline-none focus:ring-2 focus:ring-[#00b14f]/20"
                                        />
                                    ) : (
                                        <span className="text-sm text-gray-700">
                                            {profile?.dateOfBirth ? new Date(profile.dateOfBirth).toLocaleDateString('vi-VN') : 'Chưa cập nhật'}
                                        </span>
                                    )}
                                </div>
                            </div>
                        </div>
                    </Card>

                    <Card className="bg-white border border-gray-100 rounded-xl shadow-lg hover:shadow-xl transition">
                        <div className="p-6">
                            <h3 className="text-lg font-semibold text-gray-900 flex items-center gap-2 mb-4">
                                <LinkIcon className="h-5 w-5 text-[#00b14f]" />
                                Liên kết mạng xã hội
                            </h3>
                            <div className="space-y-4">
                                <div className="flex items-center gap-3">
                                    <Linkedin className="h-4 w-4 text-gray-500" />
                                    {isEditing ? (
                                        <input
                                            type="url"
                                            value={formData.linkedInUrl || ''}
                                            onChange={(e: React.ChangeEvent<HTMLInputElement>) => handleChange('linkedInUrl', e.target.value)}
                                            className="flex-1 border border-gray-300 rounded px-3 py-1 focus:border-[#00b14f] focus:outline-none focus:ring-2 focus:ring-[#00b14f]/20"
                                            placeholder="LinkedIn URL"
                                        />
                                    ) : profile?.linkedInUrl ? (
                                        <div className="flex flex-1 items-center justify-between">
                                            <a href={profile.linkedInUrl} target="_blank" rel="noopener noreferrer" className="text-sm text-[#00b14f] hover:underline flex items-center gap-1">
                                                {profile.linkedInUrl}
                                                <ExternalLink className="h-3 w-3" />
                                            </a>
                                        </div>
                                    ) : (
                                        <span className="text-sm text-gray-400">Chưa cập nhật</span>
                                    )}
                                </div>
                                {!isEditing && (
                                    <Button
                                        onClick={() => setShowLinkedInSync(true)}
                                        disabled={syncLinkedInMutation.isPending}
                                        className="w-full bg-blue-600 hover:bg-blue-700 text-white text-sm"
                                    >
                                        <Linkedin className="h-4 w-4 mr-2" />
                                        {syncLinkedInMutation.isPending ? 'Đang đồng bộ...' : 'Đồng bộ LinkedIn'}
                                    </Button>
                                )}
                                <div className="pt-3 border-t border-gray-200">
                                    <div className="flex items-center gap-3 mb-3">
                                        <Github className="h-4 w-4 text-gray-500" />
                                        {isEditing ? (
                                            <input
                                                type="url"
                                                value={formData.gitHubUrl || ''}
                                                onChange={(e: React.ChangeEvent<HTMLInputElement>) => handleChange('gitHubUrl', e.target.value)}
                                                className="flex-1 border border-gray-300 rounded px-3 py-1 focus:border-[#00b14f] focus:outline-none focus:ring-2 focus:ring-[#00b14f]/20"
                                                placeholder="GitHub URL"
                                            />
                                        ) : profile?.gitHubUrl ? (
                                            <a href={profile.gitHubUrl} target="_blank" rel="noopener noreferrer" className="text-sm text-[#00b14f] hover:underline flex items-center gap-1">
                                                {profile.gitHubUrl}
                                                <ExternalLink className="h-3 w-3" />
                                            </a>
                                        ) : (
                                            <span className="text-sm text-gray-400">Chưa liên kết</span>
                                        )}
                                    </div>

                                    {!isEditing && (
                                        profile?.gitHubUrl ? (
                                            <div className="space-y-2">
                                                <div className="flex items-center gap-2 text-xs text-green-600 font-medium mb-2">
                                                    <span className="w-2 h-2 rounded-full bg-green-500 inline-block"></span>
                                                    Đã liên kết GitHub
                                                </div>
                                                <Button
                                                    onClick={() => setShowRepoModal(true)}
                                                    disabled={syncSkillsMutation.isPending || !!syncJobId}
                                                    className="w-full bg-gray-800 hover:bg-gray-900 text-white text-sm"
                                                >
                                                    <Github className="h-4 w-4 mr-2" />
                                                    Chọn repos &amp; Trích xuất skills
                                                </Button>
                                                {syncJobId && jobStatus && (
                                                    <div className="mt-2 space-y-1">
                                                        <div className="flex items-center justify-between text-xs text-gray-600">
                                                            <span className="flex items-center gap-1">
                                                                <RefreshCw className="h-3 w-3 animate-spin" />
                                                                {jobStatus.currentStep || 'Đang phân tích...'}
                                                            </span>
                                                            <span>{jobStatus.percentComplete}%</span>
                                                        </div>
                                                        <div className="w-full bg-gray-200 rounded-full h-1.5">
                                                            <div
                                                                className="bg-gray-800 h-1.5 rounded-full transition-all duration-500"
                                                                style={{ width: `${jobStatus.percentComplete}%` }}
                                                            />
                                                        </div>
                                                    </div>
                                                )}
                                            </div>
                                        ) : (
                                            <Button
                                                onClick={handleGitHubOAuth}
                                                disabled={isOAuthLoading}
                                                isLoading={isOAuthLoading}
                                                className="w-full bg-gray-800 hover:bg-gray-900 text-white text-sm"
                                            >
                                                <Github className="h-4 w-4 mr-2" />
                                                Kết nối GitHub
                                            </Button>
                                        )
                                    )}
                                </div>
                            </div>
                        </div>
                    </Card>
                </div>

                {/* About Section */}
                <Card className="mb-6 bg-white border border-gray-100 rounded-xl shadow-lg hover:shadow-xl transition">
                    <div className="p-6">
                        <h3 className="text-lg font-semibold text-gray-900 mb-4">Giới thiệu</h3>
                        {isEditing ? (
                            <textarea
                                value={formData.bio || ''}
                                onChange={(e: React.ChangeEvent<HTMLTextAreaElement>) => handleChange('bio', e.target.value)}
                                className="w-full min-h-24 border border-gray-300 rounded-lg py-3 px-4 focus:border-[#00b14f] focus:outline-none focus:ring-2 focus:ring-[#00b14f]/20"
                                rows={4}
                                placeholder="Hãy giới thiệu về bản thân bạn..."
                            />
                        ) : (
                            <p className="text-gray-700 leading-relaxed whitespace-pre-wrap">{profile?.bio || 'Chưa cập nhật'}</p>
                        )}
                    </div>
                </Card>

                {/* Skills Section */}
                <Card className="mb-6 bg-white border border-gray-100 rounded-xl shadow-lg hover:shadow-xl transition">
                    <div className="p-6">
                        <h3 className="text-lg font-semibold text-gray-900 mb-4">Kỹ năng</h3>
                        {isEditing ? (
                            <input
                                type="text"
                                value={formData.skillIds?.join(', ') || ''}
                                onChange={(e: React.ChangeEvent<HTMLInputElement>) => handleChange('skillIds', e.target.value.split(',').map((s: string) => s.trim()))}
                                className="w-full border border-gray-300 rounded-lg py-3 px-4 focus:border-[#00b14f] focus:outline-none focus:ring-2 focus:ring-[#00b14f]/20"
                                placeholder="Nhập kỹ năng, cách nhau bằng dấu phẩy"
                            />
                        ) : (
                            <div className="flex flex-wrap gap-2">
                                {profile?.skillIds?.length ? (
                                    profile.skillIds.map((skill: string, index: number) => (
                                        <span key={index} className="px-3 py-1 bg-[#00b14f]/10 text-[#00b14f] rounded-full text-sm font-medium">
                                            {skill}
                                        </span>
                                    ))
                                ) : (
                                    <p className="text-gray-500">Chưa cập nhật</p>
                                )}
                            </div>
                        )}
                    </div>
                </Card>

                {/* Experience Section */}
                <Card className="mb-6 bg-white border border-gray-100 rounded-xl shadow-lg hover:shadow-xl transition">
                    <div className="p-6">
                        <h3 className="text-lg font-semibold text-gray-900 flex items-center gap-2 mb-4">
                            <Briefcase className="h-5 w-5 text-[#00b14f]" />
                            Kinh nghiệm làm việc
                        </h3>
                        {isEditing ? (
                            <textarea
                                value={formData.experience || ''}
                                onChange={(e: React.ChangeEvent<HTMLTextAreaElement>) => handleChange('experience', e.target.value)}
                                className="w-full min-h-24 border border-gray-300 rounded-lg py-3 px-4 focus:border-[#00b14f] focus:outline-none focus:ring-2 focus:ring-[#00b14f]/20"
                                rows={4}
                                placeholder="Mô tả kinh nghiệm làm việc của bạn..."
                            />
                        ) : (
                            <p className="text-gray-700 leading-relaxed whitespace-pre-wrap">{profile?.experience || 'Chưa cập nhật'}</p>
                        )}
                    </div>
                </Card>

                {/* Education Section */}
                <Card className="bg-white border border-gray-100 rounded-xl shadow-lg hover:shadow-xl transition">
                    <div className="p-6">
                        <h3 className="text-lg font-semibold text-gray-900 flex items-center gap-2 mb-4">
                            <GraduationCap className="h-5 w-5 text-[#00b14f]" />
                            Học vấn
                        </h3>
                        {isEditing ? (
                            <textarea
                                value={formData.education || ''}
                                onChange={(e: React.ChangeEvent<HTMLTextAreaElement>) => handleChange('education', e.target.value)}
                                className="w-full min-h-24 border border-gray-300 rounded-lg py-3 px-4 focus:border-[#00b14f] focus:outline-none focus:ring-2 focus:ring-[#00b14f]/20"
                                rows={4}
                                placeholder="Mô tả học vấn của bạn..."
                            />
                        ) : (
                            <p className="text-gray-700 leading-relaxed whitespace-pre-wrap">{profile?.education || 'Chưa cập nhật'}</p>
                        )}
                    </div>
                </Card>
            </div>

            {/* GitHub Repo Selection Modal */}
            {showRepoModal && (
                <div className="fixed inset-0 bg-black/10 backdrop-blur-sm flex items-center justify-center z-50 p-4">
                    <Card className="w-full max-w-lg bg-white rounded-xl shadow-2xl max-h-[80vh] flex flex-col">
                        <div className="p-6 border-b border-gray-100">
                            <div className="flex items-center justify-between">
                                <h3 className="text-lg font-semibold text-gray-900 flex items-center gap-2">
                                    <Github className="h-5 w-5 text-gray-800" />
                                    Chọn repositories để phân tích
                                </h3>
                                <button
                                    onClick={() => { setShowRepoModal(false); setSelectedRepos([]); }}
                                    className="text-gray-500 hover:text-gray-700"
                                >
                                    <X className="h-5 w-5" />
                                </button>
                            </div>
                            <p className="text-sm text-gray-500 mt-1">
                                Gemini AI sẽ phân tích code và tự động cập nhật kỹ năng vào hồ sơ
                            </p>
                        </div>

                        <div className="flex-1 overflow-y-auto p-6">
                            {isLoadingRepos ? (
                                <div className="flex items-center justify-center py-8">
                                    <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-gray-800"></div>
                                    <span className="ml-3 text-sm text-gray-600">Đang tải repositories...</span>
                                </div>
                            ) : reposData?.repositories.length ? (
                                <div className="space-y-2">
                                    <div className="flex items-center justify-between mb-3">
                                        <span className="text-xs text-gray-500">{reposData.totalCount} repositories</span>
                                        <button
                                            onClick={() => {
                                                if (selectedRepos.length === reposData.repositories.length) {
                                                    setSelectedRepos([]);
                                                } else {
                                                    setSelectedRepos(reposData.repositories.map(r => r.name));
                                                }
                                            }}
                                            className="text-xs text-gray-800 hover:underline font-medium"
                                        >
                                            {selectedRepos.length === reposData.repositories.length ? 'Bỏ chọn tất cả' : 'Chọn tất cả'}
                                        </button>
                                    </div>
                                    {reposData.repositories.map((repo) => (
                                        <label
                                            key={repo.name}
                                            className={`flex items-start gap-3 p-3 rounded-lg border cursor-pointer transition-colors ${
                                                selectedRepos.includes(repo.name)
                                                    ? 'border-gray-800 bg-gray-50'
                                                    : 'border-gray-200 hover:border-gray-300'
                                            }`}
                                        >
                                            <input
                                                type="checkbox"
                                                checked={selectedRepos.includes(repo.name)}
                                                onChange={(e) => {
                                                    if (e.target.checked) {
                                                        setSelectedRepos(prev => [...prev, repo.name]);
                                                    } else {
                                                        setSelectedRepos(prev => prev.filter(r => r !== repo.name));
                                                    }
                                                }}
                                                className="mt-0.5 h-4 w-4 rounded accent-gray-800"
                                            />
                                            <div className="flex-1 min-w-0">
                                                <div className="flex items-center gap-2 flex-wrap">
                                                    <span className="text-sm font-medium text-gray-900 truncate">{repo.name}</span>
                                                    {repo.language && (
                                                        <span className="text-xs px-2 py-0.5 bg-gray-100 text-gray-600 rounded-full">{repo.language}</span>
                                                    )}
                                                    {repo.isFork && (
                                                        <span className="text-xs px-2 py-0.5 bg-yellow-100 text-yellow-700 rounded-full">Fork</span>
                                                    )}
                                                </div>
                                                {repo.description && (
                                                    <p className="text-xs text-gray-500 mt-0.5 truncate">{repo.description}</p>
                                                )}
                                                <div className="flex items-center gap-3 mt-1 text-xs text-gray-400">
                                                    <span>★ {repo.stars}</span>
                                                    <span>⑂ {repo.forks}</span>
                                                </div>
                                            </div>
                                        </label>
                                    ))}
                                </div>
                            ) : (
                                <div className="text-center py-8 text-gray-500 text-sm">
                                    Không tìm thấy repositories
                                </div>
                            )}
                        </div>

                        <div className="p-6 border-t border-gray-100 flex gap-3">
                            <Button
                                onClick={() => syncSkillsMutation.mutate(selectedRepos)}
                                disabled={selectedRepos.length === 0 || syncSkillsMutation.isPending}
                                isLoading={syncSkillsMutation.isPending}
                                className="flex-1 bg-gray-800 hover:bg-gray-900 text-white"
                            >
                                Phân tích {selectedRepos.length > 0 ? `(${selectedRepos.length})` : ''} repos
                            </Button>
                            <Button
                                onClick={() => { setShowRepoModal(false); setSelectedRepos([]); }}
                                variant="outline"
                                className="flex-1"
                            >
                                Hủy
                            </Button>
                        </div>
                    </Card>
                </div>
            )}

            {/* LinkedIn Sync Modal */}
            {showLinkedInSync && (
                <div className="fixed inset-0 bg-black/10 backdrop-blur-sm flex items-center justify-center z-50">
                    <Card className="w-96 bg-white rounded-xl shadow-2xl">
                        <div className="p-6">
                            <div className="flex items-center justify-between mb-4">
                                <h3 className="text-lg font-semibold text-gray-900 flex items-center gap-2">
                                    <Linkedin className="h-5 w-5 text-blue-600" />
                                    Đồng bộ LinkedIn
                                </h3>
                                <button
                                    onClick={() => {
                                        setShowLinkedInSync(false);
                                        setLinkedInData('');
                                    }}
                                    className="text-gray-500 hover:text-gray-700"
                                >
                                    <X className="h-5 w-5" />
                                </button>
                            </div>
                            <p className="text-sm text-gray-600 mb-4">
                                Nhập dữ liệu LinkedIn của bạn để đồng bộ hồ sơ
                            </p>
                            <textarea
                                value={linkedInData}
                                onChange={(e) => setLinkedInData(e.target.value)}
                                placeholder="Dán dữ liệu LinkedIn tại đây (JSON format hoặc profile URL)"
                                className="w-full h-24 border border-gray-300 rounded-lg p-3 focus:border-blue-600 focus:outline-none focus:ring-2 focus:ring-blue-600/20 text-sm"
                            />
                            <div className="flex gap-2 mt-4">
                                <Button
                                    onClick={() => syncLinkedInMutation.mutate(linkedInData)}
                                    disabled={!linkedInData.trim() || syncLinkedInMutation.isPending}
                                    className="flex-1 bg-blue-600 hover:bg-blue-700 text-white"
                                >
                                    {syncLinkedInMutation.isPending ? 'Đang đồng bộ...' : 'Đồng bộ'}
                                </Button>
                                <Button
                                    onClick={() => {
                                        setShowLinkedInSync(false);
                                        setLinkedInData('');
                                    }}
                                    variant="outline"
                                    className="flex-1 border-gray-300"
                                >
                                    Hủy
                                </Button>
                            </div>
                        </div>
                    </Card>
                </div>
            )}
        </div>
    );
}