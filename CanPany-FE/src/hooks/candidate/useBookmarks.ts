import { useMemo } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import toast from 'react-hot-toast';
import { isAxiosError } from 'axios';
import { companiesApi, jobsApi } from '../../api';
import type { Job } from '../../types';
import { useAuthStore } from '../../stores/auth.store';
import { bookmarkKeys } from '../../lib/queryKeys';

async function hydrateJobsWithCompanies(jobs: Job[]): Promise<Job[]> {
    const uniqueCompanyIds = Array.from(
        new Set(
            jobs
                .filter((job) => !job.company && Boolean(job.companyId))
                .map((job) => job.companyId)
        )
    );

    if (uniqueCompanyIds.length === 0) {
        return jobs;
    }

    const companyResponses = await Promise.allSettled(
        uniqueCompanyIds.map((companyId) => companiesApi.getById(companyId))
    );

    const companiesById = new Map<string, Awaited<ReturnType<typeof companiesApi.getById>>>();
    companyResponses.forEach((result, index) => {
        if (result.status === 'fulfilled' && result.value) {
            companiesById.set(uniqueCompanyIds[index], result.value);
        }
    });

    return jobs.map((job) => {
        if (job.company || !job.companyId) {
            return job;
        }

        const company = companiesById.get(job.companyId);
        return company
            ? {
                ...job,
                company,
            }
            : job;
    });
}

export async function fetchBookmarkedJobsHydrated(): Promise<Job[]> {
    const jobs = await jobsApi.getBookmarked();
    return hydrateJobsWithCompanies(jobs);
}

/**
 * Hook that manages bookmark state for the current candidate.
 *
 * - Fetches the full list of bookmarked jobs once per session (1 min stale).
 * - Exposes `isBookmarked(jobId)` for cheap O(1) lookup throughout the app.
 * - `toggle(job)` performs an optimistic update so the UI reacts instantly,
 *   then invalidates the cache to get the server-confirmed state.
 * - Only active when the user is authenticated.
 */
export function useBookmarks() {
    const { isAuthenticated } = useAuthStore();
    const queryClient = useQueryClient();

    const bookmarksQuery = useQuery({
        queryKey: bookmarkKeys.list(),
        queryFn: fetchBookmarkedJobsHydrated,
        enabled: isAuthenticated,
        // Bookmarks don't change without user action, so 1 min stale is safe.
        staleTime: 60_000,
    });

    // Build a Set so isBookmarked() is O(1) instead of O(n) on every render.
    const bookmarkedIds = useMemo(
        () => new Set((bookmarksQuery.data ?? []).map((job) => job.id)),
        [bookmarksQuery.data]
    );

    const toggleMutation = useMutation({
        mutationFn: async (job: Job) => {
            if (bookmarkedIds.has(job.id)) {
                await jobsApi.removeBookmark(job.id);
            } else {
                await jobsApi.bookmark(job.id);
                // Fire and forget tracking. type: 3 = Bookmark
                jobsApi.trackInteraction(job.id, 3).catch(console.error);
            }
            return job;
        },

        onMutate: async (job) => {
            // Prevent stale refetch from overwriting the optimistic state.
            await queryClient.cancelQueries({ queryKey: bookmarkKeys.list() });

            const previousList = queryClient.getQueryData<Job[]>(bookmarkKeys.list());

            if (bookmarkedIds.has(job.id)) {
                // Optimistic remove: filter the job out immediately.
                queryClient.setQueryData<Job[]>(bookmarkKeys.list(), (current = []) =>
                    current.filter((j) => j.id !== job.id)
                );
            } else {
                // Optimistic add: append the full job object to the list.
                queryClient.setQueryData<Job[]>(bookmarkKeys.list(), (current = []) => [
                    ...current,
                    job,
                ]);
            }

            return { previousList };
        },

        onError: (_error, _job, context) => {
            // Roll back to the snapshot captured in onMutate.
            queryClient.setQueryData(bookmarkKeys.list(), context?.previousList);

            const message = isAxiosError(_error)
                ? _error.response?.data?.message || 'Không thể cập nhật trạng thái lưu'
                : 'Không thể cập nhật trạng thái lưu';
            toast.error(message);
        },

        onSuccess: (job) => {
            // Optimistic update already set the UI — invalidate for server consistency.
            queryClient.invalidateQueries({ queryKey: bookmarkKeys.list(), exact: true });

            // Check the cache *after* the optimistic flip to determine the final direction.
            const currentIds = new Set(
                (queryClient.getQueryData<Job[]>(bookmarkKeys.list()) ?? []).map((j) => j.id)
            );
            if (currentIds.has(job.id)) {
                toast.success('Đã lưu việc làm');
            } else {
                toast.success('Đã bỏ lưu việc làm');
            }
        },
    });

    return {
        /** True when the initial bookmark list is being fetched. */
        isLoading: bookmarksQuery.isLoading,

        /** True when bookmarks are being refreshed in background. */
        isFetching: bookmarksQuery.isFetching,

        /** Last query error (if any). */
        error: bookmarksQuery.error,

        /** Force refresh bookmarked jobs from server. */
        refetch: bookmarksQuery.refetch,

        /** Full list of saved jobs (for the SavedJobsPage). */
        savedJobs: bookmarksQuery.data ?? [],

        /** O(1) lookup: is this job currently bookmarked? */
        isBookmarked: (jobId: string): boolean => bookmarkedIds.has(jobId),

        /**
         * Toggle bookmark for a job.
         * Requires the full Job object so the optimistic-add path can insert it
         * into the cache without an extra server round-trip.
         * Shows a login toast and does nothing when the user is not authenticated.
         */
        toggle: (job: Job) => {
            if (!isAuthenticated) {
                toast.error('Vui lòng đăng nhập để lưu việc làm');
                return;
            }
            toggleMutation.mutate(job);
        },

        /** True when this specific job's toggle is in flight. */
        isToggling: (jobId: string): boolean =>
            toggleMutation.isPending && toggleMutation.variables?.id === jobId,
    };
}
