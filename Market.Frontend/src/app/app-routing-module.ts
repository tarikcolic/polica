import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import {myAuthData, myAuthGuard} from './core/guards/my-auth-guard';

/**
 * Polica (#4): top-level routes.
 *
 * Polica is an internal system for pharmacy staff, so the starter's public shop (/, /search, /cart,
 * /checkout, /converter) and the customer area (/client) were removed. What is left:
 *   /auth/...  login, logout, forgot and reset password (no self-registration: the administrator
 *              creates accounts in #8)
 *   /admin/... one shell for all three roles; pages inside it may require a specific role
 *              (see admin-routing-module.ts)
 * Every other address goes to /admin. For a visitor who is not logged in, myAuthGuard then
 * redirects to /auth/login?returnUrl=/admin, so the login page is the effective start page.
 */
const routes: Routes = [
  {
    path: 'admin',
    canActivate: [myAuthGuard],
    // any logged-in user (administrator, manager or pharmacist); the starter required the admin role here
    data: myAuthData({ requireAuth: true }),
    loadChildren: () =>
      import('./modules/admin/admin-module').then(m => m.AdminModule)
  },
  {
    path: 'auth',
    loadChildren: () =>
      import('./modules/auth/auth-module').then(m => m.AuthModule)
  },
  { path: '', redirectTo: 'admin', pathMatch: 'full' },
  // fallback 404
  { path: '**', redirectTo: 'admin' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
