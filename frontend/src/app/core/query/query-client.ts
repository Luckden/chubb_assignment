import { QueryClient } from '@tanstack/angular-query';

/** App-wide TanStack Query defaults. Server state lives in the query cache, not in component signals. */
export function createQueryClient(): QueryClient {
  return new QueryClient({
    defaultOptions: {
      queries: {
        staleTime: 30_000,
        retry: 1,
        refetchOnWindowFocus: true,
      },
    },
  });
}
