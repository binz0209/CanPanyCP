import { useQuery } from '@tanstack/react-query';
import { isAxiosError } from 'axios';
import { companiesApi } from '../../api';
import { companyKeys } from '../../lib/queryKeys';

export function useCompanyWorkspace() {
    const companyQuery = useQuery({
        queryKey: companyKeys.me(),
        queryFn: () => companiesApi.getMe(),
        retry: false,
    });

    const company = companyQuery.data;
    const companyId = company?.id;
    const isMissingProfile = isAxiosError(companyQuery.error) && companyQuery.error.response?.status === 404;
    const hasFatalError = Boolean(companyQuery.error && !isMissingProfile);

    return {
        companyQuery,
        company,
        companyId,
        isLoading: companyQuery.isLoading,
        isMissingProfile,
        hasFatalError,
        error: companyQuery.error,
    };
}
