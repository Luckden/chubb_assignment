import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatRadioModule } from '@angular/material/radio';
import { MatSnackBar } from '@angular/material/snack-bar';
import { RouterLink } from '@angular/router';

import type { Claim, DecisionOutcome } from '../../core/api/api-types';
import {
  injectClaimQuery,
  injectPickUpClaimMutation,
  injectRejectClaimMutation,
  injectRequestInformationMutation,
  injectSettleClaimMutation,
} from '../../core/api/claims.queries';
import { problemMessage } from '../../core/api/problem';
import { CurrentUser } from '../../core/session/current-user';
import { ClaimActivity } from '../../shared/claims/claim-activity';
import { ClaimFacts } from '../../shared/claims/claim-facts';
import { QueryState } from '../../shared/query-state';

const MAX_TEXT = 2000;

/** UC-05 pick up, UC-06 review and ask for information, UC-07 settle or reject. */
@Component({
  selector: 'app-staff-claim-page',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatRadioModule,
    ClaimFacts,
    ClaimActivity,
    QueryState,
  ],
  template: `
    <a mat-button routerLink="/work" class="mb-2">
      <mat-icon aria-hidden="true">arrow_back</mat-icon>
      Work queue
    </a>

    <app-query-state [pending]="claim.isPending()" [error]="claim.error()" what="this claim" />

    @if (claim.data(); as c) {
      <div class="grid grid-cols-1 gap-4 lg:grid-cols-3">
        <div class="flex flex-col gap-4 lg:col-span-2">
          <app-claim-facts [claim]="c" [showPeople]="true" />
          <app-claim-activity [claim]="c" />
        </div>

        <aside class="flex flex-col gap-4" aria-label="Actions">
          @if (actionError(); as error) {
            <p class="m-0 rounded-xl bg-error-container p-4 text-on-error-container" role="alert">
              {{ error }}
            </p>
          }

          @switch (stage()) {
            @case ('incoming') {
              <mat-card appearance="outlined">
                <mat-card-header>
                  <mat-card-title><h2 class="m-0 text-lg font-medium">Incoming</h2></mat-card-title>
                </mat-card-header>
                <mat-card-content>
                  <p>Nobody is handling this claim yet.</p>
                </mat-card-content>
                <mat-card-actions align="end">
                  <button mat-flat-button (click)="pickUp(c)" [disabled]="pickUpClaim.isPending()">
                    Pick up
                  </button>
                </mat-card-actions>
              </mat-card>
            }

            @case ('reviewing') {
              <mat-card appearance="outlined">
                <mat-card-header>
                  <mat-card-title>
                    <h2 class="m-0 text-lg font-medium">Ask the claimant</h2>
                  </mat-card-title>
                </mat-card-header>
                <mat-card-content>
                  <form
                    [formGroup]="questionForm"
                    (ngSubmit)="askQuestion(c)"
                    class="flex flex-col gap-2 pt-2"
                  >
                    <mat-form-field>
                      <mat-label>Question</mat-label>
                      <textarea
                        matInput
                        formControlName="question"
                        rows="3"
                        [maxlength]="maxText"
                      ></textarea>
                      <mat-error>Enter a question.</mat-error>
                    </mat-form-field>
                    <div class="flex justify-end">
                      <button
                        mat-stroked-button
                        type="submit"
                        [disabled]="requestInformation.isPending()"
                      >
                        Send question
                      </button>
                    </div>
                  </form>
                </mat-card-content>
              </mat-card>

              <mat-card appearance="outlined">
                <mat-card-header>
                  <mat-card-title><h2 class="m-0 text-lg font-medium">Decision</h2></mat-card-title>
                </mat-card-header>
                <mat-card-content>
                  <form
                    [formGroup]="decisionForm"
                    (ngSubmit)="decide(c)"
                    class="flex flex-col gap-2 pt-2"
                  >
                    <mat-radio-group
                      formControlName="outcome"
                      aria-label="Outcome"
                      class="flex gap-4"
                    >
                      <mat-radio-button value="Settled">Settle</mat-radio-button>
                      <mat-radio-button value="Rejected">Reject</mat-radio-button>
                    </mat-radio-group>
                    <mat-form-field>
                      <mat-label>{{ reasonLabel() }}</mat-label>
                      <textarea
                        matInput
                        formControlName="reason"
                        rows="3"
                        [maxlength]="maxText"
                      ></textarea>
                      <mat-hint>Shown to the claimant.</mat-hint>
                      <mat-error>A rejection needs a reason.</mat-error>
                    </mat-form-field>
                    <div class="flex justify-end">
                      <button mat-flat-button type="submit" [disabled]="deciding()">
                        {{ outcome() === 'Rejected' ? 'Reject claim' : 'Settle claim' }}
                      </button>
                    </div>
                  </form>
                </mat-card-content>
              </mat-card>
            }

            @case ('waiting') {
              <p class="m-0 rounded-xl bg-surface-container p-4">
                Waiting for the claimant to answer your question.
              </p>
            }

            @case ('someone-else') {
              <p class="m-0 rounded-xl bg-surface-container p-4">
                {{ currentUser.nameOf(c.assigneeId) }} is handling this claim.
              </p>
            }

            @case ('decided') {
              <p class="m-0 rounded-xl bg-surface-container p-4">This claim is closed.</p>
            }
          }
        </aside>
      </div>
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StaffClaimPage {
  /** Route parameter. */
  readonly id = input.required<string>();

  protected readonly currentUser = inject(CurrentUser);
  private readonly snackBar = inject(MatSnackBar);
  private readonly fb = inject(NonNullableFormBuilder);
  protected readonly maxText = MAX_TEXT;

  protected readonly claim = injectClaimQuery(() => this.id());
  protected readonly pickUpClaim = injectPickUpClaimMutation();
  protected readonly requestInformation = injectRequestInformationMutation();
  protected readonly settleClaim = injectSettleClaimMutation();
  protected readonly rejectClaim = injectRejectClaimMutation();

  /** Which actions the current officer has on this claim (see docs/design/04-claim-lifecycle.md). */
  protected readonly stage = computed(() => {
    const claim = this.claim.data();
    if (!claim) return null;
    if (claim.status === 'Settled' || claim.status === 'Rejected') return 'decided';
    if (claim.status === 'Submitted') return 'incoming';
    if (claim.assigneeId !== this.currentUser.id()) return 'someone-else';
    return claim.status === 'InfoRequested' ? 'waiting' : 'reviewing';
  });

  protected readonly questionForm = this.fb.group({
    question: ['', [Validators.required, Validators.maxLength(MAX_TEXT)]],
  });

  protected readonly decisionForm = this.fb.group({
    outcome: this.fb.control<DecisionOutcome>('Settled'),
    reason: ['', Validators.maxLength(MAX_TEXT)],
  });

  protected readonly outcome = toSignal(this.decisionForm.controls.outcome.valueChanges, {
    initialValue: this.decisionForm.controls.outcome.value,
  });

  protected readonly reasonLabel = computed(() =>
    this.outcome() === 'Rejected' ? 'Reason (required)' : 'Reason (optional)',
  );

  protected readonly deciding = computed(
    () => this.settleClaim.isPending() || this.rejectClaim.isPending(),
  );

  protected readonly actionError = computed(() => {
    const error = [
      this.pickUpClaim.error(),
      this.requestInformation.error(),
      this.settleClaim.error(),
      this.rejectClaim.error(),
    ].find((e) => e !== null);
    return error ? problemMessage(error) : null;
  });

  protected pickUp(claim: Claim): void {
    this.pickUpClaim.mutate(
      { id: claim.id, body: { officerId: this.currentUser.id() } },
      { onSuccess: (c) => this.notify(`You picked up ${c.claimNumber}`) },
    );
  }

  protected askQuestion(claim: Claim): void {
    if (this.questionForm.invalid) {
      this.questionForm.markAllAsTouched();
      return;
    }
    this.requestInformation.mutate(
      {
        id: claim.id,
        body: { officerId: this.currentUser.id(), ...this.questionForm.getRawValue() },
      },
      {
        onSuccess: () => {
          this.questionForm.reset();
          this.notify('Question sent to the claimant');
        },
      },
    );
  }

  protected decide(claim: Claim): void {
    const { outcome, reason } = this.decisionForm.getRawValue();
    const officerId = this.currentUser.id();
    const trimmed = reason.trim();

    if (outcome === 'Rejected') {
      if (!trimmed) {
        this.decisionForm.controls.reason.setErrors({ required: true });
        this.decisionForm.controls.reason.markAsTouched();
        return;
      }
      this.rejectClaim.mutate(
        { id: claim.id, body: { officerId, reason: trimmed } },
        { onSuccess: (c) => this.notify(`${c.claimNumber} rejected`) },
      );
    } else {
      this.settleClaim.mutate(
        { id: claim.id, body: { officerId, reason: trimmed || null } },
        { onSuccess: (c) => this.notify(`${c.claimNumber} settled`) },
      );
    }
  }

  private notify(message: string): void {
    this.snackBar.open(message, 'OK', { duration: 5000 });
  }
}
