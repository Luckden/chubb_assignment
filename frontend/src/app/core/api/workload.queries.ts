import { inject } from '@angular/core';
import { injectQuery } from '@tanstack/angular-query';
import { lastValueFrom } from 'rxjs';

import { claimKeys } from './claims.queries';
import { POLL_INTERVAL_MS } from './polling';
import { TeamApiService } from './team-api.service';

/** Under `claimKeys.team()`, so every claim mutation also refreshes the workload. */
export const workloadKeys = {
  all: [...claimKeys.team(), 'workload'] as const,
  forOfficer: (officerId: string) => [...workloadKeys.all, officerId] as const,
};

/** R8: the acting officer's team workload and performance, refreshed by polling (RFC-0003 section 4). */
export function injectWorkloadQuery(officerId: () => string) {
  const api = inject(TeamApiService);
  return injectQuery(() => {
    const current = officerId();
    return {
      queryKey: workloadKeys.forOfficer(current),
      queryFn: () => lastValueFrom(api.workload(current)),
      refetchInterval: POLL_INTERVAL_MS,
      refetchIntervalInBackground: false,
    };
  });
}
