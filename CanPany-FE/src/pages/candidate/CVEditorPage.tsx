import { useState, useCallback, useEffect, useRef } from 'react';
import { useParams, useNavigate, useSearchParams } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
    Save, Download, ArrowLeft, Plus, Trash2,
    Loader2, CheckCircle, AlertCircle, User, Mail, Phone,
    MapPin, Linkedin, Github, Globe, FileText, Briefcase, GraduationCap, Wrench,
    History, GitBranch
} from 'lucide-react';
import toast from 'react-hot-toast';
import { cvApi, type CVStructuredData, type CVExperienceEntry, type CVEducationEntry } from '../../api/cv.api';
import { downloadCVAsPdf } from '../../utils/cv-pdf';
import { useTranslation } from 'react-i18next';
import { useAuthStore } from '../../stores/auth.store';

// ─── helpers ─────────────────────────────────────────────────────────────────
function blankExp(): CVExperienceEntry { return { company: '', role: '', period: '', bullets: [''] }; }
function blankEdu(): CVEducationEntry { return { institution: '', degree: '', period: '', notes: '' }; }

// ─── page ─────────────────────────────────────────────────────────────────────
export function CVEditorPage() {
    const { t } = useTranslation('candidate');
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const { user } = useAuthStore();
    const [searchParams] = useSearchParams();
    const queryClient = useQueryClient();
    const [saved, setSaved] = useState(false);
    const [isDownloading, setIsDownloading] = useState(false);
    const autoDownloadTriggered = useRef(false);
    const [showVersions, setShowVersions] = useState(false);
    const [versionNote, setVersionNote] = useState('');

    const { data: initial, isLoading, error } = useQuery({
        queryKey: ['cv-data', id],
        queryFn: () => cvApi.getCVData(id!),
        enabled: !!id && id !== 'new',
    });

    const [cv, setCv] = useState<CVStructuredData | null>(null);
    // Sync initial data into local state once
    if (initial && !cv) setCv(JSON.parse(JSON.stringify(initial)));
    
    // Initialize blank CV for "new" route
    if (id === 'new' && !cv) {
        setCv({
            fullName: user?.fullName || '',
            email: user?.email || '',
            phone: '',
            location: '',
            linkedIn: '',
            gitHub: '',
            portfolio: '',
            summary: '',
            title: '',
            skills: [],
            languages: [],
            certifications: [],
            experience: [blankExp()],
            education: [blankEdu()],
        });
    }

    const saveMutation = useMutation({
        mutationFn: (data: CVStructuredData) => cvApi.updateCVData(id!, data),
        onSuccess: () => {
            setSaved(true);
            setTimeout(() => setSaved(false), 2500);
            queryClient.invalidateQueries({ queryKey: ['candidate-cvs'] });
        },
        onError: () => toast.error(t('cv.editor.toast.saveFail')),
    });

    const save = useCallback(() => {
        if (cv) saveMutation.mutate(cv);
    }, [cv, saveMutation]);

    const handleDownloadPDF = useCallback(async () => {
        if (!cv || isDownloading) return;
        setIsDownloading(true);
        try {
            // Save first, then download
            if (cv) saveMutation.mutate(cv);
            await downloadCVAsPdf(cv, cv.fullName ? `CV-${cv.fullName}` : undefined);
            toast.success(t('cv.editor.toast.downloadSuccess'));
        } catch (err) {
            console.error('PDF download failed:', err);
            toast.error(t('cv.editor.toast.downloadFail'));
        } finally {
            setIsDownloading(false);
        }
    }, [cv, isDownloading, saveMutation]);

    // Auto-download when ?download=1 is in URL
    useEffect(() => {
        if (searchParams.get('download') === '1' && cv && !autoDownloadTriggered.current) {
            autoDownloadTriggered.current = true;
            // Small delay to ensure rendering is complete
            const timer = setTimeout(() => handleDownloadPDF(), 500);
            return () => clearTimeout(timer);
        }
    }, [searchParams, cv, handleDownloadPDF]);

    const setField = <K extends keyof CVStructuredData>(key: K, value: CVStructuredData[K]) =>
        setCv(prev => prev ? { ...prev, [key]: value } : prev);

    // ── Versioning ──
    const { data: versions = [], isLoading: versionsLoading } = useQuery({
        queryKey: ['cv-versions', id],
        queryFn: () => cvApi.getCVVersions(id!),
        enabled: !!id && id !== 'new' && showVersions,
    });

    const saveVersionMutation = useMutation({
        mutationFn: () => cvApi.saveCVVersion(id!, versionNote || undefined),
        onSuccess: () => {
            toast.success('Đã lưu phiên bản mới');
            setVersionNote('');
            queryClient.invalidateQueries({ queryKey: ['cv-versions', id] });
        },
        onError: () => toast.error('Lỗi khi lưu phiên bản'),
    });

    const setExp = (i: number, field: keyof CVExperienceEntry, value: string | string[]) =>
        setCv(prev => {
            if (!prev) return prev;
            const exp = [...prev.experience];
            exp[i] = { ...exp[i], [field]: value };
            return { ...prev, experience: exp };
        });

    const setExpBullet = (ei: number, bi: number, value: string) =>
        setCv(prev => {
            if (!prev) return prev;
            const exp = [...prev.experience];
            const bullets = [...exp[ei].bullets];
            bullets[bi] = value;
            exp[ei] = { ...exp[ei], bullets };
            return { ...prev, experience: exp };
        });

    const addExpBullet = (ei: number) =>
        setCv(prev => {
            if (!prev) return prev;
            const exp = [...prev.experience];
            exp[ei] = { ...exp[ei], bullets: [...exp[ei].bullets, ''] };
            return { ...prev, experience: exp };
        });

    const removeExpBullet = (ei: number, bi: number) =>
        setCv(prev => {
            if (!prev) return prev;
            const exp = [...prev.experience];
            exp[ei] = { ...exp[ei], bullets: exp[ei].bullets.filter((_, j) => j !== bi) };
            return { ...prev, experience: exp };
        });

    const setEdu = (i: number, field: keyof CVEducationEntry, value: string) =>
        setCv(prev => {
            if (!prev) return prev;
            const edu = [...prev.education];
            edu[i] = { ...edu[i], [field]: value };
            return { ...prev, education: edu };
        });

    if (isLoading || !cv) {
        return (
            <div className="flex min-h-[60vh] items-center justify-center">
                <Loader2 className="h-8 w-8 animate-spin text-[#00b14f]" />
            </div>
        );
    }

    if (error) {
        return (
            <div className="flex min-h-[60vh] flex-col items-center justify-center gap-3 text-red-500">
                <AlertCircle className="h-8 w-8" />
                <p>{t('cv.editor.error.title')}</p>
                <button onClick={() => navigate(-1)} className="text-sm text-gray-500 underline">{t('cv.editor.error.back')}</button>
            </div>
        );
    }

    return (
        <>
            {/* ── Print styles ── */}
            <style>{`
            @media print {
                .no-print { display: none !important; }
                .cv-paper { box-shadow: none !important; border: none !important; }
                body { background: white !important; }
                [contentEditable] { outline: none !important; }
            }
            [contentEditable]:empty:before { content: attr(data-placeholder); color: #aaa; pointer-events: none; }
        `}</style>

            {/* ── Toolbar (hidden on print) ── */}
            <div className="no-print mb-6 flex flex-wrap items-center justify-between gap-3">
                <button
                    onClick={() => navigate('/candidate/cv/list')}
                    className="flex items-center gap-2 text-sm text-gray-500 hover:text-gray-800 transition-colors"
                >
                    <ArrowLeft className="h-4 w-4" />
                    {t('cv.editor.toolbar.back')}
                </button>
                <div className="flex items-center gap-3">
                    {saved && (
                        <span className="flex items-center gap-1 text-sm text-emerald-600">
                            <CheckCircle className="h-4 w-4" /> {t('cv.editor.toolbar.saved')}
                        </span>
                    )}
                    {id !== 'new' && (
                        <button
                            onClick={() => setShowVersions(v => !v)}
                            className={`flex items-center gap-2 rounded-xl border px-4 py-2 text-sm font-medium transition-colors ${
                                showVersions ? 'border-indigo-300 bg-indigo-50 text-indigo-700' : 'border-gray-200 hover:bg-gray-50'
                            }`}
                        >
                            <History className="h-4 w-4" />
                            Lịch sử
                        </button>
                    )}
                    <button
                        onClick={save}
                        disabled={saveMutation.isPending}
                        className="flex items-center gap-2 rounded-xl border border-gray-200 px-4 py-2 text-sm font-medium hover:bg-gray-50 transition-colors disabled:opacity-60"
                    >
                        {saveMutation.isPending ? <Loader2 className="h-4 w-4 animate-spin" /> : <Save className="h-4 w-4" />}
                        {t('cv.editor.toolbar.save')}
                    </button>
                    <button
                        onClick={handleDownloadPDF}
                        disabled={isDownloading}
                        className="flex items-center gap-2 rounded-xl bg-[#00b14f] text-white px-4 py-2 text-sm font-medium hover:bg-[#009940] transition-colors disabled:opacity-60"
                    >
                        {isDownloading ? <Loader2 className="h-4 w-4 animate-spin" /> : <Download className="h-4 w-4" />}
                        {isDownloading ? t('cv.editor.toolbar.downloading') : t('cv.editor.toolbar.download')}
                    </button>
                </div>
            </div>

            {/* ── Version History Panel ── */}
            {showVersions && (
                <div className="no-print mb-6 rounded-2xl border border-indigo-100 bg-white p-5 shadow-sm">
                    <div className="flex items-center justify-between mb-4">
                        <h3 className="text-sm font-semibold text-gray-900 flex items-center gap-2">
                            <GitBranch className="h-4 w-4 text-indigo-500" />
                            Lịch sử phiên bản
                        </h3>
                        {/* Save version form */}
                        <div className="flex items-center gap-2">
                            <input
                                value={versionNote}
                                onChange={(e) => setVersionNote(e.target.value)}
                                placeholder="Ghi chú phiên bản..."
                                className="rounded-lg border border-gray-200 px-3 py-1.5 text-xs w-48 focus:ring-2 focus:ring-indigo-400 focus:border-transparent"
                            />
                            <button
                                onClick={() => saveVersionMutation.mutate()}
                                disabled={saveVersionMutation.isPending}
                                className="flex items-center gap-1.5 rounded-lg bg-indigo-600 text-white px-3 py-1.5 text-xs font-medium hover:bg-indigo-700 disabled:opacity-50 transition-colors"
                            >
                                {saveVersionMutation.isPending ? <Loader2 className="h-3 w-3 animate-spin" /> : <GitBranch className="h-3 w-3" />}
                                Lưu phiên bản
                            </button>
                        </div>
                    </div>

                    {versionsLoading ? (
                        <div className="flex items-center justify-center py-6">
                            <Loader2 className="h-5 w-5 animate-spin text-indigo-400" />
                        </div>
                    ) : versions.length === 0 ? (
                        <p className="text-xs text-gray-400 text-center py-4">Chưa có phiên bản nào được lưu.</p>
                    ) : (
                        <div className="space-y-2 max-h-48 overflow-y-auto">
                            {versions.map((v: any, i: number) => (
                                <div key={v.id || i} className="flex items-center gap-3 rounded-lg bg-gray-50 px-3 py-2">
                                    <div className="h-2 w-2 rounded-full bg-indigo-400 shrink-0" />
                                    <div className="flex-1 min-w-0">
                                        <p className="text-xs font-medium text-gray-800 truncate">
                                            v{v.version ?? i + 1}
                                            {v.versionNote && <span className="text-gray-500"> — {v.versionNote}</span>}
                                        </p>
                                        <p className="text-[10px] text-gray-400">
                                            {new Date(v.createdAt).toLocaleString('vi-VN')}
                                        </p>
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}
                </div>
            )}

            {/* ── CV Paper ── */}
            <div className="cv-paper mx-auto w-full max-w-3xl rounded-2xl bg-white p-10 shadow-lg border border-gray-100 print:shadow-none print:p-8">

                {/* Header */}
                <div className="border-b-2 border-[#005f73] pb-6 mb-6">
                    <div className="flex flex-col gap-1">
                        <EditableField
                            value={cv.fullName}
                            onChange={v => setField('fullName', v)}
                            className="text-3xl font-bold text-gray-900"
                                placeholder={t('cv.editor.placeholders.fullName')}
                        />
                        <EditableField
                            value={cv.title}
                            onChange={v => setField('title', v)}
                            className="text-lg text-[#005f73] font-medium"
                                placeholder={t('cv.editor.placeholders.title')}
                        />
                    </div>
                    <div className="mt-4 flex flex-wrap gap-x-5 gap-y-1 text-sm text-gray-600">
                            <ContactField icon={<Mail className="h-3.5 w-3.5" />} value={cv.email} onChange={v => setField('email', v)} placeholder={t('cv.editor.placeholders.email')} />
                            <ContactField icon={<Phone className="h-3.5 w-3.5" />} value={cv.phone} onChange={v => setField('phone', v)} placeholder={t('cv.editor.placeholders.phone')} />
                            <ContactField icon={<MapPin className="h-3.5 w-3.5" />} value={cv.location} onChange={v => setField('location', v)} placeholder={t('cv.editor.placeholders.location')} />
                            {cv.linkedIn && <ContactField icon={<Linkedin className="h-3.5 w-3.5" />} value={cv.linkedIn} onChange={v => setField('linkedIn', v)} placeholder={t('cv.editor.placeholders.linkedIn')} />}
                            {cv.gitHub && <ContactField icon={<Github className="h-3.5 w-3.5" />} value={cv.gitHub} onChange={v => setField('gitHub', v)} placeholder={t('cv.editor.placeholders.gitHub')} />}
                            {cv.portfolio && <ContactField icon={<Globe className="h-3.5 w-3.5" />} value={cv.portfolio} onChange={v => setField('portfolio', v)} placeholder={t('cv.editor.placeholders.portfolio')} />}
                    </div>
                    {cv.targetJobTitle && (
                        <p className="no-print mt-2 text-xs text-[#00b14f] font-medium">{t('cv.editor.target', { job: cv.targetJobTitle })}</p>
                    )}
                </div>

                {/* Professional Summary */}
                    <SectionHeader icon={<User className="h-4 w-4" />} title={t('cv.editor.sections.summary')} />
                <EditableField
                    value={cv.summary}
                    onChange={v => setField('summary', v)}
                    multiline
                    className="text-sm text-gray-700 leading-relaxed mb-6"
                        placeholder={t('cv.editor.placeholders.summary')}
                />

                {/* Work Experience */}
                    <SectionHeader icon={<Briefcase className="h-4 w-4" />} title={t('cv.editor.sections.experience')} />
                <div className="space-y-5 mb-6">
                    {cv.experience.map((exp, ei) => (
                        <div key={ei} className="relative group">
                            <div className="flex justify-between items-start gap-2">
                                <div className="flex-1">
                                    <div className="flex gap-2 flex-wrap">
                                                <EditableField value={exp.role} onChange={v => setExp(ei, 'role', v)} className="font-semibold text-gray-900 text-sm" placeholder={t('cv.editor.placeholders.expRole')} />
                                        <span className="text-gray-400 text-sm">·</span>
                                                <EditableField value={exp.company} onChange={v => setExp(ei, 'company', v)} className="font-medium text-[#005f73] text-sm" placeholder={t('cv.editor.placeholders.expCompany')} />
                                    </div>
                                            <EditableField value={exp.period} onChange={v => setExp(ei, 'period', v)} className="text-xs text-gray-400 mt-0.5" placeholder={t('cv.editor.placeholders.expPeriod')} />
                                </div>
                                <button
                                    onClick={() => setCv(p => p ? { ...p, experience: p.experience.filter((_, j) => j !== ei) } : p)}
                                    className="no-print opacity-0 group-hover:opacity-100 text-red-400 hover:text-red-600 transition-all"
                                >
                                    <Trash2 className="h-3.5 w-3.5" />
                                </button>
                            </div>
                            <ul className="mt-2 ml-4 list-disc space-y-1">
                                {exp.bullets.map((b, bi) => (
                                    <li key={bi} className="flex items-start gap-2 group/bullet">
                                        <EditableField
                                            value={b}
                                            onChange={v => setExpBullet(ei, bi, v)}
                                            className="flex-1 text-sm text-gray-600"
                                                    placeholder={t('cv.editor.placeholders.expBullet')}
                                        />
                                        <button
                                            onClick={() => removeExpBullet(ei, bi)}
                                            className="no-print opacity-0 group-hover/bullet:opacity-100 text-red-300 hover:text-red-500 shrink-0"
                                        >
                                            <Trash2 className="h-3 w-3" />
                                        </button>
                                    </li>
                                ))}
                            </ul>
                            <button
                                onClick={() => addExpBullet(ei)}
                                className="no-print mt-1 ml-4 flex items-center gap-1 text-xs text-gray-400 hover:text-[#00b14f]"
                            >
                                <Plus className="h-3 w-3" /> {t('cv.editor.placeholders.addBullet')}
                            </button>
                        </div>
                    ))}
                    <button
                        onClick={() => setCv(p => p ? { ...p, experience: [...p.experience, blankExp()] } : p)}
                        className="no-print flex items-center gap-1.5 text-sm text-[#00b14f] hover:text-[#009940] transition-colors"
                    >
                        <Plus className="h-4 w-4" /> {t('cv.editor.placeholders.addExperience')}
                    </button>
                </div>

                {/* Education */}
                <SectionHeader icon={<GraduationCap className="h-4 w-4" />} title={t('cv.editor.sections.education')} />
                <div className="space-y-3 mb-6">
                    {cv.education.map((edu, ei) => (
                        <div key={ei} className="flex justify-between group">
                            <div className="flex-1">
                                <EditableField value={edu.degree} onChange={v => setEdu(ei, 'degree', v)} className="font-semibold text-sm text-gray-900" placeholder={t('cv.editor.placeholders.eduDegree')} />
                                <EditableField value={edu.institution} onChange={v => setEdu(ei, 'institution', v)} className="text-sm text-[#005f73]" placeholder={t('cv.editor.placeholders.eduInstitution')} />
                                <EditableField value={edu.period} onChange={v => setEdu(ei, 'period', v)} className="text-xs text-gray-400" placeholder={t('cv.editor.placeholders.eduPeriod')} />
                                {edu.notes !== undefined && (
                                    <EditableField value={edu.notes ?? ''} onChange={v => setEdu(ei, 'notes', v)} className="text-xs text-gray-500 italic" placeholder={t('cv.editor.placeholders.eduNotes')} />
                                )}
                            </div>
                            <button
                                onClick={() => setCv(p => p ? { ...p, education: p.education.filter((_, j) => j !== ei) } : p)}
                                className="no-print opacity-0 group-hover:opacity-100 text-red-400 hover:text-red-600"
                            >
                                <Trash2 className="h-3.5 w-3.5" />
                            </button>
                        </div>
                    ))}
                    <button
                        onClick={() => setCv(p => p ? { ...p, education: [...p.education, blankEdu()] } : p)}
                        className="no-print flex items-center gap-1.5 text-sm text-[#00b14f] hover:text-[#009940]"
                    >
                        <Plus className="h-4 w-4" /> {t('cv.editor.placeholders.addEducation')}
                    </button>
                </div>

                {/* Technical Skills */}
                <SectionHeader icon={<Wrench className="h-4 w-4" />} title={t('cv.editor.sections.skills')} />
                <div className="mb-6">
                    <SkillTagEditor
                        tags={cv.skills}
                        onChange={v => setField('skills', v)}
                        placeholder={t('cv.editor.placeholders.skills')}
                    />
                </div>

                {/* Languages */}
                {(cv.languages.length > 0 || true) && (
                    <>
                        <SectionHeader icon={<FileText className="h-4 w-4" />} title={t('cv.editor.sections.languages')} />
                        <div className="mb-6">
                            <SkillTagEditor
                                tags={cv.languages}
                                onChange={v => setField('languages', v)}
                                placeholder={t('cv.editor.placeholders.languages')}
                            />
                        </div>
                    </>
                )}

                {/* Certifications */}
                {(cv.certifications.length > 0 || true) && (
                    <>
                        <SectionHeader icon={<FileText className="h-4 w-4" />} title={t('cv.editor.sections.certifications')} />
                        <div className="mb-2">
                            <SkillTagEditor
                                tags={cv.certifications}
                                onChange={v => setField('certifications', v)}
                                placeholder={t('cv.editor.placeholders.certifications')}
                            />
                        </div>
                    </>
                )}
            </div>
        </>
    );
}

// ─── Sub-components ───────────────────────────────────────────────────────────

function SectionHeader({ icon, title }: { icon: React.ReactNode; title: string }) {
    return (
        <div className="flex items-center gap-2 mb-3">
            <div className="text-[#005f73]">{icon}</div>
            <h2 className="text-sm font-bold uppercase tracking-widest text-[#005f73]">{title}</h2>
            <div className="flex-1 h-px bg-[#005f73]/20" />
        </div>
    );
}

function ContactField({ icon, value, onChange, placeholder }: {
    icon: React.ReactNode; value: string; onChange: (v: string) => void; placeholder: string;
}) {
    return (
        <span className="flex items-center gap-1">
            <span className="text-gray-400">{icon}</span>
            <EditableField value={value} onChange={onChange} className="text-sm text-gray-600" placeholder={placeholder} />
        </span>
    );
}

function EditableField({ value, onChange, className, placeholder, multiline }: {
    value: string;
    onChange: (v: string) => void;
    className?: string;
    placeholder?: string;
    multiline?: boolean;
}) {
    const Tag = multiline ? 'div' : 'span';
    return (
        <Tag
            key={`EditableField-${value}`}
            contentEditable
            suppressContentEditableWarning
            onBlur={e => onChange(e.currentTarget.textContent ?? '')}
            className={`outline-none focus:bg-[#00b14f]/5 focus:ring-1 focus:ring-[#00b14f]/30 rounded px-1 -mx-1 min-w-[20px] cursor-text transition-all ${className ?? ''}`}
            data-placeholder={placeholder}
            dangerouslySetInnerHTML={{ __html: value }}
        />
    );
}

function SkillTagEditor({ tags, onChange, placeholder }: {
    tags: string[];
    onChange: (v: string[]) => void;
    placeholder: string;
}) {
    const [input, setInput] = useState('');

    const add = () => {
        const trimmed = input.trim();
        if (trimmed && !tags.includes(trimmed)) {
            onChange([...tags, trimmed]);
        }
        setInput('');
    };

    return (
        <div className="flex flex-wrap gap-2">
            {tags.map((tag, i) => (
                <span key={tag} className="group flex items-center gap-1 rounded-full bg-[#005f73]/10 text-[#005f73] px-3 py-1 text-xs font-medium">
                    {tag}
                    <button
                        onClick={() => onChange(tags.filter((_, j) => j !== i))}
                        className="no-print opacity-0 group-hover:opacity-100 hover:text-red-500 transition-all"
                    >
                        ×
                    </button>
                </span>
            ))}
            <span className="no-print flex items-center gap-1">
                <input
                    value={input}
                    onChange={e => setInput(e.target.value)}
                    onKeyDown={e => { if (e.key === 'Enter' || e.key === ',') { e.preventDefault(); add(); } }}
                    placeholder={placeholder}
                    className="rounded-full border border-dashed border-gray-300 px-3 py-1 text-xs focus:outline-none focus:border-[#00b14f] w-32"
                />
                <button onClick={add} className="text-[#00b14f] hover:text-[#009940]">
                    <Plus className="h-3.5 w-3.5" />
                </button>
            </span>
        </div>
    );
}
