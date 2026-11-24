import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import type { SystemInfo } from './api-types';

/**
 * Placeholder API client proving frontend → backend wiring.
 * Pattern for real clients: HttpClient + types generated from the backend OpenAPI document (see api-types.ts).
 */
@Injectable({ providedIn: 'root' })
export class SystemApiService {
  private readonly http = inject(HttpClient);

  getInfo(): Observable<SystemInfo> {
    return this.http.get<SystemInfo>(`${environment.apiBaseUrl}/system/info`);
  }
}
