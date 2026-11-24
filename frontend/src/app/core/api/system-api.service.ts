import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';

export interface SystemInfo {
  service: string;
  environment: string;
  version: string | null;
}

/**
 * Placeholder API client proving frontend → backend wiring.
 * TODO: replace with domain API clients (ideally generated from the backend OpenAPI document).
 */
@Injectable({ providedIn: 'root' })
export class SystemApiService {
  private readonly http = inject(HttpClient);

  getInfo(): Observable<SystemInfo> {
    return this.http.get<SystemInfo>(`${environment.apiBaseUrl}/system/info`);
  }
}
