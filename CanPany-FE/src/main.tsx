import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { I18nextProvider } from 'react-i18next';
import './index.css';
import i18n from './i18n';
import App from './App.tsx';

// I18nextProvider passes the i18n instance explicitly through React context.
// This guarantees all useTranslation() hooks re-render when changeLanguage() is called.
createRoot(document.getElementById('root')!).render(
    <StrictMode>
        <I18nextProvider i18n={i18n}>
            <App />
        </I18nextProvider>
    </StrictMode>
);
