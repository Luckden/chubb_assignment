import { expect, test, type Page } from '@playwright/test';

const ALICE = '00000000-0000-7000-8000-000000000001';
const CLAIM_ID = '11111111-1111-7111-8111-111111111111';

function aClaim(overrides: Record<string, unknown> = {}) {
  return {
    id: CLAIM_ID,
    claimNumber: 'CLM-0001',
    claimantId: ALICE,
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
    statusChangedAt: '2026-11-21T09:00:00Z',
    statusHistory: [
      {
        fromStatus: null,
        toStatus: 'Submitted',
        changedBy: ALICE,
        changedAt: '2026-11-21T09:00:00Z',
        note: null,
      },
    ],
    informationRequests: [],
    ...overrides,
  };
}

/** A one-claim fake of /api/claims: list (with its filters), get, submit and pick up. */
async function mockClaimsApi(page: Page, initial: ReturnType<typeof aClaim> | null) {
  let claim = initial;
  await page.route('**/api/system/info', (route) =>
    route.fulfill({ json: { service: 'claims-api', environment: 'E2E', version: '1.0.0' } }),
  );
  await page.route('**/api/notifications**', (route) => route.fulfill({ json: [] }));
  await page.route('**/api/claims**', async (route) => {
    const request = route.request();
    const url = new URL(request.url());
    if (request.method() === 'GET' && url.pathname === '/api/claims') {
      const unassigned = url.searchParams.get('unassigned') === 'true';
      const assigneeId = url.searchParams.get('assigneeId');
      const status = url.searchParams.get('status');
      const matches =
        claim &&
        (!unassigned || claim.assigneeId === null) &&
        (!assigneeId || claim.assigneeId === assigneeId) &&
        (!status || claim.status === status);
      return route.fulfill({ json: matches ? [claim] : [] });
    }
    if (request.method() === 'POST' && url.pathname === '/api/claims') {
      claim = aClaim(request.postDataJSON());
      return route.fulfill({ status: 201, json: claim });
    }
    if (request.method() === 'POST' && url.pathname.endsWith('/pick-up')) {
      claim = aClaim({ status: 'UnderReview', assigneeId: request.postDataJSON().officerId });
      return route.fulfill({ json: claim });
    }
    if (claim && url.pathname === `/api/claims/${claim.id}`) return route.fulfill({ json: claim });
    return route.fulfill({ status: 404, json: { title: 'Not Found' } });
  });
}

test.describe('claims (API mocked)', () => {
  test('a claimant reports an incident and sees the new claim', async ({ page }) => {
    await mockClaimsApi(page, null);

    await page.goto('/');
    await expect(page.getByRole('heading', { name: 'My claims' })).toBeVisible();
    await expect(page.getByText('You have no claims yet.')).toBeVisible();

    await page.getByRole('main').getByRole('link', { name: 'Report an incident' }).click();
    await page.getByRole('combobox', { name: 'Type of claim' }).click();
    await page.getByRole('option', { name: 'Motor' }).click();
    await page.getByLabel('Date of the incident').fill('2026-11-20');
    await page.getByLabel('What happened?').fill('Rear-ended at a junction.');
    await page.getByLabel('Amount claimed').fill('1500');
    await page.getByRole('button', { name: 'Submit claim' }).click();

    await expect(page.getByRole('heading', { name: 'Claim CLM-0001' })).toBeVisible();
    await expect(page.getByText('Not yet assigned')).toBeVisible();
  });

  test('an officer picks up an incoming claim', async ({ page }) => {
    await mockClaimsApi(page, aClaim());

    await page.goto('/');
    await page.getByRole('combobox', { name: 'Acting as' }).click();
    await page.getByRole('option', { name: 'Carol Wong' }).click();

    await expect(page.getByRole('heading', { name: 'Work queue' })).toBeVisible();
    await page.getByRole('tab', { name: 'Unassigned (1)' }).click();
    await page.getByRole('link', { name: 'CLM-0001' }).click();
    await page.getByRole('button', { name: 'Pick up' }).click();

    await expect(page.getByRole('heading', { name: 'Ask the claimant' })).toBeVisible();
    await expect(page.getByRole('button', { name: 'Settle claim' })).toBeVisible();
    await expect(page.getByRole('definition').filter({ hasText: 'Carol Wong' })).toBeVisible();
  });
});
