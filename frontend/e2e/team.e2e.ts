import { expect, test } from '@playwright/test';

const CAROL = '00000000-0000-7000-8000-000000000011';
const DAVE = '00000000-0000-7000-8000-000000000012';

const exposure = {
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

const workload = {
  teamId: 'team-a',
  asOf: '2026-11-25T12:00:00Z',
  periodDays: 30,
  unassigned: { count: 1, oldestSubmittedAt: '2026-11-22T12:00:00Z' },
  officers: [
    {
      officerId: CAROL,
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
      officerId: DAVE,
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

test.describe('team page (API mocked)', () => {
  test('an officer sees claimed exposure and the team workload', async ({ page }) => {
    await page.route('**/api/system/info', (route) =>
      route.fulfill({ json: { service: 'claims-api', environment: 'E2E', version: '1.0.0' } }),
    );
    await page.route('**/api/claims**', (route) => route.fulfill({ json: [] }));
    await page.route('**/api/exposure**', (route) => route.fulfill({ json: exposure }));
    await page.route('**/api/workload**', (route) => route.fulfill({ json: workload }));

    await page.goto('/');
    await page.getByRole('combobox', { name: 'Acting as' }).click();
    await page.getByRole('option', { name: 'Carol Wong' }).click();
    await page.getByRole('link', { name: 'Team' }).click();

    await expect(page.getByRole('heading', { name: 'Claimed exposure' })).toBeVisible();
    await expect(page.getByText('6,000.00').first()).toBeVisible();
    await expect(page.getByRole('table', { name: 'Claimed exposure By status' })).toContainText(
      'Information requested',
    );
    const carol = page.getByRole('table', { name: 'Team workload' }).getByRole('row', {
      name: /Carol Wong \(you\)/,
    });
    await expect(carol).toContainText('2.5');
  });

  test('a claimant has no Team link', async ({ page }) => {
    await page.route('**/api/**', (route) => route.fulfill({ json: [] }));

    await page.goto('/');

    await expect(page.getByRole('heading', { name: 'My claims' })).toBeVisible();
    await expect(page.getByRole('link', { name: 'Team' })).toHaveCount(0);
  });
});
