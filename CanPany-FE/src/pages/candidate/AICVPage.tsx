import { Wand2, Sparkles, FileText, Brain, MessageSquare, Clock, Construction, ArrowLeft } from 'lucide-react';
import { Button, Card, CardContent, CardHeader, CardTitle, Badge } from '../../components/ui';
import { Link } from 'react-router-dom';

export function AICVPage() {
    const upcomingFeatures = [
        {
            icon: <Sparkles className="h-6 w-6 text-purple-500" />,
            title: 'Tạo CV bằng AI',
            description: 'Tạo CV chuyên nghiệp chỉ trong vài phút với trợ lý AI thông minh',
            status: 'coming_soon',
        },
        {
            icon: <Brain className="h-6 w-6 text-blue-500" />,
            title: 'Phân tích CV thông minh',
            description: 'Đánh giá điểm mạnh, điểm yếu và nhận gợi ý cải thiện CV',
            status: 'coming_soon',
        },
        {
            icon: <FileText className="h-6 w-6 text-green-500" />,
            title: 'Tối ưu hóa từ khóa',
            description: 'Tự động tối ưu CV để vượt qua hệ thống lọc ATS',
            status: 'coming_soon',
        },
        {
            icon: <MessageSquare className="h-6 w-6 text-amber-500" />,
            title: 'Tư vấn CV 1-1',
            description: 'Chat với AI advisor để nhận phản hồi chi tiết về CV',
            status: 'coming_soon',
        },
    ];

    return (
        <div className="min-h-screen bg-gray-50">
            {/* Page Header */}
            <div className="bg-white border-b border-gray-200">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
                    <div className="flex items-center gap-4 mb-4">
                        <Link to="/candidate/cv/list">
                            <Button variant="ghost" size="sm">
                                <ArrowLeft className="h-4 w-4 mr-2" />
                                Quay lại
                            </Button>
                        </Link>
                    </div>
                    <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
                        <div>
                            <h1 className="text-3xl font-bold text-gray-900">AI CV Assistant</h1>
                            <p className="mt-2 text-gray-600">
                                Công nghệ AI giúp CV của bạn nổi bật và thu hút nhà tuyển dụng
                            </p>
                        </div>
                    </div>
                </div>
            </div>

            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
                {/* Development Notice */}
                <Card className="mb-12 border-amber-200 bg-amber-50/50">
                    <CardContent className="p-8">
                        <div className="flex flex-col items-center text-center">
                            <div className="h-20 w-20 rounded-full bg-amber-100 flex items-center justify-center mb-6">
                                <Construction className="h-10 w-10 text-amber-600" />
                            </div>
                            <h2 className="text-2xl font-bold text-gray-900 mb-3">
                                Tính năng đang phát triển
                            </h2>
                            <p className="text-gray-600 max-w-xl mb-6">
                                Chúng tôi đang xây dựng các công cụ AI tiên tiến để giúp bạn tạo và 
                                tối ưu hóa CV. Hãy quay lại sớm để trải nghiệm những tính năng mới!
                            </p>
                            <div className="flex items-center gap-2 text-amber-600">
                                <Clock className="h-5 w-5" />
                                <span className="font-medium">Dự kiến ra mắt: Q2 2025</span>
                            </div>
                        </div>
                    </CardContent>
                </Card>

                {/* Coming Soon Features */}
                <div className="mb-12">
                    <h2 className="text-xl font-bold text-gray-900 mb-6 text-center">
                        Tính năng sắp ra mắt
                    </h2>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                        {upcomingFeatures.map((feature, index) => (
                            <Card
                                key={index}
                                className="opacity-75 hover:opacity-100 transition-opacity"
                            >
                                <CardHeader className="pb-3">
                                    <div className="flex items-start justify-between">
                                        <div className="flex items-center gap-3">
                                            <div className="h-12 w-12 rounded-lg bg-gray-100 flex items-center justify-center">
                                                {feature.icon}
                                            </div>
                                            <div>
                                                <CardTitle className="text-base">
                                                    {feature.title}
                                                </CardTitle>
                                                <Badge variant="secondary" className="mt-1">
                                                    Sắp ra mắt
                                                </Badge>
                                            </div>
                                        </div>
                                    </div>
                                </CardHeader>
                                <CardContent>
                                    <p className="text-sm text-gray-600">
                                        {feature.description}
                                    </p>
                                </CardContent>
                            </Card>
                        ))}
                    </div>
                </div>

                {/* Call to Action */}
                <Card className="bg-gradient-to-br from-[#00b14f]/5 to-[#00a045]/5 border-[#00b14f]/20">
                    <CardContent className="p-8">
                        <div className="flex flex-col md:flex-row items-center justify-between gap-6">
                            <div className="flex items-center gap-4">
                                <div className="h-14 w-14 rounded-full bg-[#00b14f]/10 flex items-center justify-center">
                                    <Wand2 className="h-7 w-7 text-[#00b14f]" />
                                </div>
                                <div>
                                    <h3 className="text-lg font-semibold text-gray-900">
                                        Sẵn sàng cho CV của bạn?
                                    </h3>
                                    <p className="text-gray-600">
                                        Trong lúc chờ đợi, bạn có thể quản lý CV hiện tại
                                    </p>
                                </div>
                            </div>
                            <Link to="/candidate/cv/list">
                                <Button className="bg-[#00b14f] hover:bg-[#00a045] whitespace-nowrap">
                                    <FileText className="h-4 w-4 mr-2" />
                                    Quản lý CV của tôi
                                </Button>
                            </Link>
                        </div>
                    </CardContent>
                </Card>

                {/* Notification Signup */}
                <div className="mt-12 text-center">
                    <p className="text-gray-500 text-sm">
                        Bạn muốn nhận thông báo khi tính năng AI CV ra mắt?
                    </p>
                    <p className="text-gray-400 text-xs mt-1">
                        Thông báo sẽ tự động được gửi đến email đăng ký của bạn
                    </p>
                </div>
            </div>
        </div>
    );
}
