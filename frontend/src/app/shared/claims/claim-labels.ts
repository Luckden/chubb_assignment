import type { ClaimStatus, ClaimType } from '../../core/api/api-types';

export const CLAIM_STATUS_LABELS: Record<ClaimStatus, string> = {
  Submitted: 'Submitted',
  UnderReview: 'Under review',
  InfoRequested: 'Information requested',
  Settled: 'Settled',
  Rejected: 'Rejected',
};

export const CLAIM_TYPE_LABELS: Record<ClaimType, string> = {
  Motor: 'Motor',
  Property: 'Property',
};

export const CLAIM_TYPES = Object.keys(CLAIM_TYPE_LABELS) as ClaimType[];
