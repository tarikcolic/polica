import { Component, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { DosageFormsApiService } from '../../../../api-services/dosage-forms/dosage-forms-api.service';
import {
  ListDosageFormsQueryDto,
  ListDosageFormsRequest,
  ListDosageFormsResponse,
} from '../../../../api-services/dosage-forms/dosage-forms-api.model';
import { CodeListMessages, CodeListPageComponent } from '../code-list-page';
import { DosageFormUpsertComponent } from './dosage-form-upsert/dosage-form-upsert.component';

/**
 * Polica (#6): /admin/code-lists/dosage-forms, the dosage forms code list (administrator only).
 * Same page as ManufacturersComponent, with a name column only; the behavior is in CodeListPageComponent.
 */
@Component({
  selector: 'app-dosage-forms',
  standalone: false,
  templateUrl: './dosage-forms.component.html',
  styleUrl: './dosage-forms.component.scss',
})
export class DosageFormsComponent extends CodeListPageComponent<
  ListDosageFormsQueryDto,
  ListDosageFormsRequest
> {
  private readonly api = inject(DosageFormsApiService);

  readonly displayedColumns = ['name', 'actions'];

  protected readonly upsertDialog = DosageFormUpsertComponent;
  protected readonly messages: CodeListMessages = {
    created: 'CODE_LISTS.DOSAGE_FORMS.CREATED',
    updated: 'CODE_LISTS.DOSAGE_FORMS.UPDATED',
    deleted: 'CODE_LISTS.DOSAGE_FORMS.DELETED',
  };

  constructor() {
    super();
    this.request = new ListDosageFormsRequest();
  }

  protected fetchPage(request: ListDosageFormsRequest): Observable<ListDosageFormsResponse> {
    return this.api.list(request);
  }

  protected deleteById(id: number): Observable<void> {
    return this.api.delete(id);
  }
}
