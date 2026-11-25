import { inject } from '@angular/core';
import { QueryClient, injectMutation, injectQuery } from '@tanstack/angular-query';
import { Observable, lastValueFrom } from 'rxjs';

import type {
  Claim,
  ClaimListFilter,
  PickUpClaimRequest,
  ProvideInformationRequest,
  RejectClaimRequest,
  RequestInformationRequest,
  SettleClaimRequest,
  SubmitClaimRequest,
} from './api-types';
import { ClaimsApiService } from './claims-api.service';
import { POLL_INTERVAL_MS } from './polling';

/**
 * Query keys for claims. Every mutation invalidates `lists()` and `team()` (the workload and exposure views built
 * from claims) and replaces the claim's `detail()`.
 */
export const claimKeys = {
  all: ['claims'] as const,
  lists: () => [...claimKeys.all, 'list'] as const,
  team: () => [...claimKeys.all, 'team'] as const,
  list: (filter: ClaimListFilter) => [...claimKeys.lists(), filter] as const,
  detail: (id: string) => [...claimKeys.all, 'detail', id] as const,
};

export function injectClaimListQuery(filter: () => ClaimListFilter) {
  const api = inject(ClaimsApiService);
  return injectQuery(() => {
    const current = filter();
    return {
      queryKey: claimKeys.list(current),
      queryFn: () => lastValueFrom(api.list(current)),
      refetchInterval: POLL_INTERVAL_MS,
      refetchIntervalInBackground: false,
    };
  });
}

export function injectClaimQuery(id: () => string) {
  const api = inject(ClaimsApiService);
  return injectQuery(() => {
    const current = id();
    return {
      queryKey: claimKeys.detail(current),
      queryFn: () => lastValueFrom(api.get(current)),
      refetchInterval: POLL_INTERVAL_MS,
      refetchIntervalInBackground: false,
    };
  });
}

/** Every claim command returns the updated claim, so the detail cache is replaced instead of refetched. */
function injectClaimMutation<TVariables>(
  call: (api: ClaimsApiService, variables: TVariables) => Observable<Claim>,
) {
  const api = inject(ClaimsApiService);
  const queryClient = inject(QueryClient);
  return injectMutation(() => ({
    mutationFn: (variables: TVariables) => lastValueFrom(call(api, variables)),
    onSuccess: (claim: Claim) => {
      queryClient.setQueryData(claimKeys.detail(claim.id), claim);
      return Promise.all([
        queryClient.invalidateQueries({ queryKey: claimKeys.lists() }),
        queryClient.invalidateQueries({ queryKey: claimKeys.team() }),
      ]);
    },
  }));
}

interface ClaimCommand<TBody> {
  id: string;
  body: TBody;
}

export const injectSubmitClaimMutation = () =>
  injectClaimMutation((api, body: SubmitClaimRequest) => api.submit(body));

export const injectPickUpClaimMutation = () =>
  injectClaimMutation((api, { id, body }: ClaimCommand<PickUpClaimRequest>) =>
    api.pickUp(id, body),
  );

export const injectRequestInformationMutation = () =>
  injectClaimMutation((api, { id, body }: ClaimCommand<RequestInformationRequest>) =>
    api.requestInformation(id, body),
  );

export const injectProvideInformationMutation = () =>
  injectClaimMutation((api, { id, body }: ClaimCommand<ProvideInformationRequest>) =>
    api.provideInformation(id, body),
  );

export const injectSettleClaimMutation = () =>
  injectClaimMutation((api, { id, body }: ClaimCommand<SettleClaimRequest>) =>
    api.settle(id, body),
  );

export const injectRejectClaimMutation = () =>
  injectClaimMutation((api, { id, body }: ClaimCommand<RejectClaimRequest>) =>
    api.reject(id, body),
  );
