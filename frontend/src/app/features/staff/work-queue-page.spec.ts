import type { ClaimSummary } from '../../core/api/api-types';
import { ALICE, CAROL } from '../../testing/claims';
import { setUpPage } from '../../testing/setup';
import { WorkQueuePage } from './work-queue-page';

const row: ClaimSummary = {
  id: '11111111-1111-7111-8111-111111111111',
  claimNumber: 'CLM-0001',
  claimantId: ALICE.id,
  assigneeId: CAROL.id,
  type: 'Motor',
  status: 'UnderReview',
  claimedAmount: 1500,
  submittedAt: '2026-11-20T09:00:00Z',
  statusChangedAt: '2026-11-21T09:00:00Z',
};

describe('WorkQueuePage', () => {
  it('asks for each tab oldest in status first and shows counts and time in status (D6)', async () => {
    const { fixture, http, element } = await setUpPage(WorkQueuePage, CAROL);
    fixture.detectChanges();

    const lists = http.match((r) => r.url === '/api/claims');
    const filters = lists.map((r) => r.request.params.toString());
    expect(filters).toEqual([
      `assigneeId=${CAROL.id}&status=UnderReview&sort=StatusChangedAt`,
      `assigneeId=${CAROL.id}&status=InfoRequested&sort=StatusChangedAt`,
      'unassigned=true&sort=StatusChangedAt',
    ]);
    lists[0].flush([row]);
    lists[1].flush([]);
    lists[2].flush([]);
    await fixture.whenStable();

    const tabs = [...element.querySelectorAll('[role="tab"]')].map((t) => t.textContent?.trim());
    expect(tabs).toEqual(['Needs me (1)', 'Waiting on claimant (0)', 'Unassigned (0)']);
    expect(element.textContent).toContain('In status for');
    expect(element.textContent).toMatch(/\d+ days/);
  });
});
