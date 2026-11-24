import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { AdminLayoutComponent } from './admin-layout/admin-layout.component';
import {MyProfileComponent} from '../account/my-profile/my-profile.component';
import {AdminSettingsComponent} from './admin-settings/admin-settings.component';
import { HomeComponent } from './home/home.component';
import { UiKitComponent } from './ui-kit/ui-kit.component';
import { myAuthData, myAuthGuard } from '../../core/guards/my-auth-guard';

/**
 * Polica (#4): child routes of the shared staff shell.
 *
 * app-routing-module.ts only checks that the user is logged in (any role) before this module loads.
 * Pages for a specific role add their own guard here; e.g. Settings is for the chain administrator only.
 * When a role check fails, myAuthGuard redirects to CurrentUserService.getDefaultRoute() (= /admin),
 * which is the home page below, open to every role.
 *
 * The guard only hides pages in the UI. The backend checks the role again on every endpoint
 * (policies and handler checks), because anyone can call the API directly.
 */
const routes: Routes = [
  {
    path: '',
    component: AdminLayoutComponent,
    children: [
      // HOME – default page for every role (replaced by the dashboard in #21)
      {
        path: '',
        component: HomeComponent,
        pathMatch: 'full',
      },

      // APPLICATION SETTINGS – chain administrator only (real settings come in #11)
      {
        path: 'settings',
        component: AdminSettingsComponent,
        canActivate: [myAuthGuard],
        data: myAuthData({ requireAdmin: true }),
      },

      // UI KIT (#5) – chain administrator only, not in the menu: sample of the regional formats and the
      // shared components (SharedModule) for development and review; open it by typing /admin/ui-kit
      {
        path: 'ui-kit',
        component: UiKitComponent,
        canActivate: [myAuthGuard],
        data: myAuthData({ requireAdmin: true }),
      },

      // CODE LISTS (#6) – chain administrator only: manufacturers, dosage forms, write-off reasons.
      // Lazy-loaded module with its own child routes (code-lists/code-lists-routing-module.ts); the guard
      // here covers all of them. Other roles still read the lists through the API (dropdowns in #10, #20).
      {
        path: 'code-lists',
        canActivate: [myAuthGuard],
        data: myAuthData({ requireAdmin: true }),
        loadChildren: () =>
          import('./code-lists/code-lists-module').then(m => m.CodeListsModule),
      },

      // MY PROFILE – every role (page declared in AccountModule)
      {
        path: 'profile',
        component: MyProfileComponent,
      },

      // unknown address inside /admin (e.g. an old /admin/products bookmark) → home
      {
        path: '**',
        redirectTo: '',
      },
    ],
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class AdminRoutingModule {}
