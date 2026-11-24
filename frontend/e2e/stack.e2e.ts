import { expect, test } from '@playwright/test';

// Runs only when E2E_BASE_URL is set (see playwright.config.ts), e.g. against `docker compose up`.
test('SPA reaches the real API through the proxy @stack', async ({ page }) => {
  await page.goto('/');

  await expect(page.getByRole('status')).toHaveText(/API: connected/);
});
