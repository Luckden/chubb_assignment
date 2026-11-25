import { Routes } from '@angular/router';

export const claimantRoutes: Routes = [
  {
    path: '',
    title: 'My claims',
    loadComponent: () => import('./my-claims-page').then((m) => m.MyClaimsPage),
  },
  {
    path: 'new',
    title: 'Report an incident',
    loadComponent: () => import('./report-incident-page').then((m) => m.ReportIncidentPage),
  },
  {
    path: ':id',
    title: 'Claim',
    loadComponent: () => import('./claimant-claim-page').then((m) => m.ClaimantClaimPage),
  },
];
