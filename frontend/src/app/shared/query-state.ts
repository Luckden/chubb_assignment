import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { MatProgressBarModule } from '@angular/material/progress-bar';

import { problemMessage } from '../core/api/problem';

/** Loading bar or error banner for a query. Render the data yourself once it's there. */
@Component({
  selector: 'app-query-state',
  imports: [MatProgressBarModule],
  template: `
    @if (pending()) {
      <mat-progress-bar mode="indeterminate" [attr.aria-label]="'Loading ' + what()" />
    } @else if (error()) {
      <p class="m-0 rounded-xl bg-error-container p-4 text-on-error-container" role="alert">
        Couldn't load {{ what() }}. {{ message(error()) }}
      </p>
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class QueryState {
  readonly pending = input.required<boolean>();
  readonly error = input<unknown>(null);
  /** e.g. "your claims". */
  readonly what = input.required<string>();

  protected readonly message = problemMessage;
}
