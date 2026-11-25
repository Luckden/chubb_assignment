import { DatePipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';

import type { Claim } from '../../core/api/api-types';
import { CurrentUser } from '../../core/session/current-user';
import { CLAIM_TYPE_LABELS } from './claim-labels';
import { ClaimStatusChip } from './claim-status-chip';

/** Header card for a claim: what was reported, where it is, and the decision once there is one (R2, R4). */
@Component({
  selector: 'app-claim-facts',
  imports: [MatCardModule, MatIconModule, DatePipe, DecimalPipe, ClaimStatusChip],
  template: `
    @let c = claim();
    <mat-card appearance="outlined">
      <mat-card-header>
        <mat-card-title>
          <h2 class="m-0 text-xl font-medium">Claim {{ c.claimNumber }}</h2>
        </mat-card-title>
        <mat-card-subtitle>{{ typeLabels[c.type] }} claim</mat-card-subtitle>
        <div class="ml-auto"><app-claim-status-chip [status]="c.status" /></div>
      </mat-card-header>
      <mat-card-content>
        <dl class="mt-4 grid grid-cols-1 gap-x-6 gap-y-3 sm:grid-cols-2 lg:grid-cols-4">
          <div>
            <dt class="text-sm text-on-surface-variant">Incident date</dt>
            <dd class="m-0">{{ c.incidentDate | date: 'mediumDate' }}</dd>
          </div>
          <div>
            <dt class="text-sm text-on-surface-variant">Claimed amount</dt>
            <dd class="m-0">{{ c.claimedAmount | number: '1.2-2' }}</dd>
          </div>
          <div>
            <dt class="text-sm text-on-surface-variant">Submitted</dt>
            <dd class="m-0">{{ c.submittedAt | date: 'medium' }}</dd>
          </div>
          @if (showPeople()) {
            <div>
              <dt class="text-sm text-on-surface-variant">Claimant</dt>
              <dd class="m-0">{{ users.nameOf(c.claimantId) }}</dd>
            </div>
          }
          <div>
            <dt class="text-sm text-on-surface-variant">Handled by</dt>
            <dd class="m-0">
              {{ c.assigneeId ? users.nameOf(c.assigneeId) : 'Not yet assigned' }}
            </dd>
          </div>
          <div class="sm:col-span-2 lg:col-span-4">
            <dt class="text-sm text-on-surface-variant">What happened</dt>
            <dd class="m-0 whitespace-pre-line">{{ c.description }}</dd>
          </div>
        </dl>

        @if (c.decisionOutcome) {
          <section
            class="mt-4 flex gap-3 rounded-xl p-4"
            [class]="
              c.decisionOutcome === 'Settled'
                ? 'bg-primary-container text-on-primary-container'
                : 'bg-error-container text-on-error-container'
            "
            aria-labelledby="decision-heading"
          >
            <mat-icon aria-hidden="true">{{
              c.decisionOutcome === 'Settled' ? 'check_circle' : 'cancel'
            }}</mat-icon>
            <div>
              <h3 id="decision-heading" class="m-0 text-base font-medium">
                Decision: {{ c.decisionOutcome === 'Settled' ? 'settled' : 'rejected' }}
              </h3>
              <p class="m-0 text-sm">{{ c.decidedAt | date: 'medium' }}</p>
              @if (c.decisionReason) {
                <p class="mt-2 mb-0 whitespace-pre-line">{{ c.decisionReason }}</p>
              }
            </div>
          </section>
        }
      </mat-card-content>
    </mat-card>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ClaimFacts {
  readonly claim = input.required<Claim>();
  /** Staff see who the claimant is; claimants don't need their own name. */
  readonly showPeople = input(false);

  protected readonly users = inject(CurrentUser);
  protected readonly typeLabels = CLAIM_TYPE_LABELS;
}
