import { Routes } from '@angular/router';

export const staffRoutes: Routes = [
  {
    path: '',
    title: 'Work queue',
    loadComponent: () => import('./work-queue-page').then((m) => m.WorkQueuePage),
  },
  {
    path: ':id',
    title: 'Claim',
    loadComponent: () => import('./staff-claim-page').then((m) => m.StaffClaimPage),
  },
];
