import type { Control } from 'react-hook-form';
import { useWatch } from 'react-hook-form';
import { Card } from '../../ui';
import type { CompanyJobFormValues } from './JobFormFields';

interface CompanyJobPreviewCardProps {
    control: Control<CompanyJobFormValues>;
}

export function CompanyJobPreviewCard({ control }: CompanyJobPreviewCardProps) {
    const [previewTitle, previewLocation, previewIsRemote, previewSkills, previewDescription] = useWatch({
        control,
        name: ['title', 'location', 'isRemote', 'skillIdsText', 'description'],
    });

    return (
        <Card className="p-6">
            <h2 className="text-lg font-semibold text-gray-900">Preview nhanh</h2>
            <div className="mt-4 space-y-3">
                <div>
                    <p className="text-sm text-gray-500">Tiêu đề</p>
                    <p className="mt-1 font-semibold text-gray-900">{previewTitle || 'Chưa có tiêu đề'}</p>
                </div>
                <div>
                    <p className="text-sm text-gray-500">Location</p>
                    <p className="mt-1 text-sm text-gray-700">
                        {previewLocation || (previewIsRemote ? 'Remote' : 'Chưa cập nhật')}
                    </p>
                </div>
                <div>
                    <p className="text-sm text-gray-500">Skills</p>
                    <p className="mt-1 text-sm text-gray-700">
                        {previewSkills || 'Chưa có skills'}
                    </p>
                </div>
                <div>
                    <p className="text-sm text-gray-500">Mô tả</p>
                    <p className="mt-1 whitespace-pre-wrap text-sm leading-6 text-gray-700">
                        {previewDescription || 'Chưa có mô tả'}
                    </p>
                </div>
            </div>
        </Card>
    );
}
