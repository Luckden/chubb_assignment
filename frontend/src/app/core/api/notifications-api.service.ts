import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import type { MarkNotificationReadRequest, Notification } from './api-types';

/** HttpClient calls for `/api/notifications`. Components use the helpers in notifications.queries.ts. */
@Injectable({ providedIn: 'root' })
export class NotificationsApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/notifications`;

  list(userId: string): Observable<Notification[]> {
    return this.http.get<Notification[]>(this.baseUrl, {
      params: new HttpParams().set('userId', userId),
    });
  }

  markRead(id: string, body: MarkNotificationReadRequest): Observable<Notification> {
    return this.http.post<Notification>(`${this.baseUrl}/${id}/read`, body);
  }
}
