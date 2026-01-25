import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { User, Edit, Save, X, Calendar, MapPin, Phone, Link as LinkIcon, Github, Linkedin } from 'lucide-react';
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

    return (
        <div className="min-h-screen bg-gray-50">
            <div className="mx-auto max-w-4xl px-4 py-8">
                {/* Header */}
                <div className="mb-8">
                    <div className="flex items-center justify-between">
                        <h1 className="text-3xl font-bold text-gray-900">Hồ sơ cá nhân</h1>
                        {!isEditing ? (
                            <Button onClick={handleEdit} className="flex items-center gap-2">
                                <Edit className="h-4 w-4" />
                                Chỉnh sửa
                            </Button>
                        ) : (
                            <div className="flex gap-2">
                                <Button onClick={handleSave} disabled={updateMutation.isPending} className="flex items-center gap-2">
                                    <Save className="h-4 w-4" />
                                    Lưu
                                </Button>
                                <Button variant="outline" onClick={handleCancel} className="flex items-center gap-2">
                                    <X className="h-4 w-4" />
                                    Hủy
                                </Button>
                            </div>
                        )}
                    </div>
                </div>

                <div className="grid gap-6 lg:grid-cols-3">
                    {/* Avatar and Basic Info */}
                    <div className="lg:col-span-1">
                        <Card className="p-6">
                            <div className="text-center">
                                {user.avatarUrl ? (
                                    <img
                                        src={user.avatarUrl}
                                        alt={user.fullName}
                                        className="w-24 h-24 rounded-full mx-auto mb-4"
                                    />
                                ) : (
                                    <div className="w-24 h-24 bg-gray-200 rounded-full mx-auto mb-4 flex items-center justify-center">
                                        <User className="h-12 w-12 text-gray-400" />
                                    </div>
                                )}
                                <h2 className="text-xl font-semibold text-gray-900">{user.fullName}</h2>
                                <p className="text-gray-600">{user.email}</p>
                            </div>
                        </Card>
                    </div>

                    {/* Profile Details */}
                    <div className="lg:col-span-2 space-y-6">
                        {/* Chuyên ngành */}
                        <Card className="p-6">
                            <label className="text-sm font-medium text-gray-700">Chuyên ngành</label>
                            {isEditing ? (
                                <input
                                    type="text"
                                    value={formData.title || ''}
                                    onChange={(e: React.ChangeEvent<HTMLInputElement>) => handleChange('title', e.target.value)}
                                    className="mt-2 w-full rounded-lg border border-gray-300 py-3 px-4 focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20"
                                />
                            ) : (
                                <p className="mt-2 text-gray-900">{profile?.title || 'Chưa cập nhật'}</p>
                            )}
                        </Card>

                        {/* Bio */}
                        <Card className="p-6">
                            <label className="text-sm font-medium text-gray-700">Bio</label>
                            {isEditing ? (
                                <textarea
                                    value={formData.bio || ''}
                                    onChange={(e: React.ChangeEvent<HTMLTextAreaElement>) => handleChange('bio', e.target.value)}
                                    className="mt-2 w-full rounded-lg border border-gray-300 py-3 px-4 focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20"
                                    rows={4}
                                />
                            ) : (
                                <p className="mt-2 text-gray-900 whitespace-pre-wrap">{profile?.bio || 'Chưa cập nhật'}</p>
                            )}
                        </Card>

                        {/* Phone */}
                        <Card className="p-6">
                            <label className="text-sm font-medium text-gray-700 flex items-center gap-2">
                                <Phone className="h-4 w-4" />
                                Số điện thoại
                            </label>
                            {isEditing ? (
                                <input
                                    type="text"
                                    value={formData.phone || ''}
                                    onChange={(e: React.ChangeEvent<HTMLInputElement>) => handleChange('phone', e.target.value)}
                                    className="mt-2 w-full rounded-lg border border-gray-300 py-3 px-4 focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20"
                                />
                            ) : (
                                <p className="mt-2 text-gray-900">{profile?.phone || 'Chưa cập nhật'}</p>
                            )}
                        </Card>

                        {/* Address */}
                        <Card className="p-6">
                            <label className="text-sm font-medium text-gray-700 flex items-center gap-2">
                                <MapPin className="h-4 w-4" />
                                Địa chỉ
                            </label>
                            {isEditing ? (
                                <input
                                    type="text"
                                    value={formData.address || ''}
                                    onChange={(e: React.ChangeEvent<HTMLInputElement>) => handleChange('address', e.target.value)}
                                    className="mt-2 w-full rounded-lg border border-gray-300 py-3 px-4 focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20"
                                />
                            ) : (
                                <p className="mt-2 text-gray-900">{profile?.address || 'Chưa cập nhật'}</p>
                            )}
                        </Card>

                        {/* Date of Birth */}
                        <Card className="p-6">
                            <label className="text-sm font-medium text-gray-700 flex items-center gap-2">
                                <Calendar className="h-4 w-4" />
                                Ngày sinh
                            </label>
                            {isEditing ? (
                                <input
                                    type="date"
                                    value={formData.dateOfBirth ? new Date(formData.dateOfBirth).toISOString().split('T')[0] : ''}
                                    onChange={(e: React.ChangeEvent<HTMLInputElement>) => handleChange('dateOfBirth', new Date(e.target.value))}
                                    className="mt-2 w-full rounded-lg border border-gray-300 py-3 px-4 focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20"
                                />
                            ) : (
                                <p className="mt-2 text-gray-900">
                                    {profile?.dateOfBirth ? new Date(profile.dateOfBirth).toLocaleDateString('vi-VN') : 'Chưa cập nhật'}
                                </p>
                            )}
                        </Card>

                        {/* Skills */}
                        <Card className="p-6">
                            <label className="text-sm font-medium text-gray-700">Kỹ năng</label>
                            {isEditing ? (
                                <input
                                    type="text"
                                    value={formData.skillIds?.join(', ') || ''}
                                    onChange={(e: React.ChangeEvent<HTMLInputElement>) => handleChange('skillIds', e.target.value.split(',').map((s: string) => s.trim()))}
                                    className="mt-2 w-full rounded-lg border border-gray-300 py-3 px-4 focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20"
                                    placeholder="Nhập kỹ năng, cách nhau bằng dấu phẩy"
                                />
                            ) : (
                                <div className="mt-2 flex flex-wrap gap-2">
                                    {profile?.skillIds?.length ? (
                                        profile.skillIds.map((skill: string, index: number) => (
                                            <span key={index} className="px-3 py-1 bg-blue-100 text-blue-800 rounded-full text-sm">
                                                {skill}
                                            </span>
                                        ))
                                    ) : (
                                        <p className="text-gray-500">Chưa cập nhật</p>
                                    )}
                                </div>
                            )}
                        </Card>

                        {/* Kinh nghiệm */}
                        <Card className="p-6">
                            <label className="text-sm font-medium text-gray-700">Kinh nghiệm</label>
                            {isEditing ? (
                                <textarea
                                    value={formData.experience || ''}
                                    onChange={(e: React.ChangeEvent<HTMLTextAreaElement>) => handleChange('experience', e.target.value)}
                                    className="mt-2 w-full rounded-lg border border-gray-300 py-3 px-4 focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20"
                                    rows={4}
                                />
                            ) : (
                                <p className="mt-2 text-gray-900 whitespace-pre-wrap">{profile?.experience || 'Chưa cập nhật'}</p>
                            )}
                        </Card>

                        {/* Bằng cấp */}
                        <Card className="p-6">
                            <label className="text-sm font-medium text-gray-700">Bằng cấp</label>
                            {isEditing ? (
                                <textarea
                                    value={formData.education || ''}
                                    onChange={(e: React.ChangeEvent<HTMLTextAreaElement>) => handleChange('education', e.target.value)}
                                    className="mt-2 w-full rounded-lg border border-gray-300 py-3 px-4 focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20"
                                    rows={4}
                                />
                            ) : (
                                <p className="mt-2 text-gray-900 whitespace-pre-wrap">{profile?.education || 'Chưa cập nhật'}</p>
                            )}
                        </Card>

                        {/* LinkedIn */}
                        <Card className="p-6">
                            <label className="text-sm font-medium text-gray-700 flex items-center gap-2">
                                <Linkedin className="h-4 w-4" />
                                LinkedIn URL
                            </label>
                            {isEditing ? (
                                <input
                                    type="url"
                                    value={formData.linkedInUrl || ''}
                                    onChange={(e: React.ChangeEvent<HTMLInputElement>) => handleChange('linkedInUrl', e.target.value)}
                                    className="mt-2 w-full rounded-lg border border-gray-300 py-3 px-4 focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20"
                                />
                            ) : (
                                <p className="mt-2 text-gray-900">
                                    {profile?.linkedInUrl ? (
                                        <a href={profile.linkedInUrl} target="_blank" rel="noopener noreferrer" className="text-blue-600 hover:underline">
                                            {profile.linkedInUrl}
                                        </a>
                                    ) : 'Chưa cập nhật'}
                                </p>
                            )}
                        </Card>

                        {/* GitHub */}
                        <Card className="p-6">
                            <label className="text-sm font-medium text-gray-700 flex items-center gap-2">
                                <Github className="h-4 w-4" />
                                GitHub URL
                            </label>
                            {isEditing ? (
                                <input
                                    type="url"
                                    value={formData.gitHubUrl || ''}
                                    onChange={(e: React.ChangeEvent<HTMLInputElement>) => handleChange('gitHubUrl', e.target.value)}
                                    className="mt-2 w-full rounded-lg border border-gray-300 py-3 px-4 focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20"
                                />
                            ) : (
                                <p className="mt-2 text-gray-900">
                                    {profile?.gitHubUrl ? (
                                        <a href={profile.gitHubUrl} target="_blank" rel="noopener noreferrer" className="text-blue-600 hover:underline">
                                            {profile.gitHubUrl}
                                        </a>
                                    ) : 'Chưa cập nhật'}
                                </p>
                            )}
                        </Card>
                    </div>
                </div>
            </div>
        </div>
    );
}