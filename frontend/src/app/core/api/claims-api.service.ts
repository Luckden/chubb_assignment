import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import type {
  Claim,
  ClaimListFilter,
  ClaimSummary,
  PickUpClaimRequest,
  ProvideInformationRequest,
  RejectClaimRequest,
  RequestInformationRequest,
  SettleClaimRequest,
  SubmitClaimRequest,
} from './api-types';

/** HttpClient calls for `/api/claims`. Components use the helpers in claims.queries.ts, not this service. */
@Injectable({ providedIn: 'root' })
export class ClaimsApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/claims`;

  list(filter: ClaimListFilter): Observable<ClaimSummary[]> {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(filter)) {
      if (value !== undefined && value !== null) params = params.set(key, String(value));
    }
    return this.http.get<ClaimSummary[]>(this.baseUrl, { params });
  }

  get(id: string): Observable<Claim> {
    return this.http.get<Claim>(`${this.baseUrl}/${id}`);
  }

  submit(body: SubmitClaimRequest): Observable<Claim> {
    return this.http.post<Claim>(this.baseUrl, body);
  }

  pickUp(id: string, body: PickUpClaimRequest): Observable<Claim> {
    return this.http.post<Claim>(`${this.baseUrl}/${id}/pick-up`, body);
  }

  requestInformation(id: string, body: RequestInformationRequest): Observable<Claim> {
    return this.http.post<Claim>(`${this.baseUrl}/${id}/information-requests`, body);
  }

  provideInformation(id: string, body: ProvideInformationRequest): Observable<Claim> {
    return this.http.post<Claim>(`${this.baseUrl}/${id}/information-response`, body);
  }

  settle(id: string, body: SettleClaimRequest): Observable<Claim> {
    return this.http.post<Claim>(`${this.baseUrl}/${id}/settle`, body);
  }

  reject(id: string, body: RejectClaimRequest): Observable<Claim> {
    return this.http.post<Claim>(`${this.baseUrl}/${id}/reject`, body);
  }
}
