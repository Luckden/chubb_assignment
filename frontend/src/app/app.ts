import { ChangeDetectionStrategy, Component, computed } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatToolbarModule } from '@angular/material/toolbar';
import { RouterOutlet } from '@angular/router';

import { injectSystemInfoQuery } from './core/api/system.queries';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, MatToolbarModule, MatIconModule],
  templateUrl: './app.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App {
  /** Backend connectivity shown in the shell. TODO: remove once real features exist. */
  protected readonly systemInfo = injectSystemInfoQuery();

  protected readonly apiStatus = computed(() => {
    if (this.systemInfo.isPending()) return 'checking…';
    if (this.systemInfo.isError()) return 'unreachable';
    return `connected (${this.systemInfo.data()?.environment})`;
  });

  protected readonly apiStatusClass = computed(() => {
    if (this.systemInfo.isPending()) return 'bg-surface-container-high text-on-surface-variant';
    return this.systemInfo.isError()
      ? 'bg-error-container text-on-error-container'
      : 'bg-primary-container text-on-primary-container';
  });
}
