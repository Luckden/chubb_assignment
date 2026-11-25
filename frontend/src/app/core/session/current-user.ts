import { Injectable, computed, signal } from '@angular/core';

export type UserRole = 'Claimant' | 'Officer';

export interface DemoUser {
  id: string;
  name: string;
  role: UserRole;
}

/**
 * The API has no authentication yet: the acting user's id is sent in each request. These mirror the backend's
 * `DemoUsers` seed (DemoUserSeeder.cs). Replace with the signed-in user once auth is decided.
 */
export const DEMO_USERS: readonly DemoUser[] = [
  { id: '00000000-0000-7000-8000-000000000001', name: 'Alice Tan', role: 'Claimant' },
  { id: '00000000-0000-7000-8000-000000000002', name: 'Bob Lim', role: 'Claimant' },
  { id: '00000000-0000-7000-8000-000000000011', name: 'Carol Wong', role: 'Officer' },
  { id: '00000000-0000-7000-8000-000000000012', name: 'Dave Ng', role: 'Officer' },
  { id: '00000000-0000-7000-8000-000000000022', name: 'Sample Staff', role: 'Officer' },
];

const STORAGE_KEY = 'claims.actingUserId';

/** Home route for each role. */
export const ROLE_HOME: Record<UserRole, string> = {
  Claimant: '/my-claims',
  Officer: '/work',
};

/** Who the app is acting as. Kept in sessionStorage so a reload keeps the same user. */
@Injectable({ providedIn: 'root' })
export class CurrentUser {
  private readonly selectedId = signal(sessionStorage.getItem(STORAGE_KEY) ?? DEMO_USERS[0].id);

  readonly user = computed(
    () => DEMO_USERS.find((u) => u.id === this.selectedId()) ?? DEMO_USERS[0],
  );
  readonly id = computed(() => this.user().id);
  readonly isOfficer = computed(() => this.user().role === 'Officer');

  select(id: string): void {
    sessionStorage.setItem(STORAGE_KEY, id);
    this.selectedId.set(id);
  }

  /** Display name for a user id from the API (assignee, history author). */
  nameOf(id: string | null | undefined): string {
    if (!id) return '—';
    return DEMO_USERS.find((u) => u.id === id)?.name ?? 'Unknown user';
  }
}
