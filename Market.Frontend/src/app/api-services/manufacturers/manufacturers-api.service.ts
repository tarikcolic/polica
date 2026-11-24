import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { buildHttpParams } from '../../core/models/build-http-params';
import { CreatedResponse } from '../../core/models/created-response';
import {
  CreateManufacturerCommand,
  GetManufacturerByIdQueryDto,
  ListManufacturersRequest,
  ListManufacturersResponse,
  ManufacturerLookupQueryDto,
  UpdateManufacturerCommand,
} from './manufacturers-api.model';

/**
 * Polica (#6): thin HTTP client for ManufacturersController (one method per endpoint, no state,
 * no UI logic; see api-services/readme.md).
 *
 * Reads work for every logged-in user. create/update/delete need the administrator: for anyone else the
 * backend answers 403 and errorLoggingInterceptor shows "Nemate dozvolu za ovu akciju." by itself.
 * The access token is added by authInterceptor, so no method here deals with it.
 */
@Injectable({
  providedIn: 'root',
})
export class ManufacturersApiService {
  private readonly baseUrl = `${environment.apiUrl}/api/manufacturers`;
  private http = inject(HttpClient);

  /** GET /api/manufacturers – one page, sorted by name, optionally filtered by part of the name. */
  list(request: ListManufacturersRequest): Observable<ListManufacturersResponse> {
    return this.http.get<ListManufacturersResponse>(this.baseUrl, {
      params: buildHttpParams(request),
    });
  }

  /** GET /api/manufacturers/lookup – all manufacturers as { id, name } for dropdowns. */
  lookup(): Observable<ManufacturerLookupQueryDto[]> {
    return this.http.get<ManufacturerLookupQueryDto[]>(`${this.baseUrl}/lookup`);
  }

  /** GET /api/manufacturers/{id} – 404 if it does not exist (or was deleted). */
  getById(id: number): Observable<GetManufacturerByIdQueryDto> {
    return this.http.get<GetManufacturerByIdQueryDto>(`${this.baseUrl}/${id}`);
  }

  /** POST /api/manufacturers → 201 { id }. A duplicate name → 400 with a field error on "name". */
  create(payload: CreateManufacturerCommand): Observable<CreatedResponse> {
    return this.http.post<CreatedResponse>(this.baseUrl, payload);
  }

  /** PUT /api/manufacturers/{id} → 204. */
  update(id: number, payload: UpdateManufacturerCommand): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, payload);
  }

  /** DELETE /api/manufacturers/{id} → 204 (soft delete on the backend). */
  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
