import { inject } from '@angular/core';
import { injectQuery } from '@tanstack/angular-query';
import { lastValueFrom } from 'rxjs';

import { claimKeys } from './claims.queries';
import { POLL_INTERVAL_MS } from './polling';
import { TeamApiService } from './team-api.service';

/** Under `claimKeys.team()`, so every claim mutation also refreshes the exposure. */
export const exposureKeys = {
  all: [...claimKeys.team(), 'exposure'] as const,
  forOfficer: (officerId: string) => [...exposureKeys.all, officerId] as const,
};

/** D7: claimed exposure over the whole book, refreshed by polling (RFC-0003 section 4). */
export function injectExposureQuery(officerId: () => string) {
  const api = inject(TeamApiService);
  return injectQuery(() => {
    const current = officerId();
    return {
      queryKey: exposureKeys.forOfficer(current),
      queryFn: () => lastValueFrom(api.exposure(current)),
      refetchInterval: POLL_INTERVAL_MS,
      refetchIntervalInBackground: false,
    };
  });
}
