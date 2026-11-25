import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatToolbarModule } from '@angular/material/toolbar';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

import { injectSystemInfoQuery } from './core/api/system.queries';
import { CurrentUser, DEMO_USERS, ROLE_HOME } from './core/session/current-user';
import { NotificationsMenu } from './shared/notifications/notifications-menu';

@Component({
  selector: 'app-root',
  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    MatToolbarModule,
    MatIconModule,
    MatButtonModule,
    MatFormFieldModule,
    MatSelectModule,
    NotificationsMenu,
  ],
  templateUrl: './app.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App {
  private readonly router = inject(Router);
  protected readonly currentUser = inject(CurrentUser);

  /** No authentication yet: pick which demo user the app acts as. */
  protected readonly userGroups = [
    {
      role: 'Claimant',
      label: 'Claimants',
      users: DEMO_USERS.filter((u) => u.role === 'Claimant'),
    },
    {
      role: 'Officer',
      label: 'Claims staff',
      users: DEMO_USERS.filter((u) => u.role === 'Officer'),
    },
  ];

  /** Backend connectivity shown in the shell. */
  protected readonly systemInfo = injectSystemInfoQuery();

  protected readonly apiStatus = computed(() => {
    if (this.systemInfo.isPending()) return 'checking…';
    if (this.systemInfo.isError()) return 'unreachable';
    return `connected (${this.systemInfo.data()?.environment})`;
  });

  protected readonly apiStatusClass = computed(() => {
    if (this.systemInfo.isPending()) return 'bg-surface-container-high text-on-surface-variant';
    return this.systemInfo.isError()
      ? 'bg-error-container text-on-error-container'
      : 'bg-primary-container text-on-primary-container';
  });

  protected actAs(id: string): void {
    this.currentUser.select(id);
    void this.router.navigateByUrl(ROLE_HOME[this.currentUser.user().role]);
  }
}
