import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { User, Edit, Save, X, Calendar, MapPin, Phone, Link as LinkIcon, Github, Linkedin, Mail, Briefcase, GraduationCap } from 'lucide-react';
import { Button, Card } from '../../components/ui';
import { candidateApi } from '../../api';
import { useAuthStore } from '../../stores/auth.store';
import type { UserProfile } from '../../types';

export function CandidateProfilePage() {
    const [isEditing, setIsEditing] = useState(false);
    const [formData, setFormData] = useState<Partial<UserProfile>>({});

    const queryClient = useQueryClient();
    const { user: currentUser, isAuthenticated } = useAuthStore();

    const userId = currentUser?.id;

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
                            <div className="space-y-3">
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
                                        <a href={profile.linkedInUrl} target="_blank" rel="noopener noreferrer" className="text-sm text-[#00b14f] hover:underline">
                                            {profile.linkedInUrl}
                                        </a>
                                    ) : (
                                        <span className="text-sm text-gray-400">Chưa cập nhật</span>
                                    )}
                                </div>
                                <div className="flex items-center gap-3">
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
                                        <a href={profile.gitHubUrl} target="_blank" rel="noopener noreferrer" className="text-sm text-[#00b14f] hover:underline">
                                            {profile.gitHubUrl}
                                        </a>
                                    ) : (
                                        <span className="text-sm text-gray-400">Chưa cập nhật</span>
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
        </div>
    );
}