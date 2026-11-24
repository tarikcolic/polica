import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { unsavedChangesGuard } from '../../shared/guards/unsaved-changes.guard';
import { PharmaciesComponent } from './pharmacies/pharmacies.component';
import { PharmacyUpsertComponent } from './pharmacy-upsert/pharmacy-upsert.component';

/**
 * Polica (#7): routes of the pharmacy registry, relative to /admin/pharmacies (see admin-routing-module.ts,
 * where the administrator check sits on the parent route, so it covers all three pages).
 *
 * The create/edit form is a PAGE (its own URL), not a dialog like the code lists: a pharmacy has more fields,
 * and a page can be bookmarked or opened in a new tab. The same component serves both URLs; it reads ":id"
 * from the route to know whether it edits.
 * canDeactivate: leaving the form with unsaved changes asks for confirmation (shared guard from the starter).
 */
const routes: Routes = [
  { path: '', component: PharmaciesComponent },
  { path: 'new', component: PharmacyUpsertComponent, canDeactivate: [unsavedChangesGuard] },
  { path: ':id/edit', component: PharmacyUpsertComponent, canDeactivate: [unsavedChangesGuard] },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  // exporting RouterModule makes routerLink available in the components of PharmaciesModule
  exports: [RouterModule],
})
export class PharmaciesRoutingModule {}
