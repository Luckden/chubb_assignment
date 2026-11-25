import type { Claim } from '../core/api/api-types';
import { DEMO_USERS } from '../core/session/current-user';

export const [ALICE, BOB, CAROL, DAVE] = DEMO_USERS;

/** A claim as the API returns it, Submitted by Alice. Override what the test cares about. */
export function aClaim(overrides: Partial<Claim> = {}): Claim {
  return {
    id: '11111111-1111-7111-8111-111111111111',
    claimNumber: 'CLM-0001',
    claimantId: ALICE.id,
    assigneeId: null,
    type: 'Motor',
    status: 'Submitted',
    incidentDate: '2026-11-20',
    description: 'Rear-ended at a junction.',
    claimedAmount: 1500,
    decisionOutcome: null,
    decisionReason: null,
    decidedAt: null,
    submittedAt: '2026-11-21T09:00:00Z',
    statusHistory: [
      {
        fromStatus: null,
        toStatus: 'Submitted',
        changedBy: ALICE.id,
        changedAt: '2026-11-21T09:00:00Z',
        note: null,
      },
    ],
    informationRequests: [],
    ...overrides,
  };
}
