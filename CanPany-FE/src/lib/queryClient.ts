import { QueryClient } from '@tanstack/react-query';

export const queryClient = new QueryClient({
    defaultOptions: {
        queries: {
            staleTime: 1000 * 30,       // 30 seconds — data refreshes on navigate
            gcTime: 1000 * 60 * 5,      // keep in cache 5 min (was cacheTime in v4)
            retry: 1,
            refetchOnWindowFocus: true, // re-fetch when user returns to tab/page
            refetchOnMount: true,       // re-fetch when component mounts after being unmounted
        },
        mutations: {
            retry: 0,
        },
    },
});
