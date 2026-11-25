import { DatePipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { MatTableModule } from '@angular/material/table';

import { injectExposureQuery } from '../../core/api/exposure.queries';
import { injectWorkloadQuery } from '../../core/api/workload.queries';
import { CurrentUser } from '../../core/session/current-user';
import { CLAIM_STATUS_LABELS, CLAIM_TYPE_LABELS } from '../../shared/claims/claim-labels';
import { formatAge } from '../../shared/format-age';
import { QueryState } from '../../shared/query-state';
import { BreakdownTable, type BreakdownRow } from './breakdown-table';

/**
 * R8 and D7 (RFC-0003): claimed exposure over the whole book, then the team's open work and recent decisions.
 * Tables and numbers only; both views refresh by polling.
 */
@Component({
  selector: 'app-team-page',
  imports: [MatTableModule, DatePipe, DecimalPipe, BreakdownTable, QueryState],
  template: `
    <h1 class="mt-0 mb-6 text-2xl font-normal">Team</h1>

    <section aria-labelledby="exposure-heading" class="mb-10">
      <h2 id="exposure-heading" class="mt-0 mb-1 text-xl font-normal">Claimed exposure</h2>
      <p class="mt-0 mb-4 max-w-3xl text-on-surface-variant">
        The claimed amount on every claim not yet decided. It is an upper bound on what will be
        paid, not a reserve: claims get rejected or settled for less. One currency is assumed.
      </p>

      <app-query-state
        [pending]="exposure.isPending()"
        [error]="exposure.error()"
        what="the claimed exposure"
      />

      @if (exposure.data(); as e) {
        <dl class="m-0 mb-4 grid grid-cols-1 gap-4 sm:grid-cols-3">
          <div class="rounded-xl bg-primary-container p-4 text-on-primary-container">
            <dt class="text-sm">Claimed exposure</dt>
            <dd class="m-0 text-3xl">{{ e.claimedExposure | number: '1.2-2' }}</dd>
          </div>
          <div class="rounded-xl bg-surface-container p-4">
            <dt class="text-sm text-on-surface-variant">Open claims</dt>
            <dd class="m-0 text-3xl">{{ e.openClaims }}</dd>
          </div>
          <div class="rounded-xl bg-surface-container p-4">
            <dt class="text-sm text-on-surface-variant">Not yet picked up</dt>
            <dd class="m-0 text-3xl">{{ notPickedUp() | number: '1.2-2' }}</dd>
          </div>
        </dl>
        <div class="grid grid-cols-1 gap-4 lg:grid-cols-3">
          <app-breakdown-table title="By status" labelHeader="Status" [rows]="byStatus()" />
          <app-breakdown-table title="By type" labelHeader="Type" [rows]="byType()" />
          <app-breakdown-table title="By age since submission" labelHeader="Age" [rows]="byAge()" />
        </div>
        <p class="mt-2 mb-0 text-sm text-on-surface-variant">As of {{ e.asOf | date: 'medium' }}</p>
      }
    </section>

    <section aria-labelledby="workload-heading">
      <h2 id="workload-heading" class="mt-0 mb-1 text-xl font-normal">Team workload</h2>

      <app-query-state
        [pending]="workload.isPending()"
        [error]="workload.error()"
        what="the team workload"
      />

      @if (workload.data(); as w) {
        <p class="mt-0 mb-4 text-on-surface-variant">
          Open claims per officer, and their decisions in the last {{ w.periodDays }} days.
        </p>

        <div class="mb-4 rounded-xl bg-surface-container p-4" role="group" aria-label="Unassigned">
          <p class="m-0">
            <span class="text-2xl">{{ w.unassigned.count }}</span>
            unassigned
            @if (w.unassigned.oldestSubmittedAt) {
              <span class="text-on-surface-variant"
                >· oldest waiting {{ age(w.unassigned.oldestSubmittedAt) }}</span
              >
            }
          </p>
        </div>

        <div class="overflow-x-auto">
          <table mat-table [dataSource]="w.officers" aria-label="Team workload" class="w-full">
            <ng-container matColumnDef="officer">
              <th mat-header-cell *matHeaderCellDef scope="col">Officer</th>
              <td mat-cell *matCellDef="let o">
                {{ users.nameOf(o.officerId) }}
                @if (o.officerId === users.id()) {
                  <span class="text-on-surface-variant">(you)</span>
                }
              </td>
            </ng-container>
            <ng-container matColumnDef="underReview">
              <th mat-header-cell *matHeaderCellDef scope="col" class="text-right">Under review</th>
              <td mat-cell *matCellDef="let o" class="text-right">{{ o.underReview }}</td>
            </ng-container>
            <ng-container matColumnDef="infoRequested">
              <th mat-header-cell *matHeaderCellDef scope="col" class="text-right">
                Waiting on claimant
              </th>
              <td mat-cell *matCellDef="let o" class="text-right">{{ o.infoRequested }}</td>
            </ng-container>
            <ng-container matColumnDef="oldest">
              <th mat-header-cell *matHeaderCellDef scope="col">Oldest in status</th>
              <td mat-cell *matCellDef="let o">
                {{ o.oldestOpenStatusChangedAt ? age(o.oldestOpenStatusChangedAt) : '—' }}
              </td>
            </ng-container>
            <ng-container matColumnDef="decided">
              <th mat-header-cell *matHeaderCellDef scope="col" class="text-right">Decided</th>
              <td mat-cell *matCellDef="let o" class="text-right">{{ o.decidedInPeriod }}</td>
            </ng-container>
            <ng-container matColumnDef="settled">
              <th mat-header-cell *matHeaderCellDef scope="col" class="text-right">Settled</th>
              <td mat-cell *matCellDef="let o" class="text-right">{{ o.settledInPeriod }}</td>
            </ng-container>
            <ng-container matColumnDef="rejected">
              <th mat-header-cell *matHeaderCellDef scope="col" class="text-right">Rejected</th>
              <td mat-cell *matCellDef="let o" class="text-right">{{ o.rejectedInPeriod }}</td>
            </ng-container>
            <ng-container matColumnDef="averageDays">
              <th mat-header-cell *matHeaderCellDef scope="col" class="text-right">
                Avg days to decision
              </th>
              <td mat-cell *matCellDef="let o" class="text-right">
                {{
                  o.averageDaysToDecision === null
                    ? '—'
                    : (o.averageDaysToDecision | number: '1.1-1')
                }}
              </td>
            </ng-container>

            <tr mat-header-row *matHeaderRowDef="workloadColumns"></tr>
            <tr mat-row *matRowDef="let row; columns: workloadColumns"></tr>
          </table>
        </div>
      }
    </section>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TeamPage {
  protected readonly users = inject(CurrentUser);

  protected readonly exposure = injectExposureQuery(() => this.users.id());
  protected readonly workload = injectWorkloadQuery(() => this.users.id());

  protected readonly workloadColumns = [
    'officer',
    'underReview',
    'infoRequested',
    'oldest',
    'decided',
    'settled',
    'rejected',
    'averageDays',
  ];

  protected readonly notPickedUp = computed(
    () => this.exposure.data()?.byStatus.find((s) => s.status === 'Submitted')?.claimedAmount ?? 0,
  );

  protected readonly byStatus = computed<BreakdownRow[]>(() =>
    (this.exposure.data()?.byStatus ?? []).map((s) => ({
      label: CLAIM_STATUS_LABELS[s.status],
      count: s.count,
      amount: s.claimedAmount,
    })),
  );

  protected readonly byType = computed<BreakdownRow[]>(() =>
    (this.exposure.data()?.byType ?? []).map((t) => ({
      label: CLAIM_TYPE_LABELS[t.type],
      count: t.count,
      amount: t.claimedAmount,
    })),
  );

  protected readonly byAge = computed<BreakdownRow[]>(() =>
    (this.exposure.data()?.byAge ?? []).map((a) => ({
      label: a.bucket,
      count: a.count,
      amount: a.claimedAmount,
    })),
  );

  /** Ages are measured at the workload's `asOf`, so they match the figures beside them. */
  protected age(since: string): string {
    const asOf = this.workload.data()?.asOf;
    return formatAge(since, asOf ? Date.parse(asOf) : Date.now());
  }
}
