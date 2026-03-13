import { useMemo } from 'react';
import { useQueries } from '@tanstack/react-query';
import { candidateApi } from '../../api';
import type { CandidateFullProfile } from '../../api/candidate.api';
import { candidateKeys } from '../../lib/queryKeys';

export function useCandidateProfilesMap(candidateIds: string[]) {
    const uniqueCandidateIds = useMemo(
        () => Array.from(new Set(candidateIds.filter(Boolean))),
        [candidateIds]
    );

    const candidateProfileQueries = useQueries({
        queries: uniqueCandidateIds.map((candidateId) => ({
            queryKey: candidateKeys.profile(candidateId),
            queryFn: () => candidateApi.getCandidateProfile(candidateId),
            enabled: Boolean(candidateId),
        })),
    });

    const candidateProfilesMap = useMemo<Record<string, CandidateFullProfile>>(
        () =>
            uniqueCandidateIds.reduce<Record<string, CandidateFullProfile>>((accumulator, candidateId, index) => {
                const profile = candidateProfileQueries[index]?.data;
                if (profile) {
                    accumulator[candidateId] = profile;
                }
                return accumulator;
            }, {}),
        [candidateProfileQueries, uniqueCandidateIds]
    );

    return {
        candidateProfilesMap,
        isLoading: candidateProfileQueries.some((query) => query.isLoading),
        isFetching: candidateProfileQueries.some((query) => query.isFetching),
    };
}
