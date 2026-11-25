import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatTabsModule } from '@angular/material/tabs';

import type { ClaimListFilter } from '../../core/api/api-types';
import { injectClaimListQuery } from '../../core/api/claims.queries';
import { CurrentUser } from '../../core/session/current-user';
import { ClaimTable } from '../../shared/claims/claim-table';
import { QueryState } from '../../shared/query-state';

/**
 * UC-05 and D6 (RFC-0003): what needs the officer, what waits on the claimant, and what nobody has picked up.
 * Every tab lists the longest in its current status first.
 */
@Component({
  selector: 'app-work-queue-page',
  imports: [MatTabsModule, ClaimTable, QueryState],
  template: `
    <h1 class="mt-0 mb-4 text-2xl font-normal">Work queue</h1>

    <mat-tab-group mat-stretch-tabs="false" animationDuration="0ms">
      @for (tab of tabs; track tab.label) {
        <mat-tab [label]="tab.label + ' (' + (tab.query.data()?.length ?? '…') + ')'">
          <div class="pt-4">
            <app-query-state
              [pending]="tab.query.isPending()"
              [error]="tab.query.error()"
              [what]="tab.what"
            />
            @if (tab.query.data(); as data) {
              @if (data.length === 0) {
                <p class="rounded-xl bg-surface-container p-6 text-on-surface-variant">
                  {{ tab.empty }}
                </p>
              } @else {
                <app-claim-table
                  [claims]="data"
                  basePath="/work"
                  [label]="tab.label"
                  [showClaimant]="true"
                  [showTimeInStatus]="true"
                  [asOf]="tab.query.dataUpdatedAt()"
                />
              }
            }
          </div>
        </mat-tab>
      }
    </mat-tab-group>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class WorkQueuePage {
  private readonly currentUser = inject(CurrentUser);

  private readonly mine = (status: ClaimListFilter['status']): ClaimListFilter => ({
    assigneeId: this.currentUser.id(),
    status,
    sort: 'StatusChangedAt',
  });

  protected readonly tabs = [
    {
      label: 'Needs me',
      what: 'claims that need you',
      empty: 'Nothing needs you right now.',
      query: injectClaimListQuery(() => this.mine('UnderReview')),
    },
    {
      label: 'Waiting on claimant',
      what: 'claims waiting on the claimant',
      empty: 'No claims are waiting on a claimant.',
      query: injectClaimListQuery(() => this.mine('InfoRequested')),
    },
    {
      label: 'Unassigned',
      what: 'unassigned claims',
      empty: 'No unassigned claims. Everything has been picked up.',
      query: injectClaimListQuery(() => ({ unassigned: true, sort: 'StatusChangedAt' })),
    },
  ];
}
