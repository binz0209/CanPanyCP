import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import LanguageDetector from 'i18next-browser-languagedetector';

import viCommon from './locales/vi/common.json';
import viAuth from './locales/vi/auth.json';
import viPublic from './locales/vi/public.json';
import viCompany from './locales/vi/company.json';
import viCandidate from './locales/vi/candidate.json';

import enCommon from './locales/en/common.json';
import enAuth from './locales/en/auth.json';
import enPublic from './locales/en/public.json';
import enCompany from './locales/en/company.json';
import enCandidate from './locales/en/candidate.json';

// With inline resources, i18next.init() resolves synchronously.
// DO NOT use `void` — it discards the promise and may cause React to
// render before i18n is ready, breaking language-change subscriptions.
i18n
    .use(LanguageDetector)
    .use(initReactI18next)
    .init({
        fallbackLng: 'vi',
        supportedLngs: ['vi', 'en'],
        debug: false,
        // Suppress i18next's Locize promotional console message in dev
        keySeparator: '.',
        nsSeparator: ':',
        defaultNS: 'common',
        ns: ['common', 'auth', 'public', 'company', 'candidate'],
        interpolation: {
            escapeValue: false,
        },
        detection: {
            order: ['localStorage', 'navigator'],
            caches: ['localStorage'],
        },
        resources: {
            vi: {
                common: viCommon,
                auth: viAuth,
                public: viPublic,
                company: viCompany,
                candidate: viCandidate,
            },
            en: {
                common: enCommon,
                auth: enAuth,
                public: enPublic,
                company: enCompany,
                candidate: enCandidate,
            },
        },
    });

export default i18n;
