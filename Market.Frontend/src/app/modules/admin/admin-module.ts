import {NgModule} from '@angular/core';

import {AdminRoutingModule} from './admin-routing-module';
import {AdminLayoutComponent} from './admin-layout/admin-layout.component';
import {SharedModule} from '../shared/shared-module';
import {AccountModule} from '../account/account-module';
import { AdminSettingsComponent } from './admin-settings/admin-settings.component';
import { HomeComponent } from './home/home.component';
import { UiKitComponent } from './ui-kit/ui-kit.component';

/**
 * Polica (#4): the staff area for all three roles, lazy-loaded at /admin (see app-routing-module.ts).
 * The starter's shop pages (products, categories, promotions, orders, exchange rates) were removed.
 * Feature areas of later issues (code lists, purchase orders, stock, transfers) get their own
 * lazy-loaded modules as child routes of this shell, so each one is downloaded only when it is opened.
 */
@NgModule({
  declarations: [
    AdminLayoutComponent,
    AdminSettingsComponent,
    HomeComponent,
    // Polica (#5): /admin/ui-kit, sample page of the shared components (admin only)
    UiKitComponent,
  ],
  imports: [
    AdminRoutingModule,
    SharedModule,
    // "Moj profil" (/admin/profile) – page declared in AccountModule
    AccountModule,
  ]
})
export class AdminModule { }
