import { DatePipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { MatTableModule } from '@angular/material/table';
import { RouterLink } from '@angular/router';

import type { ClaimSummary, ClaimType } from '../../core/api/api-types';
import { CurrentUser } from '../../core/session/current-user';
import { formatAge } from '../format-age';
import { CLAIM_TYPE_LABELS } from './claim-labels';
import { ClaimStatusChip } from './claim-status-chip';

/**
 * A list of claims; the claim number links to `<basePath>/<id>`. With `showTimeInStatus`, an "In status for" column
 * shows how long each claim has been in its current status (RFC-0003), measured at `asOf`.
 */
@Component({
  selector: 'app-claim-table',
  imports: [MatTableModule, RouterLink, DatePipe, DecimalPipe, ClaimStatusChip],
  template: `
    <table mat-table [dataSource]="claims()" [attr.aria-label]="label()" class="w-full">
      <ng-container matColumnDef="claimNumber">
        <th mat-header-cell *matHeaderCellDef scope="col">Claim</th>
        <td mat-cell *matCellDef="let claim">
          <a class="text-primary" [routerLink]="[basePath(), claim.id]">{{ claim.claimNumber }}</a>
        </td>
      </ng-container>
      <ng-container matColumnDef="claimant">
        <th mat-header-cell *matHeaderCellDef scope="col">Claimant</th>
        <td mat-cell *matCellDef="let claim">{{ users.nameOf(claim.claimantId) }}</td>
      </ng-container>
      <ng-container matColumnDef="type">
        <th mat-header-cell *matHeaderCellDef scope="col">Type</th>
        <td mat-cell *matCellDef="let claim">{{ typeLabel(claim.type) }}</td>
      </ng-container>
      <ng-container matColumnDef="status">
        <th mat-header-cell *matHeaderCellDef scope="col">Status</th>
        <td mat-cell *matCellDef="let claim"><app-claim-status-chip [status]="claim.status" /></td>
      </ng-container>
      <ng-container matColumnDef="claimedAmount">
        <th mat-header-cell *matHeaderCellDef scope="col" class="text-right">Amount</th>
        <td mat-cell *matCellDef="let claim" class="text-right">
          {{ claim.claimedAmount | number: '1.2-2' }}
        </td>
      </ng-container>
      <ng-container matColumnDef="inStatusFor">
        <th mat-header-cell *matHeaderCellDef scope="col">In status for</th>
        <td mat-cell *matCellDef="let claim">{{ ages().get(claim.id) }}</td>
      </ng-container>
      <ng-container matColumnDef="submittedAt">
        <th mat-header-cell *matHeaderCellDef scope="col">Submitted</th>
        <td mat-cell *matCellDef="let claim">{{ claim.submittedAt | date: 'mediumDate' }}</td>
      </ng-container>

      <tr mat-header-row *matHeaderRowDef="columns()"></tr>
      <tr mat-row *matRowDef="let row; columns: columns()"></tr>
    </table>
  `,
  host: { class: 'block overflow-x-auto' },
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ClaimTable {
  readonly claims = input.required<ClaimSummary[]>();
  readonly basePath = input.required<string>();
  readonly label = input.required<string>();
  readonly showClaimant = input(false);
  readonly showTimeInStatus = input(false);
  /** When the data was fetched (query `dataUpdatedAt`), so ages move on with each poll. */
  readonly asOf = input(0);

  protected readonly users = inject(CurrentUser);
  protected readonly typeLabel = (type: ClaimType) => CLAIM_TYPE_LABELS[type];
  protected readonly columns = computed(() => [
    'claimNumber',
    ...(this.showClaimant() ? ['claimant'] : []),
    'type',
    'status',
    'claimedAmount',
    ...(this.showTimeInStatus() ? ['inStatusFor'] : []),
    'submittedAt',
  ]);
  protected readonly ages = computed(() => {
    const now = this.asOf() || Date.now();
    return new Map(this.claims().map((c) => [c.id, formatAge(c.statusChangedAt, now)]));
  });
}
