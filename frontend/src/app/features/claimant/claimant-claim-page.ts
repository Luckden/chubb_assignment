import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { RouterLink } from '@angular/router';

import { injectClaimQuery, injectProvideInformationMutation } from '../../core/api/claims.queries';
import { problemMessage } from '../../core/api/problem';
import { CurrentUser } from '../../core/session/current-user';
import { ClaimActivity } from '../../shared/claims/claim-activity';
import { ClaimFacts } from '../../shared/claims/claim-facts';
import { QueryState } from '../../shared/query-state';

/** UC-02 track, UC-03 answer a question, UC-04 see the decision. */
@Component({
  selector: 'app-claimant-claim-page',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatButtonModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    ClaimFacts,
    ClaimActivity,
    QueryState,
  ],
  template: `
    <a mat-button routerLink="/my-claims" class="mb-2">
      <mat-icon aria-hidden="true">arrow_back</mat-icon>
      My claims
    </a>

    <app-query-state [pending]="claim.isPending()" [error]="claim.error()" what="this claim" />

    @if (claim.data(); as c) {
      @if (c.claimantId !== currentUser.id()) {
        <p class="rounded-xl bg-surface-container p-6" role="alert">
          This claim belongs to another claimant.
        </p>
      } @else {
        <div class="flex flex-col gap-4">
          <app-claim-facts [claim]="c" />

          @if (openRequest(); as request) {
            <section
              class="rounded-xl bg-tertiary-container p-4 text-on-tertiary-container"
              aria-labelledby="question-heading"
            >
              <h2 id="question-heading" class="m-0 text-lg font-medium">
                We need more information
              </h2>
              <p class="whitespace-pre-line">{{ request.question }}</p>
              <form [formGroup]="answerForm" (ngSubmit)="answer(c.id)" class="flex flex-col gap-2">
                <mat-form-field>
                  <mat-label>Your answer</mat-label>
                  <textarea
                    matInput
                    formControlName="response"
                    rows="4"
                    maxlength="2000"
                  ></textarea>
                  <mat-error>Enter your answer.</mat-error>
                </mat-form-field>
                @if (answerError(); as error) {
                  <p
                    class="m-0 rounded-xl bg-error-container p-4 text-on-error-container"
                    role="alert"
                  >
                    {{ error }}
                  </p>
                }
                <div class="flex justify-end">
                  <button mat-flat-button type="submit" [disabled]="provideInformation.isPending()">
                    Send answer
                  </button>
                </div>
              </form>
            </section>
          }

          <app-claim-activity [claim]="c" />
        </div>
      }
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ClaimantClaimPage {
  /** Route parameter. */
  readonly id = input.required<string>();

  protected readonly currentUser = inject(CurrentUser);
  protected readonly claim = injectClaimQuery(() => this.id());
  protected readonly provideInformation = injectProvideInformationMutation();

  protected readonly openRequest = computed(() => {
    const claim = this.claim.data();
    if (claim?.status !== 'InfoRequested') return null;
    return claim.informationRequests.find((r) => r.response === null) ?? null;
  });

  protected readonly answerForm = inject(NonNullableFormBuilder).group({
    response: ['', [Validators.required, Validators.maxLength(2000)]],
  });

  protected readonly answerError = computed(() =>
    this.provideInformation.isError() ? problemMessage(this.provideInformation.error()) : null,
  );

  protected answer(id: string): void {
    if (this.answerForm.invalid) {
      this.answerForm.markAllAsTouched();
      return;
    }
    this.provideInformation.mutate(
      { id, body: { claimantId: this.currentUser.id(), ...this.answerForm.getRawValue() } },
      { onSuccess: () => this.answerForm.reset() },
    );
  }
}
