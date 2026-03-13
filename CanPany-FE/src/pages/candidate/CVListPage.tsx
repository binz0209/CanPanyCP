import { useState, useRef } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
    FileText,
    Upload,
    Download,
    Trash2,
    Edit2,
    Star,
    X,
    Eye,
    FileCheck,
    Calendar,
    CheckCircle2
} from 'lucide-react';
import toast from 'react-hot-toast';
import { Button, Card, CardContent, CardHeader, CardTitle, Badge, Input } from '../../components/ui';
import { cvApi } from '../../api';
import type { CV } from '../../types';

export function CVListPage() {
    const queryClient = useQueryClient();
    const fileInputRef = useRef<HTMLInputElement>(null);
    const [selectedCV, setSelectedCV] = useState<CV | null>(null);
    const [isDetailOpen, setIsDetailOpen] = useState(false);
    const [isEditingName, setIsEditingName] = useState(false);
    const [editName, setEditName] = useState('');
    const [isUploading, setIsUploading] = useState(false);
    const [isDragging, setIsDragging] = useState(false);

    // Fetch CVs
    const { data: cvs = [], isLoading } = useQuery({
        queryKey: ['candidate-cvs'],
        queryFn: () => cvApi.getCVs(),
    });

    // Upload CV mutation
    const uploadMutation = useMutation({
        mutationFn: (file: File) => cvApi.uploadCV({ file }),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['candidate-cvs'] });
            toast.success('Tải lên CV thành công!');
            setIsUploading(false);
        },
        onError: () => {
            toast.error('Tải lên CV thất bại. Vui lòng thử lại.');
            setIsUploading(false);
        },
    });

    // Update CV mutation
    const updateMutation = useMutation({
        mutationFn: ({ id, fileName }: { id: string; fileName: string }) =>
            cvApi.updateCV(id, { fileName }),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['candidate-cvs'] });
            toast.success('Cập nhật tên CV thành công!');
            setIsEditingName(false);
            if (selectedCV) {
                setSelectedCV({ ...selectedCV, fileName: editName });
            }
        },
        onError: () => {
            toast.error('Cập nhật thất bại. Vui lòng thử lại.');
        },
    });

    // Delete CV mutation
    const deleteMutation = useMutation({
        mutationFn: (id: string) => cvApi.deleteCV(id),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['candidate-cvs'] });
            toast.success('Xóa CV thành công!');
            setIsDetailOpen(false);
            setSelectedCV(null);
        },
        onError: () => {
            toast.error('Xóa CV thất bại. Vui lòng thử lại.');
        },
    });

    // Set default CV mutation
    const setDefaultMutation = useMutation({
        mutationFn: (id: string) => cvApi.setDefaultCV(id),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['candidate-cvs'] });
            toast.success('Đặt CV mặc định thành công!');
        },
        onError: () => {
            toast.error('Thao tác thất bại. Vui lòng thử lại.');
        },
    });

    const handleFileUpload = (file: File) => {
        // Validate file type
        const allowedTypes = [
            'application/pdf',
            'application/msword',
            'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
        ];
        if (!allowedTypes.includes(file.type)) {
            toast.error('Vui lòng chọn file PDF hoặc Word (.doc, .docx)');
            return;
        }

        // Validate file size (10MB)
        if (file.size > 10 * 1024 * 1024) {
            toast.error('Kích thước file không được vượt quá 10MB');
            return;
        }

        setIsUploading(true);
        uploadMutation.mutate(file);
    };

    const handleDrop = (e: React.DragEvent) => {
        e.preventDefault();
        setIsDragging(false);
        const files = e.dataTransfer.files;
        if (files.length > 0) {
            handleFileUpload(files[0]);
        }
    };

    const handleDragOver = (e: React.DragEvent) => {
        e.preventDefault();
        setIsDragging(true);
    };

    const handleDragLeave = () => {
        setIsDragging(false);
    };

    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (file) {
            handleFileUpload(file);
        }
    };

    const openDetailModal = (cv: CV) => {
        setSelectedCV(cv);
        setEditName(cv.fileName);
        setIsDetailOpen(true);
        setIsEditingName(false);
    };

    const closeDetailModal = () => {
        setIsDetailOpen(false);
        setSelectedCV(null);
        setIsEditingName(false);
    };

    const handleUpdateName = () => {
        if (!selectedCV || !editName.trim()) return;
        updateMutation.mutate({ id: selectedCV.id, fileName: editName.trim() });
    };

    const handleDelete = () => {
        if (!selectedCV) return;
        if (window.confirm('Bạn có chắc chắn muốn xóa CV này?')) {
            deleteMutation.mutate(selectedCV.id);
        }
    };

    const handleSetDefault = (cvId: string) => {
        setDefaultMutation.mutate(cvId);
    };

    const formatFileSize = (bytes: number) => {
        if (bytes === 0) return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    };

    const formatDate = (dateString: string) => {
        return new Date(dateString).toLocaleDateString('vi-VN', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric',
        });
    };

    return (
        <div className="min-h-screen bg-gray-50">
            {/* Page Header */}
            <div className="bg-white border-b border-gray-200">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
                    <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
                        <div>
                            <h1 className="text-3xl font-bold text-gray-900">Quản lý CV</h1>
                            <p className="mt-2 text-gray-600">
                                Quản lý tất cả CV của bạn tại một nơi
                            </p>
                        </div>
                        <Button
                            onClick={() => fileInputRef.current?.click()}
                            disabled={isUploading}
                            className="bg-[#00b14f] hover:bg-[#00a045]"
                        >
                            <Upload className="h-4 w-4 mr-2" />
                            {isUploading ? 'Đang tải lên...' : 'Tải lên CV mới'}
                        </Button>
                    </div>
                </div>
            </div>

            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
                {/* Upload Area */}
                <Card
                    className={`mb-8 border-2 border-dashed transition-all ${
                        isDragging
                            ? 'border-[#00b14f] bg-[#00b14f]/5'
                            : 'border-gray-300 hover:border-gray-400'
                    }`}
                    onDrop={handleDrop}
                    onDragOver={handleDragOver}
                    onDragLeave={handleDragLeave}
                >
                    <CardContent className="py-12">
                        <div className="text-center">
                            <div className="mx-auto h-16 w-16 rounded-full bg-[#00b14f]/10 flex items-center justify-center mb-4">
                                <Upload className="h-8 w-8 text-[#00b14f]" />
                            </div>
                            <h3 className="text-lg font-medium text-gray-900 mb-2">
                                Kéo và thả file CV vào đây
                            </h3>
                            <p className="text-gray-500 mb-4">
                                Hoặc{' '}
                                <button
                                    onClick={() => fileInputRef.current?.click()}
                                    className="text-[#00b14f] font-medium hover:underline"
                                >
                                    chọn file từ máy tính
                                </button>
                            </p>
                            <p className="text-xs text-gray-400">
                                Hỗ trợ PDF, DOC, DOCX (Tối đa 10MB)
                            </p>
                        </div>
                    </CardContent>
                </Card>

                {/* Hidden File Input */}
                <input
                    ref={fileInputRef}
                    type="file"
                    accept=".pdf,.doc,.docx,application/pdf,application/msword,application/vnd.openxmlformats-officedocument.wordprocessingml.document"
                    onChange={handleInputChange}
                    className="hidden"
                />

                {/* Stats */}
                <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-8">
                    <Card>
                        <CardContent className="p-6">
                            <div className="flex items-center gap-4">
                                <div className="h-12 w-12 rounded-full bg-blue-50 flex items-center justify-center">
                                    <FileText className="h-6 w-6 text-blue-600" />
                                </div>
                                <div>
                                    <p className="text-sm text-gray-500">Tổng số CV</p>
                                    <p className="text-2xl font-bold text-gray-900">{cvs.length}</p>
                                </div>
                            </div>
                        </CardContent>
                    </Card>
                    <Card>
                        <CardContent className="p-6">
                            <div className="flex items-center gap-4">
                                <div className="h-12 w-12 rounded-full bg-green-50 flex items-center justify-center">
                                    <FileCheck className="h-6 w-6 text-green-600" />
                                </div>
                                <div>
                                    <p className="text-sm text-gray-500">CV mặc định</p>
                                    <p className="text-2xl font-bold text-gray-900">
                                        {cvs.filter((cv) => cv.isDefault).length}
                                    </p>
                                </div>
                            </div>
                        </CardContent>
                    </Card>
                    <Card>
                        <CardContent className="p-6">
                            <div className="flex items-center gap-4">
                                <div className="h-12 w-12 rounded-full bg-purple-50 flex items-center justify-center">
                                    <Calendar className="h-6 w-6 text-purple-600" />
                                </div>
                                <div>
                                    <p className="text-sm text-gray-500">CV mới nhất</p>
                                    <p className="text-lg font-bold text-gray-900">
                                        {cvs.length > 0
                                            ? formatDate(
                                                  [...cvs].sort(
                                                      (a, b) =>
                                                          new Date(b.createdAt).getTime() -
                                                          new Date(a.createdAt).getTime()
                                                  )[0].createdAt
                                              )
                                            : '—'}
                                    </p>
                                </div>
                            </div>
                        </CardContent>
                    </Card>
                </div>

                {/* CV List */}
                {isLoading ? (
                    <div className="flex justify-center py-12">
                        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-[#00b14f]"></div>
                    </div>
                ) : cvs.length === 0 ? (
                    <Card className="py-16">
                        <CardContent className="text-center">
                            <div className="mx-auto h-20 w-20 rounded-full bg-gray-100 flex items-center justify-center mb-4">
                                <FileText className="h-10 w-10 text-gray-400" />
                            </div>
                            <h3 className="text-lg font-medium text-gray-900 mb-2">
                                Chưa có CV nào
                            </h3>
                            <p className="text-gray-500 mb-6 max-w-md mx-auto">
                                Tải lên CV đầu tiên của bạn để bắt đầu ứng tuyển các cơ hội việc làm hấp dẫn
                            </p>
                            <Button
                                onClick={() => fileInputRef.current?.click()}
                                className="bg-[#00b14f] hover:bg-[#00a045]"
                            >
                                <Upload className="h-4 w-4 mr-2" />
                                Tải lên CV ngay
                            </Button>
                        </CardContent>
                    </Card>
                ) : (
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                        {cvs.map((cv) => (
                            <Card
                                key={cv.id}
                                className={`group cursor-pointer transition-all hover:shadow-lg ${
                                    cv.isDefault ? 'ring-2 ring-[#00b14f] ring-offset-2' : ''
                                }`}
                                onClick={() => openDetailModal(cv)}
                            >
                                <CardHeader className="pb-3">
                                    <div className="flex items-start justify-between">
                                        <div className="flex-1 min-w-0">
                                            <div className="flex items-center gap-2 mb-2">
                                                <div className="h-10 w-10 rounded-lg bg-[#00b14f]/10 flex items-center justify-center flex-shrink-0">
                                                    <FileText className="h-5 w-5 text-[#00b14f]" />
                                                </div>
                                                <div className="flex-1 min-w-0">
                                                    <CardTitle className="text-base truncate">
                                                        {cv.fileName}
                                                    </CardTitle>
                                                </div>
                                            </div>
                                        </div>
                                        {cv.isDefault && (
                                            <Badge variant="success" className="flex-shrink-0">
                                                <Star className="h-3 w-3 mr-1 fill-current" />
                                                Mặc định
                                            </Badge>
                                        )}
                                    </div>
                                </CardHeader>
                                <CardContent>
                                    <div className="space-y-2 text-sm text-gray-500">
                                        <div className="flex justify-between">
                                            <span>Kích thước:</span>
                                            <span className="font-medium text-gray-700">
                                                {formatFileSize(cv.fileSize)}
                                            </span>
                                        </div>
                                        <div className="flex justify-between">
                                            <span>Định dạng:</span>
                                            <span className="font-medium text-gray-700">
                                                {cv.mimeType.includes('pdf') ? 'PDF' : 'Word'}
                                            </span>
                                        </div>
                                        <div className="flex justify-between">
                                            <span>Tải lên:</span>
                                            <span className="font-medium text-gray-700">
                                                {formatDate(cv.createdAt)}
                                            </span>
                                        </div>
                                    </div>
                                    <div className="mt-4 pt-4 border-t border-gray-100">
                                        <div className="flex items-center justify-between">
                                            <Button
                                                variant="ghost"
                                                size="sm"
                                                className="text-[#00b14f] hover:text-[#00a045]"
                                                onClick={(e) => {
                                                    e.stopPropagation();
                                                    openDetailModal(cv);
                                                }}
                                            >
                                                <Eye className="h-4 w-4 mr-1" />
                                                Xem chi tiết
                                            </Button>
                                            {!cv.isDefault && (
                                                <Button
                                                    variant="ghost"
                                                    size="sm"
                                                    onClick={(e) => {
                                                        e.stopPropagation();
                                                        handleSetDefault(cv.id);
                                                    }}
                                                    disabled={setDefaultMutation.isPending}
                                                >
                                                    <Star className="h-4 w-4 mr-1" />
                                                    Đặt mặc định
                                                </Button>
                                            )}
                                        </div>
                                    </div>
                                </CardContent>
                            </Card>
                        ))}
                    </div>
                )}
            </div>

            {/* Detail Modal */}
            {isDetailOpen && selectedCV && (
                <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
                    <div className="bg-white rounded-2xl shadow-2xl max-w-2xl w-full max-h-[90vh] overflow-y-auto">
                        {/* Modal Header */}
                        <div className="sticky top-0 bg-white border-b border-gray-200 px-6 py-4 flex items-center justify-between z-10">
                            <div className="flex items-center gap-3">
                                <div className="h-10 w-10 rounded-lg bg-[#00b14f]/10 flex items-center justify-center">
                                    <FileText className="h-5 w-5 text-[#00b14f]" />
                                </div>
                                <div>
                                    {isEditingName ? (
                                        <div className="flex items-center gap-2">
                                            <Input
                                                value={editName}
                                                onChange={(e) => setEditName(e.target.value)}
                                                className="w-64 h-9"
                                                autoFocus
                                            />
                                            <Button
                                                size="sm"
                                                onClick={handleUpdateName}
                                                disabled={updateMutation.isPending}
                                                className="bg-[#00b14f] hover:bg-[#00a045]"
                                            >
                                                <CheckCircle2 className="h-4 w-4" />
                                            </Button>
                                            <Button
                                                size="sm"
                                                variant="outline"
                                                onClick={() => {
                                                    setIsEditingName(false);
                                                    setEditName(selectedCV.fileName);
                                                }}
                                            >
                                                <X className="h-4 w-4" />
                                            </Button>
                                        </div>
                                    ) : (
                                        <>
                                            <h2 className="text-lg font-semibold text-gray-900">
                                                {selectedCV.fileName}
                                            </h2>
                                            <div className="flex items-center gap-2">
                                                {selectedCV.isDefault && (
                                                    <Badge variant="success">
                                                        <Star className="h-3 w-3 mr-1 fill-current" />
                                                        CV mặc định
                                                    </Badge>
                                                )}
                                            </div>
                                        </>
                                    )}
                                </div>
                            </div>
                            <button
                                onClick={closeDetailModal}
                                className="p-2 hover:bg-gray-100 rounded-full transition-colors"
                            >
                                <X className="h-5 w-5 text-gray-500" />
                            </button>
                        </div>

                        {/* Modal Content */}
                        <div className="p-6 space-y-6">
                            {/* Info Grid */}
                            <div className="grid grid-cols-2 gap-4">
                                <div className="bg-gray-50 rounded-lg p-4">
                                    <p className="text-sm text-gray-500 mb-1">Kích thước file</p>
                                    <p className="font-medium text-gray-900">
                                        {formatFileSize(selectedCV.fileSize)}
                                    </p>
                                </div>
                                <div className="bg-gray-50 rounded-lg p-4">
                                    <p className="text-sm text-gray-500 mb-1">Định dạng</p>
                                    <p className="font-medium text-gray-900">
                                        {selectedCV.mimeType.includes('pdf') ? 'PDF' : 'Microsoft Word'}
                                    </p>
                                </div>
                                <div className="bg-gray-50 rounded-lg p-4">
                                    <p className="text-sm text-gray-500 mb-1">Ngày tải lên</p>
                                    <p className="font-medium text-gray-900">
                                        {formatDate(selectedCV.createdAt)}
                                    </p>
                                </div>
                                <div className="bg-gray-50 rounded-lg p-4">
                                    <p className="text-sm text-gray-500 mb-1">Cập nhật lần cuối</p>
                                    <p className="font-medium text-gray-900">
                                        {selectedCV.updatedAt
                                            ? formatDate(selectedCV.updatedAt)
                                            : formatDate(selectedCV.createdAt)}
                                    </p>
                                </div>
                            </div>

                            {/* Extracted Skills */}
                            {selectedCV.extractedSkills && selectedCV.extractedSkills.length > 0 && (
                                <div>
                                    <h3 className="text-sm font-medium text-gray-900 mb-3">
                                        Kỹ năng được trích xuất
                                    </h3>
                                    <div className="flex flex-wrap gap-2">
                                        {selectedCV.extractedSkills.map((skill, index) => (
                                            <Badge key={index} variant="secondary">
                                                {skill}
                                            </Badge>
                                        ))}
                                    </div>
                                </div>
                            )}

                            {/* Actions */}
                            <div className="flex flex-wrap gap-3 pt-4 border-t border-gray-200">
                                <Button
                                    variant="outline"
                                    onClick={() => setIsEditingName(true)}
                                    disabled={isEditingName}
                                >
                                    <Edit2 className="h-4 w-4 mr-2" />
                                    Chỉnh sửa tên
                                </Button>
                                <Button
                                    variant="outline"
                                    onClick={() => window.open(selectedCV.fileUrl, '_blank')}
                                >
                                    <Eye className="h-4 w-4 mr-2" />
                                    Xem file
                                </Button>
                                <Button
                                    variant="outline"
                                    onClick={() => {
                                        const link = document.createElement('a');
                                        link.href = selectedCV.fileUrl;
                                        link.download = selectedCV.fileName;
                                        link.click();
                                    }}
                                >
                                    <Download className="h-4 w-4 mr-2" />
                                    Tải xuống
                                </Button>
                                {!selectedCV.isDefault && (
                                    <Button
                                        variant="outline"
                                        onClick={() => handleSetDefault(selectedCV.id)}
                                        disabled={setDefaultMutation.isPending}
                                    >
                                        <Star className="h-4 w-4 mr-2" />
                                        Đặt làm mặc định
                                    </Button>
                                )}
                                <Button
                                    variant="destructive"
                                    onClick={handleDelete}
                                    disabled={deleteMutation.isPending}
                                    className="ml-auto"
                                >
                                    <Trash2 className="h-4 w-4 mr-2" />
                                    Xóa CV
                                </Button>
                            </div>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}
