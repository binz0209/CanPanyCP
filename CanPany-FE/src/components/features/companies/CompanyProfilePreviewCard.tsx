import { Building2 } from 'lucide-react';
import { Card } from '../../ui';

interface CompanyProfilePreviewCardProps {
    logoUrl?: string;
    name?: string;
    address?: string;
    description?: string;
}

export function CompanyProfilePreviewCard({
    logoUrl,
    name,
    address,
    description,
}: CompanyProfilePreviewCardProps) {
    return (
        <Card className="p-6">
            <h2 className="text-lg font-semibold text-gray-900">Xem trước hồ sơ</h2>
            <div className="mt-5 flex items-start gap-4">
                {logoUrl ? (
                    <img
                        src={logoUrl}
                        alt="Logo preview"
                        className="h-16 w-16 rounded-xl border border-gray-200 object-cover"
                    />
                ) : (
                    <div className="flex h-16 w-16 items-center justify-center rounded-xl bg-[#00b14f]/10 text-[#00b14f]">
                        <Building2 className="h-8 w-8" />
                    </div>
                )}

                <div className="min-w-0">
                    <p className="truncate font-semibold text-gray-900">{name || 'Tên công ty'}</p>
                    <p className="mt-1 text-sm text-gray-500">
                        {address || 'Địa chỉ sẽ hiển thị ở đây'}
                    </p>
                    <p className="mt-2 line-clamp-4 text-sm leading-6 text-gray-600">
                        {description || 'Mô tả công ty sẽ hiển thị ở đây'}
                    </p>
                </div>
            </div>
        </Card>
    );
}
