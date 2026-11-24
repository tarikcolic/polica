import { Component, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ManufacturersApiService } from '../../../../api-services/manufacturers/manufacturers-api.service';
import {
  ListManufacturersQueryDto,
  ListManufacturersRequest,
  ListManufacturersResponse,
} from '../../../../api-services/manufacturers/manufacturers-api.model';
import { CodeListMessages, CodeListPageComponent } from '../code-list-page';
import { ManufacturerUpsertComponent } from './manufacturer-upsert/manufacturer-upsert.component';

/**
 * Polica (#6): /admin/code-lists/manufacturers, the manufacturers code list (administrator only).
 * Search, paging, create/edit and delete come from CodeListPageComponent; this class only connects
 * them to ManufacturersApiService and ManufacturerUpsertComponent, and names the table columns.
 */
@Component({
  selector: 'app-manufacturers',
  standalone: false,
  templateUrl: './manufacturers.component.html',
  styleUrl: './manufacturers.component.scss',
})
export class ManufacturersComponent extends CodeListPageComponent<
  ListManufacturersQueryDto,
  ListManufacturersRequest
> {
  private readonly api = inject(ManufacturersApiService);

  /** Column order of the mat-table (matColumnDef names in the template). */
  readonly displayedColumns = ['name', 'country', 'actions'];

  protected readonly upsertDialog = ManufacturerUpsertComponent;
  protected readonly messages: CodeListMessages = {
    created: 'CODE_LISTS.MANUFACTURERS.CREATED',
    updated: 'CODE_LISTS.MANUFACTURERS.UPDATED',
    deleted: 'CODE_LISTS.MANUFACTURERS.DELETED',
  };

  constructor() {
    super();
    // BaseListPagedComponent reads paging from this.request (page 1, 10 rows)
    this.request = new ListManufacturersRequest();
  }

  protected fetchPage(request: ListManufacturersRequest): Observable<ListManufacturersResponse> {
    return this.api.list(request);
  }

  protected deleteById(id: number): Observable<void> {
    return this.api.delete(id);
  }
}
