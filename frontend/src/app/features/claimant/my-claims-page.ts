import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { RouterLink } from '@angular/router';

import { injectClaimListQuery } from '../../core/api/claims.queries';
import { CurrentUser } from '../../core/session/current-user';
import { ClaimTable } from '../../shared/claims/claim-table';
import { QueryState } from '../../shared/query-state';

/** UC-02: the claimant's own claims. */
@Component({
  selector: 'app-my-claims-page',
  imports: [MatButtonModule, MatIconModule, RouterLink, ClaimTable, QueryState],
  template: `
    <div class="mb-4 flex flex-wrap items-center gap-4">
      <h1 class="m-0 flex-1 text-2xl font-normal">My claims</h1>
      <a mat-flat-button routerLink="/my-claims/new">
        <mat-icon aria-hidden="true">add</mat-icon>
        Report an incident
      </a>
    </div>

    <app-query-state [pending]="claims.isPending()" [error]="claims.error()" what="your claims" />

    @if (claims.data(); as data) {
      @if (data.length === 0) {
        <p class="rounded-xl bg-surface-container p-6 text-on-surface-variant">
          You have no claims yet. Report an incident to start one.
        </p>
      } @else {
        <app-claim-table [claims]="data" basePath="/my-claims" label="My claims" />
      }
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MyClaimsPage {
  private readonly currentUser = inject(CurrentUser);

  protected readonly claims = injectClaimListQuery(() => ({ claimantId: this.currentUser.id() }));
}
