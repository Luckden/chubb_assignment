import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { RouterOutlet } from '@angular/router';
import { catchError, map, of } from 'rxjs';

import { SystemApiService } from './core/api/system-api.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App {
  private readonly systemApi = inject(SystemApiService);

  /** Backend connectivity status shown in the shell. TODO: remove once real features exist. */
  protected readonly apiStatus = toSignal(
    this.systemApi.getInfo().pipe(
      map((info) => `connected (${info.environment})`),
      catchError(() => of('unreachable')),
    ),
    { initialValue: 'checking…' },
  );
}
