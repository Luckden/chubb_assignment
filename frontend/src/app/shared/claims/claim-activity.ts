import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { MatCardModule } from '@angular/material/card';

import type { Claim } from '../../core/api/api-types';
import { CurrentUser } from '../../core/session/current-user';
import { CLAIM_STATUS_LABELS } from './claim-labels';

/** Questions asked and answered, then the status history, newest first. */
@Component({
  selector: 'app-claim-activity',
  imports: [MatCardModule, DatePipe],
  template: `
    @if (claim().informationRequests.length > 0) {
      <mat-card appearance="outlined">
        <mat-card-header>
          <mat-card-title><h2 class="m-0 text-lg font-medium">Questions</h2></mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <ul class="m-0 mt-2 flex list-none flex-col gap-4 p-0">
            @for (request of claim().informationRequests; track request.id) {
              <li class="rounded-xl bg-surface-container p-4">
                <p class="m-0 text-sm text-on-surface-variant">
                  {{ users.nameOf(request.requestedBy) }} asked ·
                  {{ request.requestedAt | date: 'medium' }}
                </p>
                <p class="mt-1 mb-0 whitespace-pre-line">{{ request.question }}</p>
                @if (request.response) {
                  <p class="mt-3 mb-0 text-sm text-on-surface-variant">
                    Answered · {{ request.respondedAt | date: 'medium' }}
                  </p>
                  <p class="mt-1 mb-0 whitespace-pre-line">{{ request.response }}</p>
                } @else {
                  <p class="mt-3 mb-0 text-sm text-on-surface-variant">Waiting for an answer</p>
                }
              </li>
            }
          </ul>
        </mat-card-content>
      </mat-card>
    }

    <mat-card appearance="outlined">
      <mat-card-header>
        <mat-card-title><h2 class="m-0 text-lg font-medium">History</h2></mat-card-title>
      </mat-card-header>
      <mat-card-content>
        <ol
          class="m-0 mt-2 flex list-none flex-col gap-3 border-l-2 border-outline-variant p-0 pl-4"
        >
          @for (change of history(); track change.changedAt) {
            <li>
              <p class="m-0 font-medium">{{ statusLabels[change.toStatus] }}</p>
              <p class="m-0 text-sm text-on-surface-variant">
                {{ users.nameOf(change.changedBy) }} · {{ change.changedAt | date: 'medium' }}
              </p>
              @if (change.note) {
                <p class="mt-1 mb-0 whitespace-pre-line">{{ change.note }}</p>
              }
            </li>
          }
        </ol>
      </mat-card-content>
    </mat-card>
  `,
  host: { class: 'flex flex-col gap-4' },
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ClaimActivity {
  readonly claim = input.required<Claim>();

  protected readonly users = inject(CurrentUser);
  protected readonly statusLabels = CLAIM_STATUS_LABELS;
  protected readonly history = computed(() =>
    [...this.claim().statusHistory].sort((a, b) => b.changedAt.localeCompare(a.changedAt)),
  );
}
