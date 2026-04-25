import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import toast from 'react-hot-toast';
import {
    Tags, Wrench, Image, Package, Plus, Pencil, Trash2, Check, X,
    Loader2, Search
} from 'lucide-react';
import { Button, Badge, Card } from '../../components/ui';
import { adminApi } from '../../api';

type Tab = 'categories' | 'skills' | 'banners' | 'packages';

const TABS: { id: Tab; label: string; Icon: any }[] = [
    { id: 'categories', label: 'Categories', Icon: Tags },
    { id: 'skills',     label: 'Skills',      Icon: Wrench },
    { id: 'banners',    label: 'Banners',      Icon: Image },
    { id: 'packages',   label: 'Premium Pkgs', Icon: Package },
];

/* ─── tiny shared input ─── */
function Field({ value, onChange, placeholder, type = 'text' }: {
    value: string; onChange: (v: string) => void; placeholder?: string; type?: string;
}) {
    return (
        <input
            type={type}
            value={value}
            onChange={(e) => onChange(e.target.value)}
            placeholder={placeholder}
            className="w-full rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-slate-900"
        />
    );
}

/* ─── inline edit row ─── */
function EditRow({ onSave, onCancel, children }: { onSave: () => void; onCancel: () => void; children: React.ReactNode }) {
    return (
        <div className="flex items-center gap-2">
            <div className="flex-1 flex gap-2">{children}</div>
            <button onClick={onSave} className="rounded p-1.5 text-green-600 hover:bg-green-50"><Check className="h-4 w-4" /></button>
            <button onClick={onCancel} className="rounded p-1.5 text-gray-400 hover:bg-gray-100"><X className="h-4 w-4" /></button>
        </div>
    );
}

/* ═══════════════════════════════╗
   CATEGORIES TAB
╚═══════════════════════════════ */
function CategoriesTab() {
    const qc = useQueryClient();
    const [search, setSearch] = useState('');
    const [createName, setCreateName] = useState('');
    const [editId, setEditId] = useState<string | null>(null);
    const [editName, setEditName] = useState('');

    const { data: categories = [], isLoading } = useQuery({
        queryKey: ['admin-categories'],
        queryFn: () => adminApi.getCategories(),
    });


    const createMut = useMutation({
        mutationFn: () => adminApi.createCategory(createName.trim()),
        onSuccess: () => { toast.success('Category created'); setCreateName(''); qc.invalidateQueries({ queryKey: ['admin-categories'] }); },
        onError: () => toast.error('Failed to create'),
    });
    const updateMut = useMutation({
        mutationFn: () => adminApi.updateCategory(editId!, editName.trim()),
        onSuccess: () => { toast.success('Updated'); setEditId(null); qc.invalidateQueries({ queryKey: ['admin-categories'] }); },
        onError: () => toast.error('Failed to update'),
    });
    const deleteMut = useMutation({
        mutationFn: (id: string) => adminApi.deleteCategory(id),
        onSuccess: () => { toast.success('Deleted'); qc.invalidateQueries({ queryKey: ['admin-categories'] }); },
        onError: () => toast.error('Failed to delete'),
    });

    const filtered = categories.filter((c: any) => (c.name ?? '').toLowerCase().includes(search.toLowerCase()));

    return (
        <div className="space-y-4">
            {/* Create */}
            <div className="flex gap-2">
                <input value={createName} onChange={(e) => setCreateName(e.target.value)} placeholder="New category name…"
                    className="flex-1 rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-slate-900"
                    onKeyDown={(e) => e.key === 'Enter' && createName.trim() && createMut.mutate()}
                />
                <Button className="bg-slate-900 hover:bg-slate-800 text-white" disabled={!createName.trim() || createMut.isPending}
                    onClick={() => createMut.mutate()}>
                    <Plus className="h-4 w-4 mr-1" /> Add
                </Button>
            </div>
            {/* Search */}
            <div className="relative">
                <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-gray-400" />
                <input value={search} onChange={(e) => setSearch(e.target.value)} placeholder="Filter…"
                    className="w-full rounded-lg border border-gray-200 py-2 pl-9 pr-3 text-sm outline-none focus:border-slate-900" />
            </div>
            {/* List */}
            {isLoading ? <div className="py-8 text-center"><Loader2 className="h-5 w-5 animate-spin text-gray-400 mx-auto" /></div> : (
                <div className="divide-y divide-gray-50 rounded-xl border border-gray-100">
                    {filtered.length === 0 ? <div className="py-8 text-center text-sm text-gray-400">No categories</div> : filtered.map((c: any) => (
                        <div key={c.id} className="flex items-center gap-3 px-4 py-3 hover:bg-gray-50/80">
                            {editId === c.id ? (
                                <EditRow onSave={() => updateMut.mutate()} onCancel={() => setEditId(null)}>
                                    <Field value={editName} onChange={setEditName} placeholder="Category name" />
                                </EditRow>
                            ) : (
                                <>
                                    <span className="flex-1 text-sm font-medium text-gray-800">{c.name}</span>
                                    <span className="text-xs text-gray-400 font-mono">{c.id}</span>
                                    <button onClick={() => { setEditId(c.id); setEditName(c.name); }}
                                        className="rounded p-1.5 text-gray-400 hover:bg-blue-50 hover:text-blue-600">
                                        <Pencil className="h-3.5 w-3.5" />
                                    </button>
                                    <button onClick={() => deleteMut.mutate(c.id)}
                                        className="rounded p-1.5 text-gray-400 hover:bg-red-50 hover:text-red-600">
                                        <Trash2 className="h-3.5 w-3.5" />
                                    </button>
                                </>
                            )}
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
}

/* ═══════════════════════════════╗
   SKILLS TAB
╚═══════════════════════════════ */
function SkillsTab() {
    const qc = useQueryClient();
    const [search, setSearch] = useState('');
    const [createName, setCreateName] = useState('');
    const [createCatId, setCreateCatId] = useState('');
    const [editId, setEditId] = useState<string | null>(null);
    const [editName, setEditName] = useState('');
    const [editCatId, setEditCatId] = useState('');

    const { data: skills = [], isLoading } = useQuery({
        queryKey: ['admin-skills'],
        queryFn: () => adminApi.getSkills(),
    });

    const createMut = useMutation({
        mutationFn: () => adminApi.createSkill(createName.trim(), createCatId.trim() || undefined),
        onSuccess: () => { toast.success('Skill created'); setCreateName(''); setCreateCatId(''); qc.invalidateQueries({ queryKey: ['admin-skills'] }); },
        onError: () => toast.error('Failed to create'),
    });
    const updateMut = useMutation({
        mutationFn: () => adminApi.updateSkill(editId!, editName.trim(), editCatId.trim() || undefined),
        onSuccess: () => { toast.success('Updated'); setEditId(null); qc.invalidateQueries({ queryKey: ['admin-skills'] }); },
        onError: () => toast.error('Failed to update'),
    });
    const deleteMut = useMutation({
        mutationFn: (id: string) => adminApi.deleteSkill(id),
        onSuccess: () => { toast.success('Deleted'); qc.invalidateQueries({ queryKey: ['admin-skills'] }); },
        onError: () => toast.error('Failed to delete'),
    });

    const filtered = skills.filter((s: any) => (s.name ?? '').toLowerCase().includes(search.toLowerCase()));

    return (
        <div className="space-y-4">
            <div className="flex gap-2">
                <input value={createName} onChange={(e) => setCreateName(e.target.value)} placeholder="Skill name…"
                    className="flex-1 rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-slate-900" />
                <input value={createCatId} onChange={(e) => setCreateCatId(e.target.value)} placeholder="Category ID (optional)"
                    className="w-48 rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-slate-900" />
                <Button className="bg-slate-900 hover:bg-slate-800 text-white" disabled={!createName.trim() || createMut.isPending}
                    onClick={() => createMut.mutate()}>
                    <Plus className="h-4 w-4 mr-1" /> Add
                </Button>
            </div>
            <div className="relative">
                <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-gray-400" />
                <input value={search} onChange={(e) => setSearch(e.target.value)} placeholder="Filter…"
                    className="w-full rounded-lg border border-gray-200 py-2 pl-9 pr-3 text-sm outline-none focus:border-slate-900" />
            </div>
            {isLoading ? <div className="py-8 text-center"><Loader2 className="h-5 w-5 animate-spin text-gray-400 mx-auto" /></div> : (
                <div className="divide-y divide-gray-50 rounded-xl border border-gray-100">
                    {filtered.length === 0 ? <div className="py-8 text-center text-sm text-gray-400">No skills</div> : filtered.map((s: any) => (
                        <div key={s.id} className="flex items-center gap-3 px-4 py-3 hover:bg-gray-50/80">
                            {editId === s.id ? (
                                <EditRow onSave={() => updateMut.mutate()} onCancel={() => setEditId(null)}>
                                    <Field value={editName} onChange={setEditName} placeholder="Skill name" />
                                    <Field value={editCatId} onChange={setEditCatId} placeholder="Category ID" />
                                </EditRow>
                            ) : (
                                <>
                                    <span className="flex-1 text-sm font-medium text-gray-800">{s.name}</span>
                                    {s.categoryId && <Badge variant="secondary" className="text-xs">{s.categoryId.slice(0, 8)}…</Badge>}
                                    <button onClick={() => { setEditId(s.id); setEditName(s.name); setEditCatId(s.categoryId ?? ''); }}
                                        className="rounded p-1.5 text-gray-400 hover:bg-blue-50 hover:text-blue-600">
                                        <Pencil className="h-3.5 w-3.5" />
                                    </button>
                                    <button onClick={() => deleteMut.mutate(s.id)}
                                        className="rounded p-1.5 text-gray-400 hover:bg-red-50 hover:text-red-600">
                                        <Trash2 className="h-3.5 w-3.5" />
                                    </button>
                                </>
                            )}
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
}

/* ═══════════════════════════════╗
   BANNERS TAB
╚═══════════════════════════════ */
function BannersTab() {
    const qc = useQueryClient();
    const [showCreate, setShowCreate] = useState(false);
    const [form, setForm] = useState({ title: '', imageUrl: '', linkUrl: '', order: '0', isActive: true });
    const [editId, setEditId] = useState<string | null>(null);
    const [editForm, setEditForm] = useState<any>({});

    const { data: banners = [], isLoading } = useQuery({
        queryKey: ['admin-banners'],
        queryFn: () => adminApi.getBanners(),
    });

    const createMut = useMutation({
        mutationFn: () => adminApi.createBanner({ title: form.title.trim(), imageUrl: form.imageUrl.trim(), linkUrl: form.linkUrl.trim() || undefined, order: Number(form.order), isActive: form.isActive }),
        onSuccess: () => { toast.success('Banner created'); setShowCreate(false); setForm({ title: '', imageUrl: '', linkUrl: '', order: '0', isActive: true }); qc.invalidateQueries({ queryKey: ['admin-banners'] }); },
        onError: () => toast.error('Failed to create'),
    });
    const updateMut = useMutation({
        mutationFn: () => adminApi.updateBanner(editId!, { title: editForm.title, imageUrl: editForm.imageUrl, linkUrl: editForm.linkUrl || undefined, order: Number(editForm.order), isActive: editForm.isActive }),
        onSuccess: () => { toast.success('Updated'); setEditId(null); qc.invalidateQueries({ queryKey: ['admin-banners'] }); },
        onError: () => toast.error('Failed to update'),
    });
    const deleteMut = useMutation({
        mutationFn: (id: string) => adminApi.deleteBanner(id),
        onSuccess: () => { toast.success('Deleted'); qc.invalidateQueries({ queryKey: ['admin-banners'] }); },
        onError: () => toast.error('Failed to delete'),
    });

    return (
        <div className="space-y-4">
            <div className="flex justify-end">
                <Button className="bg-slate-900 hover:bg-slate-800 text-white" onClick={() => setShowCreate(s => !s)}>
                    <Plus className="h-4 w-4 mr-1" /> New Banner
                </Button>
            </div>
            {showCreate && (
                <Card className="p-4 space-y-3 border border-slate-200">
                    <h3 className="font-medium text-gray-900 text-sm">Create Banner</h3>
                    <Field value={form.title} onChange={(v) => setForm(f => ({ ...f, title: v }))} placeholder="Title *" />
                    <Field value={form.imageUrl} onChange={(v) => setForm(f => ({ ...f, imageUrl: v }))} placeholder="Image URL *" />
                    <Field value={form.linkUrl} onChange={(v) => setForm(f => ({ ...f, linkUrl: v }))} placeholder="Link URL (optional)" />
                    <div className="flex gap-3">
                        <Field value={form.order} onChange={(v) => setForm(f => ({ ...f, order: v }))} placeholder="Order" type="number" />
                        <label className="flex items-center gap-2 text-sm text-gray-700 whitespace-nowrap">
                            <input type="checkbox" checked={form.isActive} onChange={(e) => setForm(f => ({ ...f, isActive: e.target.checked }))} />
                            Active
                        </label>
                    </div>
                    <div className="flex justify-end gap-2">
                        <Button variant="outline" onClick={() => setShowCreate(false)}>Cancel</Button>
                        <Button className="bg-slate-900 hover:bg-slate-800 text-white" disabled={!form.title.trim() || !form.imageUrl.trim() || createMut.isPending} onClick={() => createMut.mutate()}>
                            Create
                        </Button>
                    </div>
                </Card>
            )}
            {isLoading ? <div className="py-8 text-center"><Loader2 className="h-5 w-5 animate-spin text-gray-400 mx-auto" /></div> : (
                <div className="divide-y divide-gray-50 rounded-xl border border-gray-100">
                    {banners.length === 0 ? <div className="py-8 text-center text-sm text-gray-400">No banners</div> : banners.map((b: any) => (
                        <div key={b.id} className="px-4 py-3 hover:bg-gray-50/80">
                            {editId === b.id ? (
                                <div className="space-y-2">
                                    <Field value={editForm.title ?? ''} onChange={(v) => setEditForm((f: any) => ({ ...f, title: v }))} placeholder="Title" />
                                    <Field value={editForm.imageUrl ?? ''} onChange={(v) => setEditForm((f: any) => ({ ...f, imageUrl: v }))} placeholder="Image URL" />
                                    <Field value={editForm.linkUrl ?? ''} onChange={(v) => setEditForm((f: any) => ({ ...f, linkUrl: v }))} placeholder="Link URL" />
                                    <div className="flex gap-3">
                                        <Field value={String(editForm.order ?? 0)} onChange={(v) => setEditForm((f: any) => ({ ...f, order: v }))} placeholder="Order" type="number" />
                                        <label className="flex items-center gap-2 text-sm text-gray-700 whitespace-nowrap">
                                            <input type="checkbox" checked={editForm.isActive ?? true} onChange={(e) => setEditForm((f: any) => ({ ...f, isActive: e.target.checked }))} />
                                            Active
                                        </label>
                                    </div>
                                    <div className="flex justify-end gap-2">
                                        <Button variant="outline" size="sm" onClick={() => setEditId(null)}>Cancel</Button>
                                        <Button size="sm" className="bg-slate-900 text-white" onClick={() => updateMut.mutate()}>Save</Button>
                                    </div>
                                </div>
                            ) : (
                                <div className="flex items-center gap-3">
                                    {b.imageUrl && <img src={b.imageUrl} alt="" className="h-10 w-16 rounded object-cover border border-gray-100" />}
                                    <div className="flex-1 min-w-0">
                                        <p className="text-sm font-medium text-gray-900 truncate">{b.title}</p>
                                        <p className="text-xs text-gray-400 truncate">{b.linkUrl ?? 'No link'}</p>
                                    </div>
                                    <Badge className={b.isActive ? 'bg-green-50 text-green-700 text-xs' : 'bg-gray-100 text-gray-500 text-xs'}>
                                        {b.isActive ? 'Active' : 'Inactive'}
                                    </Badge>
                                    <span className="text-xs text-gray-400">#{b.order ?? 0}</span>
                                    <button onClick={() => { setEditId(b.id); setEditForm({ title: b.title, imageUrl: b.imageUrl, linkUrl: b.linkUrl ?? '', order: b.order ?? 0, isActive: b.isActive ?? true }); }}
                                        className="rounded p-1.5 text-gray-400 hover:bg-blue-50 hover:text-blue-600">
                                        <Pencil className="h-3.5 w-3.5" />
                                    </button>
                                    <button onClick={() => deleteMut.mutate(b.id)}
                                        className="rounded p-1.5 text-gray-400 hover:bg-red-50 hover:text-red-600">
                                        <Trash2 className="h-3.5 w-3.5" />
                                    </button>
                                </div>
                            )}
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
}

/* ═══════════════════════════════╗
   PREMIUM PACKAGES TAB
╚═══════════════════════════════ */
function PackagesTab() {
    const qc = useQueryClient();
    const [showCreate, setShowCreate] = useState(false);
    const [form, setForm] = useState({ name: '', description: '', price: '', durationDays: '30', isActive: true });
    const [editId, setEditId] = useState<string | null>(null);
    const [editForm, setEditForm] = useState<any>({});

    const { data: packages = [], isLoading } = useQuery({
        queryKey: ['admin-packages'],
        queryFn: () => adminApi.getPremiumPackages(),
    });

    const createMut = useMutation({
        mutationFn: () => adminApi.createPremiumPackage({ name: form.name.trim(), description: form.description.trim() || undefined, price: Number(form.price), durationDays: Number(form.durationDays), isActive: form.isActive }),
        onSuccess: () => { toast.success('Package created'); setShowCreate(false); setForm({ name: '', description: '', price: '', durationDays: '30', isActive: true }); qc.invalidateQueries({ queryKey: ['admin-packages'] }); },
        onError: () => toast.error('Failed to create'),
    });
    const updateMut = useMutation({
        mutationFn: () => adminApi.updatePremiumPackage(editId!, { name: editForm.name, description: editForm.description || undefined, price: Number(editForm.price), durationDays: Number(editForm.durationDays), isActive: editForm.isActive }),
        onSuccess: () => { toast.success('Updated'); setEditId(null); qc.invalidateQueries({ queryKey: ['admin-packages'] }); },
        onError: () => toast.error('Failed to update'),
    });
    const deleteMut = useMutation({
        mutationFn: (id: string) => adminApi.deletePremiumPackage(id),
        onSuccess: () => { toast.success('Deleted'); qc.invalidateQueries({ queryKey: ['admin-packages'] }); },
        onError: () => toast.error('Failed to delete'),
    });

    const formatPrice = (price: number) => {
        // BE stores in minor units (×100)
        const vnd = price > 1000 ? price : price * 100;
        return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(vnd);
    };

    return (
        <div className="space-y-4">
            <div className="flex justify-end">
                <Button className="bg-slate-900 hover:bg-slate-800 text-white" onClick={() => setShowCreate(s => !s)}>
                    <Plus className="h-4 w-4 mr-1" /> New Package
                </Button>
            </div>
            {showCreate && (
                <Card className="p-4 space-y-3 border border-slate-200">
                    <h3 className="font-medium text-gray-900 text-sm">Create Premium Package</h3>
                    <Field value={form.name} onChange={(v) => setForm(f => ({ ...f, name: v }))} placeholder="Package name *" />
                    <Field value={form.description} onChange={(v) => setForm(f => ({ ...f, description: v }))} placeholder="Description (optional)" />
                    <div className="flex gap-3">
                        <Field value={form.price} onChange={(v) => setForm(f => ({ ...f, price: v }))} placeholder="Price (VND) *" type="number" />
                        <Field value={form.durationDays} onChange={(v) => setForm(f => ({ ...f, durationDays: v }))} placeholder="Duration (days)" type="number" />
                    </div>
                    <label className="flex items-center gap-2 text-sm text-gray-700">
                        <input type="checkbox" checked={form.isActive} onChange={(e) => setForm(f => ({ ...f, isActive: e.target.checked }))} />
                        Active
                    </label>
                    <div className="flex justify-end gap-2">
                        <Button variant="outline" onClick={() => setShowCreate(false)}>Cancel</Button>
                        <Button className="bg-slate-900 hover:bg-slate-800 text-white" disabled={!form.name.trim() || !form.price || createMut.isPending} onClick={() => createMut.mutate()}>
                            Create
                        </Button>
                    </div>
                </Card>
            )}
            {isLoading ? <div className="py-8 text-center"><Loader2 className="h-5 w-5 animate-spin text-gray-400 mx-auto" /></div> : (
                <div className="divide-y divide-gray-50 rounded-xl border border-gray-100">
                    {packages.length === 0 ? <div className="py-8 text-center text-sm text-gray-400">No packages</div> : packages.map((pkg: any) => (
                        <div key={pkg.id} className="px-4 py-4 hover:bg-gray-50/80">
                            {editId === pkg.id ? (
                                <div className="space-y-2">
                                    <Field value={editForm.name ?? ''} onChange={(v) => setEditForm((f: any) => ({ ...f, name: v }))} placeholder="Package name" />
                                    <Field value={editForm.description ?? ''} onChange={(v) => setEditForm((f: any) => ({ ...f, description: v }))} placeholder="Description" />
                                    <div className="flex gap-3">
                                        <Field value={String(editForm.price ?? 0)} onChange={(v) => setEditForm((f: any) => ({ ...f, price: v }))} placeholder="Price" type="number" />
                                        <Field value={String(editForm.durationDays ?? 30)} onChange={(v) => setEditForm((f: any) => ({ ...f, durationDays: v }))} placeholder="Days" type="number" />
                                    </div>
                                    <label className="flex items-center gap-2 text-sm text-gray-700">
                                        <input type="checkbox" checked={editForm.isActive ?? true} onChange={(e) => setEditForm((f: any) => ({ ...f, isActive: e.target.checked }))} />
                                        Active
                                    </label>
                                    <div className="flex justify-end gap-2">
                                        <Button variant="outline" size="sm" onClick={() => setEditId(null)}>Cancel</Button>
                                        <Button size="sm" className="bg-slate-900 text-white" onClick={() => updateMut.mutate()}>Save</Button>
                                    </div>
                                </div>
                            ) : (
                                <div className="flex items-center gap-3">
                                    <div className="flex-1 min-w-0">
                                        <div className="flex items-center gap-2">
                                            <p className="font-medium text-gray-900">{pkg.name}</p>
                                            <Badge className={pkg.isActive ? 'bg-green-50 text-green-700 text-xs' : 'bg-gray-100 text-gray-500 text-xs'}>
                                                {pkg.isActive ? 'Active' : 'Inactive'}
                                            </Badge>
                                        </div>
                                        {pkg.description && <p className="text-xs text-gray-500 mt-0.5">{pkg.description}</p>}
                                        <div className="flex gap-4 mt-1 text-xs text-gray-500">
                                            <span className="font-semibold text-gray-800">{formatPrice(pkg.price ?? 0)}</span>
                                            <span>{pkg.durationDays ?? 30} days</span>
                                            {pkg.userType && <span>For: {pkg.userType}</span>}
                                        </div>
                                    </div>
                                    <button onClick={() => { setEditId(pkg.id); setEditForm({ name: pkg.name, description: pkg.description ?? '', price: pkg.price ?? 0, durationDays: pkg.durationDays ?? 30, isActive: pkg.isActive ?? true }); }}
                                        className="rounded p-1.5 text-gray-400 hover:bg-blue-50 hover:text-blue-600">
                                        <Pencil className="h-3.5 w-3.5" />
                                    </button>
                                    <button onClick={() => deleteMut.mutate(pkg.id)}
                                        className="rounded p-1.5 text-gray-400 hover:bg-red-50 hover:text-red-600">
                                        <Trash2 className="h-3.5 w-3.5" />
                                    </button>
                                </div>
                            )}
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
}

/* ═══════════════════════════════╗
   MAIN PAGE
╚═══════════════════════════════ */
export function AdminCatalogPage() {
    const [activeTab, setActiveTab] = useState<Tab>('categories');

    return (
        <div className="space-y-6">
            <div>
                <h1 className="text-2xl font-bold text-gray-900">Catalog Management</h1>
                <p className="mt-1 text-sm text-gray-500">Manage categories, skills, banners and premium packages.</p>
            </div>

            {/* Tabs */}
            <div className="flex gap-1 rounded-xl bg-gray-100 p-1">
                {TABS.map(({ id, label, Icon }) => (
                    <button
                        key={id}
                        onClick={() => setActiveTab(id)}
                        className={`flex flex-1 items-center justify-center gap-2 rounded-lg px-4 py-2.5 text-sm font-medium transition-all ${
                            activeTab === id
                                ? 'bg-white text-slate-900 shadow-sm'
                                : 'text-gray-500 hover:text-gray-700'
                        }`}
                    >
                        <Icon className="h-4 w-4" />
                        <span className="hidden sm:inline">{label}</span>
                    </button>
                ))}
            </div>

            {/* Tab Content */}
            <Card className="p-6">
                {activeTab === 'categories' && <CategoriesTab />}
                {activeTab === 'skills'     && <SkillsTab />}
                {activeTab === 'banners'    && <BannersTab />}
                {activeTab === 'packages'   && <PackagesTab />}
            </Card>
        </div>
    );
}
