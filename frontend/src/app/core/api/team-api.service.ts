import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import type { Exposure, Workload } from './api-types';

/**
 * HttpClient calls for the officer team views (RFC-0003): `/api/workload` and `/api/exposure`.
 * Components use the helpers in workload.queries.ts and exposure.queries.ts.
 */
@Injectable({ providedIn: 'root' })
export class TeamApiService {
  private readonly http = inject(HttpClient);

  workload(officerId: string): Observable<Workload> {
    return this.http.get<Workload>(`${environment.apiBaseUrl}/workload`, {
      params: new HttpParams().set('officerId', officerId),
    });
  }

  exposure(officerId: string): Observable<Exposure> {
    return this.http.get<Exposure>(`${environment.apiBaseUrl}/exposure`, {
      params: new HttpParams().set('officerId', officerId),
    });
  }
}
