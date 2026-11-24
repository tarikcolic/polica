import { NgModule } from '@angular/core';
import { SharedModule } from '../../shared/shared-module';
import { PharmaciesRoutingModule } from './pharmacies-routing-module';
import { PharmaciesComponent } from './pharmacies/pharmacies.component';
import { PharmacyUpsertComponent } from './pharmacy-upsert/pharmacy-upsert.component';

/**
 * Polica (#7): the pharmacy registry of the chain (list + create/edit page). Lazy-loaded at /admin/pharmacies
 * (admin-routing-module.ts), so its code is downloaded only when an administrator opens it.
 *
 * SharedModule brings Angular Material, forms, the translate pipe and the shared components (paginator bar,
 * table skeleton, empty state, status badge, field error). Other screens that only need a pharmacy dropdown
 * (employee form in #8, transfer in #18) call PharmaciesApiService.lookup() and do not import this module.
 */
@NgModule({
  declarations: [PharmaciesComponent, PharmacyUpsertComponent],
  imports: [PharmaciesRoutingModule, SharedModule],
})
export class PharmaciesModule {}
