import { useTranslation } from 'react-i18next';

export function LanguageSwitcher() {
    const { i18n } = useTranslation();
    const current = i18n.language || 'vi';

    const setLanguage = (lng: 'vi' | 'en') => {
        // i18next-browser-languagedetector handles localStorage persistence automatically
        void i18n.changeLanguage(lng);
    };

    return (
        <div className="flex items-center gap-1 rounded-full border border-gray-200 bg-white px-1 py-0.5 text-xs font-medium text-gray-600 dark:border-gray-700 dark:bg-gray-800 dark:text-gray-300">
            <button
                type="button"
                onClick={() => setLanguage('vi')}
                className={
                    current.startsWith('vi')
                        ? 'rounded-full bg-[#00b14f] px-2 py-0.5 text-white'
                        : 'rounded-full px-2 py-0.5 hover:bg-gray-100 dark:hover:bg-gray-700'
                }
            >
                VI
            </button>
            <button
                type="button"
                onClick={() => setLanguage('en')}
                className={
                    current.startsWith('en')
                        ? 'rounded-full bg-[#00b14f] px-2 py-0.5 text-white'
                        : 'rounded-full px-2 py-0.5 hover:bg-gray-100 dark:hover:bg-gray-700'
                }
            >
                EN
            </button>
        </div>
    );
}
