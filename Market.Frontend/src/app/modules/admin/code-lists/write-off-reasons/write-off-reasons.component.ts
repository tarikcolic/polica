import { Component, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { WriteOffReasonsApiService } from '../../../../api-services/write-off-reasons/write-off-reasons-api.service';
import {
  ListWriteOffReasonsQueryDto,
  ListWriteOffReasonsRequest,
  ListWriteOffReasonsResponse,
} from '../../../../api-services/write-off-reasons/write-off-reasons-api.model';
import { CodeListMessages, CodeListPageComponent } from '../code-list-page';
import { WriteOffReasonUpsertComponent } from './write-off-reason-upsert/write-off-reason-upsert.component';

/**
 * Polica (#6): /admin/code-lists/write-off-reasons, the write-off reasons code list (administrator only).
 * Same page as ManufacturersComponent; the extra column shows whether a reason requires a note
 * (a status badge from SharedModule), which the write-off form uses in #20.
 */
@Component({
  selector: 'app-write-off-reasons',
  standalone: false,
  templateUrl: './write-off-reasons.component.html',
  styleUrl: './write-off-reasons.component.scss',
})
export class WriteOffReasonsComponent extends CodeListPageComponent<
  ListWriteOffReasonsQueryDto,
  ListWriteOffReasonsRequest
> {
  private readonly api = inject(WriteOffReasonsApiService);

  readonly displayedColumns = ['name', 'requiresNote', 'actions'];

  protected readonly upsertDialog = WriteOffReasonUpsertComponent;
  protected readonly messages: CodeListMessages = {
    created: 'CODE_LISTS.WRITE_OFF_REASONS.CREATED',
    updated: 'CODE_LISTS.WRITE_OFF_REASONS.UPDATED',
    deleted: 'CODE_LISTS.WRITE_OFF_REASONS.DELETED',
  };

  constructor() {
    super();
    this.request = new ListWriteOffReasonsRequest();
  }

  protected fetchPage(request: ListWriteOffReasonsRequest): Observable<ListWriteOffReasonsResponse> {
    return this.api.list(request);
  }

  protected deleteById(id: number): Observable<void> {
    return this.api.delete(id);
  }
}
