import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DosageFormsComponent } from './dosage-forms/dosage-forms.component';
import { ManufacturersComponent } from './manufacturers/manufacturers.component';
import { WriteOffReasonsComponent } from './write-off-reasons/write-off-reasons.component';

/**
 * Polica (#6): routes of the code lists, relative to /admin/code-lists (see admin-routing-module.ts,
 * where the administrator check sits on the parent route, so it covers all three pages).
 * The pages open inside the staff shell (AdminLayoutComponent's <router-outlet>), with the menu on the left.
 */
const routes: Routes = [
  // /admin/code-lists on its own → the first list in the menu
  { path: '', redirectTo: 'manufacturers', pathMatch: 'full' },
  { path: 'manufacturers', component: ManufacturersComponent },
  { path: 'dosage-forms', component: DosageFormsComponent },
  { path: 'write-off-reasons', component: WriteOffReasonsComponent },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class CodeListsRoutingModule {}
