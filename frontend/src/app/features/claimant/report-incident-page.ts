import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router, RouterLink } from '@angular/router';

import type { ClaimType } from '../../core/api/api-types';
import { injectSubmitClaimMutation } from '../../core/api/claims.queries';
import { problemMessage } from '../../core/api/problem';
import { CurrentUser } from '../../core/session/current-user';
import { CLAIM_TYPES, CLAIM_TYPE_LABELS } from '../../shared/claims/claim-labels';

const MAX_TEXT = 2000;

/** UC-01: report an incident. */
@Component({
  selector: 'app-report-incident-page',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
  ],
  template: `
    <h1 class="mt-0 mb-4 text-2xl font-normal">Report an incident</h1>

    <mat-card appearance="outlined" class="max-w-2xl">
      <mat-card-content>
        <form [formGroup]="form" (ngSubmit)="submit()" class="flex flex-col gap-2 pt-2">
          <mat-form-field>
            <mat-label>Type of claim</mat-label>
            <mat-select formControlName="type">
              @for (type of types; track type) {
                <mat-option [value]="type">{{ typeLabels[type] }}</mat-option>
              }
            </mat-select>
            <mat-error>Choose the type of claim.</mat-error>
          </mat-form-field>

          <mat-form-field>
            <mat-label>Date of the incident</mat-label>
            <input matInput type="date" formControlName="incidentDate" />
            <mat-error>Enter the date of the incident.</mat-error>
          </mat-form-field>

          <mat-form-field>
            <mat-label>What happened?</mat-label>
            <textarea
              matInput
              formControlName="description"
              rows="5"
              [maxlength]="maxText"
            ></textarea>
            <mat-hint align="end"
              >{{ form.controls.description.value.length }} / {{ maxText }}</mat-hint
            >
            <mat-error>Describe what happened.</mat-error>
          </mat-form-field>

          <mat-form-field>
            <mat-label>Amount claimed</mat-label>
            <input matInput type="number" min="0.01" step="0.01" formControlName="claimedAmount" />
            <mat-error>Enter an amount greater than 0.</mat-error>
          </mat-form-field>

          @if (submitError(); as error) {
            <p class="m-0 rounded-xl bg-error-container p-4 text-on-error-container" role="alert">
              {{ error }}
            </p>
          }

          <div class="flex justify-end gap-2">
            <a mat-button routerLink="/my-claims">Cancel</a>
            <button mat-flat-button type="submit" [disabled]="submitClaim.isPending()">
              Submit claim
            </button>
          </div>
        </form>
      </mat-card-content>
    </mat-card>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ReportIncidentPage {
  private readonly currentUser = inject(CurrentUser);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly types = CLAIM_TYPES;
  protected readonly typeLabels = CLAIM_TYPE_LABELS;
  protected readonly maxText = MAX_TEXT;

  private readonly fb = inject(NonNullableFormBuilder);
  protected readonly form = this.fb.group({
    type: this.fb.control<ClaimType | null>(null, Validators.required),
    incidentDate: ['', Validators.required],
    description: ['', [Validators.required, Validators.maxLength(MAX_TEXT)]],
    claimedAmount: this.fb.control<number | null>(null, [
      Validators.required,
      Validators.min(0.01),
    ]),
  });

  protected readonly submitClaim = injectSubmitClaimMutation();
  protected readonly submitError = computed(() =>
    this.submitClaim.isError() ? problemMessage(this.submitClaim.error()) : null,
  );

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const { type, incidentDate, description, claimedAmount } = this.form.getRawValue();
    if (type === null || claimedAmount === null) return;

    this.submitClaim.mutate(
      { claimantId: this.currentUser.id(), type, incidentDate, description, claimedAmount },
      {
        onSuccess: (claim) => {
          this.snackBar.open(`Claim ${claim.claimNumber} submitted`, 'OK', { duration: 5000 });
          void this.router.navigate(['/my-claims', claim.id]);
        },
      },
    );
  }
}
