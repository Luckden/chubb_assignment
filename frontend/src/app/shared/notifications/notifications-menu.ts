import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { MatBadgeModule } from '@angular/material/badge';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { RouterLink } from '@angular/router';

import type { Notification } from '../../core/api/api-types';
import {
  injectMarkNotificationReadMutation,
  injectNotificationsQuery,
} from '../../core/api/notifications.queries';
import { CurrentUser } from '../../core/session/current-user';

/** Toolbar bell for claimants: unread count and the latest notifications (R3, R4). Opening one marks it read. */
@Component({
  selector: 'app-notifications-menu',
  imports: [MatBadgeModule, MatButtonModule, MatIconModule, MatMenuModule, RouterLink],
  template: `
    <button
      mat-icon-button
      [matMenuTriggerFor]="menu"
      [matBadge]="unreadCount()"
      [matBadgeHidden]="unreadCount() === 0"
      matBadgeColor="warn"
      matBadgeSize="small"
      [attr.aria-label]="'Notifications, ' + unreadCount() + ' unread'"
    >
      <mat-icon aria-hidden="true">notifications</mat-icon>
    </button>
    <mat-menu #menu="matMenu">
      @for (n of notifications(); track n.id) {
        <a mat-menu-item [routerLink]="['/my-claims', n.claimId]" (click)="open(n)">
          <span [class.font-semibold]="!n.readAt">{{ n.message }}</span>
        </a>
      } @empty {
        <p class="m-0 px-4 py-2 text-on-surface-variant">No notifications</p>
      }
    </mat-menu>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NotificationsMenu {
  private readonly currentUser = inject(CurrentUser);
  private readonly query = injectNotificationsQuery(() => this.currentUser.id());
  private readonly markRead = injectMarkNotificationReadMutation();

  protected readonly notifications = computed(() => this.query.data() ?? []);
  protected readonly unreadCount = computed(
    () => this.notifications().filter((n) => !n.readAt).length,
  );

  protected open(notification: Notification): void {
    if (!notification.readAt) {
      this.markRead.mutate({ id: notification.id, userId: this.currentUser.id() });
    }
  }
}
