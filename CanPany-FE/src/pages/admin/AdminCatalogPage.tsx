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
            toast.success(t('catalog.toasts.category.create.success'));
            setCategoryCreateName('');
        },
        onError: () => toast.error(t('catalog.toasts.category.create.error')),
    });

    const updateCategory = useMutation({
        mutationFn: () => adminApi.updateCategory(categoryUpdateId.trim(), categoryUpdateName.trim()),
        onSuccess: () => {
            toast.success(t('catalog.toasts.category.update.success'));
            setCategoryUpdateId('');
            setCategoryUpdateName('');
        },
        onError: () => toast.error(t('catalog.toasts.category.update.error')),
    });

    const deleteCategory = useMutation({
        mutationFn: () => adminApi.deleteCategory(categoryDeleteId.trim()),
        onSuccess: () => {
            toast.success(t('catalog.toasts.category.delete.success'));
            setCategoryDeleteId('');
        },
        onError: () => toast.error(t('catalog.toasts.category.delete.error')),
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
            toast.success(t('catalog.toasts.skill.create.success'));
            setSkillCreateName('');
            setSkillCreateCategoryId('');
        },
        onError: () => toast.error(t('catalog.toasts.skill.create.error')),
    });

    const updateSkill = useMutation({
        mutationFn: () =>
            adminApi.updateSkill(
                skillUpdateId.trim(),
                skillUpdateName.trim(),
                skillUpdateCategoryId.trim() || undefined
            ),
        onSuccess: () => {
            toast.success(t('catalog.toasts.skill.update.success'));
            setSkillUpdateId('');
            setSkillUpdateName('');
            setSkillUpdateCategoryId('');
        },
        onError: () => toast.error(t('catalog.toasts.skill.update.error')),
    });

    const deleteSkill = useMutation({
        mutationFn: () => adminApi.deleteSkill(skillDeleteId.trim()),
        onSuccess: () => {
            toast.success(t('catalog.toasts.skill.delete.success'));
            setSkillDeleteId('');
        },
        onError: () => toast.error(t('catalog.toasts.skill.delete.error')),
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
            toast.success(t('catalog.toasts.banner.create.success'));
            setBannerCreateTitle('');
            setBannerCreateImageUrl('');
            setBannerCreateLinkUrl('');
        },
        onError: () => toast.error(t('catalog.toasts.banner.create.error')),
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
            toast.success(t('catalog.toasts.banner.update.success'));
            setBannerUpdateId('');
            setBannerUpdateTitle('');
            setBannerUpdateImageUrl('');
            setBannerUpdateLinkUrl('');
            setBannerUpdateOrder('0');
        },
        onError: () => toast.error(t('catalog.toasts.banner.update.error')),
    });

    const deleteBanner = useMutation({
        mutationFn: () => adminApi.deleteBanner(bannerDeleteId.trim()),
        onSuccess: () => {
            toast.success(t('catalog.toasts.banner.delete.success'));
            setBannerDeleteId('');
        },
        onError: () => toast.error(t('catalog.toasts.banner.delete.error')),
    });

    // Premium package price
    const [packageId, setPackageId] = useState('');
    const [packagePrice, setPackagePrice] = useState<number>(0);

    const updatePackage = useMutation({
        mutationFn: () => adminApi.updatePackagePrice(packageId.trim(), packagePrice),
        onSuccess: () => {
            toast.success(t('catalog.toasts.packagePrice.update.success'));
            setPackageId('');
            setPackagePrice(0);
        },
        onError: () => toast.error(t('catalog.toasts.packagePrice.update.error')),
    });

    return (
        <div className="space-y-6">
            <div>
                <h1 className="text-2xl font-bold text-gray-900">{title}</h1>
                <p className="mt-1 text-sm text-gray-600">{desc}</p>
            </div>

            <div className="grid gap-4 lg:grid-cols-2">
                <Card className="space-y-4 p-5">
                    <h2 className="text-lg font-semibold text-gray-900">{t('catalog.headings.categories')}</h2>

                    <div className="space-y-2">
                        <label className="block text-sm font-medium text-gray-700">{t('catalog.category.create.nameLabel')}</label>
                        <input
                            value={categoryCreateName}
                            onChange={(e) => setCategoryCreateName(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder={t('catalog.category.create.namePlaceholder')}
                        />
                        <Button
                            className="bg-[#00b14f] hover:bg-[#00b14f]/90"
                            disabled={createCategory.isPending || !categoryCreateName.trim()}
                            onClick={() => createCategory.mutate()}
                        >
                            {t('catalog.common.createButton')}
                        </Button>
                    </div>

                    <div className="space-y-2">
                        <label className="block text-sm font-medium text-gray-700">{t('catalog.category.update.idLabel')}</label>
                        <input
                            value={categoryUpdateId}
                            onChange={(e) => setCategoryUpdateId(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder={t('catalog.category.update.idPlaceholder')}
                        />
                        <input
                            value={categoryUpdateName}
                            onChange={(e) => setCategoryUpdateName(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder={t('catalog.category.update.namePlaceholder')}
                        />
                        <Button
                            variant="outline"
                            disabled={updateCategory.isPending || !categoryUpdateId.trim() || !categoryUpdateName.trim()}
                            onClick={() => updateCategory.mutate()}
                        >
                            {t('catalog.common.updateButton')}
                        </Button>
                    </div>

                    <div className="space-y-2">
                        <label className="block text-sm font-medium text-gray-700">{t('catalog.category.delete.idLabel')}</label>
                        <input
                            value={categoryDeleteId}
                            onChange={(e) => setCategoryDeleteId(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder={t('catalog.category.delete.idPlaceholder')}
                        />
                        <Button
                            variant="outline"
                            className="border-red-200 text-red-600 hover:bg-red-50"
                            disabled={deleteCategory.isPending || !categoryDeleteId.trim()}
                            onClick={() => deleteCategory.mutate()}
                        >
                            {t('catalog.common.deleteButton')}
                        </Button>
                    </div>
                </Card>

                <Card className="space-y-4 p-5">
                    <h2 className="text-lg font-semibold text-gray-900">{t('catalog.headings.skills')}</h2>

                    <div className="space-y-2">
                        <label className="block text-sm font-medium text-gray-700">{t('catalog.skill.create.sectionLabel')}</label>
                        <input
                            value={skillCreateName}
                            onChange={(e) => setSkillCreateName(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder={t('catalog.skill.create.namePlaceholder')}
                        />
                        <input
                            value={skillCreateCategoryId}
                            onChange={(e) => setSkillCreateCategoryId(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder={t('catalog.skill.create.categoryOptionalPlaceholder')}
                        />
                        <Button
                            className="bg-[#00b14f] hover:bg-[#00b14f]/90"
                            disabled={createSkill.isPending || !skillCreateName.trim()}
                            onClick={() => createSkill.mutate()}
                        >
                            {t('catalog.common.createButton')}
                        </Button>
                    </div>

                    <div className="space-y-2">
                        <label className="block text-sm font-medium text-gray-700">{t('catalog.skill.update.sectionLabel')}</label>
                        <input
                            value={skillUpdateId}
                            onChange={(e) => setSkillUpdateId(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder={t('catalog.skill.update.idPlaceholder')}
                        />
                        <input
                            value={skillUpdateName}
                            onChange={(e) => setSkillUpdateName(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder={t('catalog.skill.update.namePlaceholder')}
                        />
                        <input
                            value={skillUpdateCategoryId}
                            onChange={(e) => setSkillUpdateCategoryId(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder={t('catalog.skill.update.categoryOptionalPlaceholder')}
                        />
                        <Button
                            variant="outline"
                            disabled={updateSkill.isPending || !skillUpdateId.trim() || !skillUpdateName.trim()}
                            onClick={() => updateSkill.mutate()}
                        >
                            {t('catalog.common.updateButton')}
                        </Button>
                    </div>

                    <div className="space-y-2">
                        <label className="block text-sm font-medium text-gray-700">{t('catalog.skill.delete.sectionLabel')}</label>
                        <input
                            value={skillDeleteId}
                            onChange={(e) => setSkillDeleteId(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder={t('catalog.skill.delete.idPlaceholder')}
                        />
                        <Button
                            variant="outline"
                            className="border-red-200 text-red-600 hover:bg-red-50"
                            disabled={deleteSkill.isPending || !skillDeleteId.trim()}
                            onClick={() => deleteSkill.mutate()}
                        >
                            {t('catalog.common.deleteButton')}
                        </Button>
                    </div>
                </Card>

                <Card className="space-y-4 p-5">
                    <h2 className="text-lg font-semibold text-gray-900">{t('catalog.headings.banners')}</h2>

                    <div className="space-y-2">
                        <label className="block text-sm font-medium text-gray-700">{t('catalog.banner.create.sectionLabel')}</label>
                        <input
                            value={bannerCreateTitle}
                            onChange={(e) => setBannerCreateTitle(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder={t('catalog.banner.create.titlePlaceholder')}
                        />
                        <input
                            value={bannerCreateImageUrl}
                            onChange={(e) => setBannerCreateImageUrl(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder={t('catalog.banner.create.imagePlaceholder')}
                        />
                        <input
                            value={bannerCreateLinkUrl}
                            onChange={(e) => setBannerCreateLinkUrl(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder={t('catalog.banner.create.linkOptionalPlaceholder')}
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
                            {t('catalog.common.activeLabel')}
                        </label>
                        <Button
                            className="bg-[#00b14f] hover:bg-[#00b14f]/90"
                            disabled={createBanner.isPending || !bannerCreateTitle.trim() || !bannerCreateImageUrl.trim()}
                            onClick={() => createBanner.mutate()}
                        >
                            {t('catalog.banner.create.submitButton')}
                        </Button>
                    </div>

                    <div className="space-y-2">
                        <label className="block text-sm font-medium text-gray-700">{t('catalog.banner.update.sectionLabel')}</label>
                        <input
                            value={bannerUpdateId}
                            onChange={(e) => setBannerUpdateId(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder={t('catalog.banner.update.idPlaceholder')}
                        />
                        <input
                            value={bannerUpdateTitle}
                            onChange={(e) => setBannerUpdateTitle(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder={t('catalog.banner.update.titleOptionalPlaceholder')}
                        />
                        <input
                            value={bannerUpdateImageUrl}
                            onChange={(e) => setBannerUpdateImageUrl(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder={t('catalog.banner.update.imageOptionalPlaceholder')}
                        />
                        <input
                            value={bannerUpdateLinkUrl}
                            onChange={(e) => setBannerUpdateLinkUrl(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder={t('catalog.banner.update.linkOptionalPlaceholder')}
                        />
                        <input
                            value={bannerUpdateOrder}
                            onChange={(e) => setBannerUpdateOrder(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder={t('catalog.banner.update.orderOptionalPlaceholder')}
                        />
                        <label className="flex items-center gap-2 text-sm text-gray-700">
                            <input
                                type="checkbox"
                                checked={bannerUpdateIsActive}
                                onChange={(e) => setBannerUpdateIsActive(e.target.checked)}
                            />
                            {t('catalog.common.activeLabel')}
                        </label>
                        <Button
                            variant="outline"
                            disabled={updateBanner.isPending || !bannerUpdateId.trim()}
                            onClick={() => updateBanner.mutate()}
                        >
                            {t('catalog.common.updateButton')}
                        </Button>
                    </div>

                    <div className="space-y-2">
                        <label className="block text-sm font-medium text-gray-700">{t('catalog.banner.delete.sectionLabel')}</label>
                        <input
                            value={bannerDeleteId}
                            onChange={(e) => setBannerDeleteId(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder={t('catalog.banner.delete.idPlaceholder')}
                        />
                        <Button
                            variant="outline"
                            className="border-red-200 text-red-600 hover:bg-red-50"
                            disabled={deleteBanner.isPending || !bannerDeleteId.trim()}
                            onClick={() => deleteBanner.mutate()}
                        >
                            {t('catalog.common.deleteButton')}
                        </Button>
                    </div>
                </Card>

                <Card className="space-y-4 p-5">
                    <h2 className="text-lg font-semibold text-gray-900">{t('catalog.headings.premiumPackagePrice')}</h2>
                    <div className="space-y-2">
                        <label className="block text-sm font-medium text-gray-700">{t('catalog.packagePrice.packageIdLabel')}</label>
                        <input
                            value={packageId}
                            onChange={(e) => setPackageId(e.target.value)}
                            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                            placeholder={t('catalog.packagePrice.packageIdPlaceholder')}
                        />
                        <label className="block text-sm font-medium text-gray-700">{t('catalog.packagePrice.priceLabel')}</label>
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
                            {t('catalog.packagePrice.updatePriceButton')}
                        </Button>
                    </div>
                </Card>
            </div>
        </div>
    );
}

