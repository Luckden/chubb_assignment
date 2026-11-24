import { ChangeDetectionStrategy, Component, computed } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { LucideAngularModule, ShieldCheck } from 'lucide-angular';

import { injectSystemInfoQuery } from './core/api/system.queries';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, LucideAngularModule],
  templateUrl: './app.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App {
  protected readonly logoIcon = ShieldCheck;

  /** Backend connectivity shown in the shell. TODO: remove once real features exist. */
  protected readonly systemInfo = injectSystemInfoQuery();

  protected readonly apiStatus = computed(() => {
    if (this.systemInfo.isPending()) return 'checking…';
    if (this.systemInfo.isError()) return 'unreachable';
    return `connected (${this.systemInfo.data()?.environment})`;
  });

  protected readonly apiBadgeClass = computed(() => {
    if (this.systemInfo.isPending()) return 'badge-ghost';
    return this.systemInfo.isError() ? 'badge-error' : 'badge-success';
  });
}
