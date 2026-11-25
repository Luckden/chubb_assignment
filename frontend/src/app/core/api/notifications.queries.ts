import { inject } from '@angular/core';
import { QueryClient, injectMutation, injectQuery } from '@tanstack/angular-query';
import { lastValueFrom } from 'rxjs';

import { NotificationsApiService } from './notifications-api.service';
import { POLL_INTERVAL_MS } from './polling';

export const notificationKeys = {
  all: ['notifications'] as const,
  list: (userId: string) => [...notificationKeys.all, 'list', userId] as const,
};

/** The user's notifications, refreshed by polling. */
export function injectNotificationsQuery(userId: () => string) {
  const api = inject(NotificationsApiService);
  return injectQuery(() => {
    const current = userId();
    return {
      queryKey: notificationKeys.list(current),
      queryFn: () => lastValueFrom(api.list(current)),
      refetchInterval: POLL_INTERVAL_MS,
      refetchIntervalInBackground: false,
    };
  });
}

export function injectMarkNotificationReadMutation() {
  const api = inject(NotificationsApiService);
  const queryClient = inject(QueryClient);
  return injectMutation(() => ({
    mutationFn: (variables: { id: string; userId: string }) =>
      lastValueFrom(api.markRead(variables.id, { userId: variables.userId })),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: notificationKeys.all }),
  }));
}
