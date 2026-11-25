import type { Exposure, Workload } from '../../core/api/api-types';
import { CAROL, DAVE } from '../../testing/claims';
import { setUpPage } from '../../testing/setup';
import { TeamPage } from './team-page';

const exposure: Exposure = {
  asOf: '2026-11-25T12:00:00Z',
  openClaims: 3,
  claimedExposure: 6000,
  byStatus: [
    { status: 'Submitted', count: 1, claimedAmount: 1000 },
    { status: 'UnderReview', count: 1, claimedAmount: 2000 },
    { status: 'InfoRequested', count: 1, claimedAmount: 3000 },
  ],
  byType: [
    { type: 'Motor', count: 2, claimedAmount: 3000 },
    { type: 'Property', count: 1, claimedAmount: 3000 },
  ],
  byAge: [
    { bucket: '0-7 days', count: 3, claimedAmount: 6000 },
    { bucket: '8-30 days', count: 0, claimedAmount: 0 },
    { bucket: '31+ days', count: 0, claimedAmount: 0 },
  ],
};

const workload: Workload = {
  teamId: 'team-a',
  asOf: '2026-11-25T12:00:00Z',
  periodDays: 30,
  unassigned: { count: 1, oldestSubmittedAt: '2026-11-22T12:00:00Z' },
  officers: [
    {
      officerId: CAROL.id,
      name: 'Carol Wong (officer)',
      underReview: 1,
      infoRequested: 1,
      oldestOpenStatusChangedAt: '2026-11-24T12:00:00Z',
      decidedInPeriod: 4,
      settledInPeriod: 3,
      rejectedInPeriod: 1,
      averageDaysToDecision: 2.5,
    },
    {
      officerId: DAVE.id,
      name: 'Dave Ng (officer)',
      underReview: 0,
      infoRequested: 0,
      oldestOpenStatusChangedAt: null,
      decidedInPeriod: 0,
      settledInPeriod: 0,
      rejectedInPeriod: 0,
      averageDaysToDecision: null,
    },
  ],
};

describe('TeamPage', () => {
  async function render() {
    const page = await setUpPage(TeamPage, CAROL);
    page.fixture.detectChanges();
    return page;
  }

  it('shows claimed exposure with its breakdowns (D7)', async () => {
    const { fixture, http, element } = await render();

    http.expectOne(`/api/exposure?officerId=${CAROL.id}`).flush(exposure);
    http.expectOne(`/api/workload?officerId=${CAROL.id}`).flush(workload);
    await fixture.whenStable();

    expect(element.textContent).toContain('6,000.00');
    expect(element.textContent).toContain('upper bound');
    const statusTable = element.querySelector('[aria-label="Claimed exposure By status"]');
    expect(statusTable?.textContent).toContain('Information requested');
    expect(statusTable?.textContent).toContain('3,000.00');
  });

  it('shows open work and decisions per officer (R8)', async () => {
    const { fixture, http, element } = await render();

    http.expectOne(`/api/exposure?officerId=${CAROL.id}`).flush(exposure);
    http.expectOne(`/api/workload?officerId=${CAROL.id}`).flush(workload);
    await fixture.whenStable();

    const rows = [...element.querySelectorAll('[aria-label="Team workload"] tr[mat-row]')].map(
      (r) => [...r.querySelectorAll('td')].map((td) => td.textContent?.trim()),
    );
    expect(rows).toEqual([
      ['Carol Wong (you)', '1', '1', '24 hours', '4', '3', '1', '2.5'],
      ['Dave Ng', '0', '0', '—', '0', '0', '0', '—'],
    ]);
    expect(element.querySelector('[aria-label="Unassigned"]')?.textContent).toContain(
      'oldest waiting 3 days',
    );
  });

  it('shows the API error for a non-officer', async () => {
    const { fixture, http, element } = await render();

    http
      .expectOne(`/api/exposure?officerId=${CAROL.id}`)
      .flush(
        { title: 'Forbidden', detail: 'Not an officer.' },
        { status: 403, statusText: 'Forbidden' },
      );
    http.expectOne(`/api/workload?officerId=${CAROL.id}`).flush(workload);
    await fixture.whenStable();

    expect(element.querySelector('[role="alert"]')?.textContent).toContain('Not an officer.');
  });
});
