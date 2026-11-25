import { DEMO_USERS } from '../../core/session/current-user';
import { setUpPage } from '../../testing/setup';
import { NotificationsMenu } from './notifications-menu';

const alice = DEMO_USERS[0];

function aNotification(overrides: Record<string, unknown> = {}) {
  return {
    id: 'n1',
    claimId: 'c1',
    message: 'Your claim CLM-1 needs more information.',
    createdAt: '2026-11-25T10:00:00Z',
    readAt: null,
    ...overrides,
  };
}

describe('NotificationsMenu', () => {
  async function render(notifications: unknown[]) {
    const page = await setUpPage(NotificationsMenu, alice);
    page.fixture.detectChanges();
    page.http
      .expectOne((r) => r.url === '/api/notifications' && r.params.get('userId') === alice.id)
      .flush(notifications);
    await page.fixture.whenStable();
    page.fixture.detectChanges();
    return page;
  }

  it('shows the unread count on the bell', async () => {
    const { element } = await render([aNotification(), aNotification({ id: 'n2', readAt: 'x' })]);

    expect(element.querySelector('button')?.getAttribute('aria-label')).toBe(
      'Notifications, 1 unread',
    );
  });

  it('shows no unread count when everything is read', async () => {
    const { element } = await render([aNotification({ readAt: '2026-11-25T11:00:00Z' })]);

    expect(element.querySelector('button')?.getAttribute('aria-label')).toBe(
      'Notifications, 0 unread',
    );
  });

  it('marks a notification read when it is opened', async () => {
    const { fixture, http } = await render([aNotification()]);

    // Calls the click handler directly: a real click would also navigate, and this test has no routes.
    (fixture.componentInstance as unknown as { open(n: unknown): void }).open(aNotification());

    const request = await vi.waitFor(() => http.expectOne('/api/notifications/n1/read'));
    expect(request.request.body).toEqual({ userId: alice.id });
  });
});
