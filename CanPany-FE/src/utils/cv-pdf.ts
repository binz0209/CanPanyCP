import html2pdf from 'html2pdf.js';
import type { CVStructuredData } from '../api/cv.api';

/**
 * Build a self-contained HTML string from CVStructuredData.
 * This HTML is styled inline so html2pdf can render it without external CSS.
 */
function buildCVHtml(cv: CVStructuredData): string {
    const esc = (s: string | undefined | null) =>
        (s ?? '').replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');

    const contactItems: string[] = [];
    if (cv.email) contactItems.push(`<span>✉ ${esc(cv.email)}</span>`);
    if (cv.phone) contactItems.push(`<span>📞 ${esc(cv.phone)}</span>`);
    if (cv.location) contactItems.push(`<span>📍 ${esc(cv.location)}</span>`);
    if (cv.linkedIn) contactItems.push(`<span>🔗 ${esc(cv.linkedIn)}</span>`);
    if (cv.gitHub) contactItems.push(`<span>💻 ${esc(cv.gitHub)}</span>`);
    if (cv.portfolio) contactItems.push(`<span>🌐 ${esc(cv.portfolio)}</span>`);

    const experienceHtml = cv.experience
        .map(
            (exp) => `
        <div style="margin-bottom:12px;">
            <div style="display:flex;justify-content:space-between;align-items:baseline;">
                <div>
                    <span style="font-weight:600;color:#1a1a1a;">${esc(exp.role)}</span>
                    <span style="color:#888;margin:0 6px;">·</span>
                    <span style="font-weight:500;color:#005f73;">${esc(exp.company)}</span>
                </div>
                <span style="font-size:12px;color:#888;white-space:nowrap;">${esc(exp.period)}</span>
            </div>
            ${exp.bullets.length > 0
                    ? `<ul style="margin:6px 0 0 18px;padding:0;list-style:disc;color:#444;font-size:13px;line-height:1.6;">
                        ${exp.bullets.map((b) => `<li>${esc(b)}</li>`).join('')}
                       </ul>`
                    : ''
                }
        </div>`
        )
        .join('');

    const educationHtml = cv.education
        .map(
            (edu) => `
        <div style="margin-bottom:10px;">
            <div style="display:flex;justify-content:space-between;align-items:baseline;">
                <div>
                    <span style="font-weight:600;color:#1a1a1a;">${esc(edu.degree)}</span>
                    <span style="color:#888;margin:0 6px;">·</span>
                    <span style="color:#005f73;">${esc(edu.institution)}</span>
                </div>
                <span style="font-size:12px;color:#888;white-space:nowrap;">${esc(edu.period)}</span>
            </div>
            ${edu.notes ? `<p style="margin:4px 0 0;font-size:12px;color:#666;font-style:italic;">${esc(edu.notes)}</p>` : ''}
        </div>`
        )
        .join('');

    const sectionTitle = (title: string) =>
        `<div style="display:flex;align-items:center;gap:8px;margin:18px 0 10px;">
            <h2 style="margin:0;font-size:13px;font-weight:700;text-transform:uppercase;letter-spacing:2px;color:#005f73;white-space:nowrap;">${title}</h2>
            <div style="flex:1;height:1px;background:#005f73;opacity:0.2;"></div>
        </div>`;

    const tagList = (items: string[]) =>
        items.length > 0
            ? `<div style="display:flex;flex-wrap:wrap;gap:6px;">
                ${items.map((t) => `<span style="background:#005f73;color:#fff;padding:3px 10px;border-radius:12px;font-size:11px;font-weight:500;">${esc(t)}</span>`).join('')}
               </div>`
            : '';

    return `
    <div style="font-family:'Segoe UI',Tahoma,Geneva,Verdana,sans-serif;max-width:700px;margin:0 auto;padding:40px 36px;color:#333;line-height:1.5;font-size:14px;">
        <!-- Header -->
        <div style="border-bottom:2px solid #005f73;padding-bottom:20px;margin-bottom:20px;">
            <h1 style="margin:0;font-size:28px;font-weight:700;color:#1a1a1a;">${esc(cv.fullName)}</h1>
            ${cv.title ? `<p style="margin:4px 0 0;font-size:16px;color:#005f73;font-weight:500;">${esc(cv.title)}</p>` : ''}
            <div style="margin-top:12px;display:flex;flex-wrap:wrap;gap:12px;font-size:12px;color:#666;">
                ${contactItems.join('')}
            </div>
        </div>

        <!-- Summary -->
        ${cv.summary ? `${sectionTitle('Professional Summary')}<p style="margin:0 0 6px;font-size:13px;color:#444;line-height:1.7;">${esc(cv.summary)}</p>` : ''}

        <!-- Experience -->
        ${cv.experience.length > 0 ? `${sectionTitle('Work Experience')}${experienceHtml}` : ''}

        <!-- Education -->
        ${cv.education.length > 0 ? `${sectionTitle('Education')}${educationHtml}` : ''}

        <!-- Skills -->
        ${cv.skills.length > 0 ? `${sectionTitle('Technical Skills')}${tagList(cv.skills)}` : ''}

        <!-- Languages -->
        ${cv.languages.length > 0 ? `${sectionTitle('Languages')}${tagList(cv.languages)}` : ''}

        <!-- Certifications -->
        ${cv.certifications.length > 0 ? `${sectionTitle('Certifications')}${tagList(cv.certifications)}` : ''}
    </div>`;
}

/**
 * Generate and download a PDF from CVStructuredData using html2pdf.js.
 * @param cv  The structured CV data
 * @param filename  Output filename (without .pdf extension)
 */
export async function downloadCVAsPdf(cv: CVStructuredData, filename?: string): Promise<void> {
    const html = buildCVHtml(cv);

    // Create a temporary container (off-screen) so html2pdf can measure it
    const container = document.createElement('div');
    container.innerHTML = html;
    container.style.position = 'fixed';
    container.style.left = '-9999px';
    container.style.top = '0';
    container.style.width = '700px';
    document.body.appendChild(container);

    const safeName = (filename ?? cv.fullName ?? 'CV').replace(/[^a-zA-Z0-9_\-\s]/g, '').trim() || 'CV';

    try {
        await html2pdf()
            .set({
                margin: [10, 10, 10, 10],
                filename: `${safeName}.pdf`,
                image: { type: 'jpeg', quality: 0.98 },
                html2canvas: {
                    scale: 2,
                    useCORS: true,
                    letterRendering: true,
                    // Ignore elements with problematic CSS classes
                    ignoreElements: (el: HTMLElement) => {
                        const classList = el.classList || [];
                        // Skip elements with Tailwind's color utilities that use oklch
                        return Array.from(classList).some(cls =>
                            cls.startsWith('bg-') || cls.startsWith('text-') || cls.startsWith('text-')
                        );
                    }
                },
                jsPDF: { unit: 'mm', format: 'a4', orientation: 'portrait' },
            })
            .from(container)
            .save();
    } catch (pdfError) {
        console.error('PDF generation failed:', pdfError);

        // Fallback: Open print dialog for browser's native PDF generation
        const printWindow = window.open('', '_blank');
        if (printWindow) {
            printWindow.document.write(`
                <!DOCTYPE html>
                <html>
                <head>
                    <title>${safeName}</title>
                    <style>
                        body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; padding: 20px; }
                        h1 { color: #1a1a1a; font-size: 24px; }
                        h2 { color: #005f73; font-size: 14px; margin-top: 16px; border-bottom: 1px solid #005f73; padding-bottom: 4px; }
                        .contact { color: #666; font-size: 12px; }
                        .section { margin: 12px 0; }
                        ul { margin: 4px 0 0 18px; padding: 0; }
                        li { margin: 2px 0; color: #444; font-size: 13px; }
                        .tag { display: inline-block; background: #005f73; color: white; padding: 2px 8px; border-radius: 10px; font-size: 11px; margin: 2px; }
                    </style>
                </head>
                <body>${html}</body>
                </html>
            `);
            printWindow.document.close();
            printWindow.print();
        } else {
            throw new Error('Không thể tạo cửa sổ in. Vui lòng cho phép popups.');
        }
    } finally {
        // Clean up
        if (document.body.contains(container)) {
            document.body.removeChild(container);
        }
    }
}
