import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

import type { ClaimStatus } from '../../core/api/api-types';
import { CLAIM_STATUS_LABELS } from './claim-labels';

const STATUS_CLASSES: Record<ClaimStatus, string> = {
  Submitted: 'bg-surface-container-high text-on-surface-variant',
  UnderReview: 'bg-secondary-container text-on-secondary-container',
  InfoRequested: 'bg-tertiary-container text-on-tertiary-container',
  Settled: 'bg-primary-container text-on-primary-container',
  Rejected: 'bg-error-container text-on-error-container',
};

@Component({
  selector: 'app-claim-status-chip',
  template: `<span
    class="inline-block rounded-full px-3 py-1 text-sm whitespace-nowrap"
    [class]="classes()"
    >{{ label() }}</span
  >`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ClaimStatusChip {
  readonly status = input.required<ClaimStatus>();

  protected readonly label = computed(() => CLAIM_STATUS_LABELS[this.status()]);
  protected readonly classes = computed(() => STATUS_CLASSES[this.status()]);
}
