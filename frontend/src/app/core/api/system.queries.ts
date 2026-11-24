import { inject } from '@angular/core';
import { injectQuery } from '@tanstack/angular-query';
import { lastValueFrom } from 'rxjs';

import { SystemApiService } from './system-api.service';

/**
 * Query keys and query helpers for the system endpoints.
 *
 * Pattern for every API area: one `*.queries.ts` exporting
 *  - a key factory (used for `invalidateQueries` after mutations), and
 *  - `inject*Query()` / `inject*Mutation()` helpers that components call from a field initialiser.
 *
 * Resolve services with `inject()` *before* `injectQuery`: its options callback re-runs later, outside the
 * injection context, so calling `inject()` inside it throws NG0203.
 */
export const systemKeys = {
  all: ['system'] as const,
  info: () => [...systemKeys.all, 'info'] as const,
};

export function injectSystemInfoQuery() {
  const api = inject(SystemApiService);
  return injectQuery(() => ({
    queryKey: systemKeys.info(),
    queryFn: () => lastValueFrom(api.getInfo()),
  }));
}
