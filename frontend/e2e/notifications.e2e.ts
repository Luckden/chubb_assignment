import { expect, test, type Page } from '@playwright/test';

const ALICE = '00000000-0000-7000-8000-000000000001';
const CLAIM_ID = '11111111-1111-7111-8111-111111111111';
const NOTIFICATION_ID = '22222222-2222-7222-8222-222222222222';

const claim = {
  id: CLAIM_ID,
  claimNumber: 'CLM-0001',
  claimantId: ALICE,
  assigneeId: null,
  type: 'Motor',
  status: 'InfoRequested',
  incidentDate: '2026-11-20',
  description: 'Rear-ended at a junction.',
  claimedAmount: 1500,
  decisionOutcome: null,
  decisionReason: null,
  decidedAt: null,
  submittedAt: '2026-11-21T09:00:00Z',
  statusHistory: [],
  informationRequests: [],
};

/** Fakes the claim and notification endpoints. Returns the ids the SPA marked as read. */
async function mockApi(page: Page): Promise<string[]> {
  const markedRead: string[] = [];
  let readAt: string | null = null;

  await page.route('**/api/system/info', (route) =>
    route.fulfill({ json: { service: 'claims-api', environment: 'E2E', version: '1.0.0' } }),
  );
  await page.route('**/api/claims**', (route) => {
    const path = new URL(route.request().url()).pathname;
    return route.fulfill({ json: path === '/api/claims' ? [claim] : claim });
  });
  await page.route('**/api/notifications**', (route) => {
    const request = route.request();
    const path = new URL(request.url()).pathname;
    if (request.method() === 'POST' && path === `/api/notifications/${NOTIFICATION_ID}/read`) {
      markedRead.push(request.postDataJSON().userId);
      readAt = '2026-11-25T11:00:00Z';
    }
    const notification = {
      id: NOTIFICATION_ID,
      claimId: CLAIM_ID,
      message: 'Your claim CLM-0001 needs more information.',
      createdAt: '2026-11-25T10:00:00Z',
      readAt,
    };
    return route.fulfill({ json: request.method() === 'POST' ? notification : [notification] });
  });

  return markedRead;
}

test.describe('notifications (API mocked)', () => {
  test('a claimant sees an unread notification, opens it and it is marked read', async ({
    page,
  }) => {
    const markedRead = await mockApi(page);

    await page.goto('/');
    const bell = page.getByRole('button', { name: /^Notifications/ });
    await expect(bell).toHaveAccessibleName('Notifications, 1 unread');

    await bell.click();
    await page
      .getByRole('menuitem', { name: 'Your claim CLM-0001 needs more information.' })
      .click();

    await expect(page.getByRole('heading', { name: 'Claim CLM-0001' })).toBeVisible();
    await expect(bell).toHaveAccessibleName('Notifications, 0 unread');
    expect(markedRead).toEqual([ALICE]);
  });

  test('an officer has no notifications bell', async ({ page }) => {
    await mockApi(page);

    await page.goto('/');
    await page.getByRole('combobox', { name: 'Acting as' }).click();
    await page.getByRole('option', { name: 'Carol Wong' }).click();

    await expect(page.getByRole('heading', { name: 'Work queue' })).toBeVisible();
    await expect(page.getByRole('button', { name: /^Notifications/ })).toHaveCount(0);
  });
});
