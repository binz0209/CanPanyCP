import { Link } from 'react-router-dom';
import { Briefcase, Facebook, Linkedin, Mail, Phone, MapPin } from 'lucide-react';

export function Footer() {
    return (
        <footer className="border-t border-gray-100 bg-gray-50">
            <div className="mx-auto max-w-7xl px-4 py-12 sm:px-6 lg:px-8">
                <div className="grid grid-cols-1 gap-8 md:grid-cols-4">
                    {/* Brand */}
                    <div className="col-span-1 md:col-span-2">
                        <Link to="/" className="flex items-center gap-2">
                            <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-gradient-to-br from-[#00b14f] to-[#008f3c]">
                                <Briefcase className="h-5 w-5 text-white" />
                            </div>
                            <span className="text-xl font-bold text-gray-900">
                                Can<span className="text-[#00b14f]">Pany</span>
                            </span>
                        </Link>
                        <p className="mt-4 max-w-md text-sm leading-relaxed text-gray-600">
                            Nền tảng tuyển dụng thông minh được hỗ trợ bởi AI, kết nối ứng viên tài năng với các công ty hàng đầu Việt Nam.
                        </p>
                        <div className="mt-6 flex gap-3">
                            <a
                                href="#"
                                className="flex h-10 w-10 items-center justify-center rounded-full bg-gray-100 text-gray-600 transition-colors hover:bg-[#00b14f] hover:text-white"
                            >
                                <Facebook className="h-5 w-5" />
                            </a>
                            <a
                                href="#"
                                className="flex h-10 w-10 items-center justify-center rounded-full bg-gray-100 text-gray-600 transition-colors hover:bg-[#00b14f] hover:text-white"
                            >
                                <Linkedin className="h-5 w-5" />
                            </a>
                        </div>
                    </div>

                    {/* Quick Links */}
                    <div>
                        <h3 className="text-sm font-semibold uppercase tracking-wider text-gray-900">Dành cho ứng viên</h3>
                        <ul className="mt-4 space-y-3">
                            <li>
                                <Link to="/jobs" className="text-sm text-gray-600 transition-colors hover:text-[#00b14f]">
                                    Tìm việc làm
                                </Link>
                            </li>
                            <li>
                                <Link to="/companies" className="text-sm text-gray-600 transition-colors hover:text-[#00b14f]">
                                    Danh sách công ty
                                </Link>
                            </li>
                            <li>
                                <Link to="/auth/register" className="text-sm text-gray-600 transition-colors hover:text-[#00b14f]">
                                    Tạo CV online
                                </Link>
                            </li>
                            <li>
                                <Link to="/auth/register" className="text-sm text-gray-600 transition-colors hover:text-[#00b14f]">
                                    Cẩm nang nghề nghiệp
                                </Link>
                            </li>
                        </ul>
                    </div>

                    {/* Contact */}
                    <div>
                        <h3 className="text-sm font-semibold uppercase tracking-wider text-gray-900">Liên hệ</h3>
                        <ul className="mt-4 space-y-3">
                            <li className="flex items-center gap-2 text-sm text-gray-600">
                                <div className="flex h-8 w-8 items-center justify-center rounded-full bg-[#00b14f]/10">
                                    <Mail className="h-4 w-4 text-[#00b14f]" />
                                </div>
                                support@canpany.vn
                            </li>
                            <li className="flex items-center gap-2 text-sm text-gray-600">
                                <div className="flex h-8 w-8 items-center justify-center rounded-full bg-[#00b14f]/10">
                                    <Phone className="h-4 w-4 text-[#00b14f]" />
                                </div>
                                1900 xxxx xx
                            </li>
                            <li className="flex items-start gap-2 text-sm text-gray-600">
                                <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-[#00b14f]/10">
                                    <MapPin className="h-4 w-4 text-[#00b14f]" />
                                </div>
                                Quận 1, TP. Hồ Chí Minh
                            </li>
                        </ul>
                    </div>
                </div>

                <div className="mt-12 border-t border-gray-200 pt-8">
                    <div className="flex flex-col items-center justify-between gap-4 sm:flex-row">
                        <p className="text-sm text-gray-500">
                            © {new Date().getFullYear()} CanPany. All rights reserved.
                        </p>
                        <div className="flex gap-6">
                            <a href="#" className="text-sm text-gray-500 hover:text-[#00b14f]">Điều khoản</a>
                            <a href="#" className="text-sm text-gray-500 hover:text-[#00b14f]">Bảo mật</a>
                            <a href="#" className="text-sm text-gray-500 hover:text-[#00b14f]">Trợ giúp</a>
                        </div>
                    </div>
                </div>
            </div>
        </footer>
    );
}
