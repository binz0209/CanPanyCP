import { useMutation } from '@tanstack/react-query';
import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import { Button, Card } from '../../components/ui';
import { adminApi } from '../../api';

export function AdminCatalogPage() {
    const { t } = useTranslation('admin');

    const title = t('placeholders.catalog.title');
    const desc = t('placeholders.catalog.description');

    // Category
    const [categoryCreateName, setCategoryCreateName] = useState('');
    const [categoryUpdateId, setCategoryUpdateId] = useState('');
    const [categoryUpdateName, setCategoryUpdateName] = useState('');
    const [categoryDeleteId, setCategoryDeleteId] = useState('');

    const createCategory = useMutation({
        mutationFn: () => adminApi.createCategory(categoryCreateName.trim()),
        onSuccess: () => {
            toast.success('Đã tạo category.');
            setCategoryCreateName('');
        },
        onError: () => toast.error('Không thể tạo category.'),
    });

    const updateCategory = useMutation({
        mutationFn: () => adminApi.updateCategory(categoryUpdateId.trim(), categoryUpdateName.trim()),
        onSuccess: () => {
            toast.success('Đã cập nhật category.');
            setCategoryUpdateId('');
            setCategoryUpdateName('');
        },
        onError: () => toast.error('Không thể cập nhật category.'),
    });

    const deleteCategory = useMutation({
        mutationFn: () => adminApi.deleteCategory(categoryDeleteId.trim()),
        onSuccess: () => {
            toast.success('Đã xóa category.');
            setCategoryDeleteId('');
        },
        onError: () => toast.error('Không thể xóa category.'),
    });

    // Skill
    const [skillCreateName, setSkillCreateName] = useState('');
    const [skillCreateCategoryId, setSkillCreateCategoryId] = useState('');
    const [skillUpdateId, setSkillUpdateId] = useState('');
    const [skillUpdateName, setSkillUpdateName] = useState('');
    const [skillUpdateCategoryId, setSkillUpdateCategoryId] = useState('');
    const [skillDeleteId, setSkillDeleteId] = useState('');

    const createSkill = useMutation({
        mutationFn: () =>
            adminApi.createSkill(
                skillCreateName.trim(),
                skillCreateCategoryId.trim() || undefined
            ),
        onSuccess: () => {
            toast.success('Đã tạo skill.');
            setSkillCreateName('');
            setSkillCreateCategoryId('');
        },
        onError: () => toast.error('Không thể tạo skill.'),
    });

    const updateSkill = useMutation({
        mutationFn: () =>
            adminApi.updateSkill(
                skillUpdateId.trim(),
                skillUpdateName.trim(),
                skillUpdateCategoryId.trim() || undefined
            ),
        onSuccess: () => {
            toast.success('Đã cập nhật skill.');
            setSkillUpdateId('');
            setSkillUpdateName('');
            setSkillUpdateCategoryId('');
        },
        onError: () => toast.error('Không thể cập nhật skill.'),
    });

    const deleteSkill = useMutation({
        mutationFn: () => adminApi.deleteSkill(skillDeleteId.trim()),
        onSuccess: () => {
            toast.success('Đã xóa skill.');
            setSkillDeleteId('');
        },
        onError: () => toast.error('Không thể xóa skill.'),
    });

    // Banner
    const [bannerCreateTitle, setBannerCreateTitle] = useState('');
    const [bannerCreateImageUrl, setBannerCreateImageUrl] = useState('');
    const [bannerCreateLinkUrl, setBannerCreateLinkUrl] = useState('');
    const [bannerCreateOrder, setBannerCreateOrder] = useState<number>(0);
    const [bannerCreateIsActive, setBannerCreateIsActive] = useState(true);

    const [bannerUpdateId, setBannerUpdateId] = useState('');
    const [bannerUpdateTitle, setBannerUpdateTitle] = useState('');
    const [bannerUpdateImageUrl, setBannerUpdateImageUrl] = useState('');
    const [bannerUpdateLinkUrl, setBannerUpdateLinkUrl] = useState('');
    const [bannerUpdateOrder, setBannerUpdateOrder] = useState<string>('0');
    const [bannerUpdateIsActive, setBannerUpdateIsActive] = useState<boolean>(true);

    const [bannerDeleteId, setBannerDeleteId] = useState('');

    const createBanner = useMutation({
        mutationFn: () =>
            adminApi.createBanner({
                title: bannerCreateTitle.trim(),
                imageUrl: bannerCreateImageUrl.trim(),
                linkUrl: bannerCreateLinkUrl.trim() || undefined,
                order: bannerCreateOrder,
                isActive: bannerCreateIsActive,
            }),
        onSuccess: () => {
            toast.success('Đã tạo banner.');
            setBannerCreateTitle('');
            setBannerCreateImageUrl('');
            setBannerCreateLinkUrl('');
        },
        onError: () => toast.error('Không thể tạo banner.'),
    });

    const updateBanner = useMutation({
        mutationFn: () => {
            const payload: any = {
                title: bannerUpdateTitle.trim() || undefined,
                imageUrl: bannerUpdateImageUrl.trim() || undefined,
                linkUrl: bannerUpdateLinkUrl.trim() || undefined,
                order:
                    bannerUpdateOrder.trim() === ''
                        ? undefined
                        : Number(bannerUpdateOrder.trim()),
                isActive: bannerUpdateIsActive,
            };
            // Remove undefined keys so BE nhận đúng optional fields
            Object.keys(payload).forEach((k) => payload[k] === undefined && delete payload[k]);
            return adminApi.updateBanner(bannerUpdateId.trim(), payload);
        },
        onSuccess: () => {
            toast.success('Đã cập nhật banner.');
            setBannerUpdateId('');
            setBannerUpdateTitle('');
            setBannerUpdateImageUrl('');
            setBannerUpdateLinkUrl('');
            setBannerUpdateOrder('0');
        },
        onError: () => toast.error('Không thể cập nhật banner.'),
    });

    const deleteBanner = useMutation({
        mutationFn: () => adminApi.deleteBanner(bannerDeleteId.trim()),
        onSuccess: () => {
            toast.success('Đã xóa banner.');
            setBannerDeleteId('');
        },
        onError: () => toast.error('Không thể xóa banner.'),
    });

    // Premium package price
    const [packageId, setPackageId] = useState('');
    const [packagePrice, setPackagePrice] = useState<number>(0);

    const updatePackage = useMutation({
        mutationFn: () => adminApi.updatePackagePrice(packageId.trim(), packagePrice),
        onSuccess: () => {
            toast.success('Đã cập nhật giá gói premium.');
            setPackageId('');
            setPackagePrice(0);
        },
        onError: () => toast.error('Không thể cập nhật giá gói.'),
    });

    return (
        <div className="space-y-6">
            <div>
                <h1 className="text-2xl font-bold text-gray-900">{title}</h1>
                <p className="mt-1 text-sm text-gray-600">{desc}</p>
            </div>

            <div className="grid gap-4 lg:grid-cols-2">
                <Card className="space-y-4 p-5">
                    <h2 className="text-lg font-semibold text-gray-900">Categories</h2>

                    <div className="space-y-2">
                        <label className="block text-sm font-medium text-gray-700">Create category name</label>
                        <input
                            value={categoryCreateName}
                            onChange={(e) => setCategoryCreateName(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder="VD: Technology"
                        />
                        <Button
                            className="bg-[#00b14f] hover:bg-[#00b14f]/90"
                            disabled={createCategory.isPending || !categoryCreateName.trim()}
                            onClick={() => createCategory.mutate()}
                        >
                            Tạo
                        </Button>
                    </div>

                    <div className="space-y-2">
                        <label className="block text-sm font-medium text-gray-700">Update category</label>
                        <input
                            value={categoryUpdateId}
                            onChange={(e) => setCategoryUpdateId(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder="Category ID"
                        />
                        <input
                            value={categoryUpdateName}
                            onChange={(e) => setCategoryUpdateName(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder="New name"
                        />
                        <Button
                            variant="outline"
                            disabled={updateCategory.isPending || !categoryUpdateId.trim() || !categoryUpdateName.trim()}
                            onClick={() => updateCategory.mutate()}
                        >
                            Cập nhật
                        </Button>
                    </div>

                    <div className="space-y-2">
                        <label className="block text-sm font-medium text-gray-700">Delete category</label>
                        <input
                            value={categoryDeleteId}
                            onChange={(e) => setCategoryDeleteId(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder="Category ID"
                        />
                        <Button
                            variant="outline"
                            className="border-red-200 text-red-600 hover:bg-red-50"
                            disabled={deleteCategory.isPending || !categoryDeleteId.trim()}
                            onClick={() => deleteCategory.mutate()}
                        >
                            Xóa
                        </Button>
                    </div>
                </Card>

                <Card className="space-y-4 p-5">
                    <h2 className="text-lg font-semibold text-gray-900">Skills</h2>

                    <div className="space-y-2">
                        <label className="block text-sm font-medium text-gray-700">Create skill</label>
                        <input
                            value={skillCreateName}
                            onChange={(e) => setSkillCreateName(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder="Skill name"
                        />
                        <input
                            value={skillCreateCategoryId}
                            onChange={(e) => setSkillCreateCategoryId(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder="Category ID (optional)"
                        />
                        <Button
                            className="bg-[#00b14f] hover:bg-[#00b14f]/90"
                            disabled={createSkill.isPending || !skillCreateName.trim()}
                            onClick={() => createSkill.mutate()}
                        >
                            Tạo
                        </Button>
                    </div>

                    <div className="space-y-2">
                        <label className="block text-sm font-medium text-gray-700">Update skill</label>
                        <input
                            value={skillUpdateId}
                            onChange={(e) => setSkillUpdateId(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder="Skill ID"
                        />
                        <input
                            value={skillUpdateName}
                            onChange={(e) => setSkillUpdateName(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder="New skill name"
                        />
                        <input
                            value={skillUpdateCategoryId}
                            onChange={(e) => setSkillUpdateCategoryId(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder="Category ID (optional)"
                        />
                        <Button
                            variant="outline"
                            disabled={updateSkill.isPending || !skillUpdateId.trim() || !skillUpdateName.trim()}
                            onClick={() => updateSkill.mutate()}
                        >
                            Cập nhật
                        </Button>
                    </div>

                    <div className="space-y-2">
                        <label className="block text-sm font-medium text-gray-700">Delete skill</label>
                        <input
                            value={skillDeleteId}
                            onChange={(e) => setSkillDeleteId(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder="Skill ID"
                        />
                        <Button
                            variant="outline"
                            className="border-red-200 text-red-600 hover:bg-red-50"
                            disabled={deleteSkill.isPending || !skillDeleteId.trim()}
                            onClick={() => deleteSkill.mutate()}
                        >
                            Xóa
                        </Button>
                    </div>
                </Card>

                <Card className="space-y-4 p-5">
                    <h2 className="text-lg font-semibold text-gray-900">Banners</h2>

                    <div className="space-y-2">
                        <label className="block text-sm font-medium text-gray-700">Create banner</label>
                        <input
                            value={bannerCreateTitle}
                            onChange={(e) => setBannerCreateTitle(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder="Title"
                        />
                        <input
                            value={bannerCreateImageUrl}
                            onChange={(e) => setBannerCreateImageUrl(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder="Image URL"
                        />
                        <input
                            value={bannerCreateLinkUrl}
                            onChange={(e) => setBannerCreateLinkUrl(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder="Link URL (optional)"
                        />
                        <input
                            value={bannerCreateOrder}
                            onChange={(e) => setBannerCreateOrder(Number(e.target.value))}
                            type="number"
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                        />
                        <label className="flex items-center gap-2 text-sm text-gray-700">
                            <input
                                type="checkbox"
                                checked={bannerCreateIsActive}
                                onChange={(e) => setBannerCreateIsActive(e.target.checked)}
                            />
                            Active
                        </label>
                        <Button
                            className="bg-[#00b14f] hover:bg-[#00b14f]/90"
                            disabled={createBanner.isPending || !bannerCreateTitle.trim() || !bannerCreateImageUrl.trim()}
                            onClick={() => createBanner.mutate()}
                        >
                            Tạo banner
                        </Button>
                    </div>

                    <div className="space-y-2">
                        <label className="block text-sm font-medium text-gray-700">Update banner</label>
                        <input
                            value={bannerUpdateId}
                            onChange={(e) => setBannerUpdateId(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder="Banner ID"
                        />
                        <input
                            value={bannerUpdateTitle}
                            onChange={(e) => setBannerUpdateTitle(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder="Title (optional)"
                        />
                        <input
                            value={bannerUpdateImageUrl}
                            onChange={(e) => setBannerUpdateImageUrl(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder="Image URL (optional)"
                        />
                        <input
                            value={bannerUpdateLinkUrl}
                            onChange={(e) => setBannerUpdateLinkUrl(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder="Link URL (optional)"
                        />
                        <input
                            value={bannerUpdateOrder}
                            onChange={(e) => setBannerUpdateOrder(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder="Order (optional)"
                        />
                        <label className="flex items-center gap-2 text-sm text-gray-700">
                            <input
                                type="checkbox"
                                checked={bannerUpdateIsActive}
                                onChange={(e) => setBannerUpdateIsActive(e.target.checked)}
                            />
                            Active
                        </label>
                        <Button
                            variant="outline"
                            disabled={updateBanner.isPending || !bannerUpdateId.trim()}
                            onClick={() => updateBanner.mutate()}
                        >
                            Cập nhật
                        </Button>
                    </div>

                    <div className="space-y-2">
                        <label className="block text-sm font-medium text-gray-700">Delete banner</label>
                        <input
                            value={bannerDeleteId}
                            onChange={(e) => setBannerDeleteId(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder="Banner ID"
                        />
                        <Button
                            variant="outline"
                            className="border-red-200 text-red-600 hover:bg-red-50"
                            disabled={deleteBanner.isPending || !bannerDeleteId.trim()}
                            onClick={() => deleteBanner.mutate()}
                        >
                            Xóa
                        </Button>
                    </div>
                </Card>

                <Card className="space-y-4 p-5">
                    <h2 className="text-lg font-semibold text-gray-900">Premium package price</h2>
                    <div className="space-y-2">
                        <label className="block text-sm font-medium text-gray-700">Package ID</label>
                        <input
                            value={packageId}
                            onChange={(e) => setPackageId(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder="PremiumPackage ID"
                        />
                        <label className="block text-sm font-medium text-gray-700">Price (VND)</label>
                        <input
                            type="number"
                            value={packagePrice}
                            onChange={(e) => setPackagePrice(Number(e.target.value))}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                        />
                        <Button
                            className="bg-[#00b14f] hover:bg-[#00b14f]/90"
                            disabled={updatePackage.isPending || !packageId.trim() || packagePrice <= 0}
                            onClick={() => updatePackage.mutate()}
                        >
                            Cập nhật giá
                        </Button>
                    </div>
                </Card>
            </div>
        </div>
    );
}

