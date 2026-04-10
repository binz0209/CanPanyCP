import { useEffect } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { jobsApi } from '../../api/jobs.api';
import { applicationKeys, bookmarkKeys } from '../../lib/queryKeys';
import { useAuthStore } from '../../stores/auth.store';
import { fetchBookmarkedJobsHydrated } from './useBookmarks';
import { fetchMyApplicationsHydrated } from './useCandidateApplications';

/**
 * Warm up high-traffic Candidate data so route transitions feel instant.
 * This runs once per authenticated candidate session and uses short stale windows
 * to avoid over-fetching while still improving navigation responsiveness.
 */
export function useCandidatePrefetch() {
    const queryClient = useQueryClient();
    const { isAuthenticated } = useAuthStore();

    useEffect(() => {
        if (!isAuthenticated) return;

        void queryClient.prefetchQuery({
            queryKey: applicationKeys.mine(),
            queryFn: fetchMyApplicationsHydrated,
            staleTime: 30_000,
        });

        void queryClient.prefetchQuery({
            queryKey: bookmarkKeys.list(),
            queryFn: fetchBookmarkedJobsHydrated,
            staleTime: 60_000,
        });

        void queryClient.prefetchQuery({
            queryKey: ['jobs', 'recommended', 12],
            queryFn: () => jobsApi.getRecommended(12),
            staleTime: 120_000,
        });
    }, [isAuthenticated, queryClient]);
}
