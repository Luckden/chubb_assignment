import { defineConfig, devices } from '@playwright/test';

/**
 * End-to-end tests.
 *
 * - Default: starts `ng serve` and runs against http://localhost:4200. Specs mock the API with `page.route`
 *   unless they are tagged @stack.
 * - Full stack: set E2E_BASE_URL (e.g. http://localhost:4200 served by `docker compose up`) to skip starting a
 *   dev server; @stack specs then run against the real API.
 */
const externalBaseUrl = process.env['E2E_BASE_URL'];
const baseURL = externalBaseUrl ?? 'http://localhost:4200';

export default defineConfig({
  testDir: './e2e',
  testMatch: '**/*.e2e.ts',
  fullyParallel: true,
  forbidOnly: !!process.env['CI'],
  retries: process.env['CI'] ? 2 : 0,
  reporter: [['list'], ['html', { open: 'never', outputFolder: 'playwright-report' }]],
  outputDir: 'test-results',
  use: {
    baseURL,
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
  },
  // @stack specs need the real backend, so they only run when an external URL is given.
  grepInvert: externalBaseUrl ? undefined : /@stack/,
  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }],
  webServer: externalBaseUrl
    ? undefined
    : {
        command: 'npm start',
        url: baseURL,
        reuseExistingServer: !process.env['CI'],
        timeout: 120_000,
      },
});
