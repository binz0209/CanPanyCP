import 'i18next';

import commonVi from './locales/vi/common.json';
import authVi from './locales/vi/auth.json';
import publicVi from './locales/vi/public.json';
import companyVi from './locales/vi/company.json';
import candidateVi from './locales/vi/candidate.json';

declare module 'i18next' {
    interface CustomTypeOptions {
        defaultNS: 'common';
        resources: {
            common: typeof commonVi;
            auth: typeof authVi;
            public: typeof publicVi;
            company: typeof companyVi;
            candidate: typeof candidateVi;
        };
    }
}

