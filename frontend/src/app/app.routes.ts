import { inject } from '@angular/core';
import { Routes } from '@angular/router';

import { CurrentUser, ROLE_HOME } from './core/session/current-user';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: () => ROLE_HOME[inject(CurrentUser).user().role],
  },
  {
    path: 'my-claims',
    loadChildren: () => import('./features/claimant/claimant.routes').then((m) => m.claimantRoutes),
  },
  {
    path: 'work',
    loadChildren: () => import('./features/staff/staff.routes').then((m) => m.staffRoutes),
  },
  { path: '**', redirectTo: '' },
];
