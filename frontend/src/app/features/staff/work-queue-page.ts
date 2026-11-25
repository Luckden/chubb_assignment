import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatTabsModule } from '@angular/material/tabs';

import { injectClaimListQuery } from '../../core/api/claims.queries';
import { CurrentUser } from '../../core/session/current-user';
import { ClaimTable } from '../../shared/claims/claim-table';
import { QueryState } from '../../shared/query-state';

/** UC-05: incoming claims to pick up, and the officer's own claims. */
@Component({
  selector: 'app-work-queue-page',
  imports: [MatTabsModule, ClaimTable, QueryState],
  template: `
    <h1 class="mt-0 mb-4 text-2xl font-normal">Work queue</h1>

    <mat-tab-group mat-stretch-tabs="false" animationDuration="0ms">
      <mat-tab [label]="'Incoming (' + (incoming.data()?.length ?? '…') + ')'">
        <div class="pt-4">
          <app-query-state
            [pending]="incoming.isPending()"
            [error]="incoming.error()"
            what="incoming claims"
          />
          @if (incoming.data(); as data) {
            @if (data.length === 0) {
              <p class="rounded-xl bg-surface-container p-6 text-on-surface-variant">
                No incoming claims. Everything has been picked up.
              </p>
            } @else {
              <app-claim-table
                [claims]="data"
                basePath="/work"
                label="Incoming claims"
                [showClaimant]="true"
              />
            }
          }
        </div>
      </mat-tab>

      <mat-tab [label]="'Assigned to me (' + (mine.data()?.length ?? '…') + ')'">
        <div class="pt-4">
          <app-query-state [pending]="mine.isPending()" [error]="mine.error()" what="your claims" />
          @if (mine.data(); as data) {
            @if (data.length === 0) {
              <p class="rounded-xl bg-surface-container p-6 text-on-surface-variant">
                No claims assigned to you. Pick one up from Incoming.
              </p>
            } @else {
              <app-claim-table
                [claims]="data"
                basePath="/work"
                label="Claims assigned to me"
                [showClaimant]="true"
              />
            }
          }
        </div>
      </mat-tab>
    </mat-tab-group>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class WorkQueuePage {
  private readonly currentUser = inject(CurrentUser);

  protected readonly incoming = injectClaimListQuery(() => ({ unassigned: true }));
  protected readonly mine = injectClaimListQuery(() => ({ assigneeId: this.currentUser.id() }));
}
