import { expect, test } from '@playwright/test';

test.describe('app shell (API mocked)', () => {
  test.beforeEach(async ({ page }) => {
    await page.route('**/api/notifications**', (route) => route.fulfill({ json: [] }));
  });

  test('shows connected status when the API responds', async ({ page }) => {
    await page.route('**/api/system/info', (route) =>
      route.fulfill({ json: { service: 'claims-api', environment: 'E2E', version: '1.0.0' } }),
    );

    await page.goto('/');

    await expect(page.getByRole('heading', { name: 'Claims Platform' })).toBeVisible();
    await expect(page.getByRole('status')).toHaveText('API: connected (E2E)');
  });

  test('shows unreachable status when the API is down', async ({ page }) => {
    await page.route('**/api/system/info', (route) => route.abort());

    await page.goto('/');

    await expect(page.getByRole('status')).toHaveText('API: unreachable');
  });
});
