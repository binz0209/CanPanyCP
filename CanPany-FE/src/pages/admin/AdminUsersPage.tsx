import { useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import { adminApi } from '../../api';
import { Button, Card } from '../../components/ui';
import { adminKeys } from '../../lib/queryKeys';
import type { UserRole } from '../../types';
import type adminEn from '../../i18n/locales/en/admin.json';

const ROLE_FILTERS: Array<UserRole | 'all'> = ['all', 'Candidate', 'Company', 'Admin', 'Guest'];

type UserRoleLabelKey = `users.roles.${keyof typeof adminEn.users.roles}`;

export function AdminUsersPage() {
    const { t } = useTranslation('admin');
    const { t: tCommon } = useTranslation('common');
    const queryClient = useQueryClient();
    const [search, setSearch] = useState('');
    const [roleFilter, setRoleFilter] = useState<UserRole | 'all'>('all');

    const usersQuery = useQuery({
        queryKey: adminKeys.users(),
        queryFn: () => adminApi.getUsers(),
    });

    const banMutation = useMutation({
        mutationFn: (id: string) => adminApi.banUser(id),
        onSuccess: () => {
            void queryClient.invalidateQueries({ queryKey: adminKeys.users() });
            toast.success(t('users.banSuccess'));
        },
        onError: () => toast.error(t('users.actionError')),
    });

    const unbanMutation = useMutation({
        mutationFn: (id: string) => adminApi.unbanUser(id),
        onSuccess: () => {
            void queryClient.invalidateQueries({ queryKey: adminKeys.users() });
            toast.success(t('users.unbanSuccess'));
        },
        onError: () => toast.error(t('users.actionError')),
    });

    const filtered = useMemo(() => {
        const list = usersQuery.data ?? [];
        const q = search.trim().toLowerCase();
        return list.filter((u) => {
            if (roleFilter !== 'all' && u.role !== roleFilter) return false;
            if (!q) return true;
            return (
                u.fullName.toLowerCase().includes(q) ||
                u.email.toLowerCase().includes(q)
            );
        });
    }, [usersQuery.data, search, roleFilter]);

    if (usersQuery.isError) {
        return (
            <div className="rounded-xl border border-red-100 bg-red-50 p-6 text-sm text-red-800">
                {t('users.loadError')}
            </div>
        );
    }

    const roleLabel = (role: UserRole) => t(`users.roles.${role}` as UserRoleLabelKey);

    return (
        <div className="space-y-6">
            <div>
                <h1 className="text-2xl font-bold text-gray-900">{t('users.title')}</h1>
                <p className="mt-1 text-sm text-gray-600">{t('users.subtitle')}</p>
            </div>

            <Card className="p-4">
                <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
                    <input
                        type="search"
                        value={search}
                        onChange={(e) => setSearch(e.target.value)}
                        placeholder={t('users.searchPlaceholder')}
                        className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f] sm:max-w-md"
                    />
                    <div className="flex items-center gap-2">
                        <label htmlFor="admin-user-role" className="text-sm text-gray-600">
                            {t('users.filterRole')}
                        </label>
                        <select
                            id="admin-user-role"
                            value={roleFilter}
                            onChange={(e) => setRoleFilter(e.target.value as UserRole | 'all')}
                            className="rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-[#00b14f]"
                        >
                            {ROLE_FILTERS.map((r) => (
                                <option key={r} value={r}>
                                    {r === 'all' ? t('users.allRoles') : roleLabel(r)}
                                </option>
                            ))}
                        </select>
                    </div>
                </div>
            </Card>

            <Card className="overflow-hidden p-0">
                <div className="overflow-x-auto">
                    <table className="w-full min-w-[720px] text-left text-sm">
                        <thead className="border-b border-gray-100 bg-gray-50 text-xs font-semibold uppercase text-gray-500">
                            <tr>
                                <th className="px-4 py-3">{t('users.name')}</th>
                                <th className="px-4 py-3">{t('users.email')}</th>
                                <th className="px-4 py-3">{t('users.role')}</th>
                                <th className="px-4 py-3">{t('users.status')}</th>
                                <th className="px-4 py-3 text-right">{/* actions */}</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-gray-100">
                            {usersQuery.isLoading ? (
                                <tr>
                                    <td colSpan={5} className="px-4 py-12 text-center text-gray-500">
                                        {tCommon('app.loading')}
                                    </td>
                                </tr>
                            ) : filtered.length === 0 ? (
                                <tr>
                                    <td colSpan={5} className="px-4 py-12 text-center text-gray-500">
                                        {t('users.empty')}
                                    </td>
                                </tr>
                            ) : (
                                filtered.map((u) => {
                                    const banned = u.isLocked === true;
                                    const busy =
                                        banMutation.isPending ||
                                        unbanMutation.isPending;
                                    return (
                                        <tr key={u.id} className="hover:bg-gray-50/80">
                                            <td className="px-4 py-3 font-medium text-gray-900">{u.fullName}</td>
                                            <td className="px-4 py-3 text-gray-600">{u.email}</td>
                                            <td className="px-4 py-3">
                                                <span className="rounded-full bg-gray-100 px-2.5 py-0.5 text-xs font-medium text-gray-700">
                                                    {roleLabel(u.role)}
                                                </span>
                                            </td>
                                            <td className="px-4 py-3">
                                                <span
                                                    className={
                                                        banned
                                                            ? 'text-red-600 font-medium'
                                                            : 'text-emerald-600 font-medium'
                                                    }
                                                >
                                                    {banned ? t('users.statusBanned') : t('users.statusActive')}
                                                </span>
                                            </td>
                                            <td className="px-4 py-3 text-right">
                                                {u.role !== 'Admin' && (
                                                    banned ? (
                                                        <Button
                                                            size="sm"
                                                            variant="outline"
                                                            className="text-emerald-700"
                                                            disabled={busy}
                                                            onClick={() => unbanMutation.mutate(u.id)}
                                                        >
                                                            {t('users.unban')}
                                                        </Button>
                                                    ) : (
                                                        <Button
                                                            size="sm"
                                                            variant="outline"
                                                            className="text-red-600 border-red-200 hover:bg-red-50"
                                                            disabled={busy}
                                                            onClick={() => banMutation.mutate(u.id)}
                                                        >
                                                            {t('users.ban')}
                                                        </Button>
                                                    )
                                                )}
                                            </td>
                                        </tr>
                                    );
                                })
                            )}
                        </tbody>
                    </table>
                </div>
            </Card>
        </div>
    );
}
