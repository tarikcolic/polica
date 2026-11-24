import { NgModule } from '@angular/core';
import { SharedModule } from '../../shared/shared-module';
import { CodeListsRoutingModule } from './code-lists-routing-module';
import { DosageFormUpsertComponent } from './dosage-forms/dosage-form-upsert/dosage-form-upsert.component';
import { DosageFormsComponent } from './dosage-forms/dosage-forms.component';
import { ManufacturerUpsertComponent } from './manufacturers/manufacturer-upsert/manufacturer-upsert.component';
import { ManufacturersComponent } from './manufacturers/manufacturers.component';
import { WriteOffReasonUpsertComponent } from './write-off-reasons/write-off-reason-upsert/write-off-reason-upsert.component';
import { WriteOffReasonsComponent } from './write-off-reasons/write-off-reasons.component';

/**
 * Polica (#6): the code lists (šifarnici) of the chain: manufacturers, dosage forms and write-off reasons.
 * Lazy-loaded at /admin/code-lists (admin-routing-module.ts), so its code is downloaded only when an
 * administrator opens one of the pages; the other roles never need it.
 *
 * SharedModule brings Angular Material, forms, the translate pipe and the shared components
 * (paginator bar, table skeleton, empty state, status badge, field error). The dialogs are opened with
 * MatDialog.open(Component), so they are declared here and need no route.
 *
 * Other screens that only need the dropdown options (the medication form in #10, the write-off form
 * in #20) call the lookup() methods of the API services directly and do not import this module.
 */
@NgModule({
  declarations: [
    ManufacturersComponent,
    ManufacturerUpsertComponent,
    DosageFormsComponent,
    DosageFormUpsertComponent,
    WriteOffReasonsComponent,
    WriteOffReasonUpsertComponent,
  ],
  imports: [CodeListsRoutingModule, SharedModule],
})
export class CodeListsModule {}
